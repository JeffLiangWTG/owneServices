using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusExitControlHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEH_CustomsProfile_List()
		{
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CEH_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(exitHeader.CEH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				exitHeader.CEH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(exitHeader.CEH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCEH_CustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CEH_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", exitHeader.CEH_CustomsProfileInfo, notAuthorisedUserMessage);

				exitHeader.CEH_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", exitHeader.CEH_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.CEH_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", exitHeader.CEH_CustomsProfileInfo);

					exitHeader.CEH_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", exitHeader.CEH_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.CEH_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", exitHeader.CEH_CustomsProfileInfo);

					exitHeader.CEH_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", exitHeader.CEH_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckCEH_CustomsProfile_Mandatory()
		{
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CEH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(exitHeader.CEH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				exitHeader.CEH_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(exitHeader.CEH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		CusExitControlHeader SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
		{
			broker = Factory.New<GlbStaff>();
			broker.GS_Code = "AH";
			broker.GS_LoginName = "ahtest";
			broker.StaffPlainTextPassword = "security123";

			var wrapper = GlbStaffWrapper.Get(broker);
			var cert1 = wrapper.ESBPasswordCollection.AddNew();
			cert1.GP_Name = "TestCert1";
			cert1.GP_MailBoxID = "Test";
			cert1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var cert2 = wrapper.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert2";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			authStaff = Factory.New<GlbStaff>();
			authStaff.GS_Code = "AZ";
			authStaff.GS_LoginName = "aztest";

			var authorisation = Factory.New<GlbExternalPasswordAuthorisation>();
			authorisation.GEA_GP = cert1.PK;
			authorisation.GEA_GS_AuthorisedStaff = authStaff.PK;

			Factory.Save();

			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_GS_NKCustomsAgent = broker.GS_Code;

			return exitHeader;
		}
	}
}
