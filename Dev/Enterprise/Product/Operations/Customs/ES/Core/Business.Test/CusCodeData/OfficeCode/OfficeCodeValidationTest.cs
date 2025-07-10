using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	class OfficeCodeValidationTest : CusCodeDataValidationTest
	{
		public void TestValidateESCustomForUCC6ExportPRE()
		{
			var dec = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(dec, "IsUCC6Core", true))
			{
				SetUpCustomsOffice();
				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				var officeCode = dec.CustomsOffices.AddNew();
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				officeCode.CY_Data = "IEDUB100";
				var officeOfValidation = officeCode.Validation;

				var expectedWarning = "Customs Office of Presentation should only be used in CCE (Centralized Clearance in Europe) and cannot be a Spanish office (should not start with ES)";

				CombineAssertions(() =>
				{
					AssertNoWarning("No warning expected if custom is not Spanish", officeCode.CY_DataInfo, expectedWarning);

					officeCode.CY_Data = "ES009999";
					AssertHasWarning("Warning expected if custom is Spanish", officeCode.CY_DataInfo, expectedWarning);

					officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
					officeOfValidation.ValidateCY_Data();
					AssertNoWarning("No warning expected if CY_Code is not PRE", officeCode.CY_DataInfo, expectedWarning);
				});
			}
		}

		public void TestValidateESCustomForUCC6ExportPRENoRolInOffice()
		{
			SetUpCustomsOffice();
			var dec = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(dec, "IsUCC6Core", true))
			{
				dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
				var officeCode = dec.CustomsOffices.AddNew();
				officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				officeCode.CY_Data = "ES009999";
				var officeOfValidation = officeCode.Validation;

				var expectedError = "According to reference data, this office does not fulfill this role";

				CombineAssertions(() =>
				{
					AssertNoError("No base error expected if if CY_Code is not PRE and Custom is for EXP", officeCode.CY_DataInfo, expectedError);

					officeCode.CY_Data = "IEDUB100";
					officeOfValidation.ValidateCY_Data();
					AssertHasMessageError("For PRE expected error cause Custom is for EXT not for EXP", officeCode.CY_DataInfo, expectedError);

					officeCode.CY_Data = "ES009999";
					officeCode.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExport;
					officeOfValidation.ValidateCY_Data();
					AssertNoError("Base validation expected if CY_Code is not PRE", officeCode.CY_DataInfo, expectedError);
				});
			}
		}

		void SetUpCustomsOffice()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eunZZZ);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			var irelandCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IEDUB100", "DUBLIN PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var spanishCustom = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ES009999", "TEST", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(irelandCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateNewOrGetExistingCusCodeListAttribute(spanishCustom.PK, RefCusCodeListAttributeTypes.Codes.ROLE, "EXP");
			Factory.Save();
		}
	}
}
