using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class Parties03ProviderTest : DataProviderTestCase<Parties03Provider>
	{
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

		public void TestAdditionalFiscalReference()
		{
			var providerWithNoSellerRegNo = GetProvider();
			AssertEquals(0, providerWithNoSellerRegNo.AdditionalFiscalReference.Count);

			bill.ABL_SellerRegNo = "332";
			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.AdditionalFiscalReference.Count);
				var additionalFiscalReference = Provider.AdditionalFiscalReference.Single();
				AssertEquals("332", additionalFiscalReference.Number);
				AssertEquals("FR5", additionalFiscalReference.Type);
			});
		}

		protected override Parties03Provider GetProvider()
		{
			SetUpTestData();
			return new Parties03Provider(packedItem);
		}

		void SetUpTestData()
		{
			if (packedItem == null)
			{
				var header = Factory.New<AsycudaManifestHeader>();
				bill = header.Bills.AddNew();
				pack = bill.Packs.AddNew();
				packedItem = bill.PackedItems.AddNew();
				packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			}
		}
		AsycudaBill bill;
		AsycudaPack pack;
		AsycudaPackedItem packedItem;
	}
}
