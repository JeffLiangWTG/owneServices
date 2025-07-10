using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParent()
		{
			var parent = Factory.New<NctsHeader>();
			parent.SetMovementType(NctsMovementType.Codes.Departure);
			AssertEquals(parent.Validation.Parent, parent);
		}

		public void TestCheckBH_CustomsProfile_List()
		{
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckBH_CustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);

				nctsHeader.BH_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					nctsHeader.BH_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", nctsHeader.BH_CustomsProfileInfo);

					nctsHeader.BH_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", nctsHeader.BH_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					nctsHeader.BH_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", nctsHeader.BH_CustomsProfileInfo);

					nctsHeader.BH_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", nctsHeader.BH_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckBH_CustomsProfile_Mandatory()
		{
			var nctsHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				nctsHeader.BH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.BH_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(nctsHeader.BH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		NctsHeader SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
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

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var departureMovement = header.MovementHeader;
			departureMovement.BM_GS_NKCusAgent = broker.GS_Code;

			return header;
		}
	}
}
