using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using GlbStaffWrapper = Enterprise.Customs.ES.Business.GlbStaffWrapper;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNatSimplificationIndicator()
		{
			AssertSame(Factory.GetCachedValue<NationalSimplificationIndicatorList>(), lookups.NatSimplificationIndicator);
		}

		public void TestOrgAddressesList()
		{
			AssertType<OrganisationsFindBoxCollection>(lookups.OrgAddressesList);
		}

		public void TestTADPrintProcedureLis()
		{
			AssertSame(Factory.GetCachedValue<TADPrintProcedureList>(), nctsHeader.Lookups.TADPrintProcedureList);
		}

		public void TestNctsArrivalTNNTypeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.NCTS5ArrivalTNNTypeList;
				AssertType<ESNCTS5ArrivalTNNTypeList>(list);
				AssertSame("Cached", list, lookups.NCTS5ArrivalTNNTypeList);
				AssertEquals("Count is correct", 2, list.Count);
				AssertEquals("Values in ES contains A", true, list.ContainsCode("A"));
				AssertEquals("Values in ES contains 4", true, list.ContainsCode("4"));
			});
		}

		public void TestNctsTransitStatusList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.NctsTransitStatusList;
				AssertContainsExactElementsInAnyOrder(new NctsTransitStatusList().GetAllCodes(), list.GetAllCodes());
				var commonMovement = nctsHeader.MovementHeader;
				AssertSame(commonMovement.Lookups.NctsTransitStatusList, list);
				AssertSame("Cached", list, lookups.NctsTransitStatusList);
			});
		}

		public void TestCertificateNames_Departure()
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
			var nctsDepartureMovementHeader = nctsHeader.MovementHeader;

			CombineAssertions(() =>
			{
				nctsDepartureMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				lookups = new NctsHeaderLookups(nctsHeader);
				var list = lookups.CertificateNames;
				AssertEquals("List has no values when broker is not declared", 0, list.Count);

				nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff)", "TESTCERT1, TESTCERT2", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT1 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT2 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT2"));
				AssertSame("Cached value (staff)", list, lookups.CertificateNames);

				nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff2.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff2)", "TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT4 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT5 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff2)", list, lookups.CertificateNames);

				nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff3.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff3)", "TESTCERT1, TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT1 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT4 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT5 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff3)", list, lookups.CertificateNames);

				nctsDepartureMovementHeader.BM_GS_NKCusAgent = staff4.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("List has no values when broker (staff4) declared has no certificates associated", 0, list.Count);
			});
		}

		public void TestCertificateNames_Arrival()
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

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsArrivalMovementHeader = nctsHeader.ArrivalMovementHeader;

			CombineAssertions(() =>
			{
				nctsArrivalMovementHeader.BM_GS_NKCusAgent = ZString.Empty;
				lookups = new NctsHeaderLookups(nctsHeader);
				var list = lookups.CertificateNames;
				AssertEquals("List has no values when broker is not declared", 0, list.Count);

				nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff)", "TESTCERT1, TESTCERT2", list.CodesAsString);
				AssertSame("Cached value (staff)", list, lookups.CertificateNames);

				nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff2.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff2)", "TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertSame("Cached value (staff2)", list, lookups.CertificateNames);

				nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff3.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff3)", "TESTCERT1, TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertSame("Cached value (staff3)", list, lookups.CertificateNames);

				nctsArrivalMovementHeader.BM_GS_NKCusAgent = staff4.GS_Code;
				lookups = new NctsHeaderLookups(nctsHeader);
				list = lookups.CertificateNames;
				AssertEquals("List has no values when broker (staff4) declared has no certificates associated", 0, list.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			lookups = new NctsHeaderLookups(nctsHeader);
		}
		NctsHeader nctsHeader;
		NctsHeaderLookups lookups;
	}
}
