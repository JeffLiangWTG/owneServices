using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbExternalPassword_CHD))]
class GlbExternalPassword_CHDTest : MasterFiles.Business.Testing.GlbExternalPasswordTest<GlbExternalPassword_CHD>
{
	public void TestCSI_Code()
	{
		CombineAssertions(() =>
		{
			var externalPassword = Factory.New<GlbExternalPassword_CHD>();
			AssertEquals("MaxLength", 6, externalPassword.GP_UserIDInfo.MaxLength);
		});
	}

	public void TestLookupsAndValidationType()
	{
		var externalPassword = Factory.New<GlbExternalPassword_CHD>();
		Assert(externalPassword.Validation.GetType().FullName.Contains("GlbExternalPasswordValidation_CH"));
	}

	public void TestSetDefaultValues()
	{
		AssertEquals(GlbCompany.CurrentCompany.PK, GlbExternalPassword.GP_GC);
		AssertEquals(PasswordTypesList.Codes.CHD, GlbExternalPassword.GP_PasswordType);
	}
}
