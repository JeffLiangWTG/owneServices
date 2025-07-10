using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
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

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertHasMessageError(header.AMA_NatureInfo, "The code you have selected is not in the list.");

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_Nature = ShipmentTypeList.Codes.Export22;
			AssertNoNotifications(header.AMA_NatureInfo);

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			AssertNoNotifications(header.AMA_NatureInfo);
		}

		public void TestCheckAMA_CustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusof = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var baires = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Argentina, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "001", "BUENOS AIRES", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_CustomsOffice = ZString.Empty;

			AssertHasMessageErrorContaining(header.AMA_CustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_CustomsOffice = "1X";
			AssertHasMessageErrorContaining(header.AMA_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);

			header.AMA_CustomsOffice = "001";
			AssertNoNotifications(header.AMA_CustomsOfficeInfo);
		}

		public void TestCheckRegistrationDate()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.RegistrationDate = ZDateTime.Empty;
			AssertNoNotifications(header.RegistrationDateInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.RegistrationDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(header.RegistrationDateInfo, MandatoryValidation.YouHaveNotEntered);

			header.RegistrationDate = ZDateTime.Now;
			AssertNoNotifications(header.RegistrationDateInfo);
		}

		[TestDate(2021, 12, 31)]
		public void TestCheckRegistrationNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_SystemCreateTimeUtc = ZDate.Today;

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.RegistrationNumber = ZString.Empty;
			AssertNoMessageErrors(header.RegistrationNumberInfo);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.RegistrationNumber = ZString.Empty;
			AssertHasMessageErrorContaining(header.RegistrationNumberInfo, MandatoryValidation.YouHaveNotEntered);

			header.RegistrationNumber = "202108000000001";
			AssertHasMessageErrorContaining(header.RegistrationNumberInfo, "The length should be 16");

			header.RegistrationNumber = "2020080000000010";
			AssertHasMessageErrorContaining(header.RegistrationNumberInfo, "Incorrect ID format, the correct Format is YYYYVVNNNNNNNNND (YYYY- Year, VV - 08, N - Sequential Number, D - Verification digit");

			header.RegistrationNumber = "2021070000000010";
			AssertHasMessageErrorContaining(header.RegistrationNumberInfo, "Incorrect ID format, the correct Format is YYYYVVNNNNNNNNND (YYYY- Year, VV - 08, N - Sequential Number, D - Verification digit");

			header.RegistrationNumber = "2021080000000010";
			AssertNoMessageErrors(header.RegistrationNumberInfo);
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
			var carrierOrgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_OA_Carrier = ZGuid.Empty;
			AssertHasWarningContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);

			header.AMA_OA_Carrier = carrierOrgAddress.PK;
			AssertNoWarningContaining(header.AMA_OA_CarrierInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
