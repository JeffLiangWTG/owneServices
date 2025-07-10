using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.CH.Business.Testing;

public class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOK_CustomsRegNo_AEO()
	{
		cusCode.OK_CodeType = EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;

		var redError = "Record only the seven-digit numeric part of the Swiss AEO number (nnnnnnn).";

		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "123456";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, redError);

			cusCode.OK_CustomsRegNo = "123456a";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, redError);

			cusCode.OK_CustomsRegNo = "12345678";
			AssertHasError(cusCode.OK_CustomsRegNoInfo, redError);

			cusCode.OK_CustomsRegNo = "1234567";
			AssertNoErrors(cusCode.OK_CustomsRegNoInfo);
		});
	}

	public void TestCheckOK_CustomsRegNo_UID()
	{
		cusCode.OK_CodeType = OrgCusCode.SwissCodeTypes.UID;
		CombineAssertions(() =>
		{
			AssertRegNoModulo11();
		});
	}

	public void TestCheckOK_CustomsRegNo_VAT()
	{
		cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
		CombineAssertions(() =>
		{
			AssertRegNoModulo11();
		});
	}

	void AssertRegNoModulo11()
	{
		var messageErrorFormat = $"{cusCode.OK_RN_NKCodeCountry} {cusCode.OK_CodeType} number should start with \"CHE\" or \"E\" followed by 9 numeric digits. (CHENNNNNNNNN, CHE-NNN.NNN.NNN, ENNNNNNNNN or E-NNN.NNN.NNN)";
		var messageErrorChecksum = $"{cusCode.OK_RN_NKCodeCountry} {cusCode.OK_CodeType} code is incorrect (checksum error). Please check input.";

		cusCode.OK_CustomsRegNo = "CHE375081047";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "CHE105908410";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "ABC375081047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CH3750821047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE375081a47";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE37581047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE3750810247";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE375081042";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorChecksum);

		cusCode.OK_CustomsRegNo = "CHE105908440";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorChecksum);

		cusCode.OK_CustomsRegNo = "CHE-375.081.047";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "CHE-105.908.410";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "CHE.105-908-410";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE-10.5.908.410";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "ABC-375.081.047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CH-375.821.047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE-375.081.a47";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "CHE-375.081.042";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorChecksum);

		cusCode.OK_CustomsRegNo = "E-375.081.047";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "E-105.908.410";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "E.105-908-410";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "E-10.5.908.410";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "C-375.081.047";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "E-375.081.a47";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorFormat);

		cusCode.OK_CustomsRegNo = "E-375.081.042";
		AssertHasMessageErrorContaining(cusCode.OK_CustomsRegNoInfo, messageErrorChecksum);
	}

	public void TestCheckOK_CustomsRegNo_VAT_LI()
	{
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Liechtenstein;
		cusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;

		var messageError = $"{cusCode.OK_RN_NKCodeCountry} {cusCode.OK_CodeType} number should consist of 5 numeric digits.";

		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "12345";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "1234";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "1x345";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "123456";
			AssertHasMessageError(cusCode.OK_CustomsRegNoInfo, messageError);
		});
	}

	public void TestCheckOK_CustomsRegNo_CAD()
	{
		cusCode.OK_CodeType = OrgCusCode.SwissCodeTypes.CAD;
		CombineAssertions(() =>
		{
			AssertAccountNumber();
		});
	}

	public void TestCheckOK_CustomsRegNo_CAV()
	{
		cusCode.OK_CodeType = OrgCusCode.SwissCodeTypes.CAV;
		CombineAssertions(() =>
		{
			AssertAccountNumber();
		});
	}

	void AssertAccountNumber()
	{
		var messageError = $"{cusCode.OK_RN_NKCodeCountry} {cusCode.OK_CodeType} number should consist of 5 to 8 numeric digits.";

		cusCode.OK_CustomsRegNo = "12345";
		AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

		cusCode.OK_CustomsRegNo = "1234";
		AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, messageError);

		cusCode.OK_CustomsRegNo = "1x3456";
		AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, messageError);

		cusCode.OK_CustomsRegNo = "123456789";
		AssertHasErrorContaining(cusCode.OK_CustomsRegNoInfo, messageError);
	}

	public void TestCheckOK_CustomsRegNo_CTP()
	{
		cusCode.OK_CodeType = OrgCusCode.SwissCodeTypes.CTP;

		var messageError = $"{cusCode.OK_RN_NKCodeCountry} {cusCode.OK_CodeType} numeric value should be between 1 and 999999.";

		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "123406";
			AssertNoNotifications("No error/warning", cusCode.OK_CustomsRegNoInfo);

			cusCode.OK_CustomsRegNo = "0";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "1x3456";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "1234567";
			AssertHasWarningContaining(cusCode.OK_CustomsRegNoInfo, messageError);
		});
	}

	public void TestCheckOK_CustomsRegNo_BID()
	{
		cusCode.OK_CodeType = OrgCusCode.SwissCodeTypes.BID;

		var messageError = "You have not entered a valid Business Partner ID.";

		CombineAssertions(() =>
		{
			cusCode.OK_CustomsRegNo = "123456789";
			AssertHasMessageError("ID to short", cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageError("Valid ID", cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "9876543210";
			AssertHasMessageError("ID not starting with '1'", cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "1234567890";
			AssertNoMessageError("Valid ID", cusCode.OK_CustomsRegNoInfo, messageError);

			cusCode.OK_CustomsRegNo = "123x56789A";
			AssertHasMessageError("ID containing characters", cusCode.OK_CustomsRegNoInfo, messageError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		org = Factory.NewWithValidTestData<OrgHeader>();
		cusCode = org.CustomsCodes.AddNew();
		cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Switzerland;
	}
	OrgCusCode cusCode;
	OrgHeader org;
}
