using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(OceanCarrierMessagingUsage))]
	class OceanCarrierMessagingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new OceanCarrierMessagingUsage("SHI", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.OceanCarrierMessaging, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OceanCarrierMessagingUsage("SHI", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
