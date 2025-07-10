using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Test
{
	[TestedType(typeof(ShippingPortMessagingUsage))]
	internal class ShippingPortMessagingUsageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSystemCode()
		{
			var usage = new ShippingPortMessagingUsage("SDT", Factory, new UsingParty(), EdiDateTest.MonthToday);
			AssertEquals(BillingConstants.BillingSystem.ShippingPortMessaging, usage.SystemCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ShippingPortMessagingUsage("SDT", Factory, new UsingParty(), EdiDateTest.MonthToday);
		}
	}
}
