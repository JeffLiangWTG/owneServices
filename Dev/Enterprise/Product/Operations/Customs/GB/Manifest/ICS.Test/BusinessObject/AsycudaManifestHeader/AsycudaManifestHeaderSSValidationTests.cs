using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	public class AsycudaManifestHeaderSSValidationTest : BusinessObjectValidationTestCase
	{
		public virtual void TestCheckAMA_LloydsNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			header.Validation.ValidateAMA_LloydsNumber();
			AssertNoMessageErrorContaining(header.AMA_LloydsNumberInfo, "Vessel IMO Number");

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.Validation.ValidateAMA_LloydsNumber();
			AssertHasMessageErrorContaining(header.AMA_LloydsNumberInfo, "Vessel IMO Number");
			header.AMA_LloydsNumber = "LL111";
			header.Validation.ValidateAMA_LloydsNumber();
			AssertNoMessageErrorContaining(header.AMA_LloydsNumberInfo, "Vessel IMO Number");
		}

		public void TestAtLeastTwoItineraryRecordsValidation()
		{
			const string errorMessageWithinGrid = "At least two itinerary rows are required to show the routing of these goods from country of original departure to final destination.";
			const string errorMessage = "At least two itinerary rows are required to show the routing of these goods from country of original departure to final destination. Please add the required data on the Itinerary tab.";

			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_RL_NKPortOfLoading = ZString.Empty;
			header.AMA_RL_NKPortOfFirstArrival = ZString.Empty;
			header.AMA_RL_NKPortOfDischarge = ZString.Empty;
			header.Validation.ValidateAll();
			AssertHasMessageError("With no itinerary records, message error on HasAtLeastTwoItineraryRows", header.HasAtLeastTwoItineraryRowsInfo, errorMessage);

			var itinerary1 = header.Itinerary.AddNew(CusCodeDataTypeList.Codes.IcsRouteEntry);
			header.Validation.ValidateAll();
			itinerary1.Validation.ValidateAll();
			AssertNoMessageError("With one itinerary record, message error on HasAtLeastTwoItineraryRows", header.HasAtLeastTwoItineraryRowsInfo, errorMessage);
			AssertHasRowMessageError("With one itinerary record, message error on CY_Code", itinerary1, errorMessageWithinGrid);

			var itinerary2 = header.Itinerary.AddNew(CusCodeDataTypeList.Codes.IcsRouteEntry);
			header.Validation.ValidateAll();
			itinerary1.Validation.ValidateAll();
			itinerary2.Validation.ValidateAll();
			AssertNoRowMessageError("With two itinerary records", itinerary1, errorMessageWithinGrid);
			AssertNoRowMessageError("With two itinerary records", itinerary2, errorMessageWithinGrid);
		}

		public void TestCheckAMA_RN_NKConveyanceNationality()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
			header.Validation.ValidateAll();

			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.SeaFreight;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RN_NKConveyanceNationality = Core.Constants.CountryCodes.UnitedKingdom;
			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_RN_NKConveyanceNationality = ZString.Empty;
			AssertHasMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
			AssertNoMessageErrorContaining(header.AMA_RN_NKConveyanceNationalityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_OA_Carrier = ZGuid.Empty;
			header.Validation.ValidateAll();
			AssertNoWarnings(header.AMA_OA_CarrierInfo);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.Addresses.AddNew();
			header.AMA_OA_Carrier = address.PK;
			header.Validation.ValidateAll();
			AssertNoWarnings(header.AMA_OA_CarrierInfo);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_CustomsOffice = ZString.Empty;
			header.Validation.ValidateAll();
			AssertNoMessageErrors(header.AMA_CustomsOfficeInfo);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GB123456", "Test GB Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var euCustomsOffice = header.EUCustomsOffices.AddNew();
			euCustomsOffice.CY_Code = "OOA";
			euCustomsOffice.CY_Data = "GB123456";
			header.AMA_CustomsOffice = "GB123456";
			header.Validation.ValidateAll();
			AssertNoMessageErrors(header.AMA_CustomsOfficeInfo);

			euCustomsOffice.CY_Code = "OOF";
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(header.AMA_CustomsOfficeInfo, "The Customs Office of Lodgement should be present if different from the Office of First Entry (OOF) otherwise it should not be provided.");
		}

		public void TestCheckAMA_Voyage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			header.AMA_Voyage = ZString.Empty;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, "Please enter the Voyage Identification. This is a required field when Transport Mode = RAI.");

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.RailFreight;
			header.Validation.ValidateAll();
			AssertHasMessageErrorContaining(header.AMA_VoyageInfo, "Please enter the Voyage Identification. This is a required field when Transport Mode = RAI.");

			header.AMA_TransportMode = GBSSTransportTypeList.Codes.AirFreight;
			header.Validation.ValidateAll();
			AssertNoMessageErrorContaining(header.AMA_VoyageInfo, "Please enter the Voyage Identification. This is a required field when Transport Mode = RAI.");
		}
	}
}
