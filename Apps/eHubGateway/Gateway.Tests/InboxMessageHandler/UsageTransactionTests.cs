using System.Linq;
using APIUsageTransaction = CargoWise.Billing.API.UsageTransaction;
using CargoWise.Billing.Kafka.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Gateway.Tests.InboxMessageHandler
{
	[TestClass]
	public class UsageTransactionTests
	{
		[TestMethod]
		public void TestHasSamePropertiesAsAPITransaction()
		{
			CollectionAssert.AreEqual(
				typeof(APIUsageTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList(),
				typeof(V2_1.UsageTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList());

			CollectionAssert.AreEqual(
				typeof(APIUsageTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList(),
				typeof(UsageTransactionForJSON).GetProperties().Select(p => new { p.Name, p.PropertyType })
					.OrderBy(p => p.Name).ToList());
		}
	}
}
