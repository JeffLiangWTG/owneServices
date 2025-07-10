using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(GlobalContainerTrackingUsage))]
	class GlobalContainerTrackingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new GlobalContainerTrackingUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.GlobalContainerTracking, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlobalContainerTrackingUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
