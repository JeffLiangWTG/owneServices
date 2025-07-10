using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	sealed class IcsOfficeCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom, "United Kingdom");
			_ = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "GBAR1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeaderSS>();
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "GBAR1";
			unloco.RL_IATA = "XX1";

			var office1 = header.EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfFirstEntry);
			office1.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(office1.CY_DataInfo, "A valid GB office code is needed. Example: GB000010.");

			office1.CY_Data = $"{Core.Constants.CountryCodes.UnitedKingdom}123";
			office1.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(office1.CY_DataInfo, "Entered office code is not a valid office");

			office1.CY_Data = unloco.RL_Code;
			office1.Validation.ValidateCY_Data();
			AssertHasMessageErrorContaining(office1.CY_DataInfo, "According to reference data, this office does not fulfill this role");

			var unloco1 = Factory.New<RefUNLOCO>();
			unloco1.RL_Code = "GBAR2";
			unloco1.RL_IATA = "XX2";

			_ = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.ROLE, "Role", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, Core.Constants.CountryCodes.UnitedKingdom);
			_ = helper.CreateNewOrGetExistingCusCodeList(unloco1.RL_Code, Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ["ENT"]);
			Factory.Save();

			office1.CY_Data = unloco1.RL_Code;
			office1.Validation.ValidateCY_Data();
			AssertNoNotifications(office1.CY_DataInfo);
		}

		public void TestCheck_CYDate()
		{
			const string errorMessage = "The scheduled date and time of arrival of the means of transport at the Office of First Entry is required.";

			var header = Factory.New<AsycudaManifestHeaderSS>();
			var office1 = header.EUCustomsOffices.AddNew(OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry);
			header.Validation.ValidateAll();
			AssertNoMessageError(office1.CY_DateInfo, errorMessage);

			office1.CY_Code = OfficeCodes_ICS.Codes.OfficeOfFirstEntry;
			office1.Validation.ValidateAll();
			AssertHasMessageError(office1.CY_DateInfo, errorMessage);

			office1.CY_Date = ZDateTime.Now;
			AssertNoMessageError(office1.CY_DateInfo, errorMessage);
		}
	}
}
