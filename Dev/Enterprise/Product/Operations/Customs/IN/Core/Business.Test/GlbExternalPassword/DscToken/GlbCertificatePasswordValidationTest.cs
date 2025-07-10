using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(GlbCertificatePasswordValidation))]
sealed class GlbCertificatePasswordValidationTest : GlbExternalPasswordValidationTest<GlbCertificatePassword, GlbCertificatePasswordValidation>
{
	public void TestCheckGP_CertificateAuthority_ListValidation()
	{
		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		ValidationTestHelper.AssertErrorIfInvalidCode(GlbExternalPassword.GP_CertificateAuthorityInfo, "XXX", "eMudhra", "Enter a valid Certificate Authority.");
	}

	public void TestCheckGP_CertificateAuthority_MandatoryValidation()
	{
		var glbExternalPassword = GlbExternalPassword;
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(glbExternalPassword.GP_CertificateAuthorityInfo, glbExternalPassword.GP_NameInfo, expectedMessage: "You have not entered a Certificate Authority.");
	}

	public void TestCheckGP_Name_ListValidation()
	{
		RefDataSetupTestHelper.SetupCertificateTokenData(Factory);
		ValidationTestHelper.AssertErrorIfInvalidCode(GlbExternalPassword.GP_NameInfo, "XXX", "WatchData", "Enter a valid Chipset Manufacturer.");
	}

	public void TestCheckGP_Name_MandatoryValidation()
	{
		var glbExternalPassword = GlbExternalPassword;
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(glbExternalPassword.GP_NameInfo, glbExternalPassword.GP_CertificateAuthorityInfo, expectedMessage: "You have not entered a Chipset Manufacturer.");
	}

	public void TestCheckGP_CertificateSerialNumber_MandatoryValidation()
	{
		var glbExternalPassword = GlbExternalPassword;
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(glbExternalPassword.GP_CertificateSerialNumberInfo, glbExternalPassword.GP_CertificateAuthorityInfo, expectedMessage: "You have not entered a Certificate SN.");
		ValidationTestHelper.AssertMessageErrorIfNotEnteredWhenOtherPropertyIsEntered(glbExternalPassword.GP_CertificateSerialNumberInfo, glbExternalPassword.GP_NameInfo, expectedMessage: "You have not entered a Certificate SN.");
	}
}
