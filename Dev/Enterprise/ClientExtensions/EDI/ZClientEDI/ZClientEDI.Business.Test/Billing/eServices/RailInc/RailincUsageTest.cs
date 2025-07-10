using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(RailincUsage))]
	class RailincUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new RailincUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.RailincByMessage, usage.SystemCode);
			AssertEquals("RIC", usage.PriceItemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new RailincUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
