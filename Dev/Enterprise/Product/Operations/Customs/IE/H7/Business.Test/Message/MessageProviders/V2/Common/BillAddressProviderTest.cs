using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class BillAddressProviderTest : DataProviderTestCase<BillAddressProvider>
	{
		public void TestCity()
		{
			SetUpTestData();
			AssertEquals("Consignee City", "ConsigneeCity", consigneeProvider.City);
			AssertEquals("Shipper City", "ShipperCity", shipperProvider.City);
		}

		public void TestCountry()
		{
			SetUpTestData();
			AssertEquals("Consignee Country", "FR", consigneeProvider.Country);
			AssertEquals("Shipper Country", "IE", shipperProvider.Country);
		}

		public void TestStreetAndNumber()
		{
			SetUpTestData();
			AssertEquals("Consignee StreetAndNumber", "ConsigneeStreet1 ConsigneeStreet2", consigneeProvider.StreetAndNumber);
			AssertEquals("Shipper StreetAndNumber", "ShipperStreet1 ShipperStreet2", shipperProvider.StreetAndNumber);
		}

		public void TestPostcode()
		{
			SetUpTestData();
			AssertEquals("Consignee Post Code", "1234", consigneeProvider.Postcode);
			AssertEquals("Shipper Post Code", "3456", shipperProvider.Postcode);
		}

		void SetUpTestData()
		{
			header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
			bill.ABL_ConsigneeCity = "ConsigneeCity";
			bill.ABL_ShipperCity = "ShipperCity";
			bill.ABL_RN_NKConsigneeCountry = "FR";
			bill.ABL_RN_NKShipperCountry = "IE";
			bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			bill.ABL_ShipperStreet1 = "ShipperStreet1";
			bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			bill.ABL_ShipperStreet2 = "ShipperStreet2";
			bill.ABL_ConsigneePostcode = "1234";
			bill.ABL_ShipperPostcode = "3456";

			consigneeProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Consignee);
			shipperProvider = GenerateProvider(bill, AsycudaBillAddress.AddressType.Shipper);
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
		BillAddressProvider consigneeProvider;
		BillAddressProvider shipperProvider;

		BillAddressProvider GenerateProvider(AsycudaBill bill, AsycudaBillAddress.AddressType addressType) => new BillAddressProvider(bill, addressType);

		protected sealed override BillAddressProvider GetProvider()
		{
			return new BillAddressProvider(bill, AsycudaBillAddress.AddressType.Consignee);
		}
	}
}
