using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class Parties02ProviderTest : DataProviderTestCase<Parties02Provider>
	{
		public void TestImporter()
		{
			SetUpTestData();
			bill.ABL_ConsigneeName = "Consignee";
			bill.ABL_ConsigneeStreet1 = "ConsigneeStreet1";
			bill.ABL_ConsigneeStreet2 = "ConsigneeStreet2";
			bill.ABL_ConsigneePostcode = "CPostcode";
			bill.ABL_ConsigneeCity = "ConsigneeCity";
			bill.ABL_RN_NKConsigneeCountry = "US";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Importer);
				AssertEquals("Provider.Importer.Name", "Consignee", Provider.Importer.Name);
				AssertEquals("Provider.Importer.Id", "NR", Provider.Importer.Id);
				AssertEquals("Provider.Importer.Address.StreetAndNumber", "ConsigneeStreet1 ConsigneeStreet2", Provider.Importer.Address.StreetAndNumber);
				AssertEquals("Provider.Importer.Address.City", "ConsigneeCity", Provider.Importer.Address.City);
				AssertEquals("Provider.Importer.Address.Country", "US", Provider.Importer.Address.Country);
				AssertEquals("Provider.Importer.Address.Postcode", "CPostcode", Provider.Importer.Address.Postcode);
			});
		}

		public void TestExporter_NoShipperOA()
		{
			SetUpTestData();
			bill.ABL_ShipperName = "Shipper";
			bill.ABL_ShipperStreet1 = "ShipperStreet1";
			bill.ABL_ShipperStreet2 = "ShipperStreet2";
			bill.ABL_ShipperPostcode = "SPostcode";
			bill.ABL_ShipperCity = "ShipperCity";
			bill.ABL_RN_NKShipperCountry = "US";

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Exporter);
				AssertEquals("Provider.Exporter.Name", "Shipper", Provider.Exporter.Name);
				AssertEquals("Provider.Exporter.Id", string.Empty, Provider.Exporter.Id);
				AssertEquals("Provider.Exporter.Address.StreetAndNumber", "ShipperStreet1 ShipperStreet2", Provider.Exporter.Address.StreetAndNumber);
				AssertEquals("Provider.Exporter.Address.City", "ShipperCity", Provider.Exporter.Address.City);
				AssertEquals("Provider.Exporter.Address.Country", "US", Provider.Exporter.Address.Country);
				AssertEquals("Provider.Exporter.Address.Postcode", "SPostcode", Provider.Exporter.Address.Postcode);
			});
		}

		public void TestExporter_ShipperOA()
		{
			SetUpTestData();
			var orgHeader = Factory.New<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();

			address.OA_CompanyNameOverride = "Shipper";
			address.OA_Address1 = "ShipperStreet1";
			address.OA_Address2 = "ShipperStreet2";
			address.OA_PostCode = "SPostcode";
			address.OA_City = "ShipperCity";
			address.OA_RN_NKCountryCode = "US";

			bill.ABL_OA_Shipper = address.PK;

			CombineAssertions(() =>
			{
				AssertType<BillPartyProvider>(Provider.Exporter);
				AssertEquals("Provider.Exporter.Name", "Shipper", Provider.Exporter.Name);
				AssertEquals("Provider.Exporter.Id", string.Empty, Provider.Exporter.Id);
				AssertEquals("Provider.Exporter.Address.StreetAndNumber", "ShipperStreet1 ShipperStreet2", Provider.Exporter.Address.StreetAndNumber);
				AssertEquals("Provider.Exporter.Address.City", "ShipperCity", Provider.Exporter.Address.City);
				AssertEquals("Provider.Exporter.Address.Country", "US", Provider.Exporter.Address.Country);
				AssertEquals("Provider.Exporter.Address.Postcode", "SPostcode", Provider.Exporter.Address.Postcode);
			});
		}

		protected override Parties02Provider GetProvider()
		{
			SetUpTestData();
			return new Parties02Provider(bill);
		}

		void SetUpTestData()
		{
			if (bill == null)
			{
				header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
			}
		}

		AsycudaManifestHeader header;
		AsycudaBill bill;
	}
}
