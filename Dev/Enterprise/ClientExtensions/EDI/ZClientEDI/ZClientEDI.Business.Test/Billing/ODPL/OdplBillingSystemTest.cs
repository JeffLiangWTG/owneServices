using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Client.EDI.Billing.ODPL.Test
{
	public class OdplBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			OdplBillingSystem odplBilling = new OdplBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ODM, odplBilling.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			CreateChargeableUsage(organisation1, new ZDateTime(2010, 10, 01), coreModule, 10);
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			AssertEquals(true, odplBilling.LoadSystemBills(context).First() is OdplSystemBill);
		}

		public void TestLoadRawUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			organisation1.Contacts.AddNew().OC_ContactName = "John Doe";
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[0], coreModule, new ZDateTime(2010, 10, 10), 2);
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[1], coreModule, new ZDateTime(2010, 10, 20));
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[0], forwarderModule, new ZDateTime(2010, 10, 10));

			BillingTestHelper.CreateOdplLicenceUsage(childOrganisation11, childOrganisation11.Contacts[0], coreModule, new ZDateTime(2010, 10, 10));

			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[0], coreModule, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[0], accountantModule, new ZDateTime(2010, 10, 20));

			organisation3.LicCompany.LicHeadersForAllDatabases[0].LA_AgreedLiveDate = new ZDateTime(2010, 10, 20);
			BillingTestHelper.CreateOdplLicenceUsage(organisation3, organisation3.Contacts[0], coreModule, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation3, organisation3.Contacts[0], accountantModule, new ZDateTime(2010, 10, 30));

			Factory.Save();

			LoadAndAssertRawUsage(organisation1, new string[] { coreModule, "AAA Name", "John Doe", forwarderModule, "AAA Name" });
			LoadAndAssertRawUsage(childOrganisation11, new string[] { coreModule, "AA1 Name" });
			LoadAndAssertRawUsage(organisation2, new string[] { coreModule, "BBB Name", accountantModule, "BBB Name" });

			// Raw usage for organisation3 is NOT loaded because of the LA_AgreedLiveDate condition in the query
			LoadAndAssertRawUsage(organisation3, new string[] { "" });

			var odplBilling = new OdplBillingSystem();
			var licence = organisation1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), organisation1.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);
			OdplRawUsage odplRaw = odplBilling.LoadOdplRawUsage(context) as OdplRawUsage;
			string expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Core (COR)"",""2""
""                AAA Name"",""""
""                John Doe"",""""
""Forwarder (FOR)"",""1""
""                AAA Name"",""""
";

			var builder = new ZStringBuilder();
			odplBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			odplBilling.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Oct-10 00:00"",""SYD"","""","""",""Core (COR) 2"","""","""",""1""
""01-Oct-10 00:00"",""SYD"","""","""",""AAA Name"","""","""",""1""
""01-Oct-10 00:00"",""SYD"","""","""",""John Doe"","""","""",""1""
""01-Oct-10 00:00"",""SYD"","""","""",""Forwarder (FOR) 1"","""","""",""1""
""01-Oct-10 00:00"",""SYD"","""","""",""AAA Name"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadRawUsage_COW()
		{
			EServicesBillingTestHelper.CreateTable();

			var lic1 = organisation1.LicCompany.LicHeadersForAllDatabases[0];
			var lic1a = childOrganisation11.LicCompany.LicHeadersForAllDatabases[0];
			var staff1 = BillingTestHelper.FindOrCreateDatabaseStaffByName(lic1.Database, "AAA Name");
			staff1.LS_Code = "AAA";
			var staff2 = BillingTestHelper.FindOrCreateDatabaseStaffByName(lic1.Database, "John Doe");
			staff2.LS_Code = "JD";
			var clientCompany1 = lic1.ClientCompany;
			var clientCompany1a = lic1a.ClientCompany;

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staff1, "ODM", coreModule, new ZDateTime(2010, 10, 10), 2);
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staff2, "ODM", coreModule, new ZDateTime(2010, 10, 20));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, staff1, "ODM", forwarderModule, new ZDateTime(2010, 10, 10));

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1a, staff1, "ODM", coreModule, new ZDateTime(2010, 10, 9));

			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[0], coreModule, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[0], accountantModule, new ZDateTime(2010, 10, 20));

			organisation3.LicCompany.LicHeadersForAllDatabases[0].LA_AgreedLiveDate = new ZDateTime(2010, 10, 20);
			BillingTestHelper.CreateOdplLicenceUsage(organisation3, organisation3.Contacts[0], coreModule, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation3, organisation3.Contacts[0], accountantModule, new ZDateTime(2010, 10, 30));

			Factory.Save();

			var licence = organisation1.LicCompany.LicHeadersForAllDatabases[0];
			var prices = BillingTestHelper.CreatePriceList(licence);
			var priceItemCOW = BillingTestHelper.AddPriceItem(prices, "COW", BillingConstants.FeeType.NamedUser, "", 2m);
			priceItemCOW.L7_Description = "COW - L7_Description";

			Factory.Save();

			var odplBilling = new OdplBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), organisation1.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);

			string expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Core (COR)"",""2""
""                AAA Name (AAA)"",""""
""                John Doe (JD)"",""""
";
			var builder = new ZStringBuilder();
			odplBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COW", new ZDateTime(2010, 10, 1), licence, 1);
			Factory.Save();

			expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Core (COR)"",""2""
""                AAA Name (AAA)"",""""
""                John Doe (JD)"",""""
""COW - L7_Description (COW)"",""1""
""                John Doe (JD)"",""""
";
			builder = new ZStringBuilder();
			odplBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		public void TestLoadRawUsage_CountryUsage()
		{
			EServicesBillingTestHelper.CreateTable();

			var licence = organisation1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var prices = BillingTestHelper.CreatePriceList(licence);
			var priceItemMCC = BillingTestHelper.AddPriceItem(prices, "MCC", BillingConstants.FeeType.Country, "", 12.33m);
			priceItemMCC.L7_Description = "MCC - L7_Description";
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "MCC", new ZDateTime(2010, 10, 1), licence.ClientCompany, 1);

			Factory.Save();

			var odplBilling = new OdplBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), organisation1.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);

			string expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""MCC - L7_Description (MCC)"",""1""
