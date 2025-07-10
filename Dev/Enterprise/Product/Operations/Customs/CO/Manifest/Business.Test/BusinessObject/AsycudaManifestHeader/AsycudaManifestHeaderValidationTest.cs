using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAMA_Nature()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ZString.Empty;

			AssertHasMessageError(header.AMA_NatureInfo, "You have not entered a Manifest Nature.");

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertNoNotifications(header.AMA_NatureInfo);
		}

		public void TestCheckCargoDisposition()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.CargoDispositionInfo, "30", "10");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.CargoDisposition = ZString.Empty;
			AssertNoNotifications(header.CargoDispositionInfo);
		}

		public void TestCheckTravelDocumentType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.TravelDocumentTypeInfo, "5", "0");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.TravelDocumentType = ZString.Empty;
			AssertNoNotifications(header.TravelDocumentTypeInfo);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Colombia);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateCusCodeList("CO", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CUSTT", "CustomsOffice Test", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CustomsOffice = ZString.Empty;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.AMA_CustomsOfficeInfo, "TTTTT", "CUSTT");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_CustomsOffice = ZString.Empty;
			AssertNoNotifications(header.AMA_CustomsOfficeInfo);
		}

		public void TestCheckAMA_ContainerMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_ContainerMode = ZString.Empty;
			AssertHasMessageError(header.AMA_ContainerModeInfo, "You have not entered a Container Mode.");

			header.AMA_ContainerMode = Core.Constants.ContainerModes.Bulk;
			AssertNoMessageErrors(header.AMA_ContainerModeInfo);
		}

		public void TestCheckAMA_OA_Carrier()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var org = Factory.New<OrgHeader>();
			header.AMA_OA_Carrier = ZGuid.Empty;

			AssertHasMessageError(header.AMA_OA_CarrierInfo, "You have not entered a Carrier.");

			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;
			header.AMA_OA_Carrier = orgAddress.PK;

			AssertHasMessageError(header.AMA_OA_CarrierInfo, "The selected Carrier should have NIT");

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			orgCusCode.OK_CustomsRegNo = "12345";

			header.AMA_OA_Carrier = orgAddress.PK;
			AssertNoNotifications(header.AMA_OA_CarrierInfo);
		}

		public void TestCheckAMA_OA_ShippingAgent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var org = Factory.New<OrgHeader>();

			header.AMA_OA_ShippingAgent = ZGuid.Empty;

			AssertHasMessageError(header.AMA_OA_ShippingAgentInfo, "You have not entered a Shipping Agent.");

			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;
			header.AMA_OA_ShippingAgent = orgAddress.PK;

			AssertHasMessageError(header.AMA_OA_ShippingAgentInfo, "The selected Shipping Agent should have NIT");

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = ColombiaOrgCusCodeInfo.OrgCusCodes.NIT;
			orgCusCode.OK_CustomsRegNo = "12345";

			header.AMA_OA_ShippingAgent = orgAddress.PK;
			AssertNoNotifications(header.AMA_OA_ShippingAgentInfo);
		}

		public void TestCheckDeliveryMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.DeliveryModeInfo, "5", "1");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.DeliveryMode = ZString.Empty;
			AssertNoNotifications(header.DeliveryModeInfo);
		}
	}
}
