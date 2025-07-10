using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ZACustomsUsage))]
	internal class ZACustomsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new ZACustomsUsage("ZX1", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.ZACustoms, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ZACustomsUsage("ZX1", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
