using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(FlightStatsUsage))]
	internal class FlightStatsUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new FlightStatsUsage(Factory, "FMS", "FMS", new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.FlightStats, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FlightStatsUsage(Factory, "FMS", "FMS", new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
