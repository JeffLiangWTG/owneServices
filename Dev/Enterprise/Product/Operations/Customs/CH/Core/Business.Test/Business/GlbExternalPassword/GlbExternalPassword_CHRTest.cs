using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbExternalPassword_CHR))]
class GlbExternalPassword_CHRTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbExternalPassword_CHR>
{
	public void TestValidationType()
	{
		var externalPassword = Factory.New<GlbExternalPassword_CHR>();
		AssertType<GlbExternalPasswordValidation_CHR>(externalPassword.Validation);
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
		AssertEquals(PasswordTypesList.Codes.CHR, GlbExternalPassword.GP_PasswordType);
	}
}
