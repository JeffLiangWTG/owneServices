using System.Linq;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class BillingTransactionTests
	{
		[TestMethod]
		public void TestHasSamePropertiesAsAPITransaction()
		{
			CollectionAssert.AreEqual(
				typeof(APIBillingTransaction).GetProperties().Select(p => new {p.Name, p.PropertyType})
					.OrderBy(p => p.Name).ToList(),
				typeof(BillingTransaction).GetProperties().Select(p => new {p.Name, p.PropertyType})
					.OrderBy(p => p.Name).ToList());

			CollectionAssert.AreEqual(
				typeof(APIBillingTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList(),
				typeof(BillingTransactionForJSON).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList());
		}
	}
}
