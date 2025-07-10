using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(GlbExternalPasswordValidation_CHD))]
class GlbExternalPasswordValidation_CHDTest : MasterFiles.Business.Testing.GlbExternalPasswordValidationTest<GlbExternalPassword_CHD, GlbExternalPasswordValidation_CHD>
{
	public void TestCheckGP_UserID()
	{
		var numberOnlyMsg = "Declarant number must be digits only.";

		var gpUserIDInfo = GlbExternalPassword.GP_UserIDInfo;

		AssertNoNotifications(gpUserIDInfo);

		GlbExternalPassword.GP_UserID = "123A";
		AssertHasError(gpUserIDInfo, numberOnlyMsg);

		GlbExternalPassword.GP_UserID = "1234";
		AssertNoError(gpUserIDInfo, numberOnlyMsg);
	}
}
