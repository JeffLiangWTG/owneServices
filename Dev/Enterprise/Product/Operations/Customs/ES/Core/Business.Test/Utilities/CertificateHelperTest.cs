using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class CertificateHelperTest : TestCaseWithFactory
	{
		public void TestCertificateNames()
		{
			var cacheMessagePrefix = "CacheMessage";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "AP";
			staff2.GS_LoginName = "aptest";

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AZ";
			staff3.GS_LoginName = "aztest";

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_Code = "JC";
			staff4.GS_LoginName = "jctest";

			var wrapper = GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert1";
			cert.GP_MailBoxID = "Test";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff3.PK;

			cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert2";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff2.PK;
			auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff3.PK;

			cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TestCert3";
			cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;
			auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff2.PK;

			var wrapper2 = GlbStaffWrapper.Get(staff2);
			var cert2 = wrapper2.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert4";
			cert2.GP_MailBoxID = "Test";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff3.PK;

			cert2 = wrapper2.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert5";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff3.PK;

			cert2 = wrapper2.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert6";
			cert2.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;
			auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff3.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				var list = CertificateHelper.CertificateNames(Factory, null, cacheMessagePrefix);
				AssertEquals("List has no values broker is null", 0, list.Count);

				list = CertificateHelper.CertificateNames(Factory, staff, cacheMessagePrefix);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff)", "TESTCERT1, TESTCERT2", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT1 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT2 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT2"));
				AssertSame("Cached value (staff)", list, CertificateHelper.CertificateNames(Factory, staff, cacheMessagePrefix));

				list = CertificateHelper.CertificateNames(Factory, staff2, cacheMessagePrefix);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2)", "TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT4 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT5 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff2)", list, CertificateHelper.CertificateNames(Factory, staff2, cacheMessagePrefix));

				list = CertificateHelper.CertificateNames(Factory, staff3, cacheMessagePrefix);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3)", "TESTCERT1, TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT1 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT4 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT5 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff3)", list, CertificateHelper.CertificateNames(Factory, staff3, cacheMessagePrefix));

				list = CertificateHelper.CertificateNames(Factory, staff4, cacheMessagePrefix);
				AssertEquals("List has no values when specified broker (staff4) has no certificates associated or authorised", 0, list.Count);
			});
		}

		public void TestCheckCustomsProfile_List()
		{
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "INVALID";
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckCustomsProfile_AuthorisedUser()
		{
			var notAuthorisedUserMessage = "You are not authorized to use this certificate. Please ask the Broker to authorize your user on the Staff & Resource module, Brokerage tab.";
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "TestCert1";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert1", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);

				declaration.JE_CustomsProfile = "TestCert2";
				AssertHasMessageErrorContaining("Default Current User is not authorised for cert2", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);

				using (Env.SetTemporaryUserContext(new UserContext(authStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("authStaff not authorised for cert1", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertHasMessageErrorContaining("authStaff is not authorised for cert2", declaration.JE_CustomsProfileInfo, notAuthorisedUserMessage);
				}

				using (Env.SetTemporaryUserContext(new UserContext(broker, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					declaration.JE_CustomsProfile = "TestCert1";
					AssertNoNotifications("broker is cert1's owner so is authorised", declaration.JE_CustomsProfileInfo);

					declaration.JE_CustomsProfile = "TestCert2";
					AssertNoNotifications("broker is cert2's owner so is authorised", declaration.JE_CustomsProfileInfo);
				}
			});
		}

		public void TestCheckCustomsProfile_Mandatory()
		{
			var declaration = SetUpBOWithCertificateData(out var broker, out var authStaff);

			CombineAssertions(() =>
			{
				declaration.JE_CustomsProfile = "TestCert1";
				AssertNoMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_CustomsProfile = ZString.Empty;
				AssertHasMessageErrorContaining(declaration.JE_CustomsProfileInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestGetCertificate()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "AP";
			staff2.GS_LoginName = "aptest";

			var wrapper = GlbStaffWrapper.Get(staff);
			var cert1 = wrapper.ESBPasswordCollection.AddNew();
			cert1.GP_Name = "TestCert1";
			cert1.GP_MailBoxID = "Test";
			cert1.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert1.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var cert2 = wrapper.ESBPasswordCollection.AddNew();
			cert2.GP_Name = "TestCert2";
			cert2.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert2.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff2.PK;

			var cert3 = wrapper.ESBPasswordCollection.AddNew();
			cert3.GP_Name = "TestCert3";
			cert3.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;
			auth = cert2.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AZ";
			staff3.GS_LoginName = "aztest";
			var wrapper2 = GlbStaffWrapper.Get(staff3);
			var cert4 = wrapper2.ESBPasswordCollection.AddNew();
			cert4.GP_Name = "TestCert4";
			cert4.GP_MailBoxID = "Test";
			cert4.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert4.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert4.GP_UserID = "CertThumbPrint1";

			var cert5 = wrapper2.ESBPasswordCollection.AddNew();
			cert5.GP_Name = "TestCert4";
			cert5.GP_MailBoxID = "Test";
			cert5.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert5.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			cert5.GP_UserID = "CertThumbPrint2";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertNull("Method returns null when broker is null", CertificateHelper.GetCertificate(null, "TestCert1"));
				AssertNull("Method returns null when certificateName is empty", CertificateHelper.GetCertificate(staff, ZString.Empty));

				var certificateReturned = CertificateHelper.GetCertificate(staff, "TestCert1");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct (cert1)", certificateReturned);
				AssertEquals("Certificate returned is the correct one (cert1)", cert1, certificateReturned);

				certificateReturned = CertificateHelper.GetCertificate(staff, "TestCert2");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct (cert2)", certificateReturned);
				AssertEquals("Certificate returned is the correct one (cert2)", cert2, certificateReturned);

				AssertNull("Method returns null when certificateName does not match an existing valid (status VAL) certificate for the broker", CertificateHelper.GetCertificate(staff, "TestCert3"));

				AssertNull("Method returns null when certificateName does not match an existing certificate (owned or authorized) for the broker (staff)", CertificateHelper.GetCertificate(staff, "TestCert4"));

				AssertNull("Method returns null when certificateName does not match an existing authorized certificate for the broker", CertificateHelper.GetCertificate(staff2, "TestCert1"));

				certificateReturned = CertificateHelper.GetCertificate(staff2, "TestCert2");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct, broker is authorized to use certificate (cert2)", certificateReturned);
				AssertEquals("Certificate returned is the correct one (cert2)", cert2, certificateReturned);

				AssertNull("Method returns null when certificateName does not match an existing valid (status VAL) certificate for the broker even when the broker is authorized to use it", CertificateHelper.GetCertificate(staff2, "TestCert3"));

				AssertNull("Method returns null when certificateName does not match an existing certificate (owned or authorized) for the broker (staff2)", CertificateHelper.GetCertificate(staff2, "TestCert4"));

				certificateReturned = CertificateHelper.GetCertificate(staff3, "TestCert4");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct for staff3", certificateReturned);
				AssertEquals("Certificate returned is the correct one, should be one between cert4 and cert5", true, new GlbExternalPassword[] { cert4, cert5 }.Contains(certificateReturned));

				certificateReturned = CertificateHelper.GetCertificate(staff3, "TestCert4", "CertThumbPrint1");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct, when adding thumbprint (cert4)", certificateReturned);
				AssertEquals("Certificate returned is the correct one (cert4)", cert4, certificateReturned);

				certificateReturned = CertificateHelper.GetCertificate(staff3, "TestCert4", "CertThumbPrint2");
				AssertNotNull("Method returns a certificate when broker and certificateName are correct, when adding thumbprint (cert5)", certificateReturned);
				AssertEquals("Certificate returned is the correct one (cert5)", cert5, certificateReturned);
			});
		}

		JobDeclaration SetUpBOWithCertificateData(out GlbStaff broker, out GlbStaff authStaff)
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

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = broker.GS_Code;

			return declaration;
		}
	}
}
