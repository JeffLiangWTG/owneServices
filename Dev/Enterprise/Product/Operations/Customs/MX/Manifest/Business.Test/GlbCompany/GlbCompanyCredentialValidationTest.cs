using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredentialValidation))]
	public class GlbCompanyCredentialValidationTest : GlbExternalPasswordValidationTest<GlbCompanyCredential, GlbCompanyCredentialValidation>
	{
		public void TestCheckGP_UserID()
		{
			var credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Factory.New<GlbCompany>()).GlbExternalPassword;
			var messageError = "Username must be between 12 and 13 characters.";

			credential.GP_UserID = ZString.Empty;
			AssertHasMessageError(credential.GP_UserIDInfo, "You have not entered a value.");

			credential.GP_UserID = "12345678901";
			AssertHasMessageError(credential.GP_UserIDInfo, messageError);

			credential.GP_UserID = "1234567890123";
			AssertNoNotifications(credential.GP_UserIDInfo);
		}
	}
}
