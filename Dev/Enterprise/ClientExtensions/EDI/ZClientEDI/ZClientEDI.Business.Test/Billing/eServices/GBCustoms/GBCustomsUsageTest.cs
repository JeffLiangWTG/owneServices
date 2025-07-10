using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(GBCustomsUsage))]
	internal class GBCustomsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new GBCustomsUsage("AWB", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.GBCustoms, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GBCustomsUsage("AWB", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
