using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Nature()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ZString.Empty;

			AssertHasMessageError(header.AMA_NatureInfo, "You have not entered a Manifest Nature.");

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertNoNotifications(header.AMA_NatureInfo);
		}

		public void TestCheckAMA_LloydNumber()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_LloydsNumber = ZString.Empty;

			AssertHasWarningContaining(header.AMA_LloydsNumberInfo, "You have not entered a Vessel IMO Number.");

			header.AMA_LloydsNumber = "Lloyd";
			AssertNoWarningContaining(header.AMA_LloydsNumberInfo, "You have not entered a Vessel IMO Number.");
		}

		public void TestCheckAMA_NK_Vessel()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_LloydsNumber = "Lloyd";
			header.AMA_VesselName = ZString.Empty;

			AssertHasMessageErrorContaining(header.AMA_VesselNameInfo, "You have not entered a Vessel.");

			header.AMA_VesselName = "Vessel";
			AssertNoMessageErrorContaining(header.AMA_LloydsNumberInfo, "You have not entered a Vessel.");
		}

		public void TestCheckLastForeignPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Mexico, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "48938", "Adana, Mersin", new ZDateTime(2020, 1, 1), new ZDateTime(2079, 6, 6));

			Factory.Save();

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.LastForeignPort = ZString.Empty;

			AssertNoNotifications(header.LastForeignPortInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			header.LastForeignPort = ZString.Empty;

			AssertNoNotifications(header.LastForeignPortInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.LastForeignPort = ZString.Empty;

			AssertHasMessageError(header.LastForeignPortInfo, "You have not entered a Last Foreign Port.");

			header.LastForeignPort = "12345";

			AssertHasMessageError(header.LastForeignPortInfo, "The code you have selected is not in the list.");

			header.AMA_RL_NKPortOfLoading = "MXACA";
			header.LastForeignPort = "48938";

			AssertNoNotifications(header.LastForeignPortInfo);
		}

		public void TestCheckAMA_OA_ShippingAgent()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_OA_ShippingAgent = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_OA_ShippingAgent = orgHeader.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_OA_ShippingAgent = ZGuid.Empty;
			AssertNoNotifications(header.AMA_OA_ShippingAgentInfo);

			header.AMA_OA_ShippingAgent = orgHeader.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_ShippingAgentInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;

			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, "The selected Carrier must have the CAAT entered.");

			header.AMA_OA_Carrier = orgAddress1.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, "The selected Carrier must have the CAAT entered.");

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";

			var orgAddress2 = org.MainAddress;
			orgAddress2.Address1 = "Address";

			var orgCusCode = orgAddress2.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "9992";
			orgCusCode.OK_RN_NKCodeCountry = "MX";

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, "The selected Carrier must have the CAAT entered.");

			header.AMA_OA_Carrier = orgAddress1.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, "The selected Carrier must have the CAAT entered.");

			header.AMA_OA_Carrier = orgAddress2.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, "The selected Carrier must have the CAAT entered.");
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Mexico);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList("MX", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSTT", "CustomsOffice Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CustomsOffice = ZString.Empty;

			AssertHasMessageError(header.AMA_CustomsOfficeInfo, "You have not entered a Customs Office.");

			header.AMA_CustomsOffice = "TTTTT";
			AssertHasMessageError(header.AMA_CustomsOfficeInfo, "The code you have selected is not in the list.");

			header.AMA_CustomsOffice = "CUSTT";
			AssertNoNotifications(header.AMA_CustomsOfficeInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_CustomsOffice = ZString.Empty;

			AssertNoNotifications(header.AMA_CustomsOfficeInfo);
		}
	}
}
