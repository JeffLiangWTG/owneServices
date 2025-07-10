using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(OrgCusCodeValidation))]
sealed class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOK_CustomsRegNo_BSN()
	{
		var expectedError = "Invalid BSN - Branch Serial Number. Expected code value is any number between 1 to 999.";

		CombineAssertions(() =>
		{
			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UDY;
			Code.OK_CustomsRegNo = "1000";
			AssertNoErrors("When OK_CodeType is UDY", Code.OK_CustomsRegNoInfo);

			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.BSN;
			Code.Validation.ValidateOK_CustomsRegNo();
			AssertHasError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '1000'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "999";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '999'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "A21";
			AssertHasError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is not Numeric 'A21'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "1";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '1'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "01";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '01'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "001";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '001'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "000";
			AssertHasError("When OK_RN_NKCodeCountry is India, OK_CodeType is BSN and OK_CustomsRegNo is Numeric '000'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			AssertNoError("When OK_RN_NKCodeCountry is China", Code.OK_CustomsRegNoInfo, expectedError);
		});
	}

	public void TestCheckOK_CustomsRegNo_IEC()
	{
		const string message = "Invalid IEC. The expected code should be exactly 10 Character long.";

		var orgHeader = Factory.New<OrgHeader>();
		var customsCode = orgHeader.CustomsCodes.AddNew();
		customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
		customsCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.IEC;

		CombineAssertions(() =>
		{
			customsCode.OK_CustomsRegNo = "1234567890";
			AssertNoErrorContaining("10 digits", customsCode.OK_CustomsRegNoInfo, message);

			customsCode.OK_CustomsRegNo = "12345";
			AssertHasErrorContaining("Not 10 digits", customsCode.OK_CustomsRegNoInfo, message);

			customsCode.OK_CustomsRegNo = "";
			AssertNoErrorContaining("Empty", customsCode.OK_CustomsRegNoInfo, message);

			customsCode.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UIN;
			customsCode.OK_CustomsRegNo = "1234567890";
			AssertNoErrorContaining("Not IEC", customsCode.OK_CustomsRegNoInfo, message);
		});
	}

	public void TestCheckOK_CustomsRegNo_ADC()
	{
		var expectedError = "Invalid Authorized Dealer Code. The expected code cannot be greater than 10 Char.";

		CombineAssertions(() =>
		{
			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UDY;
			Code.OK_CustomsRegNo = "0123456789A";
			AssertNoErrors("When OK_CodeType is UDY", Code.OK_CustomsRegNoInfo);

			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.ADC;
			Code.Validation.ValidateOK_CustomsRegNo();
			AssertHasError("When OK_RN_NKCodeCountry is India, OK_CodeType is ADC and OK_CustomsRegNo is 11 char long", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "0123456789";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is ADC and OK_CustomsRegNo is 10 char long'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			Code.OK_CustomsRegNo = "0123456789A";
			AssertNoError("When OK_RN_NKCodeCountry is China", Code.OK_CustomsRegNoInfo, expectedError);
		});
	}

	public void TestCheckOK_CustomsRegNo_AEO()
	{
		var expectedError = "Invalid AEO - Authorized Economic Operator. The expected code cannot be greater than 17 Char.";

		CombineAssertions(() =>
		{
			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.India;
			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.UDY;
			Code.OK_CustomsRegNo = "01234567890123456A";
			AssertNoErrors("When OK_CodeType is UDY", Code.OK_CustomsRegNoInfo);

			Code.OK_CodeType = IndiaOrgCusCodeInfo.OrgCusCodes.AEO;
			Code.Validation.ValidateOK_CustomsRegNo();
			AssertHasError("When OK_RN_NKCodeCountry is India, OK_CodeType is AEO and OK_CustomsRegNo is 18 char long", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_CustomsRegNo = "01234567890123456";
			AssertNoError("When OK_RN_NKCodeCountry is India, OK_CodeType is ADC and OK_CustomsRegNo is 17 char long'", Code.OK_CustomsRegNoInfo, expectedError);

			Code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			Code.OK_CustomsRegNo = "01234567890123456A";
			AssertNoError("When OK_RN_NKCodeCountry is China", Code.OK_CustomsRegNoInfo, expectedError);
		});
	}

	OrgCusCode Code => code ??= Factory.New<OrgHeader>().CustomsCodes.AddNew();
	OrgCusCode code;
}
