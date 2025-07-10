using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(AutomaticSignatureExternalPasswordValidation))]
sealed class AutomaticSignatureExternalPasswordValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<AutomaticSignatureExternalPassword, AutomaticSignatureExternalPasswordValidation>
{
	public void TestCheckGP_MailBoxID_ListValidation()
	{
		var password = GlbExternalPassword;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(password.GP_MailBoxIDInfo);
	}

	public void TestCheckGP_MailBoxID_ListValidation_Enabled()
	{
		AutomaticSignatureExternalPasswordLookupsTest.SetUpRefSysConfigs(Factory);

		var password = GlbExternalPassword;
		password.IsConfigurationActive = true;

		ValidationTestHelper.AssertErrorIfNotEntered(password.GP_MailBoxIDInfo);
		ValidationTestHelper.AssertErrorIfInvalidCode(password.GP_MailBoxIDInfo, "XX", "ITARDSDL01");
	}

	public void TestCheckGP_UserID_MandatoryValidation()
	{
		var password = GlbExternalPassword;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(password.GP_UserIDInfo);
	}

	public void TestCheckGP_UserID_MandatoryValidation_Enabled()
	{
		var password = GlbExternalPassword;
		password.IsConfigurationActive = true;

		ValidationTestHelper.AssertErrorIfNotEntered(password.GP_UserIDInfo);
	}

	public void TestCheckGP_Name()
	{
		var password = GlbExternalPassword;
		ValidationTestHelper.AssertErrorFieldIsNotMandatory(password.GP_NameInfo);
	}

	public void TestCheckGP_Name_Enabled_MandatoryValidation()
	{
		var password = GlbExternalPassword;
		password.IsConfigurationActive = true;

		ValidationTestHelper.AssertErrorIfNotEntered(password.GP_NameInfo);
	}

	public void TestCheckGP_Name_Enabled_Pattern()
	{
		const string message = "The User Fiscal Code pattern is invalid. Valid patterns are: AAAAAAnnAnnAnnnA.";

		var password = GlbExternalPassword;
		password.IsConfigurationActive = true;

		password.GP_Name = "AAAAAAbbAccAdddA";
		AssertHasMessageError(password.GP_NameInfo, message);

		password.GP_Name = "AAAAAA11A22A333A";
		AssertNoMessageError(password.GP_NameInfo, message);
	}
}
