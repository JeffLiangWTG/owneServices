using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business.Test;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientInvoiceDeliveryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSystemCodes()
		{
			UsageBillingSettingsTest.SetupValidTestRegistry();

			ClientInvoiceDelivery billing = Factory.New<ClientInvoiceDelivery>();
			var expectedList = BillingConstants.GetAllBillingSystems();
			expectedList.AddPair(BillingConstants.PriceHeaderType.LDaaS, "Logistics Devices as a Service");
			expectedList.AddPair("PL0", "ABC - Price List #0");
			expectedList.AddPair("PL1", "ABC - Price List #1");
			AssertContainsExactElementsInAnyOrder(expectedList, billing.Lookups.SystemCodes);
		}

		public void TestServerCodes()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			ClientInvoiceDelivery billing = lic.Company.InvoiceDeliveries.AddNew();
			AssertContainsExactElementsInAnyOrder(billing.GetServerCodes(), billing.Lookups.ServerCodes);
			var db = lic.Company.LicDatabases.AddNew();
			db.LD_ServerCode = "ZZ1";
			Assert(billing.GetServerCodes().ContainsCode("ZZ1"));
			AssertContainsExactElementsInAnyOrder(billing.GetServerCodes(), billing.Lookups.ServerCodes);
		}
	}
}
