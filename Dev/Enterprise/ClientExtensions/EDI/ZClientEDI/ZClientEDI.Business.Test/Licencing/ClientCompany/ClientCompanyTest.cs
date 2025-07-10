using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientCompany))]
	internal class ClientCompanyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUsageOwnerLicence()
		{
			var licRecent = BillingTestHelper.CreateLicence(Factory, "AAA");
			var licOldInactive = BillingTestHelper.CreateAnotherLicence(licRecent, "BBB");
			var licOldActive = BillingTestHelper.CreateAnotherLicence(licRecent, "CCC");
			var licOldActive2 = BillingTestHelper.CreateAnotherLicence(licRecent, "DDD");
			var licOther = BillingTestHelper.CreateLicence(Factory, "ZZZ");
			var db1 = licRecent.Database;

			licOldActive.LA_AgreedLiveDate = licOldActive.LA_AgreedLiveDate.AddYears(-1);
			licOldActive2.LA_AgreedLiveDate = licOldActive.LA_AgreedLiveDate;

			licOldInactive.LA_AgreedLiveDate = licOldInactive.LA_AgreedLiveDate.AddYears(-2);
			licOldInactive.LA_IsActive = false;

			var clientCompany1 = BillingTestHelper.FindOrCreateClientCompany(licRecent);
			var clientCompany2 = BillingTestHelper.FindOrCreateClientCompany(licOldInactive);
			var clientCompany3 = BillingTestHelper.FindOrCreateClientCompany(licOldActive);
			var clientCompany4 = BillingTestHelper.CreateClientCompany(db1, "EEE");

			Factory.Save();

			AssertEquals(licRecent, clientCompany1.UsageOwnerLicence);
			AssertEquals(licOldInactive, clientCompany2.UsageOwnerLicence);
			AssertEquals(licOldActive, clientCompany3.UsageOwnerLicence);
			AssertEquals("default owner is oldest, active, first company code", licOldActive, clientCompany4.UsageOwnerLicence);

			db1.LD_OH_BillingParty = licRecent.Company.LC_OH;
			Factory.Save();
			AssertEquals(licRecent, clientCompany1.UsageOwnerLicence);
			AssertEquals(licOldInactive, clientCompany2.UsageOwnerLicence);
			AssertEquals(licOldActive, clientCompany3.UsageOwnerLicence);
			AssertEquals("default owner is BillingParty", licRecent, clientCompany4.UsageOwnerLicence);

			db1.LD_OH_BillingParty = licOther.Company.LC_OH;
			Factory.Save();
			AssertEquals("default owner is on the DB if the BillingParty is not", licOldActive, clientCompany4.UsageOwnerLicence);
		}

		public void TestLoadFromLicenceCode()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			lic1.Database.LD_ServerCode = "DB1";
			lic1.Database.LicEnterprise.LE_EnterpriseCode = "EN1";
			var clientCompany1 = BillingTestHelper.FindOrCreateClientCompany(lic1);
			var clientOnlyCompany2 = BillingTestHelper.CreateClientCompany(lic1.Database, "UUU");
			var lic1b = BillingTestHelper.CreateAnotherDatabase(lic1, "DB2");
			var clientCompany1b = BillingTestHelper.FindOrCreateClientCompany(lic1b);
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			var clientCompany2 = BillingTestHelper.FindOrCreateClientCompany(lic2);
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC");
			lic3.Database.LD_ServerCode = "DBC";
			lic3.Database.LicEnterprise.LE_EnterpriseCode = "ENC";
			var clientCompany3 = BillingTestHelper.FindOrCreateClientCompany(lic3);

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			AssertEquals(clientCompany1.PK, ClientCompany.LoadFromLicenceCode(factory2, "EN1", "AAA", "DB1").PK);
			AssertEquals(clientOnlyCompany2.PK, ClientCompany.LoadFromLicenceCode(factory2, "EN1", "UUU", "DB1").PK);
			AssertEquals(clientCompany1b.PK, ClientCompany.LoadFromLicenceCode(factory2, "EN1", "AAA", "DB2").PK);
			AssertEquals(clientCompany2.PK, ClientCompany.LoadFromLicenceCode(factory2, "EN1", "BBB", "DB1").PK);
			AssertEquals(clientCompany3.PK, ClientCompany.LoadFromLicenceCode(factory2, "ENC", "CCC", "DBC").PK);
		}

		public void TestFindOrCreate()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA", "AA1", "AAA", false);
			var lic1b = BillingTestHelper.CreateDependentLicence(lic1a, "AA2", false);
			Factory.Save();

			var client1 = ClientCompany.FindOrCreate(Factory, "AAA", lic1a.LA_LD, lic1a.Company.LC_OH, "Name", "AU");
			var client2 = ClientCompany.FindOrCreate(Factory, "BBB", lic1a.LA_LD, lic1a.Company.LC_OH, "Name B", "AU");
			AssertEquals(client1.PK, client2.PK);
			AssertEquals("Name B", client1.LCC_Name);
			AssertEquals("Code not updated if org PK matches", "AAA", client2.LCC_Code);

			var client3 = ClientCompany.FindOrCreate(Factory, "AAA", lic1a.LA_LD, lic1b.Company.LC_OH, "Name 3", "AU");
			AssertEquals(client1.PK, client3.PK);
			AssertEquals("Org PK not updated if only code matches", lic1a.Company.LC_OH, client3.LCC_OH);
		}

		[TestDate(2016, 1, 25, 9, 0, 0)]
		public void TestSync()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA", "AA1", "AAA", false);
			var lic1b = BillingTestHelper.CreateDependentLicence(lic1a, "AA2", false);
			var lic1c = BillingTestHelper.CreateDependentLicence(lic1a, "AA3", true);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB", "BBB", "BBB", false);
			Factory.Save();

			var pk1 = Guid.NewGuid();
			var companyReports = new List<EDIVersionReport.EdiCompanyReport>();
			var company1 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				Name = "1 Co",
				PK = pk1,
				Code = "AA1",
				Address1 = "Addr11",
				Address2 = "Addr21",
				City = "Alexandria",
				State = "NSW",
				CountryCode = "AU",
				CurrencyCode = "AUD",
				Unloco = "AUSYD",
				IsGSTCashBasis = true,
				IsGSTRegistered = true,
				IsWHTCashBasis = true,
				IsWHTRegistered = true
			};
			companyReports.Add(company1);

			var report1Utc = new ZDateTime(2016, 1, 12, 11, 15, 0);
			TestDateAttribute.Date = report1Utc.AddMinutes(5).ToDateTime();
			bool sync1 = ClientCompany.Sync(Factory, lic1a.Database, report1Utc, companyReports, 1, 0);
			AssertEquals("unmatched ClientCompany deactivated", report1Utc, lic1c.ClientCompany.LCC_DeactivateTimeUtc);

			var pk2 = Guid.NewGuid();
			var company2 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				Name = "2 Co",
				PK = pk2,
				Code = "AA2",
				Address1 = "Addr12",
				Address2 = "Addr22",
				CountryCode = "NZ",
				CurrencyCode = "NZD",
				Unloco = "NZAKL"
			};

			companyReports.Add(company2);

			company1.Code = "ZZZ";
			company1.Address1 = "NewAddress";

			var report2Utc = new ZDateTime(2016, 1, 13, 11, 15, 0);
			var report3Utc = new ZDateTime(2016, 1, 14, 11, 15, 0);
			var report4Utc = new ZDateTime(2016, 1, 15, 11, 15, 0);
			var report5Utc = new ZDateTime(2016, 1, 16, 11, 15, 0);
			TestDateAttribute.Date = report2Utc.AddMinutes(5).ToDateTime();
			bool sync2 = ClientCompany.Sync(Factory, lic1a.Database, report2Utc, companyReports, 1, 0);

			company1.Code = "YYY";

			TestDateAttribute.Date = report3Utc.AddMinutes(5).ToDateTime();
			bool sync3 = ClientCompany.Sync(Factory, lic1a.Database, report3Utc, companyReports, 1, 0);

			company1.PK = Guid.Empty;
			company2.PK = Guid.Empty;
			company1.IsActive = false;
			company2.Address1 = "Moved";

			TestDateAttribute.Date = report4Utc.AddMinutes(5).ToDateTime();
			bool sync4 = ClientCompany.Sync(Factory, lic1a.Database, report4Utc, companyReports, 1, 0);

			company2.Code = "AA3";
			company2.PK = pk2;
			TestDateAttribute.Date = report5Utc.AddMinutes(5).ToDateTime();
			bool sync5 = ClientCompany.Sync(Factory, lic1a.Database, report5Utc, companyReports, 1, 0);

			var clientCompanies = Factory.Load<ClientCompany>(new ZQuery());
			var oldCodes = ClientCompany.LoadOldCodes(lic1a.LA_LD.ToGuid());

			AssertEquals(2, clientCompanies.Length);
			AssertEquals(3, oldCodes.Count);
			var old1 = oldCodes.Single(x => x.Code == "AA1");
			var old2 = oldCodes.Single(x => x.Code == "ZZZ");
			var old3 = oldCodes.Single(x => x.Code == "AA2");

			var actual1 = clientCompanies.Single(x => x.LCC_ClientPK == pk1);
			var actual2 = clientCompanies.Single(x => x.LCC_ClientPK == pk2);

			AssertEquals(report4Utc, actual1.LCC_DeactivateTimeUtc);
			AssertCompanyEqual(company1, actual1);
			AssertCompanyEqual(company2, actual2);
			AssertEquals(report1Utc.ToDateTime(), old1.ValidFromUtc);
			AssertEquals(report2Utc.ToDateTime(), old2.ValidFromUtc);
			AssertEquals(report2Utc.ToDateTime(), old3.ValidFromUtc);
			AssertEquals(report3Utc, actual1.LCC_CodeValidFromUtc);
			AssertEquals(report5Utc, actual2.LCC_CodeValidFromUtc);

			var historyAsText = string.Join("\r\n", Utilities.GetDataTableFromQuery(TestConnection, "select CSH_LCC, CSH_Period from dbo.ClientCompanyActiveStatusHistory;")
				.Rows.OfType<DataRow>().Select(x => $"{x["CSH_LCC"]}-{x["CSH_Period"]}").OrderBy(x => x).ToArray());

			var historyAsTextExpected = string.Join("\r\n", new string[] { $"{actual1.PK}-201601", $"{actual2.PK}-201601" }.OrderBy(x => x));

			AssertEquals(historyAsTextExpected, historyAsText);
		}

		public void TestSync_CodeSwap()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(lic1a.Database, "LNM");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(lic1a.Database, "USA");
			var clientPk1 = ZGuid.NewZGuid();
			var clientPk2 = ZGuid.NewZGuid();
			clientCompany1.LCC_ClientPK = clientPk1;
			clientCompany2.LCC_ClientPK = clientPk2;
			Factory.Save();

			var companyReports = new List<EDIVersionReport.EdiCompanyReport>();
			var company1 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				PK = clientPk1.ToGuid(),
				Code = clientCompany2.LCC_Code
			};
			companyReports.Add(company1);

			var company2 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				PK = clientPk2.ToGuid(),
				Code = clientCompany1.LCC_Code
			};
			companyReports.Add(company2);

			bool sync = ClientCompany.Sync(Factory, lic1a.Database, ZDateTime.Now, companyReports, 1, 0);
			var clientCompanies = Factory.Load<ClientCompany>(new ZQuery());
			AssertEquals(2, clientCompanies.Length);
			AssertEquals("code is swapped", "USA", clientCompany1.LCC_Code);
			AssertEquals("code is swapped", "LNM", clientCompany2.LCC_Code);
		}

		public void TestSync_CodeSwapAndMerge()
		{
			// In DB CO1 has PK1, CO2 has PK2.
			// Report has one company with code CO2 and PK1
			// Customer must have created a new company with CO2, then restored the database 
			// and instead changed the code on an existing company CO1 to CO2.
			// Sync needs to merge PK2 into PK1 since PK2 is not in the report.
			var lic1a = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var clientCompany1 = BillingTestHelper.CreateClientCompany(lic1a.Database, "CO1");
			var clientCompany2 = BillingTestHelper.CreateClientCompany(lic1a.Database, "CO2");
			clientCompany1.LCC_Address1 = "Addr 1";
			clientCompany2.LCC_Address1 = "Addr 2";
			var clientPk1 = ZGuid.NewZGuid();
			var clientPk2 = ZGuid.NewZGuid();
			clientCompany1.LCC_ClientPK = clientPk1;
			clientCompany2.LCC_ClientPK = clientPk2;
			Factory.Save();

			var companyReports = new List<EDIVersionReport.EdiCompanyReport>();
			var company1 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				PK = clientPk1.ToGuid(),
				Code = clientCompany2.LCC_Code,
				Address1 = "New Addr"
			};
			companyReports.Add(company1);

			bool sync = ClientCompany.Sync(Factory, lic1a.Database, ZDateTime.Now, companyReports, 1, 0);
			var clientCompanies = new BusinessObjectFactory().Load<ClientCompany>(new ZQuery());
			AssertEquals(1, clientCompanies.Length);
			AssertEquals("code", "CO2", clientCompanies[0].LCC_Code);
			AssertEquals("PK", clientPk1, clientCompanies[0].LCC_ClientPK);
			AssertEquals("Address1", "New Addr", clientCompanies[0].LCC_Address1);
		}

		public void TestGenerateUniqueCode()
		{
			var codesInUse = new HashSet<string>();
			codesInUse.Add("001");
			AssertEquals("000", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("002", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("003", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("004", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("005", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("006", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("007", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("008", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("009", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("00A", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("00B", ClientCompany.GenerateUniqueCode(codesInUse));
			for (int i = 12; i < 36; ++i)
			{
				ClientCompany.GenerateUniqueCode(codesInUse);
			}
			AssertEquals("010", ClientCompany.GenerateUniqueCode(codesInUse));
			AssertEquals("011", ClientCompany.GenerateUniqueCode(codesInUse));
		}

		[TestDate(2016, 1, 25, 9, 0, 0)]
		public void TestSyncLegacyReport()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA", "AA1", "AAA", false);
			var lic1b = BillingTestHelper.CreateDependentLicence(lic1a, "AA2", false);
			var lic1c = BillingTestHelper.CreateDependentLicence(lic1a, "AA3", true);

			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB", "BBB", "BBB", false);
			Factory.Save();

			var companyReports = new List<EDIVersionReport.EdiCompanyReport>();
			var company1 = new EDIVersionReport.EdiCompanyReport();
			company1.IsActive = true;
			company1.Name = "1 Co";
			company1.Code = "AA1";
			company1.Address1 = "Addr11";
			company1.Address2 = "Addr22";
			company1.CountryCode = "AU";
			company1.CurrencyCode = "AUD";
			company1.IsGSTCashBasis = true;
			company1.IsGSTRegistered = true;
			company1.IsWHTCashBasis = true;
			company1.IsWHTRegistered = true;
			company1.OrgPKThatGeneratedThisLicence = lic1a.Company.LC_OH.ToGuid();
			companyReports.Add(company1);

			var report1Utc = new ZDateTime(2016, 1, 12, 11, 15, 0);
			TestDateAttribute.Date = report1Utc.AddMinutes(5).ToDateTime();
			bool sync1 = ClientCompany.Sync(Factory, lic1a.Database, report1Utc, companyReports, 1, 0);
			AssertEquals("unmatched ClientCompany deactivated", report1Utc, lic1c.ClientCompany.LCC_DeactivateTimeUtc);

			var company2 = new EDIVersionReport.EdiCompanyReport();
			company2.IsActive = true;
			company2.Name = "2 Co";
			company2.Code = "AA2";
			company2.Address1 = "Addr12";
			company2.Address2 = "Addr22";
			company2.CountryCode = "NZ";
			company2.CurrencyCode = "NZD";
			company2.OrgPKThatGeneratedThisLicence = Guid.NewGuid();

			companyReports.Add(company2);

			company1.Code = "ZZZ";
			company1.Address1 = "NewAddress";

			var report2Utc = new ZDateTime(2016, 1, 13, 11, 15, 0);
			TestDateAttribute.Date = report2Utc.AddMinutes(5).ToDateTime();
			bool sync2 = ClientCompany.Sync(Factory, lic1a.Database, report2Utc, companyReports, 1, 0);

			// swap codes, but not OrgPk
			company1.Code = "AA2";
			company2.Code = "ZZZ";

			var report3Utc = new ZDateTime(2016, 1, 14, 11, 15, 0);
			TestDateAttribute.Date = report3Utc.AddMinutes(5).ToDateTime();
			bool sync3 = ClientCompany.Sync(Factory, lic1a.Database, report2Utc, companyReports, 1, 0);

			var clientCompanies = Factory.Load<ClientCompany>(new ZQuery());
			var oldCodes = ClientCompany.LoadOldCodes(lic1a.LA_LD.ToGuid());

			AssertEquals(3, clientCompanies.Length);
			AssertEquals(1, oldCodes.Count);

			var actual1 = clientCompanies.Single(x => x.LCC_Code == "AA2");
			var actual2 = clientCompanies.Single(x => x.LCC_Code == "ZZZ");
			var actual3 = clientCompanies.Single(x => x.LCC_Code == "AA3");
			var old1 = oldCodes.Single(x => x.Code == "AA1");

			AssertCompanyEqual(company1, actual1);
			AssertCompanyEqual(company2, actual2);

			AssertEquals(report1Utc.ToDateTime(), old1.ValidFromUtc);

			AssertEquals(report2Utc, actual1.LCC_CodeValidFromUtc);
			AssertEquals(report2Utc, actual2.LCC_CodeValidFromUtc);
		}

		[TestDate(2019, 8, 27, 9, 0, 0)]
		public void TestSync_CompanyCodeBlank()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			Factory.Save();

			var pk1 = Guid.NewGuid();
			var companyReports = new List<EDIVersionReport.EdiCompanyReport>();
			var company1 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				Name = "1 Co",
				PK = pk1,
				Code = "AA1",
				Address1 = "Addr11",
				Address2 = "Addr21",
				City = "Alexandria",
				State = "NSW",
				CountryCode = "AU",
				CurrencyCode = "AUD",
				Unloco = "AUSYD",
				IsGSTCashBasis = true,
				IsGSTRegistered = true,
				IsWHTCashBasis = true,
				IsWHTRegistered = true
			};
			companyReports.Add(company1);

			var pk2 = Guid.NewGuid();
			var company2 = new EDIVersionReport.EdiCompanyReport
			{
				IsActive = true,
				Name = "",
				PK = pk2,
				Code = "",
				Address1 = "",
				Address2 = "",
				CountryCode = "",
				CurrencyCode = ""
			};

			companyReports.Add(company2);

			var report1Utc = TestDateAttribute.Date;
			bool sync = ClientCompany.Sync(Factory, lic1a.Database, report1Utc, companyReports, 1, 0);

			var clientCompanies = Factory.Load<ClientCompany>(new ZQuery());

			AssertEquals("valid company is sync'd and invalid company ignored", 1, clientCompanies.Length);
			var actual1 = clientCompanies.Single(x => x.LCC_ClientPK == pk1);
			AssertCompanyEqual(company1, actual1);
		}

		void AssertCompanyEqual(EDIVersionReport.EdiCompanyReport expected, ClientCompany actual)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Code", expected.Code, actual.LCC_Code);
				AssertEquals("Address1", NullToEmpty(expected.Address1), actual.LCC_Address1);
				AssertEquals("Address2", NullToEmpty(expected.Address2), actual.LCC_Address2);
				AssertEquals("BusinessRegNo", NullToEmpty(expected.BusinessRegNo), actual.LCC_BusinessRegNo);
				AssertEquals("BusinessRegNo2", NullToEmpty(expected.BusinessRegNo2), actual.LCC_BusinessRegNo2);
				AssertEquals("City", NullToEmpty(expected.City), actual.LCC_City);
				AssertEquals("CountryCode", NullToEmpty(expected.CountryCode), actual.LCC_RN_NKCountryCode);
				AssertEquals("CurrencyCode", NullToEmpty(expected.CurrencyCode), actual.LCC_RX_NKLocalCurrency);
				AssertEquals("CustomsRegistrationNo", NullToEmpty(expected.CustomsRegistrationNo), actual.LCC_CustomsRegistrationNo);
				AssertEquals("IsGSTCashBasis", expected.IsGSTCashBasis, actual.LCC_IsGSTCashBasis);
				AssertEquals("IsGSTRegistered", expected.IsGSTRegistered, actual.LCC_IsGSTRegistered);
				AssertEquals("IsReciprocal", expected.IsReciprocal, actual.LCC_IsReciprocal);
				AssertEquals("IsWHTCashBasis", expected.IsWHTCashBasis, actual.LCC_IsWHTCashBasis);
				AssertEquals("IsWHTRegistered", expected.IsWHTRegistered, actual.LCC_IsWHTRegistered);
				AssertEquals("Name", NullToEmpty(expected.Name), actual.LCC_Name);
				AssertEquals("Phone", NullToEmpty(expected.Phone), actual.LCC_Phone);
				if (expected.PK != Guid.Empty)
				{
					AssertEquals("PK", expected.PK, actual.LCC_ClientPK);
				}
				AssertEquals("PostCode", NullToEmpty(expected.PostCode), actual.LCC_PostCode);
				AssertEquals("State", NullToEmpty(expected.State), actual.LCC_State);
			});
		}

		static string NullToEmpty(string s)
		{
			return s ?? string.Empty;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var company = factory.NewWithValidTestData<ClientCompany>();
			company.LCC_Code = "AAA";
			company.LCC_LD = factory.NewWithValidTestData<LicenceDatabase>().PK;
			return company;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var company = Factory.NewWithValidTestData<ClientCompany>();
			return company;
		}

		public void TestClientBranchesFilters_LD_LLC()
		{
			//Arrange
			var licence1 = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRV", false);
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CO2", "SR2", false);

			var branch1 = Factory.NewWithValidTestData<ClientBranch>();
			branch1.LCB_LD = licence1.LA_LD;
			branch1.LCB_LCC_Code = "AAA";
			branch1.LCB_Code = "YY1";

			var branch2 = Factory.NewWithValidTestData<ClientBranch>();
			branch2.LCB_LD = licence1.LA_LD;
			branch2.LCB_LCC_Code = "AAA";
			branch2.LCB_Code = "YY2";

			var branch3 = Factory.NewWithValidTestData<ClientBranch>();
			branch3.LCB_LD = licence2.LA_LD;
			branch3.LCB_LCC_Code = "AAA";
			branch3.LCB_Code = "NNN";

			var company = Factory.NewWithValidTestData<ClientCompany>();
			company.LCC_Code = "AAA";
			company.LCC_LD = licence1.LA_LD;

			Factory.Save();

			//Act
			var clientBranches = company.ClientBranches;

			//Assert
			var expectedCount = 2;
			AssertEquals(expectedCount, clientBranches.Count);

			AssertCollectionContains(branch1, clientBranches);
			AssertCollectionContains(branch2, clientBranches);
			AssertCollectionNotContains(branch3, clientBranches);
		}
	}
}
