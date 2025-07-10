using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbExternalPasswordValidation_CHR))]
class GlbExternalPasswordValidation_CHRTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_CHR, GlbExternalPasswordValidation_CHR>
{
	public void TestCheckGP_CertificateText()
	{
		ValidationTestHelper.AssertErrorIfNotEntered(GlbExternalPassword.GP_CertificateTextInfo);
	}
}
