using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class JobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
	{
		public void TestRepresentationTypeList()
		{
			var typeList = lookups.RepresentationTypeList;
			AssertEquals(typeof(ESRepresentationTypeList), typeList.GetType());
			Assert(typeList.ContainsCode("4"));
			var testList = typeList;
			AssertEquals(5, testList.Count);
		}

		public void TestLocationsList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var spainCode = Core.Constants.CountryCodes.Spain;
			var italyCode = Core.Constants.CountryCodes.Italy;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			helper.CreateNewOrGetExistingDataGrouping(spainCode, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);

			var locCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			helper.CreateNewOrGetExistingCusCodeType(locCode, "Locations");
			helper.CreateNewOrGetExistingCusCodeType("AAA", "Invalid Type");
			Factory.Save();
			var refCusCodeList1 = helper.CreateCusCodeList(eunCode, locCode, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(spainCode, locCode, "ES00010100DECO", "Test 1", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList3 = helper.CreateCusCodeList(spainCode, locCode, "ES00010101EAT", "Test 2", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList4 = helper.CreateCusCodeList(spainCode, locCode, "ES00010101GENE", "Test 3", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList5 = helper.CreateCusCodeList(italyCode, locCode, "IT00010", "Test 4", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var refCusCodeList6 = helper.CreateCusCodeList(spainCode, "AAA", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList7 = helper.CreateCusCodeList(spainCode, locCode, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList8 = helper.CreateCusCodeList(spainCode, locCode, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));

			Factory.Save();
			var locationsList = lookups.Locations;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", locationsList);
				AssertSame("Cached", locationsList, lookups.Locations);

				var completeFilter = ((ZZRefCusCodeListCombinedCollection)locationsList).CompleteFilter;
				AssertEquals("Unmatched EUN", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 1", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 2", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList3.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched ES 3", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList4.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}

		public void TestPortOfArrivals_ExportNormal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var (sydney, barcelona, rotterdam) = SetupUnlocosForPorts();

			CombineAssertions(() =>
			{
				var filter = declaration.Lookups.PortOfArrivals;
				AssertEquals("port arrivals contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("port arrivals does not contain eu ports", true, filter.Contains(rotterdam));
				AssertEquals("port arrivals does not contain local ports", true, filter.Contains(barcelona));
			});
		}

		public void TestFinalDestinations_ExportNormal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportNormal;
			var (sydney, barcelona, rotterdam) = SetupUnlocosForPorts();

			CombineAssertions(() =>
			{
				var filter = declaration.Lookups.FinalDestinations;
				AssertEquals("final destinations contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("final destinations contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("final destinations contains local ports", true, filter.Contains(barcelona));
			});
		}

		public void TestPortOfArrivals_ExportToSpecialTerritory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			var (sydney, barcelona, rotterdam) = SetupUnlocosForPorts();

			CombineAssertions(() =>
			{
				var filter = declaration.Lookups.PortOfArrivals;
				AssertEquals("port arrivals contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("port arrivals contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("port arrivals contains local ports", true, filter.Contains(barcelona));
			});
		}

		public void TestFinalDestinations_ExportToSpecialTerritory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_EntryStyle = EntryStyleListExport.Codes.ExportToSpecialTerritory;
			var (sydney, barcelona, rotterdam) = SetupUnlocosForPorts();

			CombineAssertions(() =>
			{
				var filter = declaration.Lookups.FinalDestinations;
				AssertEquals("final destinations contains foreign ports", true, filter.Contains(sydney));
				AssertEquals("final destinations contains eu ports", true, filter.Contains(rotterdam));
				AssertEquals("final destinations contains local ports", true, filter.Contains(barcelona));
			});
		}

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

			var declaration = Factory.New<JobDeclaration>();

			CombineAssertions(() =>
			{
				declaration.JE_GS_NKCusAgent = ZString.Empty;
				var list = declaration.Lookups.CertificateNames;
				AssertEquals("List has no values when broker is not declared", 0, list.Count);

				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				list = declaration.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff)", "TESTCERT1, TESTCERT2", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT1 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff), TESTCERT2 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT2"));
				AssertSame("Cached value (staff)", list, declaration.Lookups.CertificateNames);

				declaration.JE_GS_NKCusAgent = staff2.GS_Code;
				list = declaration.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff2)", "TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT4 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff2), TESTCERT5 description is empty when cert is owned by broker", ZString.Empty, list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff2)", list, declaration.Lookups.CertificateNames);

				declaration.JE_GS_NKCusAgent = staff3.GS_Code;
				list = declaration.Lookups.CertificateNames;
				AssertEquals("CertificateNames has the correct list for the declared broker (staff3)", "TESTCERT1, TESTCERT2, TESTCERT4, TESTCERT5", list.CodesAsString);
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT1 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT1"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT2 description is not empty when cert is not owned by broker but authorized", "ahtest", list.GetDescriptionFromCode("TESTCERT2"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT4 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT4"));
				AssertEquals("CertificateNames has the correct list for the specified broker (staff3), TESTCERT5 description is not empty when cert is not owned by broker but authorized", "aptest", list.GetDescriptionFromCode("TESTCERT5"));
				AssertSame("Cached value (staff3)", list, declaration.Lookups.CertificateNames);

				declaration.JE_GS_NKCusAgent = staff4.GS_Code;
				list = declaration.Lookups.CertificateNames;
				AssertEquals("List has no values when broker (staff4) declared has no certificates associated", 0, list.Count);
			});
		}

		(RefUNLOCO Sydney, RefUNLOCO Barcelona, RefUNLOCO Rotterdam) SetupUnlocosForPorts()
		{
			var loader = new RefUNLOCO.Loader(Factory);
			var sydney = loader.Load("AUSYD");
			var barcelona = loader.Load("ESBCN");
			var rotterdam = loader.Load("NLRTM");
			return (sydney, barcelona, rotterdam);
		}
	}
}
