using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class GlbExternalPasswordHelperTest : TestCaseWithFactory
	{
		public void TestAnyStaffHasCCTCertificate()
		{
			var newStaff = Factory.New<GlbStaff>();
			newStaff.GS_Code = "AB2";
			newStaff.GS_RN_NKCountryCode = CountryCodes.Austria;
			newStaff.GS_LoginName = "Test";
			var wrapper = BRGlbStaffWrapper.Get(newStaff);
			var password = wrapper.CCTPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			Factory.Save();

			AssertEquals("There is no Certificate configured in Brazil.", GlbExternalPasswordHelper.CheckAnyStaffHasCCTCertificate());

			SetupGlbExternalPassword_CCT(Factory);
			CertificateRequirementChecker.ResetForTesting();
			AssertEquals(string.Empty, GlbExternalPasswordHelper.CheckAnyStaffHasCCTCertificate());
		}

		public static void SetupGlbExternalPassword_CCT(BusinessObjectFactory factory)
		{
			var newStaff = factory.New<GlbStaff>();
			newStaff.GS_Code = "ABC";
			newStaff.GS_RN_NKCountryCode = CountryCodes.Brazil;
			newStaff.GS_LoginName = "Test2";
			var wrapper = BRGlbStaffWrapper.Get(newStaff);
			var password = wrapper.CCTPassword;
			password.GP_PasswordType = PasswordTypesList.Codes.CCT;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.GP_PasswordStatus = PasswordStatusList.Codes.Valid;

			factory.Save();
		}
	}
}
