using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	public class GbCDSH7ImportGoodsShipmentWrapperTest : TestCaseWithFactory
	{
		public void TestImporterBuyer()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = "UK";
			bill.ABL_ConsigneeRegNo = "001";
			bill.ABL_ConsigneeStreet1 = "Address1";
			bill.ABL_ConsigneeStreet2 = "Address2";
			bill.ABL_ConsigneeCity = "City";
			bill.ABL_RN_NKConsigneeCountry = "AU";
			bill.ABL_ConsigneePostcode = "12345678";
			bill.ABL_ConsigneeName = "Consignee";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			var importer = wrapper.Importer;
			CombineAssertions("Importer", () =>
			{
				AssertEquals("Id", "", importer.ID);
				AssertEquals("Name", "Consignee", importer.Name);
				AssertEquals("Country code", "AU", importer.Address.CountryCode);
				AssertEquals("Address line", "Address1 Address2", importer.Address.Line);
				AssertEquals("City", "City", importer.Address.CityName);
				AssertEquals("Postcode ID", "12345678", importer.Address.PostcodeID);
			});

			var buyer = wrapper.Buyer;
			CombineAssertions("Buyer", () =>
			{
				AssertEquals("Id", "UK001", buyer.ID);
				AssertEquals("Name", "Consignee", buyer.Name);
				AssertEquals("Country code", "AU", buyer.Address.CountryCode);
				AssertEquals("Address line", "Address1Address2", buyer.Address.Line);
				AssertEquals("City", "City", buyer.Address.CityName);
				AssertEquals("Postcode ID", "12345678", buyer.Address.PostcodeID);
			});
		}

		public void TestSellerConsignor()
		{
			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "001", "UK");

			var shipper = shipperOrg.Addresses.AddNew();
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_OA_Shipper = shipper.PK;
			bill.ABL_ShipperStreet1 = "Address1";
			bill.ABL_ShipperStreet2 = "Address2";
			bill.ABL_ShipperCity = "City";
			bill.ABL_RN_NKShipperCountry = "AU";
			bill.ABL_ShipperPostcode = "12345678";
			bill.ABL_ShipperName = "Shipper";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			var seller = wrapper.Seller;
			CombineAssertions("Seller", () =>
			{
				AssertEquals("Id", "UK001", seller.ID);
				AssertEquals("Name", "Shipper", seller.Name);
				AssertEquals("Country code", "AU", seller.Address.CountryCode);
				AssertEquals("Address line", "Address1Address2", seller.Address.Line);
				AssertEquals("City", "City", seller.Address.CityName);
				AssertEquals("Postcode ID", "12345678", seller.Address.PostcodeID);
			});

			var consignor = wrapper.Consignor;
			CombineAssertions("Consignor", () =>
			{
				AssertEquals("Id", "UK001", consignor.ID);
				AssertEquals("Name", "Shipper", consignor.Name);
				AssertEquals("Country code", "AU", consignor.Address.CountryCode);
				AssertEquals("Address line", "Address1Address2", consignor.Address.Line);
				AssertEquals("City", "City", consignor.Address.CityName);
				AssertEquals("Postcode ID", "12345678", consignor.Address.PostcodeID);
			});
		}

		public void TestArrivalTransportMeans_NonBIRDS()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = TransportModes.Sea;
			var bill = header.Bills.AddNew();

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);

			AssertNull("No arrival transport means for non BIRDS message", wrapper.ArrivalTransportMeans);
		}

		public void TestArrivalTransportMeans_BIRDS_Sea()
		{
			var seaHeader = Factory.New<AsycudaManifestHeader>();
			seaHeader.AMA_TransportMode = TransportModes.Sea;
			seaHeader.AMA_VesselName = "Sea Vessel";
			var seaBill = seaHeader.Bills.AddNew();

			AssertArrivalTransportMeansForBIRDS(seaBill, "Sea", "11", "Sea Vessel");
		}

		public void TestArrivalTransportMeans_BIRDS_Air()
		{
			var airHeader = Factory.New<AsycudaManifestHeader>();
			airHeader.AMA_TransportMode = TransportModes.Air;
			airHeader.AMA_Voyage = "Air V";
			var airBill = airHeader.Bills.AddNew();

			AssertArrivalTransportMeansForBIRDS(airBill, "Air", "40", "Air V");
		}

		public void TestArrivalTransportMeans_BIRDS_Rail()
		{
			var railHeader = Factory.New<AsycudaManifestHeader>();
			railHeader.AMA_TransportMode = TransportModes.Rail;
			railHeader.AMA_Voyage = "Rail V";
			var railBill = railHeader.Bills.AddNew();

			AssertArrivalTransportMeansForBIRDS(railBill, "Rail", "20", "Rail V");
		}

		public void TestArrivalTransportMeans_BIRDS_Road()
		{
			var roadHeader = Factory.New<AsycudaManifestHeader>();
			roadHeader.AMA_TransportMode = TransportModes.Road;
			roadHeader.AMA_VehicleRegistration = "Vehicle";
			roadHeader.AMA_Trailer1RegNo = "Trailer1";
			roadHeader.AMA_Trailer2RegNo = "Trailer2";
			var roadBill = roadHeader.Bills.AddNew();

			AssertArrivalTransportMeansForBIRDS(roadBill, "Road", "30", "Vehicle Trailer1 Trailer2");
		}

		public void TestArrivalTransportMeans_BIRDS_RoadWithNoID()
		{
			var roadHeader = Factory.New<AsycudaManifestHeader>();
			roadHeader.AMA_TransportMode = TransportModes.Road;
			var roadBill = roadHeader.Bills.AddNew();

			AssertArrivalTransportMeansForBIRDS(roadBill, "Road(No ID)", "30", "");
		}

		void AssertArrivalTransportMeansForBIRDS(AsycudaBill bill, string transportModeToTest, string expectedIdentificationTypeCode, string expectedTransportID)
		{
			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: true);

			CombineAssertions($"Arrival transport means for {transportModeToTest} transport", () =>
			{
				AssertEquals($"Mode code for {transportModeToTest}", expectedIdentificationTypeCode, wrapper.ArrivalTransportMeans.IdentificationTypeCode);
				AssertEquals($"ID for {transportModeToTest}", expectedTransportID, wrapper.ArrivalTransportMeans.ID);
			});
		}

		public void TestArrivalTransportMeans_BIRDS_UnsupportedMode()
		{
			var unsupportedModeHeader = Factory.New<AsycudaManifestHeader>();
			unsupportedModeHeader.AMA_TransportMode = TransportModes.Mail;
			var unsupportedModeBill = unsupportedModeHeader.Bills.AddNew();

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(unsupportedModeBill, isBIRDSMessage: true);

			AssertNull("No arrival transport means for unsupported transport mode", wrapper.ArrivalTransportMeans);
		}

		public void TestExportCountryID_NonBIRDS()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_RL_NKOrigin = "AUSYD";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			AssertEquals("No Export Country ID when it's not BIRDS message", "", wrapper.ExportCountryID);
		}

		public void TestExportCountryID_BIRDS()
		{
			var bill = Factory.New<AsycudaBill>();

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: true);

			CombineAssertions("Export Country ID", () =>
			{
				AssertEquals("Empty ID when no origin", "", wrapper.ExportCountryID);

				bill.ABL_RL_NKOrigin = "AUSYD";
				AssertEquals("Provide origin country code as ID", "AU", wrapper.ExportCountryID);
			});
		}

		public void TestGoodsLocation()
		{
			var bill = Factory.New<AsycudaBill>();

			bill.CusGoodsLocation.CGL_Type = "A";
			bill.CusGoodsLocation.CGL_Qualifier = "B";
			bill.CusGoodsLocation.CGL_AdditionalIdentifier = "Something";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			var goodsLocation = wrapper.GoodsLocation;
			CombineAssertions("GoodsLocation", () =>
			{
				AssertEquals("Name", "Something", goodsLocation.Name);
				AssertEquals("TypeCode", "A", goodsLocation.TypeCode);
				AssertEquals("AddressTypeCode", "B", goodsLocation.AddressTypeCode);
				AssertEquals("CountryCode", Core.Constants.CountryCodes.UnitedKingdom, goodsLocation.CountryCode);
			});
		}

		public void TestUCRTraderAssignedReferenceID()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_UCRNumber = "UCR001";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);

			AssertEquals("UCR001", wrapper.UCRTraderAssignedReferenceID);
		}

		public void TestImporterUsesABL_ConsigneeRegNoWhenItHasValidConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNoType = "ABC";
			bill.ABL_ConsigneeRegNo = "GB001";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			var importer = wrapper.Importer;
			CombineAssertions("GB", () =>
			{
				AssertEquals("Should contain ID", bill.ABL_ConsigneeRegNo, importer.ID);
				AssertEquals("Should have empty name", ZString.Empty, importer.Name);
				AssertEquals("Should have empty address", null, importer.Address);
			});

			bill.ABL_ConsigneeRegNo = "XI001";

			wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			importer = wrapper.Importer;
			CombineAssertions("XI", () =>
			{
				AssertEquals("Should contain ID", bill.ABL_ConsigneeRegNo, importer.ID);
				AssertEquals("Should have empty name", ZString.Empty, importer.Name);
				AssertEquals("Should have empty address", null, importer.Address);
			});
		}

		public void TestImporterUsesNameAndAddressWhenInvalidConsigneeRegNo()
		{
			var bill = Factory.New<AsycudaBill>();
			bill.ABL_ConsigneeRegNo = "";
			bill.ABL_ConsigneeRegNoType = "UK";
			bill.ABL_ConsigneeStreet1 = "s1";
			bill.ABL_ConsigneeStreet2 = "s2";
			bill.ABL_ConsigneeCity = "City";
			bill.ABL_RN_NKConsigneeCountry = "AU";
			bill.ABL_ConsigneePostcode = "123";
			bill.ABL_ConsigneeName = "name";

			var wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			var importer = wrapper.Importer;
			CombineAssertions("Empty Registration Number", () =>
			{
				AssertEquals("Should leave ID empty", ZString.Empty, importer.ID);
				AssertEquals("Should fill out name", bill.ABL_ConsigneeName, importer.Name);
				AssertEquals("Should fill out street", bill.ABL_ConsigneeStreet1 + " " + bill.ABL_ConsigneeStreet2, importer.Address.Line);
				AssertEquals("Should fill out city", bill.ABL_ConsigneeCity, importer.Address.CityName);
				AssertEquals("Should fill out country", bill.ABL_RN_NKConsigneeCountry, importer.Address.CountryCode);
				AssertEquals("Should fill out postcode", bill.ABL_ConsigneePostcode, importer.Address.PostcodeID);
			});

			bill.ABL_ConsigneeRegNoType = "AU";
			bill.ABL_ConsigneeRegNo = "001";

			wrapper = new GbCDSH7ImportGoodsShipmentWrapper(bill, isBIRDSMessage: false);
			importer = wrapper.Importer;
			CombineAssertions("Non British Country Code", () =>
			{
				AssertEquals("Should leave ID empty", ZString.Empty, importer.ID);
				AssertEquals("Should fill out name", bill.ABL_ConsigneeName, importer.Name);
				AssertEquals("Should fill out street", bill.ABL_ConsigneeStreet1 + " " + bill.ABL_ConsigneeStreet2, importer.Address.Line);
				AssertEquals("Should fill out city", bill.ABL_ConsigneeCity, importer.Address.CityName);
				AssertEquals("Should fill out country", bill.ABL_RN_NKConsigneeCountry, importer.Address.CountryCode);
				AssertEquals("Should fill out postcode", bill.ABL_ConsigneePostcode, importer.Address.PostcodeID);
			});
		}
	}
}
