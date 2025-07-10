using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class CusExitControlHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCertificateNames()
		{
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

			var exitHeader = Factory.New<CusExitControlHeader>();

			CombineAssertions(() =>
			{
				exitHeader.CEH_GS_NKCustomsAgent = ZString.Empty;
				var list = exitHeader.Lookups.CertificateNames;
				AssertEquals("List has no values when broker is not declared", 0, list.Count);

				exitHeader.CEH_GS_NKCustomsAgent = staff.GS_Code;
				list = exitHeader.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff)", "TESTCERT1, TESTCERT2", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT1 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT2 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT2"));
				AssertSame("Cached value (staff)", list, exitHeader.Lookups.CertificateNames);

				exitHeader.CEH_GS_NKCustomsAgent = staff2.GS_Code;
				list = exitHeader.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff2)", "TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT4 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT5 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff2)", list, exitHeader.Lookups.CertificateNames);

				exitHeader.CEH_GS_NKCustomsAgent = staff3.GS_Code;
				list = exitHeader.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff3)", "TESTCERT1, TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT1 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT4 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT5 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff3)", list, exitHeader.Lookups.CertificateNames);

				exitHeader.CEH_GS_NKCustomsAgent = staff4.GS_Code;
				list = exitHeader.Lookups.CertificateNames;
				AssertEquals("List has no values when broker (staff4) declared has no certificates associated", 0, list.Count);
			});
		}
	}
}
