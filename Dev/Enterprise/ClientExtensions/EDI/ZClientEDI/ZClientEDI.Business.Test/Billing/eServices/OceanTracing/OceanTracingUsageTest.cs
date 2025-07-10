using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(OceanTracingUsage))]
	internal class OceanTracingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new OceanTracingUsage("OCT", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.OceanTracing, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OceanTracingUsage("OCT", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