""                Australia (AU)"",""""
";

			var builder = new ZStringBuilder();
			odplBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			odplBilling.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Oct-10 00:00"",""SYD"","""","""",""MCC - L7_Description (MCC) 1"","""","""",""1""
""01-Oct-10 00:00"",""SYD"","""","""",""Australia (AU)"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadRawUsage_GPC()
		{
			EServicesBillingTestHelper.CreateTableBillingTransaction();

			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			licence.ClientCompany.LCC_RN_NKCountryCode = "AU";
			var db1 = licence.Database;
			var prices = BillingTestHelper.CreatePriceList(licence);
			prices.L6_PricelistVersion = "CW blah"; // need "CW*" for registered user as Core user
			prices.L6_ValidFrom = new DateTime(2016, 7, 1); // new a certain "valid from " for registered user as Core user

			var priceItemGPC1 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 5.11m);
			priceItemGPC1.L7_Description = "GPC - 1";
			priceItemGPC1.L7_UnitBreak = 1;

			var priceItemGPC2 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 10.22m);
			priceItemGPC2.L7_Description = "GPC - 2";
			priceItemGPC2.L7_UnitBreak = 2;

			var priceItemGPC3 = BillingTestHelper.AddPriceItem(prices, "GPC", BillingConstants.FeeType.UsersPerCountryVolumeBreak, "", 15.33m);
			priceItemGPC3.L7_Description = "GPC - 3";
			priceItemGPC3.L7_UnitBreak = 3;

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2017, 1, 15, 0, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, licence.ClientCompany.PK, "U01", "User A", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2017, 1, 15, 0, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, licence.ClientCompany.PK, "U02", "User B", "", ""));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", new ZDateTime(2017, 1, 15, 0, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, licence.ClientCompany.PK, "U03", "User C", "", ""));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", new ZDateTime(2017, 1, 1), licence.ClientCompany, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "GPC", new ZDateTime(2017, 1, 1), licence.ClientCompany, 1);

			Factory.Save();

			var odplBilling = new OdplBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2017, 1, 1), org1.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);

			string expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Registered User (COR)"",""3""
""                User A (U01)"",""""
""                User B (U02)"",""""
""                User C (U03)"",""""
""GPC - 2 (GPC)"",""3""
""                User A (U01)"",""""
""                User B (U02)"",""""
""                User C (U03)"",""""
";

			var builder = new ZStringBuilder();
			odplBilling.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			odplBilling.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""31-Dec-16 13:00"",""ABC"","""","""",""Registered User (COR) 3"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User A (U01)"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User B (U02)"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User C (U03)"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""GPC - 2 (GPC) 3"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User A (U01)"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User B (U02)"","""","""",""1""
""31-Dec-16 13:00"",""ABC"","""","""",""User C (U03)"","""","""",""1""
", writer.ToString());

			EServicesBillingTestHelper.DropTable();
		}

		public void TestLoadRawUsage_ParentAndPrice()
		{
			EServicesBillingTestHelper.CreateTable();

			string parentCode = Env.Licence.RelationshipManager.Name;
			string childCode1 = Env.Licence.RelationshipOpportunityManager.Name;
			string childCode2 = Env.Licence.RelationshipCampaignManager.Name;
			string codeFree = Env.Licence.RelationshipColdCallRegister.Name;

			string parentCodeWithChildPrices = Env.Licence.CoreCustomsModule.Name;
			string childCode1WithPrice = Env.Licence.Drawback.Name;
			string childCode2WithPrice = Env.Licence.LandedCosting.Name;

			organisation1.Contacts.AddNew().OC_ContactName = "John Doe";
			organisation2.Contacts.AddNew().OC_ContactName = "Betty";

			LicenceCompany company1 = organisation1.LicCompany;
			var prices1 = BillingTestHelper.CreatePriceList(company1);
			BillingTestHelper.AddPriceItem(prices1, parentCode, "", "", 10m);
			BillingTestHelper.AddPriceItem(prices1, childCode1, "", parentCode, 0m);
			BillingTestHelper.AddPriceItem(prices1, childCode2, "", parentCode, 0m);

			BillingTestHelper.AddPriceItem(prices1, parentCodeWithChildPrices, "", "", 50m);
			BillingTestHelper.AddPriceItem(prices1, childCode1WithPrice, "", parentCodeWithChildPrices, 30m);
			BillingTestHelper.AddPriceItem(prices1, childCode2WithPrice, "", parentCodeWithChildPrices, 28m);

			BillingTestHelper.AddPriceItem(prices1, codeFree, "", "", 0m);

			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[0], childCode1, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[1], childCode2, new ZDateTime(2010, 10, 20));
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[1], codeFree, new ZDateTime(2010, 10, 20));
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[0], childCode1WithPrice, new ZDateTime(2010, 10, 20));
			BillingTestHelper.CreateOdplLicenceUsage(organisation1, organisation1.Contacts[1], childCode2WithPrice, new ZDateTime(2010, 10, 20));

			// not included in parent since no pricelist
			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[0], parentCode, new ZDateTime(2010, 10, 10));
			BillingTestHelper.CreateOdplLicenceUsage(organisation2, organisation2.Contacts[1], childCode1, new ZDateTime(2010, 10, 20));

			Factory.Save();

			LoadAndAssertRawUsage(organisation1, new string[]
			{
				parentCode, "AAA Name", "John Doe",
				parentCodeWithChildPrices, "AAA Name", "John Doe",
				childCode1WithPrice, "AAA Name",
				childCode2WithPrice, "John Doe"
			});
			LoadAndAssertRawUsage(organisation2, new string[] { parentCode, "BBB Name", childCode1, "Betty" });
		}

		public void TestLoadRawUsage_SelfHostedDatabaseUsersFeeBasis()
		{
			EServicesBillingTestHelper.CreateTable();

			var periodStart = new ZDateTime(2010, 10, 1);

			var licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			var licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA2");
			BillingTestHelper.SetInvoicing(licHeader1a, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(licHeader1b, licHeader1a);
			var org1a = licHeader1a.Company.Header;
			licHeader1a.Database.LD_HostedLocation = "TRA";

			var priceHeader = ClientLicencePriceHeaderLookupsTest.AddPriceHeader(org1a.LicCompany, "V22c");
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "CLO", BillingConstants.FeeType.SelfHostedDatabaseUsers, "", 7m);

			BillingTestHelper.CreateEdiLicenceUsage(licHeader1a, BillingConstants.CoreModuleCode, periodStart, "Albert");
			BillingTestHelper.CreateEdiLicenceUsage(licHeader1b, BillingConstants.CoreModuleCode, periodStart, "Betty");
			BillingTestHelper.CreateEdiLicenceUsage(licHeader1b, BillingConstants.CoreModuleCode, periodStart, "Charlie");

			Factory.Save();

			var mock = new Mock<IDatabaseUsers>();
			IEnumerable<string> names = new List<string>(new string[] { "User 1", "User 2", "User 3" });
			mock.Setup(m => m.DatabaseMonthlyUserList(licHeader1a.LA_LD, periodStart))
				.Returns(names);
			var dbUsers = mock.Object;

			var odplBilling = new OdplBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, periodStart, licHeader1a.Company.LC_OH, licHeader1a.ClientCompany.PK, ZGuid.Empty, licHeader1a.Database.PK);
			context.DatabaseUsersService = dbUsers;
			var odplRaw = odplBilling.LoadOdplRawUsage(context) as OdplRawUsage;
			odplRaw.GetRawUsageSummarySections();

			AssertEquals(1, odplRaw.StaffCount(BillingConstants.CoreModuleCode));
			AssertEquals("Self Hosted Database Users module owned by org1a", 3, odplRaw.StaffCount("CLO"));
			AssertEquals(true, odplRaw.HasStaff("CLO", "User 1"));
			AssertEquals(true, odplRaw.HasStaff("CLO", "User 2"));
			AssertEquals(true, odplRaw.HasStaff("CLO", "User 3"));

			// Other company on DB is not the usage owner
			odplBilling = new OdplBillingSystem();
			context = new BillingLoadRawUsageContext(Factory, periodStart, licHeader1b.Company.LC_OH, licHeader1b.ClientCompany.PK, ZGuid.Empty, licHeader1b.Database.PK);
			context.DatabaseUsersService = dbUsers;
			odplRaw = odplBilling.LoadOdplRawUsage(context) as OdplRawUsage;
			AssertEquals(2, odplRaw.StaffCount(BillingConstants.CoreModuleCode));
			AssertEquals(0, odplRaw.StaffCount("CLO"));

			// Hosted system - shouldn't be charged
			licHeader1a.Database.LD_HostedLocation = "SYD";
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			context = new BillingLoadRawUsageContext(Factory, periodStart, licHeader1a.Company.LC_OH, licHeader1a.ClientCompany.PK, ZGuid.Empty, licHeader1a.Database.PK);
			context.DatabaseUsersService = dbUsers;
			odplRaw = odplBilling.LoadOdplRawUsage(context) as OdplRawUsage;
			AssertEquals(0, odplRaw.StaffCount("CLO"));

			mock.VerifyAll();
		}

		public void TestLoadRawUsage_WebParent()
		{
			EServicesBillingTestHelper.CreateTable();

			string module1 = Env.Licence.RelationshipClientIntelligence.Name;
			string module2 = Env.Licence.SalesDashboard.Name;
			string module3 = Env.Licence.RelationshipClientRatesTariffs.Name;
			string webMain = Env.Licence.WebTrackerWarehouse.Name;
			string web1 = Env.Licence.WebTrackerForwarding.Name;
			string web2 = Env.Licence.WebTrackerImportBrokerage.Name;
			string web3 = Env.Licence.WebTrackerBooking.Name;

			var periodStart = new ZDateTime(2010, 10, 1);

			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			BillingTestHelper.SetInvoicing(licHeader1a, Env.CurrentBranch.PK);
			var org1a = licHeader1a.Company.Header;

			var priceHeader = ClientLicencePriceHeaderLookupsTest.AddPriceHeader(org1a.LicCompany, "V22c");
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, module1, BillingConstants.FeeType.NamedUser, "", 1m);
			BillingTestHelper.AddPriceItem(priceHeader, module2, BillingConstants.FeeType.NamedUser, "", 3m);
			BillingTestHelper.AddPriceItem(priceHeader, module3, BillingConstants.FeeType.NamedUser, "", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, webMain, BillingConstants.FeeType.Module, "", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, web1, BillingConstants.FeeType.Included, webMain, 0m)
				.L7_WebParentCode = module1;
			BillingTestHelper.AddPriceItem(priceHeader, web2, BillingConstants.FeeType.Module, "", 7m)
				.L7_WebParentCode = module2;
			BillingTestHelper.AddPriceItem(priceHeader, web3, BillingConstants.FeeType.Module, "", 9m)
				.L7_WebParentCode = module3;

			BillingTestHelper.CreateEdiLicenceUsage(licHeader1a, BillingConstants.CoreModuleCode, periodStart, "Albert");
			BillingTestHelper.CreateEdiLicenceUsage(licHeader1a, module3, periodStart, "Fred");
			BillingTestHelper.CreateEdiLicenceUsage(licHeader1a, web1, periodStart, "Betty");
			BillingTestHelper.CreateEdiLicenceUsage(licHeader1a, web3, periodStart, "John");

			Factory.Save();

			LoadAndAssertRawUsage(org1a, new string[]
			{
				BillingConstants.CoreModuleCode, "Albert",
				module3, "Fred",
				web3, "John"
			});
		}

		public void TestLoadRawUsage_RegisteredUsers()
		{
			EServicesBillingTestHelper.CreateTable();

			var stdPriceEnterprise = Factory.NewWithValidTestData<LicenceEnterprise>();
			Guid stdPriceCompanyPk;
			Guid.TryParse("31754C3F-4782-4504-AC75-C92B0EEB1B73", out stdPriceCompanyPk);
			var stdPriceCompany = Factory.NewWithPrimaryKey<LicenceCompany>(stdPriceCompanyPk);
			stdPriceCompany.FillWithValidTestData();
			stdPriceCompany.LC_LE = stdPriceEnterprise.PK;
			stdPriceCompany.LC_CompanyCode = "DDD";

			var stdPriceHeader = stdPriceCompany.PriceHeaders.AddNew();
			stdPriceHeader.L6_SystemCode = "ODM";
			stdPriceHeader.L6_RX_NKCurrency = "AUD";
			stdPriceHeader.L6_PricelistVersion = "CW1 v8.1";
			stdPriceHeader.L6_IsStandard = false;
			stdPriceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "SYD", "HST");
			var org = licence.Company.Header;
			licence.LA_AgreedLiveDate = licence.LA_AgreedLiveDate.AddYears(-1);
			var db = licence.Database;
			db.LD_DatabaseNumber = 2847;
			db.LD_Product = ProductTypes.Codes.CargoWiseOne;
			BillingTestHelper.SetInvoicingTo(licence, licence.Company);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_PricelistVersion = "CW1 v8.1";
			priceHeader.L6_IsStandard = true;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 7, 1);

			BillingTestHelper.AddPriceItem(stdPriceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(stdPriceHeader, forwarderModule, BillingConstants.FeeType.NamedUser, "", 15m);

			var clientCompany1 = licence.ClientCompany;
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "ZTE";

			var clientStaff1 = Factory.New<ClientStaff>();
			clientStaff1.LS_Code = "AAA";
			clientStaff1.LS_FullName = "AAA Name";
			clientStaff1.LS_LD = db.PK;

			var clientStaff2 = Factory.New<ClientStaff>();
			clientStaff2.LS_Code = "JD";
			clientStaff2.LS_FullName = "John Doe";
			clientStaff2.LS_LD = db.PK;

			var clientStaff3 = Factory.New<ClientStaff>();
			clientStaff3.LS_Code = "BBB";
			clientStaff3.LS_FullName = "BBB Name";
			clientStaff3.LS_LD = db.PK;

			var clientStaff4 = Factory.New<ClientStaff>();
			clientStaff4.LS_Code = "TUR";
			clientStaff4.LS_FullName = "Test User";
			clientStaff4.LS_LD = db.PK;

			var clientStaff5 = Factory.New<ClientStaff>();
			clientStaff5.LS_Code = "DUR";
			clientStaff5.LS_FullName = "Demo User";
			clientStaff5.LS_LD = db.PK;

			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 10, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff2, clientStaff4);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff2, clientStaff4);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 12, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff2, clientStaff4);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2017, 1, 1), db, clientCompany1, clientCompany2, clientStaff1, clientStaff2, clientStaff4);

			Factory.Save();

			AssertRegisteredUsersRawUsage(new ZDateTime(2016, 10, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: false);

			priceHeader.L6_ValidFrom = new ZDateTime(2016, 6, 1);
			Factory.Save();
			AssertRegisteredUsersRawUsage(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: false);

			priceHeader.L6_ValidFrom = new ZDateTime(2016, 7, 1);
			Factory.Save();

			AssertRegisteredUsersRawUsage(new ZDateTime(2016, 11, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: true);
			AssertRegisteredUsersRawUsage(new ZDateTime(2016, 12, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: true);

			// Has commitment
			var discount = licence.Company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_BreakAmount = 200m;
			discount.L5_Discount = 25m;
			discount.L5_StartDate = new ZDateTime(2016, 6, 1);
			Factory.Save();

			AssertRegisteredUsersRawUsage(new ZDateTime(2017, 1, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: false);

			discount.L5_EndDate = new ZDateTime(2016, 12, 31);
			licence.Company.InvoiceDeliveries[0].L9_OH_InvoiceTo = ZGuid.Empty;
			Factory.Save();
			AssertRegisteredUsersRawUsage(new ZDateTime(2017, 1, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: true);

			stdPriceHeader.L6_PricelistVersion = "V23";
			priceHeader.L6_PricelistVersion = "V23";
			Factory.Save();
			AssertRegisteredUsersRawUsage(new ZDateTime(2017, 1, 1), db, clientCompany1, clientCompany2, expectedRegisteredUsers: false);
		}

		void CreateLoginUsagesAndActiveUsers(ZDateTime periodStart, LicenceDatabase db, ClientCompany clientCompany1, ClientCompany clientCompany2, ClientStaff clientStaff1, ClientStaff clientStaff2, ClientStaff clientStaff4)
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany1, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany2, 2);

			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff1, "ODM", coreModule, periodStart.AddDays(10));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff2, "ODM", coreModule, periodStart.AddDays(20));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany2, clientStaff4, "ODM", coreModule, periodStart.AddDays(21));
			BillingTestHelper.CreateEdiLicenceUsage(clientCompany1, clientStaff1, "ODM", forwarderModule, periodStart.AddDays(10));

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "AAA", "AAA Name (aaa.name)", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany1.PK, "TUR", "Test User (test.user)", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "JD", "John Doe (john.doe)", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "BBB", "BBB Name (bbb.name)", "", "", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("STL", "USR", periodStart.AddDays(1), "DDDABCSYD", "", db.DatabaseId, clientCompany2.PK, "DUR", "Demo User (demo.user)", "", "", null));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);
		}

		void AssertRegisteredUsersRawUsage(ZDateTime periodStart, LicenceDatabase db, ClientCompany clientCompany1, ClientCompany clientCompany2, bool expectedRegisteredUsers)
		{
			var odplBilling = new OdplBillingSystem();
			var company1Context = new BillingLoadRawUsageContext(Factory, periodStart, organisation1.PK, clientCompany1.PK, ZGuid.Empty, db.PK);
			var company2Context = new BillingLoadRawUsageContext(Factory, periodStart, organisation1.PK, clientCompany2.PK, ZGuid.Empty, db.PK);
			string expectedCsvResult = "";
			ZStringBuilder builder = null;

			if (expectedRegisteredUsers)
			{
				CombineAssertions(() =>
				{
					//Company 1
					expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Registered User (COR)"",""1""
""                AAA Name (AAA)"",""""
""Forwarder (FOR)"",""1""
""                AAA Name (AAA)"",""""
";
					builder = new ZStringBuilder();
					odplBilling.LoadRawUsageInCsv(company1Context, false, (csv) => { builder.AppendLine(csv); });
					AssertEquals(expectedCsvResult, builder.ToString());

					var dateString = company1Context.PeriodStartTimeUtc.ToLongTimeString();
					var writer1 = new CsvUsageReportWriterForTest();
					odplBilling.LoadRawUsageInCsv(company1Context, false, writer1);
					AssertEquals($@"""{dateString}"",""SYD"","""","""",""Registered User (COR) 1"","""","""",""1""
""{dateString}"",""SYD"","""","""",""AAA Name (AAA)"","""","""",""1""
""{dateString}"",""SYD"","""","""",""Forwarder (FOR) 1"","""","""",""1""
""{dateString}"",""SYD"","""","""",""AAA Name (AAA)"","""","""",""1""
", writer1.ToString());

					// Company 2
					expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Registered User (COR)"",""4""
