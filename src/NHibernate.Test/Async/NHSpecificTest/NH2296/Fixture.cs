﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Driver;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2296
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
			return !(factory.ConnectionProvider.Driver is OracleManagedDataClientDriver);
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var o = new Order() { AccountName = "Acct1" };
				o.Products.Add(new Product() { StatusReason = "Success", Order = o });
				o.Products.Add(new Product() { StatusReason = "Failure", Order = o });
				s.Save(o);

				o = new Order() { AccountName = "Acct2" };
				s.Save(o);

				o = new Order() { AccountName = "Acct3" };
				o.Products.Add(new Product() { StatusReason = "Success", Order = o });
				o.Products.Add(new Product() { StatusReason = "Failure", Order = o });
				s.Save(o);

				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.Delete("from Product");
				s.Delete("from Order");
				tx.Commit();
			}

			base.OnTearDown();
		}

		[Test]
		public async Task TestAsync()
		{
			// This test causes lazy loading of products to use the first query, restricted to id, as a "in (sub-query)" clause.
			if (!Dialect.SupportsSubSelectsWithPagingAsInPredicateRhs)
				Assert.Ignore("Current dialect does not support paging within IN sub-queries");

			using (var s = OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var orders = await (s.CreateQuery("select o from Order o") 
					.SetMaxResults(2)
					.ListAsync<Order>());

				// trigger lazy-loading of products, using subselect fetch. 
				string sr = orders[0].Products[0].StatusReason;

				// count of entities we want:
				int ourEntities = orders.Count + orders.Sum(o => o.Products.Count);

				Assert.That(s.Statistics.EntityCount, Is.EqualTo(ourEntities));
			}
		}
	}
}