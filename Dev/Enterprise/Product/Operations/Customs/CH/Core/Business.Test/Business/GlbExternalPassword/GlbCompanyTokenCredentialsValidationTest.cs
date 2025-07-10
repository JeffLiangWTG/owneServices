using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbCompanyTokenCredentialsValidation))]
class GlbCompanyTokenCredentialsValidationTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbCompanyTokenCredentials, GlbCompanyTokenCredentialsValidation>
{
	public void TestCheckGP_CertificateText()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(GlbExternalPassword.GP_CertificateTextInfo);
	}

	public void TestCheckGP_UserID()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(GlbExternalPassword.GP_UserIDInfo);
	}

	public void TestCheckCurrentDecryptedPassword()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(GlbExternalPassword.CurrentDecryptedPasswordInfo);
	}
}