""                BBB Name (bbb.name) (BBB)"",""""
""                Demo User (demo.user) (DUR)"",""""
""                John Doe (JD)"",""""
""                Test User (TUR)"",""""
";

					builder = new ZStringBuilder();
					odplBilling.LoadRawUsageInCsv(company2Context, false, (csv) => { builder.AppendLine(csv); });
					AssertEquals(expectedCsvResult, builder.ToString());

					var dateString2 = company2Context.PeriodStartTimeUtc.ToLongTimeString();
					var writer2 = new CsvUsageReportWriterForTest();
					odplBilling.LoadRawUsageInCsv(company2Context, false, writer2);
					AssertEquals($@"""{dateString2}"",""ZTE"","""","""",""Registered User (COR) 4"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""BBB Name (bbb.name) (BBB)"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""Demo User (demo.user) (DUR)"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""John Doe (JD)"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""Test User (TUR)"","""","""",""1""
", writer2.ToString());
				});
			}
			else
			{
				CombineAssertions(() =>
				{
					//Company 1
					expectedCsvResult =
@"""Module / Staff Name / Country"",""Count""
""Core (COR)"",""1""
""                AAA Name (AAA)"",""""
""Forwarder (FOR)"",""1""
""                AAA Name (AAA)"",""""
";
					builder = new ZStringBuilder();
					odplBilling.LoadRawUsageInCsv(company1Context, false, (csv) => { builder.AppendLine(csv); });
					AssertEquals(expectedCsvResult, builder.ToString());

					var dateString = company1Context.PeriodStartTimeUtc.ToLongTimeString();
					var writer1 = new CsvUsageReportWriterForTest();
					odplBilling.LoadRawUsageInCsv(company1Context, false, writer1);
					AssertEquals($@"""{dateString}"",""SYD"","""","""",""Core (COR) 1"","""","""",""1""
""{dateString}"",""SYD"","""","""",""AAA Name (AAA)"","""","""",""1""
""{dateString}"",""SYD"","""","""",""Forwarder (FOR) 1"","""","""",""1""
""{dateString}"",""SYD"","""","""",""AAA Name (AAA)"","""","""",""1""
", writer1.ToString());

					// Company 2
					expectedCsvResult =
	@"""Module / Staff Name / Country"",""Count""
""Core (COR)"",""2""
""                John Doe (JD)"",""""
""                Test User (TUR)"",""""
";
					builder = new ZStringBuilder();
					odplBilling.LoadRawUsageInCsv(company2Context, false, (csv) => { builder.AppendLine(csv); });
					AssertEquals(expectedCsvResult, builder.ToString());

					var dateString2 = company2Context.PeriodStartTimeUtc.ToLongTimeString();
					var writer2 = new CsvUsageReportWriterForTest();
					odplBilling.LoadRawUsageInCsv(company2Context, false, writer2);
					AssertEquals($@"""{dateString2}"",""ZTE"","""","""",""Core (COR) 2"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""John Doe (JD)"","""","""",""1""
""{dateString2}"",""ZTE"","""","""",""Test User (TUR)"","""","""",""1""
", writer2.ToString());
				});
			}
		}

		void LoadAndAssertRawUsage(EDIOrgHeader org, string[] moduleUsages, BillingLoadRawUsageContext context = null)
		{
			OdplBillingSystem odplBilling = new OdplBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			if (context == null)
			{
				context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2010, 10, 1), org.PK, licence.ClientCompany.PK, ZGuid.Empty, licence.Database.PK);
			}
			OdplRawUsage odplRaw = odplBilling.LoadOdplRawUsage(context) as OdplRawUsage;

			int moduleCount = 0;
			ZString currentModuleCode = "";
			int currentModuleUserCount = 0;
			foreach (string moduleUsage in moduleUsages.Where(x => !string.IsNullOrEmpty(x)))
			{
				if (moduleUsage.Length == 3) // Considering that ModuleCode is 3 letters and user names are longer
				{
					if (!currentModuleCode.IsEmpty)
					{
						AssertEquals("Expected number of users for this module", odplRaw.StaffCount(currentModuleCode), currentModuleUserCount);
						currentModuleUserCount = 0;
					}

					currentModuleCode = moduleUsage;
					moduleCount++;
				}
				else
				{
					AssertEquals("Usage found for: " + currentModuleCode + ", " + moduleUsage, true, odplRaw.HasStaff(currentModuleCode, moduleUsage));
					currentModuleUserCount++;
				}
			}

			AssertEquals("Expected number of modules", odplRaw.ModuleCount, moduleCount);
		}

		public void TestLoadUsages()
		{
			var licHeader4 = BillingTestHelper.CreateLicence(Factory, "444");
			licHeader4.LA_LicenceAdvStdOth = LicenceAdvStdOthList.Codes.Hybrid;
			licHeader4.CoreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			licHeader4.CoreModule.LM_UserCount = 11;
			Factory.Save();

			ZDateTime periodStart = new ZDateTime(2010, 10, 01);

			ClientChargeableUsage chargeableUsage1 = CreateChargeableUsage(organisation1, periodStart, coreModule, 5);
			ClientChargeableUsage chargeableUsageToIgnore = CreateChargeableUsage(organisation1, periodStart, BillingConstants.Hosting.WiseCloudUserFeeCode, 5);
			ClientChargeableUsage chargeableUsage2 = CreateChargeableUsage(organisation1, periodStart, forwarderModule, 20);
			ClientChargeableUsage chargeableUsage3 = CreateChargeableUsage(organisation1, periodStart, accountantModule, 30);

			ClientChargeableUsage chargeableUsage4 = CreateChargeableUsage(childOrganisation11, periodStart, coreModule, 7);

			ClientChargeableUsage chargeableUsage5 = CreateChargeableUsage(organisation2, periodStart, forwarderModule, 22);
			ClientChargeableUsage chargeableUsage6 = CreateChargeableUsage(organisation2, periodStart, accountantModule, 24);

			ClientChargeableUsage chargeableUsage7 = CreateChargeableUsage(licHeader4.Company.Header, periodStart, coreModule, 11);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			List<OdplUsage> allOdplUsages = new List<OdplUsage>();
			foreach (SystemBill bill in odplBilling.LoadSystemBills(context))
			{
				allOdplUsages.AddRange(bill.SystemUsages.Cast<OdplUsage>());
			}

			AssertEquals("usages count", 4, allOdplUsages.Count);
			AssertOdplUsage(allOdplUsages, chargeableUsage1, chargeableUsage2, chargeableUsage3);
			AssertOdplUsage(allOdplUsages, chargeableUsage4);
			AssertOdplUsage(allOdplUsages, chargeableUsage5, chargeableUsage6);
		}

		public void TestLoadUsages_PerDatabaseCharges()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 01);

			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA2");
			LicenceHeader licHeader1c = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA3");
			LicenceHeader licHeader2 = BillingTestHelper.CreateLicence(Factory, "ZB1");

			licHeader1c.LA_ContractExpiryDate = ZDateTime.Now.AddMonths(6);

			var lang1 = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.French];
			ZString lang1Code = lang1.Name;
			ZString lang1ChildCode = lang1.DocBuilderLanguageCheckpoint.Name;
			ZString lang2Code = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.German].Name;

			var prices1a = BillingTestHelper.CreatePriceList(licHeader1a.Company);
			var prices1b = BillingTestHelper.CreatePriceList(licHeader1b.Company);
			var prices2 = BillingTestHelper.CreatePriceList(licHeader2.Company);
			BillingTestHelper.AddPriceItem(prices1a, lang1Code, BillingConstants.FeeType.Database, "", 23m);
			BillingTestHelper.AddPriceItem(prices1a, lang1ChildCode, BillingConstants.FeeType.Included, lang1Code, 0m);
			BillingTestHelper.AddPriceItem(prices1a, lang2Code, BillingConstants.FeeType.Database, "", 16m);
			BillingTestHelper.AddPriceItem(prices1b, lang1Code, BillingConstants.FeeType.Database, "", 23m);
			BillingTestHelper.AddPriceItem(prices1b, lang2Code, BillingConstants.FeeType.Database, "", 16m);
			BillingTestHelper.AddPriceItem(prices1b, lang1ChildCode, BillingConstants.FeeType.Included, lang1Code, 0m);
			BillingTestHelper.AddPriceItem(prices2, lang1Code, BillingConstants.FeeType.Database, "", 37m);
			BillingTestHelper.AddPriceItem(prices2, lang2Code, BillingConstants.FeeType.Database, "", 19m);

			// lic1a and lic1b uses lang1, lic1b and lic2 uses lang 2
			ClientChargeableUsage chargeableUsage1aLang1 = CreateChargeableUsage(licHeader1a, periodStart, lang1Code, 10);
			ClientChargeableUsage chargeableUsage1bLang1 = CreateChargeableUsage(licHeader1b, periodStart, lang1Code, 11);
			ClientChargeableUsage chargeableUsage1bLang2 = CreateChargeableUsage(licHeader1b, periodStart, lang2Code, 30);

			ClientChargeableUsage chargeableUsage2Lang2 = CreateChargeableUsage(licHeader2, periodStart, lang2Code, 11);

			ClientChargeableUsage chargeableUsage1aCore = CreateChargeableUsage(licHeader1a, periodStart, coreModule, 22);
			ClientChargeableUsage chargeableUsage2Core = CreateChargeableUsage(licHeader2, periodStart, coreModule, 24);

			ClientChargeableUsage chargeableUsage1aLang1child = CreateChargeableUsage(licHeader1a, periodStart, lang1ChildCode, 10);
			ClientChargeableUsage chargeableUsage1bLang1Child = CreateChargeableUsage(licHeader1b, periodStart, lang1ChildCode, 11);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			List<OdplUsage> allOdplUsages = new List<OdplUsage>();
			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			foreach (SystemBill bill in systemBills)
			{
				allOdplUsages.AddRange(bill.SystemUsages.Cast<OdplUsage>());
			}

			AssertEquals("3 usages created", 3, allOdplUsages.Count);
			AssertOdplUsagePks(allOdplUsages, chargeableUsage1aCore, chargeableUsage1aLang1, chargeableUsage1bLang1, chargeableUsage1aLang1child, chargeableUsage1bLang1Child);
			AssertOdplUsagePks(allOdplUsages, chargeableUsage1bLang2);
			AssertOdplUsagePks(allOdplUsages, chargeableUsage2Core, chargeableUsage2Lang2);

			var bill1a = systemBills.First(s => s.OrganisationPK == licHeader1a.Company.Header.PK);
			var bill1b = systemBills.First(s => s.OrganisationPK == licHeader1b.Company.Header.PK);
			var bill2 = systemBills.First(s => s.OrganisationPK == licHeader2.Company.Header.PK);

			AssertEquals("first using company code pays for lang1 (plus their core use)", 23m + 22 * 3m, bill1a.Amount);
			AssertEquals("first using company code pays for lang2 (plus their core use)", 16m, bill1b.Amount);
			AssertEquals("first using company code pays for lang2 (plus their core use)", 19m + 24 * 3m, bill2.Amount);

			licHeader1a.Database.LD_OH_BillingParty = licHeader1a.Company.LC_OH;
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2010, 10, 31));
			systemBills = odplBilling.LoadSystemBills(context);
			bill1a = systemBills.First(s => s.OrganisationPK == licHeader1a.Company.Header.PK);
			bill1b = systemBills.FirstOrDefault(s => s.OrganisationPK == licHeader1b.Company.Header.PK);
			AssertEquals("billing party pays for all lang", 23m + 16m + 22 * 3m, bill1a.Amount);
			AssertNull("licence2a has no billable usage", bill1b);

			// earliest site live takes precedence
			licHeader1a.Database.LD_OH_BillingParty = ZGuid.Empty;
			licHeader1b.LA_AgreedLiveDate = new ZDateTime(2010, 1, 1);
			licHeader1a.LA_AgreedLiveDate = new ZDateTime(2010, 1, 2);
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));
			systemBills = odplBilling.LoadSystemBills(context);
			bill1a = systemBills.First(s => s.OrganisationPK == licHeader1a.Company.Header.PK);
			bill1b = systemBills.First(s => s.OrganisationPK == licHeader1b.Company.Header.PK);
			AssertEquals("later live company doesn't pay for lang (just their core use)", 22 * 3m, bill1a.Amount);
			AssertEquals("first live company code pays for all lang (plus their core use)", 23m + 16m, bill1b.Amount);

			// maintenance billing takes preference
			var lang1c = licHeader1c.Modules.FindByCode(lang1Code);
			foreach (string licenceType in new string[] { LicenceTypes.Codes.PUR, LicenceTypes.Codes.ODM, LicenceTypes.Codes.SRU, LicenceTypes.Codes.OPN, LicenceTypes.Codes.OTM })
			{
				lang1c.LM_LicenceType = licenceType;
				lang1c.LM_UserCount = 1;
				Factory.Save();
				odplBilling = new OdplBillingSystem();
				context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2010, 10, 31));
				systemBills = odplBilling.LoadSystemBills(context);
				bill1a = systemBills.First(s => s.OrganisationPK == licHeader1a.Company.Header.PK);
				bill1b = systemBills.First(s => s.OrganisationPK == licHeader1b.Company.Header.PK);
				AssertEquals(licenceType + " other companies pay", 22 * 3m, bill1a.Amount);
				AssertEquals(licenceType + " ODPL company doesn't pay lang1 as usage, the module is paid as maintenance", 16m, bill1b.Amount);
			}

			lang1c.LM_LicenceType = LicenceTypes.Codes.PUR;
			lang1c.LM_ExpiryDate = new ZDateTime(2010, 9, 3);
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2010, 10, 31));
			systemBills = odplBilling.LoadSystemBills(context);
			bill1a = systemBills.First(s => s.OrganisationPK == licHeader1a.Company.Header.PK);
			bill1b = systemBills.First(s => s.OrganisationPK == licHeader1b.Company.Header.PK);
			AssertEquals("other companies pay", 22 * 3m, bill1a.Amount);
			AssertEquals("ODPL company pays as seat has expired", 23m + 16m, bill1b.Amount);
		}

		public void TestCreateSystemUsages_PurchasedSeats()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 1);

			// A has usage. B and C have seats and no usage.
			// C has expired contract.
			// B has expired contract, but recent enough to be included.
			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA2");
			LicenceHeader licHeader1c = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA3");
			licHeader1b.CoreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			licHeader1b.CoreModule.LM_UserCount = 10;
			licHeader1c.CoreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			licHeader1c.CoreModule.LM_UserCount = 10;

			licHeader1b.LA_ContractExpiryDate = periodStart.AddMonths(-3);
			licHeader1c.LA_ContractExpiryDate = periodStart.AddMonths(-5);

			var prices1a = BillingTestHelper.CreatePriceList(licHeader1a.Company);
			BillingTestHelper.SetInvoicing(licHeader1a, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(licHeader1b, licHeader1a);
			BillingTestHelper.SetInvoicingTo(licHeader1c, licHeader1a);

			ClientChargeableUsage chargeableUsage1 = CreateChargeableUsage(licHeader1a, periodStart, BillingConstants.CoreModuleCode, 10);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("bills", 1, systemBills.Length);
			AssertNotNull("A is billed", systemBills.FirstOrDefault(s => s.OrganisationPK == licHeader1a.Company.Header.PK));

			List<OdplUsage> allOdplUsages = new List<OdplUsage>(systemBills[0].SystemUsages.Cast<OdplUsage>());
			AssertEquals("A usage created", 1, allOdplUsages.Count);
			var odplUsage1a = allOdplUsages.First(x => x.OrganisationPK == licHeader1a.Company.LC_OH);
		}

		public void TestCreateSystemUsages_PurchasedLicenceUnits()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 1);

			LicenceHeader licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			licHeader1a.Database.LD_PurchasedLicenceUnits = 1000;
			LicenceHeader licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA2");

			var prices1a = BillingTestHelper.CreatePriceList(licHeader1a.Company);
			prices1a.L6_LicenceUnitRate = 5;
			BillingTestHelper.SetInvoicing(licHeader1a, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(licHeader1b, licHeader1a);

			CreateChargeableUsage(licHeader1a, periodStart, BillingConstants.CoreModuleCode, 10);
			CreateChargeableUsage(licHeader1b, periodStart, BillingConstants.CoreModuleCode, 20);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			OdplSystemBill bill = (OdplSystemBill)odplBilling.LoadSystemBills(context)[0];
			AssertEquals(1000, bill.PurchasedLicenceUnits);
			AssertEquals(5m, bill.LicenceUnitRate);
		}

		public void TestCreateSystemUsages_DomesticDiscount()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 01);

			// Single Company
			LicenceHeader licHeader1 = BillingTestHelper.CreateLicence(Factory, "C01");
			licHeader1.Company.LC_CompanyCountry = "FR";
			licHeader1.ClientCompany.LCC_RN_NKCountryCode = "FR";
			LicenceHeader licHeader2 = BillingTestHelper.CreateLicence(Factory, "C02");
			licHeader2.CoreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			licHeader2.CoreModule.LM_UserCount = 7;
			LicenceHeader licChina1 = BillingTestHelper.CreateLicence(Factory, "C03");
			licChina1.Company.LC_CompanyCountry = "CN";
			licChina1.ClientCompany.LCC_RN_NKCountryCode = "CN";
			LicenceHeader licIndia1 = BillingTestHelper.CreateLicence(Factory, "C04");
			licIndia1.Company.LC_CompanyCountry = "IN";
			licIndia1.ClientCompany.LCC_RN_NKCountryCode = "IN";
			LicenceHeader licChina2 = BillingTestHelper.CreateLicence(Factory, "C05");
			licChina2.Company.LC_CompanyCountry = "CN";
			licChina2.ClientCompany.LCC_RN_NKCountryCode = "CN";
			LicenceHeader licIndia2 = BillingTestHelper.CreateLicence(Factory, "C06");
			licIndia2.Company.LC_CompanyCountry = "IN";
			licIndia2.ClientCompany.LCC_RN_NKCountryCode = "IN";
			ClientChargeableUsage chargeableUsage1 = CreateChargeableUsage(licHeader1, periodStart, coreModule, 15);
			ClientChargeableUsage chargeableUsage2 = CreateChargeableUsage(licHeader2, periodStart, coreModule, 25);
			ClientChargeableUsage chargeChina1 = CreateChargeableUsage(licChina1, periodStart, coreModule, 15);
			ClientChargeableUsage chargeIndia1 = CreateChargeableUsage(licIndia1, periodStart, coreModule, 15);
			ClientChargeableUsage chargeChina2 = CreateChargeableUsage(licChina2, periodStart, coreModule, 25);
			ClientChargeableUsage chargeIndia2 = CreateChargeableUsage(licIndia2, periodStart, coreModule, 25);
			ClientChargeableUsage chargeableUsage1LocalLang = CreateChargeableUsage(licHeader1, periodStart, frenchGuiModule, 9);
			ClientChargeableUsage chargeableUsage1OtherLang = CreateChargeableUsage(licHeader1, periodStart, germanGuiModule, 3);
			ClientChargeableUsage chargeChinal1LocalLang = CreateChargeableUsage(licChina1, periodStart, chinaSimplifiedGuiModule, 11);
			ClientChargeableUsage chargeChinal1LocalLang2 = CreateChargeableUsage(licChina1, periodStart, chinaTraditionalGuiModule, 17);
			ClientChargeableUsage chargeChinal1OtherLang = CreateChargeableUsage(licChina1, periodStart, frenchGuiModule, 12);
			var prices1 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader1);
			var prices2 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader2);
			var prices3 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licChina1);
			var prices4 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licIndia1);
			var prices5 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licChina2);
			var prices6 = CreatePriceListWithCoreDiscountsAndLanguagePacks(licIndia2);

			// Multiple Companies
			LicenceHeader licHeader7A = BillingTestHelper.CreateLicence(Factory, "C7A");
			licHeader7A.Company.LC_CompanyCountry = "FR";
			licHeader7A.ClientCompany.LCC_RN_NKCountryCode = "FR";
			LicenceHeader licHeader7B = BillingTestHelper.CreateAnotherLicence(licHeader7A, "C7B");
			licHeader7B.Company.LC_CompanyCountry = "FR";
			licHeader7B.ClientCompany.LCC_RN_NKCountryCode = "FR";
			LicenceHeader licHeader8A = BillingTestHelper.CreateLicence(Factory, "C8A");
			licHeader8A.Company.LC_CompanyCountry = "IN";
			licHeader8A.ClientCompany.LCC_RN_NKCountryCode = "IN";
			LicenceHeader licHeader8B = BillingTestHelper.CreateAnotherLicence(licHeader8A, "C8B");
			licHeader8A.Company.LC_CompanyCountry = "CN";
			licHeader8B.ClientCompany.LCC_RN_NKCountryCode = "CN";

			// Country group
			LicenceHeader lic1Group = BillingTestHelper.CreateLicence(Factory, "CG1");
			LicenceHeader lic2Group = BillingTestHelper.CreateAnotherLicence(lic1Group, "CG2");
			lic1Group.Company.LC_CompanyCountry = "CN";
			lic1Group.ClientCompany.LCC_RN_NKCountryCode = "CN";
			lic2Group.Company.LC_CompanyCountry = "MO";
			lic2Group.ClientCompany.LCC_RN_NKCountryCode = "MO";
			CreateChargeableUsage(lic1Group, periodStart, coreModule, 10);
			CreateChargeableUsage(lic2Group, periodStart, coreModule, 2);
			var prices1Group = CreatePriceListWithCoreDiscountsAndLanguagePacks(lic1Group);
			var prices2Group = CreatePriceListWithCoreDiscountsAndLanguagePacks(lic2Group);

			// Multiple Companies: adding different company of same enterprise on different database
			LicenceDatabase licDatabase7C = licHeader7A.Company.LicEnterprise.Databases.AddNew();
			licDatabase7C.LD_ServerCode = "S7C";
			licDatabase7C.LD_LicenceType = DatabaseTypes.Codes.Production;
			LicenceHeader licHeader7C = BillingTestHelper.CreateAnotherLicence(licDatabase7C, "C7C");
			licHeader7C.Company.LC_CompanyCountry = "NZ";
			licHeader7C.ClientCompany.LCC_RN_NKCountryCode = "NZ";

			ClientChargeableUsage chargeableUsage7A = CreateChargeableUsage(licHeader7A, periodStart, coreModule, 25);
			ClientChargeableUsage chargeableUsage7ALocalLang = CreateChargeableUsage(licHeader7A, periodStart, frenchGuiModule, 9);
			ClientChargeableUsage chargeableUsage7AOtherLang = CreateChargeableUsage(licHeader7A, periodStart, germanGuiModule, 3);
			ClientChargeableUsage chargeableUsage7B = CreateChargeableUsage(licHeader7B, periodStart, coreModule, 25);
			ClientChargeableUsage chargeableUsage7BLocalLang = CreateChargeableUsage(licHeader7B, periodStart, frenchGuiModule, 9);
			ClientChargeableUsage chargeableUsage7BOtherLang = CreateChargeableUsage(licHeader7B, periodStart, germanGuiModule, 3);
			ClientChargeableUsage chargeableUsage7C = CreateChargeableUsage(licHeader7C, periodStart, coreModule, 25);
			ClientChargeableUsage chargeableUsage8A = CreateChargeableUsage(licHeader8A, periodStart, coreModule, 15);
			ClientChargeableUsage chargeableUsage8B = CreateChargeableUsage(licHeader8B, periodStart, coreModule, 10);
			var prices7A = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader7A);
			var prices7B = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader7B);
			var prices8A = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader8A);
			var prices8B = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader8B);

			//USR
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader2.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licChina1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licIndia1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licChina2.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licIndia2.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader7A.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader7B.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader8A.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader8B.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1Group.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic2Group.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licHeader7C.ClientCompany, 1);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			List<OdplUsage> allOdplUsages = new List<OdplUsage>();
			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			foreach (SystemBill bill in systemBills)
			{
				allOdplUsages.AddRange(bill.SystemUsages.Cast<OdplUsage>());
			}

			// Discount DS1: 1 company with <20 users
			var odpUsages1 = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader1.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages1.Count);
			var moduleUsages = odpUsages1.First().ModuleUsages.Cast<OdplModuleUsage>().ToList();
			AssertEquals("modules usage count", 3, moduleUsages.Count);
			AssertEquals("COR module usage created", true, moduleUsages.Any(x => x.ModuleCode == "COR"));
			AssertEquals("DS1 module usage created", true, moduleUsages.Any(x => x.ModuleCode == BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users));
			AssertEquals("foreign lang module usage created", true, moduleUsages.Any(x => x.ModuleCode == germanGuiModule));
			var bill1 = systemBills.First(s => s.OrganisationPK == licHeader1.Company.Header.PK);
			AssertEquals("COR + DS1", 15 * 29m - 15 * 1m + 300m, bill1.Amount);

			// Discount DS2: 1 company with >=20 users
			var odpUsages2 = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader2.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages2.Count);
			AssertEquals("2 modules usages created", 2, odpUsages2.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", odpUsages2.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DS2 module usage created", BillingConstants.CoreDiscount.SingleEntityDomestic20PlusUsers, odpUsages2.First().ModuleUsages[1].ModuleCode);
			var bill2 = systemBills.First(s => s.OrganisationPK == licHeader2.Company.Header.PK);
			AssertEquals("COR + DS1", (25 - 7) * (29m - 2m), bill2.Amount);

			// Discount DS1 and DDC: 1 company with <20 users and Country=China
			var odpUsages3 = allOdplUsages.FindAll(s => s.OrganisationPK == licChina1.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages3.Count);
			moduleUsages = odpUsages3.First().ModuleUsages.Cast<OdplModuleUsage>().ToList();
			AssertEquals("modules usages created", 4, moduleUsages.Count);
			AssertEquals("COR module usage created", true, moduleUsages.Any(x => x.ModuleCode == "COR"));
			AssertEquals("DS1 module usage created", true, moduleUsages.Any(x => x.ModuleCode == BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users));
			AssertEquals("DS1 module usage created", true, moduleUsages.Any(x => x.ModuleCode == BillingConstants.CoreDiscount.ChinaDomesticUnder20Users));
			AssertEquals("other language module usage created", true, moduleUsages.Any(x => x.ModuleCode == frenchGuiModule));
			var bill3 = systemBills.First(s => s.OrganisationPK == licChina1.Company.Header.PK);
			AssertEquals("COR + DS1 + DDC + other lang", 15 * 29m - 15 * 1m - 15 * 3m + 900m, bill3.Amount);

			// Discount DS1 and DDI: 1 company with <20 users and Country=India
			var odpUsages4 = allOdplUsages.FindAll(s => s.OrganisationPK == licIndia1.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages4.Count);
			AssertEquals("3 modules usages created", 3, odpUsages4.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", odpUsages4.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DS1 module usage created", BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users, odpUsages4.First().ModuleUsages[1].ModuleCode);
			AssertEquals("DS1 module usage created", BillingConstants.CoreDiscount.IndiaDomesticUnder20Users, odpUsages4.First().ModuleUsages[2].ModuleCode);
			var bill4 = systemBills.First(s => s.OrganisationPK == licIndia1.Company.Header.PK);
			AssertEquals("COR + DS1 + DDI", 15 * 29m - 15 * 1m - 15 * 4m, bill4.Amount);

			// Discount DS2: 1 company with >20 users and Country=China
			var odpUsages5 = allOdplUsages.FindAll(s => s.OrganisationPK == licChina2.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages5.Count);
			AssertEquals("2 modules usages created", 2, odpUsages5.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", odpUsages5.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DS2 module usage created", BillingConstants.CoreDiscount.SingleEntityDomestic20PlusUsers, odpUsages5.First().ModuleUsages[1].ModuleCode);
			var bill5 = systemBills.First(s => s.OrganisationPK == licChina2.Company.Header.PK);
			AssertEquals("COR + DS2", 25 * 29m - 25 * 2m, bill5.Amount);

			// Discount DS2: 1 company with >20 users and Country=India
			var odpUsages6 = allOdplUsages.FindAll(s => s.OrganisationPK == licIndia2.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages6.Count);
			AssertEquals("2 modules usages created", 2, odpUsages6.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", odpUsages6.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DS2 module usage created", BillingConstants.CoreDiscount.SingleEntityDomestic20PlusUsers, odpUsages6.First().ModuleUsages[1].ModuleCode);
			var bill6 = systemBills.First(s => s.OrganisationPK == licIndia2.Company.Header.PK);
			AssertEquals("COR + DS2", 25 * 29m - 25 * 2m, bill6.Amount);

			// Discount DM1: 2 companies of same country
			var odpUsages7A = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader7A.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages7A.Count);
			moduleUsages = odpUsages7A.First().ModuleUsages.Cast<OdplModuleUsage>().ToList();
			AssertEquals("modules usages created", 3, moduleUsages.Count);
			AssertEquals("COR module usage created", true, moduleUsages.Any(x => x.ModuleCode == "COR"));
			AssertEquals("DM1 module usage created", true, moduleUsages.Any(x => x.ModuleCode == "DM1"));
			AssertEquals("other language module usage created", true, moduleUsages.Any(x => x.ModuleCode == germanGuiModule));
			var bill7A = systemBills.First(s => s.OrganisationPK == licHeader7A.Company.Header.PK);
			AssertEquals("COR + DM1", 25 * 29m - 25 * 0.5m + 300m, bill7A.Amount);
			var odpUsages7B = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader7B.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages7B.Count);
			AssertEquals("2 modules usages created", 2, odpUsages7B.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", odpUsages7B.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DM1 module usage created", "DM1", odpUsages7B.First().ModuleUsages[1].ModuleCode);
			var bill7B = systemBills.First(s => s.OrganisationPK == licHeader7B.Company.Header.PK);
			AssertEquals("COR + DM1", 25 * 29m - 25 * 0.5m, bill7B.Amount);

			// NO Discount: 2 companies of different country
			var odpUsages8A = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader8A.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages8A.Count);
			AssertEquals("1 modules usages created", 1, odpUsages8A.First().ModuleUsages.Count);
			var bill8A = systemBills.First(s => s.OrganisationPK == licHeader8A.Company.Header.PK);
			AssertEquals("COR", 15 * 29m, bill8A.Amount);
			var odpUsages8B = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader8B.Company.Header.PK);
			AssertEquals("1 usage created", 1, odpUsages8B.Count);
			AssertEquals("1 modules usages created", 1, odpUsages8B.First().ModuleUsages.Count);
			var bill8B = systemBills.First(s => s.OrganisationPK == licHeader8B.Company.Header.PK);
			AssertEquals("COR", 10 * 29m, bill8B.Amount);

			// Discount DM1: 2 companies of same country group
			var usage1Group = allOdplUsages.FindAll(s => s.OrganisationPK == lic1Group.Company.Header.PK);
			AssertEquals("1 usage created", 1, usage1Group.Count);
			AssertEquals("2 modules usages created", 2, usage1Group.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", usage1Group.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DM1 module usage created", "DM1", usage1Group.First().ModuleUsages[1].ModuleCode);
			var bill1Group = systemBills.First(s => s.OrganisationPK == lic1Group.Company.Header.PK);
			AssertEquals("COR + DM1", 10 * 29m - 10 * 0.5m, bill1Group.Amount);
			var usage2Group = allOdplUsages.FindAll(s => s.OrganisationPK == lic2Group.Company.Header.PK);
			AssertEquals("1 usage created", 1, usage2Group.Count);
			AssertEquals("2 modules usages created", 2, usage2Group.First().ModuleUsages.Count);
			AssertEquals("COR module usage created", "COR", usage2Group.First().ModuleUsages[0].ModuleCode);
			AssertEquals("DM1 module usage created", "DM1", usage2Group.First().ModuleUsages[1].ModuleCode);
			var bill2Group = systemBills.First(s => s.OrganisationPK == lic2Group.Company.Header.PK);
			AssertEquals("COR + DM1", 2 * 29m - 2 * 0.5m, bill2Group.Amount);
		}

		public void TestCreateSystemUsages_DomesticDiscountForMultiCompanyInDifferentCountry()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 01);

			// Multiple Companies in different countries - Should not have domestic discount
			LicenceHeader licHeaderA = BillingTestHelper.CreateLicence(Factory, "C8A");
			licHeaderA.Company.LC_CompanyCountry = "IN";
			LicenceHeader licHeaderB = BillingTestHelper.CreateAnotherLicence(licHeaderA, "C8B");
			licHeaderB.Company.LC_CompanyCountry = "CN";

			ClientChargeableUsage chargeableUsageA = CreateChargeableUsage(licHeaderA, periodStart, coreModule, 15);
			ClientChargeableUsage chargeableUsageB = CreateChargeableUsage(licHeaderB, periodStart, coreModule, 10);
			CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeaderA);
			CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeaderB);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31), licHeaderA.Company.Header.PK);
			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);

			var billA = systemBills.First(s => s.OrganisationPK == licHeaderA.Company.Header.PK);
			AssertContainsExactElementsInAnyOrder("Precondition: No discount", new ZString[] { coreModule }, billA.SystemUsages.Cast<OdplUsage>().SelectMany(x => x.ModuleUsages).Cast<OdplModuleUsage>().Select(x => x.ModuleCode));
			AssertEquals(15 * 29m, billA.Amount);
			var billB = systemBills.First(s => s.OrganisationPK == licHeaderB.Company.Header.PK);
			AssertContainsExactElementsInAnyOrder("Precondition: No discount", new ZString[] { coreModule }, billB.SystemUsages.Cast<OdplUsage>().SelectMany(x => x.ModuleUsages).Cast<OdplModuleUsage>().Select(x => x.ModuleCode));
			AssertEquals(10 * 29m, billB.Amount);
		}

		public void TestCreateSystemUsages_DomesticDiscount_MultipleCountriesClientCompanies()
		{
			var periodStart = new ZDateTime(2010, 10, 01);

			// Single Company, Multiple Client Companies
			var licHeader = BillingTestHelper.CreateLicence(Factory, "C01");
			licHeader.Company.LC_CompanyCountry = "US";

			var database = licHeader.Database;

			var clientCompany1 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany1.LCC_LD = database.PK;
			clientCompany1.LCC_Code = "NYK";
			clientCompany1.LCC_RN_NKCountryCode = "US";

			var clientCompany2 = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany2.LCC_LD = database.PK;
			clientCompany2.LCC_Code = "SYD";
			clientCompany2.LCC_RN_NKCountryCode = "AU";

			ClientChargeableUsage chargeableUsage1 = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", periodStart, clientCompany1, 15);
			chargeableUsage1.U1_LC = licHeader.Company.PK;

			ClientChargeableUsage chargeableUsage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", periodStart, clientCompany2, 25);
			chargeableUsage2.U1_LC = licHeader.Company.PK;

			var prices = CreatePriceListWithCoreDiscountsAndLanguagePacks(licHeader);

			Factory.Save();

			var odplBilling = new OdplBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			var allOdplUsages = new List<OdplUsage>();
			var systemBills = odplBilling.LoadSystemBills(context);
			foreach (SystemBill bill in systemBills)
			{
				allOdplUsages.AddRange(bill.SystemUsages.Cast<OdplUsage>());
			}

			// NO Discount: 2 client companies of different country
			var odpUsages = allOdplUsages.FindAll(s => s.OrganisationPK == licHeader.Company.Header.PK);
			AssertEquals("2 usage created", 2, odpUsages.Count);
			AssertEquals("1 modules usage created", 1, odpUsages.First().ModuleUsages.Count);

			var systemBill = systemBills.First(s => s.OrganisationPK == licHeader.Company.Header.PK);
			AssertEquals("COR", (15 + 25) * 29m, systemBill.Amount);
		}

		ClientLicencePriceHeader CreatePriceListWithCoreDiscountsAndLanguagePacks(LicenceHeader lic)
		{
			var prices = BillingTestHelper.CreatePriceList(lic);
			prices.Items.FindByCode("COR").L7_Price = 29;
			BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users, BillingConstants.FeeType.CoreUsers, "", -1m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreDiscount.SingleEntityDomestic20PlusUsers, BillingConstants.FeeType.CoreUsers, "", -2m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreDiscount.MultiEntityDomestic, BillingConstants.FeeType.CoreUsers, "", -0.5m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreDiscount.ChinaDomesticUnder20Users, BillingConstants.FeeType.CoreUsers, "", -3m);
			BillingTestHelper.AddPriceItem(prices, BillingConstants.CoreDiscount.IndiaDomesticUnder20Users, BillingConstants.FeeType.CoreUsers, "", -4m);

			var fR = BillingTestHelper.AddPriceItem(prices, frenchGuiModule, BillingConstants.FeeType.DatabaseLanguage, "", 900m);
			fR.L7_Language = Core.SharedConstants.Languages.French;
			var dE = BillingTestHelper.AddPriceItem(prices, germanGuiModule, BillingConstants.FeeType.DatabaseLanguage, "", 300m);
			dE.L7_Language = Core.SharedConstants.Languages.German;
			var zH = BillingTestHelper.AddPriceItem(prices, chinaSimplifiedGuiModule, BillingConstants.FeeType.DatabaseLanguageZ, "", 700m);
			zH.L7_Language = Core.SharedConstants.Languages.ChineseSimplified;
			var zT = BillingTestHelper.AddPriceItem(prices, chinaTraditionalGuiModule, BillingConstants.FeeType.DatabaseLanguageZ, "", 500m);
			zT.L7_Language = Core.SharedConstants.Languages.ChineseTraditional;

			return prices;
		}

		public void TestCreateSystemUsages_DomesticDiscountWithActiveUsers()
		{
			EServicesBillingTestHelper.CreateTable();

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdPriceLicence = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdPriceLicence);

			var stdPriceHeader = stdPriceLicence.Company.PriceHeaders.AddNew();
			stdPriceHeader.L6_SystemCode = "ODM";
			stdPriceHeader.L6_RX_NKCurrency = "AUD";
			stdPriceHeader.L6_PricelistVersion = "CW1 v8.1";
			stdPriceHeader.L6_IsStandard = false;
			stdPriceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var licence = BillingTestHelper.CreateLicence(Factory, "EEE", "SYD", "HST");
			var org = licence.Company.Header;
			licence.LA_AgreedLiveDate = licence.LA_AgreedLiveDate.AddYears(-1);
			var db = licence.Database;
			db.LD_DatabaseNumber = 2847;
			db.LD_Product = ProductTypes.Codes.CargoWiseOne;
			BillingTestHelper.SetInvoicingTo(licence, org.LicCompany);

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			clientCompany1.LCC_RN_NKCountryCode = "AU";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";
			clientCompany2.LCC_RN_NKCountryCode = "NZ";

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_PricelistVersion = "CW1 v8.1";
			priceHeader.L6_IsStandard = true;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 4, 1);

			BillingTestHelper.AddPriceItem(stdPriceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(stdPriceHeader, BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users, BillingConstants.FeeType.CoreUsers, "", -7m);

			var periodStart = new ZDateTime(2016, 11, 1);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany1, 2);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.RegisteredUserModuleCode, periodStart, clientCompany1, 5);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.RegisteredUserModuleCode, periodStart, clientCompany2, 1);

			Factory.Save();

			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var odplBilling = new OdplBillingSystem();
			var systemBill = odplBilling.LoadSystemBills(context)[0];
			AssertEquals(1, systemBill.SystemUsages.Count);
			var odpUsages = (OdplUsage)systemBill.SystemUsages[0];
			var moduleUsages = odpUsages.ModuleUsages.Cast<OdplModuleUsage>().ToList();
			AssertEquals("modules usage count", 2, moduleUsages.Count);
			AssertEquals("COR module usage created", true, moduleUsages.Any(x => x.ModuleCode == "COR"));
			AssertEquals("DS1 module usage created", true, moduleUsages.Any(x => x.ModuleCode == BillingConstants.CoreDiscount.SingleEntityDomesticUnder20Users));
			AssertEquals("COR + DS1", 2 * 10m - 2 * 7m, systemBill.Amount);
		}

		public void TestCommitmentDiscountAppliedIfNoUsage()
		{
			ZDateTime periodStart = new ZDateTime(2010, 10, 1);

			LicenceHeader licHeader1 = BillingTestHelper.CreateLicence(Factory, "C01");
			BillingTestHelper.SetInvoicing(licHeader1, Env.CurrentBranch.PK);
			var discount1 = licHeader1.Company.SelfBilling.BillingDiscounts.AddNew();
			discount1.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount1.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount1.L5_Type = BillingConstants.DiscountType.Commitment;
			discount1.L5_BreakAmount = 100m;
			discount1.L5_Discount = 20m;

			LicenceHeader licHeader2 = BillingTestHelper.CreateLicence(Factory, "C02");
			var discount2 = licHeader2.Company.SelfBilling.BillingDiscounts.AddNew();
			discount2.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount2.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount2.L5_Type = BillingConstants.DiscountType.Commitment;
			discount2.L5_BreakAmount = 200m;
			discount2.L5_Discount = 25m;
			discount2.L5_EndDate = periodStart.AddDays(-1);

			LicenceHeader licHeader3 = BillingTestHelper.CreateLicence(Factory, "C03");
			var discount3 = licHeader3.Company.SelfBilling.BillingDiscounts.AddNew();
			discount3.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount3.L5_BreakUnits = BillingConstants.DiscountBreakUnit.LicenceUnits;
			discount3.L5_Type = BillingConstants.DiscountType.Commitment;
			discount3.L5_BreakAmount = 300m;
			discount3.L5_Discount = 10m;
			discount3.L5_StartDate = periodStart;

			var prices1 = BillingTestHelper.CreatePriceList(licHeader1.Company);
			var prices2 = BillingTestHelper.CreatePriceList(licHeader2.Company);
			var prices3 = BillingTestHelper.CreatePriceList(licHeader3.Company);
			prices3.L6_LicenceUnitRate = 3;
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("bill count", 2, systemBills.Length);
			var bill1 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader1.Company.LC_OH);
			var bill3 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader3.Company.LC_OH);
			AssertEquals("Organisation", licHeader1.Company.LC_OH, bill1.OrganisationPK);
			AssertEquals("Amount", 100m, bill1.Amount);
			AssertEquals("DiscountAmount", 20m, bill1.DiscountAmount);

			AssertEquals("Organisation", licHeader3.Company.LC_OH, bill3.OrganisationPK);
			AssertEquals("Amount", 300m * 3, bill3.Amount);
			AssertEquals("DiscountAmount", 300m * 3 * 0.1m, bill3.DiscountAmount);

			// Context with an Org filter
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2010, 10, 31), licHeader2.Company.LC_OH);
			systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("no bills since Org filter does not include org1", 0, systemBills.Length);

			// Period with discount2 valid, discount3 not valid
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2009, 10, 31));
			systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("bill count", 2, systemBills.Length);
			bill1 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader1.Company.LC_OH);
			var bill2 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader2.Company.LC_OH);
			AssertEquals("DiscountAmount", 200m, bill2.Amount);
			AssertEquals("DiscountAmount", 200m * 0.25m, bill2.DiscountAmount);

			BusinessObjectFactory invoiceFactory = new BusinessObjectFactory();
			ARInvoice invoice = invoiceFactory.NewWithValidTestData<ARInvoice>();
			bill1.OnInvoiceFactorySaving(invoice);
			invoiceFactory.Save();

			ZQuery chargeableUsageQuery = new ZQuery(ClientChargeableUsageSchema.U1_Code, odplBilling.SystemCode);
			var chargeableUsages = Factory.Load<ClientChargeableUsage>(chargeableUsageQuery);
			AssertEquals("Chargeable usages created", 1, chargeableUsages.Length);
			ClientChargeableUsage chargeableUsage = chargeableUsages[0];
			AssertEquals(odplBilling.SystemCode, chargeableUsage.U1_Code);
			AssertEquals(new ZDateTime(2009, 10, 1), chargeableUsage.U1_PeriodStart);
			AssertEquals(0m, chargeableUsage.U1_UnitPrice);
			AssertEquals(1m, chargeableUsage.U1_UnitCount);
			AssertEquals(invoice.PK, chargeableUsage.U1_AH_Invoice);
			AssertEquals(licHeader1.LA_LC, chargeableUsage.U1_LC);

			// run again to verify ChargeableUsagePKs are loaded
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(new BusinessObjectFactory(), ZDateTime.Today, new ZDateTime(2009, 10, 31));
			systemBills = odplBilling.LoadSystemBills(context);
			bill1 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader1.Company.LC_OH);
			bill2 = systemBills.Cast<OdplSystemBill>().First(x => x.OrganisationPK == licHeader2.Company.LC_OH);
			AssertEquals(1, bill1.SystemUsages[0].ChargeableUsagePKs.Count);
			AssertEquals(0, bill2.SystemUsages[0].ChargeableUsagePKs.Count);
			AssertEquals(chargeableUsage.PK, bill1.SystemUsages[0].ChargeableUsagePKs[0]);
			bill1.ValidateAll(bill1);
		}

		public void TestCreateSystemUsages_ParentChild()
		{
			var org = organisation1;
			var licHeader = org.LicCompany.LicHeadersForAllDatabases[0];
			var modules = licHeader.Modules;
			licHeader.CoreModule.LM_LicenceType = LicenceTypes.Codes.ODM;
			licHeader.CoreModule.LM_UserCount = 7;
			var lic = Enterprise.Environment.Env.Licence;
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);

			BillingTestHelper.AddPriceItem(priceHeader, "AAA", BillingConstants.FeeType.NamedUser, "", 2m);
			BillingTestHelper.AddPriceItem(priceHeader, "AA1", BillingConstants.FeeType.Included, "AAA", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, "AA2", BillingConstants.FeeType.Included, "AAA", 7m);
			BillingTestHelper.AddPriceItem(priceHeader, "BBB", BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "CCC", BillingConstants.FeeType.NamedUser, "", 20m);
			BillingTestHelper.AddPriceItem(priceHeader, "CC1", BillingConstants.FeeType.Included, "CCC", 30m);

			BillingTestHelper.AddPriceItem(priceHeader, "DDD", BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "DD1", BillingConstants.FeeType.Module, "", 10m, "DDD");
			BillingTestHelper.AddPriceItem(priceHeader, "EEE", BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "EE1", BillingConstants.FeeType.Module, "", 10m, "EEE");

			BillingTestHelper.AddPriceItem(priceHeader, "WW1", BillingConstants.FeeType.Module, "", 9m);
			BillingTestHelper.AddPriceItem(priceHeader, "W1A", BillingConstants.FeeType.Included, "WW1", 0m, "AAA");
			BillingTestHelper.AddPriceItem(priceHeader, "W1B", BillingConstants.FeeType.Included, "WW1", 0m, "BBB");

			BillingTestHelper.AddPriceItem(priceHeader, "CUM", BillingConstants.FeeType.CoreUsers, "", 1m);

			BillingTestHelper.AddPriceItem(priceHeader, "#C1", BillingConstants.FeeType.CoreUsers, "", 3m);
			BillingTestHelper.AddPriceItem(priceHeader, "C11", BillingConstants.FeeType.Included, "#C1", 0m);

			var rating1 = modules.FindByCode(lic.RelationshipClientRatesTariffs.Name);
			BillingTestHelper.AddPriceItem(priceHeader, "#RA", BillingConstants.FeeType.NamedUser, "", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, rating1.LM_GroupModuleCode, BillingConstants.FeeType.Included, "#RA", 0m);
			rating1.LM_LicenceType = LicenceTypes.Codes.ODM;
			rating1.LM_UserCount = 3;

			var periodStart = new ZDateTime(2010, 10, 1);
			CreateChargeableUsage(org, periodStart, "AAA", 10);
			CreateChargeableUsage(org, periodStart, "AA1", 13);
			CreateChargeableUsage(org, periodStart, "AA2", 7);
			CreateChargeableUsage(org, periodStart, "BBB", 5);
			CreateChargeableUsage(org, periodStart, "CC1", 16);

			CreateChargeableUsage(org, periodStart, "DDD", 4);
			CreateChargeableUsage(org, periodStart, "DD1", 32);
			CreateChargeableUsage(org, periodStart, "EE1", 32);

			CreateChargeableUsage(org, periodStart, "W1A", 199);
			CreateChargeableUsage(org, periodStart, "W1B", 200);

			CreateChargeableUsage(org, periodStart, BillingConstants.CoreModuleCode, 20);
			CreateChargeableUsage(org, periodStart, "CUM", 3);

			CreateChargeableUsage(org, periodStart, rating1.LM_GroupModuleCode, 9);

			CreateChargeableUsage(org, periodStart, "C11", 8);

			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("SystemUsages.Count", 1, systemBills[0].SystemUsages.Count);
			var odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			AssertEquals("Precondition", 12, odplUsage.ModuleUsages.Count);
			odplUsage.CalculateAmount();

			var moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().ToArray();
			AssertEquals("No modules added", 12, moduleUsages.Length);
			AssertEquals(13, moduleUsages.First(x => x.ModuleCode == "AAA").StaffCount);
			AssertEquals(5, moduleUsages.First(x => x.ModuleCode == "BBB").StaffCount);
			AssertEquals(16, moduleUsages.First(x => x.ModuleCode == "CCC").StaffCount);
			AssertEquals(20, moduleUsages.First(x => x.ModuleCode == "CUM").StaffCount);
			AssertEquals("PurchasedStaffCount comes from core", 7, moduleUsages.First(x => x.ModuleCode == "CUM").PurchasedStaffCount);

			AssertEquals("staff count for automatically added parent with core-users fee basis is core count", 20, moduleUsages.First(x => x.ModuleCode == "#C1").StaffCount);
			AssertEquals("PurchasedStaffCount comes from core", 7, moduleUsages.First(x => x.ModuleCode == "#C1").PurchasedStaffCount);

			AssertEquals("Child has Min(child staff count, parent staff count)", 4, moduleUsages.First(x => x.ModuleCode == "DD1").StaffCount);
			AssertEquals("Parent has no usage, so child has no usage", 0, moduleUsages.First(x => x.ModuleCode == "EE1").StaffCount);

			AssertEquals("Web group usage is max of web parent usages", 13, moduleUsages.First(x => x.ModuleCode == "WW1").StaffCount);

			var ratingBundleUsage = moduleUsages.First(x => x.ModuleCode == "#RA");
			AssertEquals("StaffCount", 9, ratingBundleUsage.StaffCount);
			AssertEquals("PurchasedStaffCount", 3, ratingBundleUsage.PurchasedStaffCount);
			AssertEquals("UnitCount", 9 - 3, ratingBundleUsage.UnitCount);
		}

		public void TestCreateSystemUsages_PriceUnitBreak()
		{
			var org = organisation1;
			var licHeader = org.LicCompany.LicHeadersForAllDatabases[0];
			var db = licHeader.Database;
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var moduleCode = Env.Licence.InquiryManager.Name;
			var module = licHeader.Modules.FindByCode(moduleCode);
			module.LM_LicenceType = LicenceTypes.Codes.ODM;

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 100m);
			priceItem1.L7_UnitBreak = 0;
			priceItem1.L7_Order = 21;

			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 30m);
			priceItem2.L7_UnitBreak = 5;
			priceItem2.L7_Order = 22;

			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 20m);
			priceItem3.L7_UnitBreak = 10;
			priceItem3.L7_Order = 23;

			var periodStart = new ZDateTime(2013, 7, 1);
			var chargeableUsage = CreateChargeableUsage(org, periodStart, moduleCode, 15);
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("SystemUsages.Count", 1, systemBills[0].SystemUsages.Count);
			var odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			var moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(3, moduleUsages.Length);

			var moduleUsage1 = moduleUsages[0];
			AssertEquals(500m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage1.FeeType);

			var moduleUsage2 = moduleUsages[1];
			AssertEquals(150m, moduleUsage2.Amount);
			AssertEquals(5, moduleUsage2.StaffCount);
			AssertEquals(30m, moduleUsage2.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage2.FeeType);

			var moduleUsage3 = moduleUsages[2];
			AssertEquals(100m, moduleUsage3.Amount);
			AssertEquals(5, moduleUsage3.StaffCount);
			AssertEquals(20m, moduleUsage3.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage3.FeeType);

			chargeableUsage.U1_UnitCount = 3;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(1, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(300m, moduleUsage1.Amount);
			AssertEquals(3, moduleUsage1.StaffCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage1.FeeType);

			chargeableUsage.U1_UnitCount = 5;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(1, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(500m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage1.FeeType);
		}

		public void TestCreateSystemUsages_PriceUnitBreak_MultipleFeeTypes()
		{
			var org = organisation1;
			var licHeader = org.LicCompany.LicHeadersForAllDatabases[0];
			var db = licHeader.Database;
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var moduleCode = Env.Licence.InquiryManager.Name;
			var module = licHeader.Modules.FindByCode(moduleCode);
			module.LM_LicenceType = LicenceTypes.Codes.ODM;

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);

			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.Licence, "", 100m);
			priceItem1.L7_UnitBreak = 0;
			priceItem1.L7_Order = 21;

			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 30m);
			priceItem2.L7_UnitBreak = 5;
			priceItem2.L7_Order = 22;

			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 20m);
			priceItem3.L7_UnitBreak = 10;
			priceItem3.L7_Order = 23;

			var periodStart = new ZDateTime(2013, 7, 1);
			var chargeableUsage = CreateChargeableUsage(org, periodStart, moduleCode, 15);
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("SystemUsages.Count", 1, systemBills[0].SystemUsages.Count);
			var odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			var moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(3, moduleUsages.Length);

			var moduleUsage1 = moduleUsages[0];
			AssertEquals(100m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);

			var moduleUsage2 = moduleUsages[1];
			AssertEquals(150m, moduleUsage2.Amount);
			AssertEquals(5, moduleUsage2.StaffCount);
			AssertEquals(30m, moduleUsage2.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage2.FeeType);

			var moduleUsage3 = moduleUsages[2];
			AssertEquals(100m, moduleUsage3.Amount);
			AssertEquals(5, moduleUsage3.StaffCount);
			AssertEquals(20m, moduleUsage3.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage3.FeeType);

			chargeableUsage.U1_UnitCount = 3;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(1, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(100m, moduleUsage1.Amount);
			AssertEquals(3, moduleUsage1.StaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);

			chargeableUsage.U1_UnitCount = 5;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(1, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(100m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);
		}

		public void TestCreateSystemUsages_PriceUnitBreak_PurchasedSeats()
		{
			var org = organisation1;
			var licHeader = org.LicCompany.LicHeadersForAllDatabases[0];
			var db = licHeader.Database;
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var moduleCode = Env.Licence.InquiryManager.Name;
			var module = licHeader.Modules.FindByCode(Env.Licence.InquiryManager.Name);
			module.LM_LicenceType = LicenceTypes.Codes.ODM;
			module.LM_UserCount = 7;

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);

			var priceItem1 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.Licence, "", 100m);
			priceItem1.L7_UnitBreak = 0;
			priceItem1.L7_Order = 21;

			var priceItem2 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 30m);
			priceItem2.L7_UnitBreak = 5;
			priceItem2.L7_Order = 22;

			var priceItem3 = BillingTestHelper.AddPriceItem(priceHeader, moduleCode, BillingConstants.FeeType.NamedUser, "", 20m);
			priceItem3.L7_UnitBreak = 10;
			priceItem3.L7_Order = 23;

			var periodStart = new ZDateTime(2013, 7, 1);
			var chargeableUsage = CreateChargeableUsage(org, periodStart, moduleCode, 15);
			CreateChargeableUsage(org, periodStart, coreModule, 1);
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("SystemUsages.Count", 1, systemBills[0].SystemUsages.Count);
			var odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			var moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(3, moduleUsages.Length);

			var moduleUsage1 = moduleUsages[0];
			AssertEquals(0m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(5, moduleUsage1.PurchasedStaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);

			var moduleUsage2 = moduleUsages[1];
			AssertEquals(90m, moduleUsage2.Amount);
			AssertEquals(5, moduleUsage2.StaffCount);
			AssertEquals(2, moduleUsage2.PurchasedStaffCount);
			AssertEquals(30m, moduleUsage2.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage2.FeeType);

			var moduleUsage3 = moduleUsages[2];
			AssertEquals(100m, moduleUsage3.Amount);
			AssertEquals(5, moduleUsage3.StaffCount);
			AssertEquals(0, moduleUsage3.PurchasedStaffCount);
			AssertEquals(20m, moduleUsage3.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage3.FeeType);

			// 3 users...
			chargeableUsage.U1_UnitCount = 3;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(2, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(0m, moduleUsage1.Amount);
			AssertEquals(3, moduleUsage1.StaffCount);
			AssertEquals(5, moduleUsage1.PurchasedStaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);

			moduleUsage2 = moduleUsages[1];
			AssertEquals(0m, moduleUsage2.Amount);
			AssertEquals(0, moduleUsage2.StaffCount);
			AssertEquals(2, moduleUsage2.PurchasedStaffCount);
			AssertEquals(30m, moduleUsage2.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage2.FeeType);

			// 5 users...
			chargeableUsage.U1_UnitCount = 5;
			odplBilling = new OdplBillingSystem();
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];

			moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().Where(x => x.ModuleCode == moduleCode).ToArray();
			AssertEquals(2, moduleUsages.Length);

			moduleUsage1 = moduleUsages[0];
			AssertEquals(0m, moduleUsage1.Amount);
			AssertEquals(5, moduleUsage1.StaffCount);
			AssertEquals(5, moduleUsage1.PurchasedStaffCount);
			AssertEquals(1, moduleUsage1.MixedUnitCount);
			AssertEquals(100m, moduleUsage1.UnitPrice);
			AssertEquals(BillingConstants.FeeType.Licence, moduleUsage1.FeeType);

			moduleUsage2 = moduleUsages[1];
			AssertEquals(0m, moduleUsage2.Amount);
			AssertEquals(0, moduleUsage2.StaffCount);
			AssertEquals(2, moduleUsage2.PurchasedStaffCount);
			AssertEquals(30m, moduleUsage2.UnitPrice);
			AssertEquals(BillingConstants.FeeType.NamedUser, moduleUsage2.FeeType);
		}

		public void TestCreateSystemUsages_SelfHostedDatabaseUsersFeeBasis()
		{
			var periodStart = new ZDateTime(2010, 10, 1);

			var licHeader1a = BillingTestHelper.CreateLicence(Factory, "ZA1");
			var licHeader1b = BillingTestHelper.CreateAnotherLicence(licHeader1a, "ZA2");
			BillingTestHelper.SetInvoicing(licHeader1a, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(licHeader1b, licHeader1a);
			var org1a = licHeader1a.Company.Header;
			licHeader1a.Database.LD_HostedLocation = "TRA";
			licHeader1b.Company.LC_CompanyCountry = "NZ"; // prevent domestic discount applying

			var priceHeader = ClientLicencePriceHeaderLookupsTest.AddPriceHeader(org1a.LicCompany, "V22c");
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "CLO", BillingConstants.FeeType.SelfHostedDatabaseUsers, "", 7m);

			CreateChargeableUsage(licHeader1a, periodStart, BillingConstants.CoreModuleCode, 3);
			CreateChargeableUsage(licHeader1b, periodStart, BillingConstants.CoreModuleCode, 15);
			Factory.Save();

			var mock = new Mock<IDatabaseUsers>();
			mock.Setup(m => m.DatabaseMonthlyUserCount(licHeader1a.LA_LD, periodStart))
				.Returns(22);
			var dbUsers = mock.Object;

			var odplBilling = new OdplBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			var systemBills = odplBilling.LoadSystemBills(context);
			AssertEquals("SystemUsages.Count", 2, systemBills[0].SystemUsages.Count);
			var odplUsage1a = (OdplUsage)systemBills[0].SystemUsages.FirstOrDefault(x => x.OrganisationPK == org1a.PK);
			var odplUsage1b = (OdplUsage)systemBills[0].SystemUsages.FirstOrDefault(x => x.OrganisationPK == licHeader1b.Company.LC_OH);

			AssertEquals("Self Hosted Database Users module owned by org1a", 2, odplUsage1a.ModuleUsages.Count);
			var selfHostingModule = odplUsage1a.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "CLO");
			AssertNotNull("Self Hosted Database Users module added if usage exists", selfHostingModule);
			AssertEquals("Self Hosted Database Users count", 22, selfHostingModule.StaffCount);
			AssertEquals("Self Hosted Database Users count", 22, selfHostingModule.UnitCount);
			AssertEquals("UnitPrice", 7m, selfHostingModule.UnitPrice);
			AssertEquals("FeeType", BillingConstants.FeeType.SelfHostedDatabaseUsers, selfHostingModule.FeeType);

			AssertEquals("Core usage from org1b", 1, odplUsage1b.ModuleUsages.Count);
			AssertEquals(BillingConstants.CoreModuleCode, odplUsage1b.ModuleUsages[0].ModuleCode);

			// Hosted system - shouldn't be charged
			licHeader1a.Database.LD_HostedLocation = "SYD";
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			systemBills = odplBilling.LoadSystemBills(context);
			odplUsage1a = (OdplUsage)systemBills[0].SystemUsages.FirstOrDefault(x => x.OrganisationPK == org1a.PK);
			odplUsage1b = (OdplUsage)systemBills[0].SystemUsages.FirstOrDefault(x => x.OrganisationPK == licHeader1b.Company.LC_OH);
			AssertEquals("Only Core usage from org1a", 1, odplUsage1a.ModuleUsages.Count);
			AssertEquals(BillingConstants.CoreModuleCode, odplUsage1a.ModuleUsages[0].ModuleCode);
			AssertEquals("Only Core usage from org1b", 1, odplUsage1b.ModuleUsages.Count);
			AssertEquals(BillingConstants.CoreModuleCode, odplUsage1b.ModuleUsages[0].ModuleCode);

			mock.VerifyAll();
		}

		public void TestCreateSystemUsages_WebParentAndIncludedFeeBasis()
		{
			var org = organisation1;
			var lic = org.LicCompany.LicHeadersForAllDatabases[0];
			ClientLicencePriceHeader priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_ValidFrom = new ZDateTime(2010, 1, 1);
			var module1 = lic.Modules.AddNew();
			module1.LM_LicenceType = LicenceTypes.Codes.ODM;
			module1.LM_UserCount = 9;
			module1.LM_GroupModuleCode = "111";
			var module2 = lic.Modules.AddNew();
			module2.LM_LicenceType = LicenceTypes.Codes.ODM;
			module2.LM_UserCount = 4;
			module2.LM_GroupModuleCode = "222";
			var module3 = lic.Modules.AddNew();
			module3.LM_LicenceType = LicenceTypes.Codes.ODM;
			module3.LM_UserCount = 9;
			module3.LM_GroupModuleCode = "W11";
			var module4 = lic.Modules.AddNew();
			module4.LM_LicenceType = LicenceTypes.Codes.ODM;
			module4.LM_UserCount = 4;
			module4.LM_GroupModuleCode = "W22";

			BillingTestHelper.AddPriceItem(priceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.Module, "", 10m);
			BillingTestHelper.AddPriceItem(priceHeader, "111", BillingConstants.FeeType.NamedUser, "", 1m);
			BillingTestHelper.AddPriceItem(priceHeader, "222", BillingConstants.FeeType.NamedUser, "", 3m);
			BillingTestHelper.AddPriceItem(priceHeader, "##W", BillingConstants.FeeType.Module, "", 5m);
			BillingTestHelper.AddPriceItem(priceHeader, "W11", BillingConstants.FeeType.Included, "##W", 0m)
				.L7_WebParentCode = "111";
			BillingTestHelper.AddPriceItem(priceHeader, "W22", BillingConstants.FeeType.Included, "##W", 0m)
				.L7_WebParentCode = "222";

			var periodStart = new ZDateTime(2013, 7, 1);
			CreateChargeableUsage(org, periodStart, "111", 5);
			CreateChargeableUsage(org, periodStart, "222", 6);
			CreateChargeableUsage(org, periodStart, "W11", 100);
			CreateChargeableUsage(org, periodStart, "W22", 200);
			Factory.Save();

			OdplBillingSystem odplBilling = new OdplBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			SystemBill[] systemBills = odplBilling.LoadSystemBills(context);
			var odplUsage = (OdplUsage)systemBills[0].SystemUsages[0];
			AssertEquals(3, odplUsage.ModuleUsages.Count);
			var webGroupParent = odplUsage.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "##W");
			AssertEquals("purchased seats are the maximum child purchased seats", 9, webGroupParent.PurchasedStaffCount);
		}

		public void TestCreateSystemUsages_RegisteredUsers()
		{
			EServicesBillingTestHelper.CreateTable();

			LicenceCompany.ClearStandardPricesCompanyCache();
			var stdPriceLicence = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdPriceLicence);

			var stdPriceHeader = stdPriceLicence.Company.PriceHeaders.AddNew();
			stdPriceHeader.L6_SystemCode = "ODM";
			stdPriceHeader.L6_RX_NKCurrency = "AUD";
			stdPriceHeader.L6_PricelistVersion = "CW1 v8.1";
			stdPriceHeader.L6_IsStandard = false;
			stdPriceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);

			var licence = BillingTestHelper.CreateLicence(Factory, "EEE", "SYD", "HST");
			var org = licence.Company.Header;
			licence.LA_AgreedLiveDate = licence.LA_AgreedLiveDate.AddYears(-1);
			var db = licence.Database;
			db.LD_DatabaseNumber = 2847;
			db.LD_Product = ProductTypes.Codes.CargoWiseOne;
			BillingTestHelper.SetInvoicingTo(licence, org.LicCompany);

			var clientCompany1 = db.ClientCompanies.AddNew();
			clientCompany1.LCC_LD = db.PK;
			clientCompany1.LCC_Code = "AAA";
			var clientCompany2 = db.ClientCompanies.AddNew();
			clientCompany2.LCC_LD = db.PK;
			clientCompany2.LCC_Code = "BBB";

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = "ODM";
			priceHeader.L6_RX_NKCurrency = "AUD";
			priceHeader.L6_PricelistVersion = "CW1 v8.1";
			priceHeader.L6_IsStandard = true;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 7, 1);

			BillingTestHelper.AddPriceItem(stdPriceHeader, BillingConstants.CoreModuleCode, BillingConstants.FeeType.NamedUser, "", 10m);
			BillingTestHelper.AddPriceItem(stdPriceHeader, "#C1", BillingConstants.FeeType.CoreUsers, "", 3m);

			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 10, 1), clientCompany1, clientCompany2);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 11, 1), clientCompany1, clientCompany2);
			CreateLoginUsagesAndActiveUsers(new ZDateTime(2016, 12, 1), clientCompany1, clientCompany2);
			Factory.Save();

			var periodStart = new ZDateTime(2016, 10, 1);
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var odplBilling = new OdplBillingSystem();
			var systemBill = odplBilling.LoadSystemBills(context)[0];
			var odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count only because not yet start billing active users", 2, odplUsage1.CoreUsage.UnitCount);
			Assert("Should not show registered user as fee type", !odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			priceHeader.L6_ValidFrom = new ZDateTime(2016, 6, 1);
			Factory.Save();

			periodStart = new ZDateTime(2016, 11, 1);
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count only because client on pricelist not support billing active users", 2, odplUsage1.CoreUsage.UnitCount);
			Assert("Should not show registered user as fee type", !odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			priceHeader.L6_ValidFrom = new ZDateTime(2016, 7, 1);
			Factory.Save();

			periodStart = new ZDateTime(2016, 11, 1);
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count + active users without login count", 3, odplUsage1.CoreUsage.UnitCount);
			AssertEquals("Same as core usage count", 3, odplUsage1.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "#C1").UnitCount);
			Assert("Show registered user as fee type", odplUsage1.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "#C1").UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);
			var odplUsage2 = (OdplUsage)systemBill.SystemUsages[1];
			AssertEquals("Use login count", 2, odplUsage2.CoreUsage.UnitCount);
			AssertEquals("Same as core usage count", 2, odplUsage2.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "#C1").UnitCount);
			Assert("Show registered user as fee type", odplUsage2.ModuleUsages.Cast<OdplModuleUsage>().First(x => x.ModuleCode == "#C1").UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			periodStart = new ZDateTime(2016, 12, 1);
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count + active users without login count since 2016 November", 3, odplUsage1.CoreUsage.UnitCount);
			Assert("Show registered user as fee type", odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			var discount = licence.Company.SelfBilling.BillingDiscounts.AddNew();
			discount.L5_SystemCode = BillingConstants.BillingSystem.ODM;
			discount.L5_BreakUnits = BillingConstants.DiscountBreakUnit.Currency;
			discount.L5_Type = BillingConstants.DiscountType.Commitment;
			discount.L5_BreakAmount = 200m;
			discount.L5_Discount = 25m;
			discount.L5_StartDate = new ZDateTime(2016, 6, 1);
			Factory.Save();

			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count only because org has valid commitment discount", 2, odplUsage1.CoreUsage.UnitCount);
			Assert("Should not show registered user as fee type", !odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			periodStart = new ZDateTime(2017, 1, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.RegisteredUserModuleCode, periodStart, clientCompany1, 5);
			discount.L5_EndDate = new ZDateTime(2016, 12, 31);
			Factory.Save();

			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use active users without login count even there is no login users", 5, odplUsage1.CoreUsage.UnitCount);
			Assert("Show registered user as fee type", odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			stdPriceHeader.L6_PricelistVersion = "V23";
			priceHeader.L6_PricelistVersion = "V23";
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany1, 2);
			Factory.Save();
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use login count only because org is not CargoWiseOne customer", 2, odplUsage1.CoreUsage.UnitCount);
			Assert("Should not show registered user as fee type", !odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);

			stdPriceHeader.L6_PricelistVersion = "CW1 v9.0";
			stdPriceHeader.L6_ValidFrom = new ZDateTime(2013, 1, 1);
			Factory.Save();
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.SetPreviewOnly(licence, stdPriceHeader);
			odplBilling = new OdplBillingSystem();
			systemBill = odplBilling.LoadSystemBills(context)[0];
			odplUsage1 = (OdplUsage)systemBill.SystemUsages[0];
			AssertEquals("Use active users when it is preview", 5, odplUsage1.CoreUsage.UnitCount);
			Assert("Show registered user as fee type", odplUsage1.CoreUsage.UseRegisteredUserAsFeeTypeIfCoreRelatedUsage);
		}

		#region DAL / DAZ / DAC / GPC

		public void TestCreateSystemUsages_DAL()
		{
			var periodStart = new ZDateTime(2010, 10, 01);
			var toDate = new ZDateTime(2010, 10, 31);

			var licFR1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			licFR1.Company.LC_CompanyCountry = "FR";
			licFR1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var licFR2 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR2", "DB1");
			licFR2.Company.LC_CompanyCountry = "FR";
			licFR2.ClientCompany.LCC_RN_NKCountryCode = "FR";

			CreatePriceItem(licFR1, "F01", "DAL", Core.SharedConstants.Languages.French, 100);
			CreatePriceItem(licFR2, "F01", "DAL", Core.SharedConstants.Languages.French, 100);

			CreateChargeableUsage(licFR1, periodStart, coreModule, 15);
			CreateChargeableUsage(licFR2, periodStart, coreModule, 25);

			CreateChargeableUsage(licFR1, periodStart, "F01", 35);
			CreateChargeableUsage(licFR2, periodStart, "F01", 45);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR2.ClientCompany, 1);

			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|DM1-NULL|DM1-NULL", toDate, licFR1.Company.Header.PK);

			licFR2.ClientCompany.LCC_RN_NKCountryCode = "US";
			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|F01-F01", toDate, licFR1.Company.Header.PK);
		}

		public void TestCreateSystemUsages_DAZ()
		{
			var periodStart = new ZDateTime(2010, 10, 01);
			var toDate = new ZDateTime(2010, 10, 31);

			var licCN1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CN1", "DB1");
			licCN1.Company.LC_CompanyCountry = "CN";
			licCN1.ClientCompany.LCC_RN_NKCountryCode = "CN";

			var licFR2 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR2", "DB1");
			licFR2.Company.LC_CompanyCountry = "FR";
			licFR2.ClientCompany.LCC_RN_NKCountryCode = "FR";

			CreatePriceItem(licCN1, "ZH1", "DAZ", Core.SharedConstants.Languages.ChineseSimplified, 100);
			CreatePriceItem(licFR2, "ZH1", "DAZ", Core.SharedConstants.Languages.ChineseSimplified, 100);

			CreateChargeableUsage(licCN1, periodStart, coreModule, 15);
			CreateChargeableUsage(licFR2, periodStart, coreModule, 25);

			CreateChargeableUsage(licCN1, periodStart, "ZH1", 35);
			CreateChargeableUsage(licFR2, periodStart, "ZH1", 45);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licCN1.ClientCompany, 10);
			var usageFR = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR2.ClientCompany, 5);

			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR", toDate, licCN1.Company.Header.PK);

			usageFR.U1_UnitCount = 50;
			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|ZH1-ZH1", toDate, licCN1.Company.Header.PK);
		}

		public void TestCreateSystemUsages_DAC()
		{
			var periodStart = new ZDateTime(2010, 10, 01);
			var toDate = new ZDateTime(2010, 10, 31);

			var licFR1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			licFR1.Company.LC_CompanyCountry = "FR";
			licFR1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			var licFR2 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR2", "DB1");
			licFR2.Company.LC_CompanyCountry = "FR";
			licFR2.ClientCompany.LCC_RN_NKCountryCode = "FR";

			CreatePriceItem(licFR1, "F01", "DAC", "", 100);
			CreatePriceItem(licFR2, "F01", "DAC", "", 100);

			CreateChargeableUsage(licFR1, periodStart, coreModule, 15);
			CreateChargeableUsage(licFR2, periodStart, coreModule, 25);

			CreateChargeableUsage(licFR1, periodStart, "F01", 35);
			CreateChargeableUsage(licFR2, periodStart, "F01", 45);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR1.ClientCompany, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR2.ClientCompany, 1);

			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|DM1-NULL|DM1-NULL", toDate, licFR1.Company.Header.PK);

			licFR2.ClientCompany.LCC_RN_NKCountryCode = "US";
			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|F01-F01", toDate, licFR1.Company.Header.PK);
		}

		public void TestCreateSystemUsages_GPC()
		{
			var periodStart = new ZDateTime(2010, 10, 01);
			var toDate = new ZDateTime(2010, 10, 31);

			var licAU1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			licAU1.Company.LC_CompanyCountry = "AU";
			licAU1.ClientCompany.LCC_RN_NKCountryCode = "AU";

			var licFR1 = BillingTestHelper.CreateLicence(Factory, "EN1", "FR1", "DB1");
			licFR1.Company.LC_CompanyCountry = "FR";
			licFR1.ClientCompany.LCC_RN_NKCountryCode = "FR";

			CreatePriceItem(licAU1, "GPC", "UCB", "", 100).L7_UnitBreak = 1;
			CreatePriceItem(licFR1, "GPC", "UCB", "", 100).L7_UnitBreak = 1;

			CreateChargeableUsage(licAU1, periodStart, coreModule, 15);
			CreateChargeableUsage(licFR1, periodStart, coreModule, 25);

			CreateChargeableUsage(licAU1, periodStart, "GPC", 1);
			CreateChargeableUsage(licFR1, periodStart, "GPC", 1);

			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licAU1.ClientCompany, 20);
			BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, licFR1.ClientCompany, 30);

			Factory.Save();

			AssertModuleUsages(@"COR-COR|COR-COR|GPC-GPC|GPC-GPC", toDate, licAU1.Company.Header.PK);
		}

		public void TestCreateSystemUsages_ValidStlUsageCodesOnOdplPricelists()
		{
			var periodStart = new ZDateTime(2018, 10, 01);
			var toDate = new ZDateTime(2018, 10, 31);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			CreatePriceItem(lic1, "SHP", "NUM", "", 12);

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", periodStart, lic1.ClientCompany, 20);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "ODM", "COR", periodStart, lic1.ClientCompany, 30);
			var usage3 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHP", periodStart, lic1.ClientCompany, 40);
			var usage4 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "SHX", periodStart, lic1.ClientCompany, 50);

			var regValue = new CodeDescriptionPairList();
			regValue.AddPair("SHP", "");
			EDIDataRegistry.Instance.ValidStlUsageCodesOnOdplPricelists.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, regValue);

			Factory.Save();

			var odplUsages = AssertModuleUsages(@"COR-COR|DS2-NULL|SHP-SHP", toDate, lic1.Company.Header.PK);
			AssertEquals(1, odplUsages.Count);
			AssertOdplUsage(odplUsages, usage2, usage3);
		}

		ClientLicencePriceItem CreatePriceItem(LicenceHeader lic, string priceCode, string feeType, string language, decimal price)
		{
			var prices = BillingTestHelper.CreatePriceList(lic);
			prices.Items.FindByCode("COR").L7_Price = 29;

			var priceItem = BillingTestHelper.AddPriceItem(prices, priceCode, feeType, "", price);
			priceItem.L7_Language = language;

			return priceItem;
		}

		List<OdplUsage> AssertModuleUsages(string usagesAsText, ZDateTime toDate, ZGuid orgPk)
		{
			var odplBilling = new OdplBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, toDate, orgPk);

			var allOdplUsages = new List<OdplUsage>();
			var systemBills = odplBilling.LoadSystemBills(context);
			foreach (SystemBill bill in systemBills)
			{
				allOdplUsages.AddRange(bill.SystemUsages.Cast<OdplUsage>());
			}

			var usages = string.Join("|", allOdplUsages.SelectMany(x => x.ModuleUsages.OfType<OdplModuleUsage>())
				.Select(x => $"{x.ModuleCode}-{x.PriceItem?.L7_Code ?? "NULL"}")
				.OrderBy(x => x));

			AssertEquals(usagesAsText, usages);

			return allOdplUsages;
		}

		#endregion

		void CreateLoginUsagesAndActiveUsers(ZDateTime periodStart, ClientCompany clientCompany1, ClientCompany clientCompany2)
		{
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany1, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.CoreModuleCode, periodStart, clientCompany2, 2);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.RegisteredUserModuleCode, periodStart, clientCompany1, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, BillingConstants.RegisteredUserModuleCode, periodStart, clientCompany2, 2);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "#C1", periodStart, clientCompany1, 1);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, "#C1", periodStart, clientCompany2, 2);
		}

		ClientChargeableUsage CreateChargeableUsage(EDIOrgHeader organisation, ZDateTime periodStart, ZString moduleCode, ZInt unitCount)
		{
			var licHeader = organisation.LicCompany.LicHeadersForAllDatabases[0];
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, moduleCode, periodStart, licHeader, unitCount);
		}

		ClientChargeableUsage CreateChargeableUsage(LicenceHeader licHeader, ZDateTime periodStart, ZString moduleCode, ZInt unitCount)
		{
			return BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ODM, moduleCode, periodStart, licHeader, unitCount);
		}

		void AssertOdplUsage(IEnumerable<OdplUsage> allOdplUsages, params ClientChargeableUsage[] chargeableUsages)
		{
			OdplUsage odplUsage = allOdplUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsages[0].PK));
			AssertEquals(chargeableUsages[0].OrganisationPK, odplUsage.OrganisationPK);
			int coreDiscounts = CoreDiscountQty(odplUsage, "AU", 1);
			AssertEquals("Module usages count", chargeableUsages.Length + coreDiscounts, odplUsage.ModuleUsages.Count);

			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				AssertEquals("ODPL usage contains all related chargeable usages", true, odplUsage.ChargeableUsagePKs.Contains(chargeableUsage.PK));

				ZString expectedModuleCode = chargeableUsage.U1_SubCode;
				IEnumerable<OdplModuleUsage> moduleUsages = odplUsage.ModuleUsages.Cast<OdplModuleUsage>();
				AssertEquals("Module usage found for chargeable usage " + expectedModuleCode + " " + chargeableUsage.U1_UnitCount, true, moduleUsages.Any(x => x.ModuleCode == expectedModuleCode && x.StaffCount == chargeableUsage.U1_UnitCount));
			}
		}

		void AssertOdplUsagePks(IEnumerable<OdplUsage> allOdplUsages, params ClientChargeableUsage[] chargeableUsages)
		{
			OdplUsage odplUsage = allOdplUsages.First(x => x.ChargeableUsagePKs.Contains(chargeableUsages[0].PK));
			AssertEquals(chargeableUsages[0].OrganisationPK, odplUsage.OrganisationPK);

			foreach (ClientChargeableUsage chargeableUsage in chargeableUsages)
			{
				AssertEquals("ODPL usage contains all related chargeable usages", true, odplUsage.ChargeableUsagePKs.Contains(chargeableUsage.PK));
			}
		}

		int CoreDiscountQty(OdplUsage odplUsage, ZString countryCode, int domesticCountries)
		{
			int coreUses = 0;
			var coreModules = odplUsage.ModuleUsages.Select(u => u).Cast<OdplModuleUsage>().Where(u => u.ModuleCode == BillingConstants.CoreModuleCode);
			foreach (var coreModule in coreModules)
			{
				coreUses += coreModule.StaffCount;
			}
			return coreUses > 0 ? OdplBillingSystem.DatabaseUsage.DomesticDiscountCodes(countryCode, coreUses, domesticCountries).Length : 0;
		}

		#region Implementation

		EDIOrgHeader organisation1;
		EDIOrgHeader childOrganisation11;
		EDIOrgHeader organisation2;
		EDIOrgHeader organisation3;

		string coreModule;
		string forwarderModule;
		string accountantModule;
		string frenchGuiModule;
		string germanGuiModule;
		string chinaSimplifiedGuiModule;
		string chinaTraditionalGuiModule;

		protected override void SetUp()
		{
			base.SetUp();

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA", "SYD", "AAA");
			var lic1a = BillingTestHelper.CreateDependentLicence(lic1, "AA1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB", "SYD", "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "CCC", "SYD", "CCC");

			organisation1 = lic1.Company.Header;
			childOrganisation11 = lic1a.Company.Header;
			organisation2 = lic2.Company.Header;
			organisation3 = lic3.Company.Header;

			Factory.Save();

			var licences = new LegacyLicence();
			coreModule = licences.Core.Name;
			forwarderModule = licences.Forwarder.Name;
			accountantModule = licences.Accountant.Name;
			frenchGuiModule = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.French].GUILanguageCheckpoint.Name;
			germanGuiModule = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.German].GUILanguageCheckpoint.Name;
			chinaSimplifiedGuiModule = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.ChineseSimplified].GUILanguageCheckpoint.Name;
			chinaTraditionalGuiModule = Env.Licence.LanguagePackLookup[Enterprise.Core.Constants.Languages.ChineseTraditional].GUILanguageCheckpoint.Name;
		}

		#endregion
	}
}
