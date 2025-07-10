using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	public class CusExitHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDiscrepancies()
		{
			var warningText = "Data entered in this tab will not be sent to Spanish Customs when Discrepancies flag is not ticked.";

			CombineAssertions(() =>
			{
				var header = Factory.New<CusExitHeader>();
				var container1 = header.CusExitContainers.AddNew();
				var container2 = header.CusExitContainers.AddNew();

				var consignment1 = header.CusExitConsignments.AddNew();
				var consignment2 = header.CusExitConsignments.AddNew();
				consignment1.CXC_MovementReference = "MRN1234";
				consignment1.CXC_LocalReference = "1234";
				consignment2.CXC_MovementReference = "MRN1235";
				consignment2.CXC_LocalReference = "1235";
				AssertNotEquals("Prereq: different PK", consignment1.PK, consignment2.PK);

				var report1 = header.CusExitReports.AddNew();
				var report2 = header.CusExitReports.AddNew();

				AssertNoExceptionThrown("No Exception should be thrown when report1 and report2 have no consignment associated", () => header.Validation.ValidateDiscrepancies());

				report1.CER_CXC_Consignment = consignment1.PK;

				AssertNoExceptionThrown("No Exception should be thrown when report1 has consignment but report2 has no consignment associated", () => header.Validation.ValidateDiscrepancies());

				report2.CER_CXC_Consignment = consignment2.PK;

				header.Validation.ValidateDiscrepancies();
				header.Validation.ValidateDiscrepancies();

				AssertNoRowWarningContaining("No items for Consignment 1 and discrepancies not ticked consignment1", consignment1, warningText);
				AssertNoRowWarningContaining("No items for Consignment 1 and discrepancies not ticked container1", container1, warningText);

				AssertNoRowWarningContaining("No items for Consignment 1 and discrepancies not ticked consignment2", consignment2, warningText);
				AssertNoRowWarningContaining("No items for Consignment 1 and discrepancies not ticked container2", container2, warningText);

				var item1 = consignment1.CusExitConsignmentItems.AddNew();
				var package1 = item1.CusExitConsignmentPackagePivots.AddNew();

				AssertNoExceptionThrown("No Exception should be thrown when package1 has no container associated", () => header.Validation.ValidateDiscrepancies());

				package1.CNP_CXN_Container = container1.PK;

				var item2 = consignment2.CusExitConsignmentItems.AddNew();
				var package2 = item2.CusExitConsignmentPackagePivots.AddNew();

				AssertNoExceptionThrown("No Exception should be thrown when package2 has no container associated", () => header.Validation.ValidateDiscrepancies());

				package2.CNP_CXN_Container = container2.PK;
				report2.CER_Calc_Discrepancies = true;

				header.Validation.ValidateDiscrepancies();
				header.Validation.ValidateDiscrepancies();
				AssertHasRowWarningContaining(consignment1, warningText);
				AssertHasRowWarningContaining(item1, warningText);
				AssertHasRowWarningContaining(package1, warningText);
				AssertHasRowWarningContaining(container1, warningText);

				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked consignment2", consignment2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked item2", item2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked package2", package2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked container2", container2, warningText);

				report1.CER_Calc_Discrepancies = true;
				package1.CNP_CXN_Container = ZGuid.Empty;

				AssertNoExceptionThrown("No Exception should be thrown when package1 has no container associated", () => header.Validation.ValidateDiscrepancies());

				package1.CNP_CXN_Container = container1.PK;
				package2.CNP_CXN_Container = ZGuid.Empty;

				AssertNoExceptionThrown("No Exception should be thrown when package2 has no container associated", () => header.Validation.ValidateDiscrepancies());

				package2.CNP_CXN_Container = container2.PK;

				header.Validation.ValidateDiscrepancies();
				header.Validation.ValidateDiscrepancies();
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked consignment1", consignment1, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked item1", item1, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked package1", package1, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked container1", container1, warningText);

				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked consignment2", consignment2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked item2", item2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked package2", package2, warningText);
				AssertNoRowWarningContaining("Items for Consignment 2 and discrepancies ticked container2", container2, warningText);
			});
		}

		public void TestCheckCXH_CustomsProfile_List()
		{
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CXH_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(exitHeader.CXH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				exitHeader.CXH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(exitHeader.CXH_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCXH_CustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CXH_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", exitHeader.CXH_CustomsProfileInfo, notAuthorisedUserMessage);

				exitHeader.CXH_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", exitHeader.CXH_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.CXH_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", exitHeader.CXH_CustomsProfileInfo);

					exitHeader.CXH_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", exitHeader.CXH_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					exitHeader.CXH_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", exitHeader.CXH_CustomsProfileInfo);

					exitHeader.CXH_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", exitHeader.CXH_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckCXH_CustomsProfile_Mandatory()
		{
			var exitHeader = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				exitHeader.CXH_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(exitHeader.CXH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				exitHeader.CXH_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(exitHeader.CXH_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		CusExitHeader SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
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

			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = broker.GS_Code;

			return exitHeader;
		}
	}
}
