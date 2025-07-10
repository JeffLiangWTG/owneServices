using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CustomsRegNo_AEO()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "CHSHA";
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.AEO;
			var message = "BR AEO should be 14 digits";

			cusCode.OK_CustomsRegNo = "";
			AssertHasErrorContaining("empty", cusCode.OK_CustomsRegNoInfo, MandatoryValidation.MustBeEntered);

			cusCode.OK_CustomsRegNo = "1234567890123456";
			AssertHasWarning("16 chars", cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertHasWarning("10 chars", cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "1234567890ABCD";
			AssertHasWarning("14 chars but not all digits", cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345678901234";
			AssertNoWarning("14 digits", cusCode.OK_CustomsRegNoInfo, message);
		}

		public void TestCheckOK_CustomsRegNo_FOI()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "CHSHA";
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode;
			var message = "BR FOI should not have more than 35 characters.";

			cusCode.OK_CustomsRegNo = "";
			AssertHasErrorContaining("empty", cusCode.OK_CustomsRegNoInfo, MandatoryValidation.MustBeEntered);

			cusCode.OK_CustomsRegNo = "1234567890123456";
			AssertNoError("16 chars", cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "12345678901234567890123456789012345";
			AssertNoError("35 chars", cusCode.OK_CustomsRegNoInfo, message);

			cusCode.OK_CustomsRegNo = "123456789012345678901234567890123456";
			AssertHasError("36 chars", cusCode.OK_CustomsRegNoInfo, message);
		}

		public void TestCheckOK_CodeType_FOI()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.OH_RL_NKClosestPort = "CHSHA";
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode;

			AssertNoError("One OrgCusCode", cusCode.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");
			AssertNoError("When Main Adress is CN", cusCode.OK_CodeTypeInfo, "BR FOI code not necessary for Brazilian companies.");

			var cusCode2 = organisation.CustomsCodes.AddNew();
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Brazil;
			cusCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.ForeignOperatorInternalCode;
			AssertHasError("Two OrgCusCode", cusCode2.OK_CodeTypeInfo, "There can only be one code of this type for each country/region.");

			cusCode2.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.OccupationCodesOfIndividuals;
			organisation.OH_RL_NKClosestPort = "BRSAO";
			cusCode.Validation.ValidateOK_CodeType();
			AssertHasError("When Main Adress is BR", cusCode.OK_CodeTypeInfo, "BR FOI code not necessary for Brazilian companies.");
		}

		public void TestCheckOK_CodeType_RTC()
		{
			var organisation = Factory.New<OrgHeader>();
			organisation.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			var cusCode = organisation.CustomsCodes.AddNew();
			cusCode.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ;
			AssertHasError("CNPJ is empty", cusCode.OK_CodeTypeInfo, "Root CNPJ cannot be set if no CNPJ has been informed.");

			var cusCodeCNPJ = organisation.CustomsCodes.AddNew();
			cusCodeCNPJ.OK_CodeType = BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ;
			cusCodeCNPJ.OK_CustomsRegNo = "25043511000111";

			cusCode.Validation.ValidateOK_CodeType();
			AssertNoError("CNPJ is not empty", cusCode.OK_CodeTypeInfo, "Root CNPJ cannot be set if no CNPJ has been informed.");
		}
	}
}
