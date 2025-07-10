using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ForwardAirUsage))]
	internal class ForwardAirUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new ForwardAirUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.ForwardAir, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ForwardAirUsage(Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
