using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class PremiumServiceBillingSystemTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var billingSystem = new PremiumServiceBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.Service, billingSystem.SystemCode);
		}

		public void TestCreateSystemUsages()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var priceB = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			var priceA = BillingTestHelper.AddPriceItem(prices1, "AAA", BillingConstants.FeeType.PerDevicePerMonth, "", 7m);

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "AA2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(lic2, lic1.Company);
			var db1 = lic1.Database;

			var licInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA3");
			licInactive.LA_IsActive = false;

			var licOrgInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA4");
			licOrgInactive.Company.Header.OH_IsActive = false;

			var lic3 = BillingTestHelper.CreateLicence(Factory, "BBB");
			BillingTestHelper.SetInvoicing(lic3, Env.CurrentBranch.PK);
			var db3 = lic3.Database;

			var licDbInactive = BillingTestHelper.CreateLicence(Factory, "CCC");
			licDbInactive.Database.LD_IsActive = false;

			var licFuture = BillingTestHelper.CreateLicence(Factory, "FFF");

			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var org3 = lic3.Company.Header;

			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 2, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2012, 1, 1), new ZDateTime(2012, 12, 31));
			BillingTestHelper.CreatePremiumService(db3, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			BillingTestHelper.CreatePremiumService(db1, "AAA", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(db3, "AAA", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));

			BillingTestHelper.CreatePremiumService(licFuture.Database, "BBB", new ZDateTime(2013, 2, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(licDbInactive.Database, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			Factory.Save();

			var billing = new PremiumServiceBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31));
			var bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills loaded", 2, bills.Length);
			var bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == org3.PK);

			AssertEquals("org1 has the usage as the DB owner", 2, bill1.SystemUsages.Count);
			AssertEquals("org3 pays for itself", 2, bill2.SystemUsages.Count);

			foreach (var bill in bills)
			{
				foreach (SystemUsage systemUsage in bill.SystemUsages)
				{
					AssertEquals(true, systemUsage is PremiumServiceUsage);
				}
			}

			AssertEquals(true, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(true, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));

			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31), org1.PK);
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("Bill for Context.Organisation loaded", 1, bills.Length);

			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31), org3.PK);
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("Bill for Context.Organisation loaded", 1, bills.Length);
		}

		public void TestCreateSystemUsages_FeeTypePerDatabaseUser()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.DatabaseUsers, "", 10m);

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "AA2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK);
			var prices2 = BillingTestHelper.CreatePriceList(lic2.Company);
			BillingTestHelper.AddPriceItem(prices2, "BBB", BillingConstants.FeeType.DatabaseUsers, "", 10m);
			var db1 = lic1.Database;
			var service1a = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			service1a.CPS_Units = 3;

			var periodStart = new ZDateTime(2013, 8, 1);

			Factory.Save();
			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;

			var mock = new Mock<IDatabaseUsers>();
			mock.Setup(m => m.DatabaseMonthlyUserCount(lic1.LA_LD, periodStart)).Returns(11);
			mock.Setup(m => m.DatabaseMonthlyUserCount(lic1.LA_LD, periodStart.AddMonths(-1))).Returns(22);
			mock.Setup(m => m.DatabaseMonthlyUserCount(lic1.LA_LD, periodStart.AddMonths(1))).Returns(33);
			mock.Setup(m => m.DatabaseMonthlyUserCount(lic1.LA_LD, periodStart.AddMonths(2))).Returns(0);
			var dbUsers = mock.Object;

			var billing = new PremiumServiceBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			var bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills", 1, bills.Length);
			var bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			AssertEquals("usages", 1, bill1.SystemUsages.Count);
			AssertEquals("FeeTypeUnitCount", 11, ((PremiumServiceUsage)bill1.SystemUsages[0]).FeeTypeUnitCount);

			// prev month
			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills", 1, bills.Length);
			bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			AssertEquals("FeeTypeUnitCount", 22, ((PremiumServiceUsage)bill1.SystemUsages[0]).FeeTypeUnitCount);

			// next month
			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(2).AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills", 1, bills.Length);
			bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			AssertEquals("FeeTypeUnitCount", 33, ((PremiumServiceUsage)bill1.SystemUsages[0]).FeeTypeUnitCount);

			// later months - no usage
			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(3).AddDays(-1));
			context.DatabaseUsersService = dbUsers;
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills", 0, bills.Length);

			mock.VerifyAll();
		}

		public void TestCreateSystemUsages_UsageOwner_CPS_LCC()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var priceB = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			var priceA = BillingTestHelper.AddPriceItem(prices1, "AAA", BillingConstants.FeeType.PerDevicePerMonth, "", 7m);

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "AA2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(lic2, lic1.Company);
			var db1 = lic1.Database;

			var licInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA3");
			licInactive.LA_IsActive = false;

			var licOrgInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA4");
			licOrgInactive.Company.Header.OH_IsActive = false;

			var lic3 = BillingTestHelper.CreateLicence(Factory, "BBB");
			BillingTestHelper.SetInvoicing(lic3, Env.CurrentBranch.PK);
			var db3 = lic3.Database;
			var clientCompany1 = db3.ClientCompanies.AddNew();
			clientCompany1.FillWithValidTestData();
			var org4 = Factory.NewWithValidTestData<EDIOrgHeader>();
			clientCompany1.LCC_OH = org4.PK;

			var licDbInactive = BillingTestHelper.CreateLicence(Factory, "CCC");
			licDbInactive.Database.LD_IsActive = false;

			var licFuture = BillingTestHelper.CreateLicence(Factory, "FFF");

			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var org3 = lic3.Company.Header;

			var premiumService1 = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			premiumService1.CPS_LCC = clientCompany1.PK;
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 2, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2012, 1, 1), new ZDateTime(2012, 12, 31));

			BillingTestHelper.CreatePremiumService(db3, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			BillingTestHelper.CreatePremiumService(db1, "AAA", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(db3, "AAA", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));

			BillingTestHelper.CreatePremiumService(licFuture.Database, "BBB", new ZDateTime(2013, 2, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(licDbInactive.Database, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			Factory.Save();

			var billing = new PremiumServiceBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31));
			var bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills loaded", 3, bills.Length);
			var bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == org3.PK);
			var bill3 = bills.First(x => x.OrganisationPK == org4.PK);

			AssertEquals("org1 has the usage as the DB owner", 1, bill1.SystemUsages.Count);
			AssertEquals("org3 pays for itself", 2, bill2.SystemUsages.Count);
			AssertEquals("org4 pays for itself", 1, bill3.SystemUsages.Count);

			foreach (var bill in bills)
			{
				foreach (SystemUsage systemUsage in bill.SystemUsages)
				{
					AssertEquals(true, systemUsage is PremiumServiceUsage);
				}
			}

			AssertEquals(true, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));
			AssertEquals(true, bill3.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));
		}

		public void TestLoadOdplRawUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			var org1 = lic1.Company.Header;
			var db1 = lic1.Database;
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var db2 = lic2.Database;

			var prices = BillingTestHelper.CreatePriceList(lic1.Company);
			var servicePriceB = BillingTestHelper.AddPriceItem(prices, "BBB", BillingConstants.FeeType.Transactional, "", 10m);
			servicePriceB.L7_Description = "Item BBB";

			var servicePriceA = BillingTestHelper.AddPriceItem(prices, "AAA", BillingConstants.FeeType.Transactional, "", 10m);
			servicePriceA.L7_Description = "Item AAA";

			var periodStart = new ZDateTime(2013, 1, 1);

			var serviceB1 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart, new ZDateTime(2013, 12, 31));
			serviceB1.CPS_ClientRef = "Your ref 1";
			serviceB1.CPS_DisplayOrder = 1;
			serviceB1.CPS_Units = 5;
			var serviceB2 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart, new ZDateTime(2013, 12, 31));
			serviceB2.CPS_ClientRef = "Your ref 2";
			serviceB2.CPS_DisplayOrder = 2;
			serviceB2.CPS_Units = 13;
			var serviceB3 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart.AddMonths(1), new ZDateTime(2013, 12, 31));
			var serviceA = BillingTestHelper.CreatePremiumService(db1, "AAA", periodStart, new ZDateTime(2012, 12, 31));
			serviceA.CPS_DisplayOrder = 3;
			serviceA.CPS_Units = 8;
			var serviceDb2 = BillingTestHelper.CreatePremiumService(db2, "BBB", periodStart, ZDateTime.Empty);

			var usageB1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1.LA_LC, 5);
			usageB1.U1_Parent = serviceB1.PK;
			var usageB2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1.LA_LC, 13);
			usageB2.U1_Parent = serviceB2.PK;
			var usageB1Later = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart.AddMonths(1), lic1.LA_LC, 7);
			usageB1Later.U1_Parent = serviceB1.PK;
			var usageB3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart.AddMonths(1), lic1.LA_LC, 7);
			usageB3.U1_Parent = serviceB3.PK;
			var usageA = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "AAA", periodStart, lic1.LA_LC, 11);
			usageA.U1_Parent = serviceA.PK;
			Factory.Save();

			var billing = new PremiumServiceBillingSystem();
			var licence = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2013, 1, 1), org1.PK, ZGuid.Empty, licence.Company.PK, licence.Database.PK);
			var rawUsage = (SystemCodeRawUsage)billing.LoadOdplRawUsage(context);
			AssertEquals(org1.OH_Code, rawUsage.OrgCode);
			AssertEquals(new ZDateTime(2013, 1, 1), rawUsage.PeriodStart);
			AssertEquals("Service", rawUsage.Summary.Header.Column1);
			AssertEquals("Reference", rawUsage.Summary.Header.Column2);
			AssertEquals("Unit Count", rawUsage.Summary.Header.Column3);

			AssertEquals(3, rawUsage.Summary.Lines.Count);
			AssertEquals("Item BBB", rawUsage.Summary.Lines[0].Column1);
			AssertEquals("Your ref 1", rawUsage.Summary.Lines[0].Column2);
			AssertEquals("5", rawUsage.Summary.Lines[0].Column3);

			AssertEquals("Item BBB", rawUsage.Summary.Lines[1].Column1);
			AssertEquals("Your ref 2", rawUsage.Summary.Lines[1].Column2);
			AssertEquals("13", rawUsage.Summary.Lines[1].Column3);

			AssertEquals("Item AAA", rawUsage.Summary.Lines[2].Column1);
			AssertEquals("", rawUsage.Summary.Lines[2].Column2);
			AssertEquals("8", rawUsage.Summary.Lines[2].Column3);

			string expectedCsvResult =
@"""Service"",""Reference"",""Unit Count""
""Item BBB"",""Your ref 1"",""5""
""Item BBB"",""Your ref 2"",""13""
""Item AAA"","""",""8""
";

			var builder = new ZStringBuilder();
			billing.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billing.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Jan-13 00:00"",""AA1"","""","""",""Item BBB Your ref 1 5"","""","""",""1""
""01-Jan-13 00:00"",""AA1"","""","""",""Item BBB Your ref 2 13"","""","""",""1""
""01-Jan-13 00:00"",""AA1"","""","""",""Item AAA  8"","""","""",""1""
", writer.ToString());
		}

		public void TestLoadOdplRawUsage_FeeTypePerDatabaseUser()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AA1");
			var lic1b = BillingTestHelper.CreateAnotherDatabase(lic1a, "BB1");
			BillingTestHelper.SetInvoicing(lic1a, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1a.Company);
			var price1b = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.DatabaseUsers, "", 10m);
			price1b.L7_Description = "Item BBB";

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1a, "AA2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK);
			var prices2 = BillingTestHelper.CreatePriceList(lic2.Company);
			BillingTestHelper.AddPriceItem(prices2, "BBB", BillingConstants.FeeType.DatabaseUsers, "", 10m);
			var db1 = lic1a.Database;
			var db2 = lic1b.Database;
			var service1a = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			service1a.CPS_Units = 1;
			service1a.CPS_DisplayOrder = 1;
			service1a.CPS_ClientRef = "Ref 1a";
			var service1b = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			service1b.CPS_Units = 1;
			service1b.CPS_DisplayOrder = 2;
			service1b.CPS_ClientRef = "Ref 1b";

			var periodStart = new ZDateTime(2013, 8, 1);
			var usage1a = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1a.LA_LC, 2);
			usage1a.U1_Parent = service1a.PK;
			var usage1b = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1a.LA_LC, 2);
			usage1b.U1_Parent = service1b.PK;

			Factory.Save();
			var org1 = lic1a.Company.Header;
			var org2 = lic2.Company.Header;

			var mock = new Mock<IDatabaseUsers>();
			IEnumerable<string> names = new List<string>(new string[] { "User 1", "User 2", "User 3" });
			mock.Setup(m => m.DatabaseMonthlyUserList(lic1a.LA_LD, periodStart)).Returns(names);
			var dbUsers = mock.Object;

			var billing = new PremiumServiceBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, licence1.Company.PK, licence1.Database.PK);
			context.DatabaseUsersService = dbUsers;
			var rawUsage = (SystemCodeRawUsage)billing.LoadOdplRawUsage(context);
			var summarySection = rawUsage.Summary;
			AssertEquals("", summarySection.Header.TopLevelDescription);
			AssertEquals("Reference / Staff Name", summarySection.Header.Column2);
			AssertEquals("Unit Count", summarySection.Header.Column3);
			AssertEquals(5, summarySection.Lines.Count);
			AssertEquals("Item BBB", summarySection.Lines[0].Column1);
			AssertEquals("Ref 1a", summarySection.Lines[0].Column2);
			AssertEquals("3", summarySection.Lines[0].Column3);
			AssertEquals("Ref 1b", summarySection.Lines[1].Column2);
			AssertEquals("3", summarySection.Lines[1].Column3);
			AssertEquals("        User 1", summarySection.Lines[2].Column1);
			AssertEquals("        User 2", summarySection.Lines[3].Column1);
			AssertEquals("        User 3", summarySection.Lines[4].Column1);

			service1a.CPS_DisplayOrder = 10;
			service1b.CPS_DisplayOrder = 1;
			Factory.Save();
			billing = new PremiumServiceBillingSystem();
			context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, licence1.Company.PK, licence1.Database.PK);
			context.DatabaseUsersService = dbUsers;
			rawUsage = (SystemCodeRawUsage)billing.LoadOdplRawUsage(context);
			summarySection = rawUsage.Summary;
			AssertEquals(5, summarySection.Lines.Count);
			AssertEquals("Item BBB", summarySection.Lines[0].Column1);
			AssertEquals("Ref 1b", summarySection.Lines[0].Column2);
			AssertEquals("3", summarySection.Lines[0].Column3);
			AssertEquals("Ref 1a", summarySection.Lines[1].Column2);
			AssertEquals("3", summarySection.Lines[1].Column3);
			AssertEquals("        User 1", summarySection.Lines[2].Column1);
			AssertEquals("        User 2", summarySection.Lines[3].Column1);
			AssertEquals("        User 3", summarySection.Lines[4].Column1);

			mock.VerifyAll();
		}

		public void TestLoadStlRawUsage()
		{
			var wiseCloudCode = BillingConstants.Hosting.WiseCloudProductionLicenceCode;
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			var org1 = lic1.Company.Header;
			var db1 = lic1.Database;
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var db2 = lic2.Database;
			var prices = BillingTestHelper.CreateStlPriceList(lic1.Company, "AAA", "BBB", "CCC", wiseCloudCode);
			var wiseCloudPrice = prices.Items.FindByCode(wiseCloudCode);
			wiseCloudPrice.L7_Description = "WiseCloudProductionLicence";
			var link = db1.PriceHeaderLinks.AddNew();
			link.PHL_L6 = prices.PK;
			ZDateTime periodStart = new ZDateTime(2013, 1, 1);
			link.PHL_ValidFrom = periodStart;

			var stdLicCompany = ClientLicencePriceHeaderCollectionTest.CreateAndSetStandardPricesCompany(Factory);
			var globalPrices = BillingTestHelper.CreateGoldenTaxPriceList(stdLicCompany);
			globalPrices.L6_ValidFrom = periodStart;
			var globalServicePriceItem = BillingTestHelper.AddPriceItem(globalPrices, new UsageCodeKey(BillingConstants.BillingSystem.Service, "GTM"), BillingConstants.FeeType.PerDevicePerMonth, 200m);
			globalServicePriceItem.L7_Description = "Golden Tax Service";

			var serviceB1 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart, new ZDateTime(2013, 12, 31));
			serviceB1.CPS_ClientRef = "Your ref 1";
			serviceB1.CPS_DisplayOrder = 1;
			serviceB1.CPS_Units = 5;
			var serviceB2 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart, new ZDateTime(2013, 12, 31));
			serviceB2.CPS_ClientRef = "Your ref 2";
			serviceB2.CPS_DisplayOrder = 2;
			serviceB2.CPS_Units = 13;
			var serviceB3 = BillingTestHelper.CreatePremiumService(db1, "BBB", periodStart.AddMonths(1), new ZDateTime(2013, 12, 31));
			var serviceA = BillingTestHelper.CreatePremiumService(db1, "AAA", periodStart, new ZDateTime(2012, 12, 31));
			serviceA.CPS_DisplayOrder = 3;
			serviceA.CPS_Units = 8;
			var serviceDb2 = BillingTestHelper.CreatePremiumService(db2, "BBB", periodStart, ZDateTime.Empty);
			var globalService = BillingTestHelper.CreatePremiumService(db1, globalServicePriceItem.L7_Code, periodStart, ZDateTime.Empty);
			globalService.CPS_DisplayOrder = 4;
			globalService.CPS_ClientRef = "Your global";
			globalService.CPS_Units = 1;
			globalService.CPS_PriceHeaderCode = globalServicePriceItem.Parent.L6_SystemCode;

			var usageB1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1.LA_LC, 5);
			usageB1.U1_Parent = serviceB1.PK;
			usageB1.U1_LD = db1.PK;
			var usageB2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart, lic1.LA_LC, 13);
			usageB2.U1_Parent = serviceB2.PK;
			usageB2.U1_LD = db1.PK;
			var usageB1Later = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart.AddMonths(1), lic1.LA_LC, 7);
			usageB1Later.U1_Parent = serviceB1.PK;
			usageB1Later.U1_LD = db1.PK;
			var usageB3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", periodStart.AddMonths(1), lic1.LA_LC, 7);
			usageB3.U1_Parent = serviceB3.PK;
			usageB3.U1_LD = db1.PK;
			var usageA = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "AAA", periodStart, lic1.LA_LC, 11);
			usageA.U1_Parent = serviceA.PK;
			usageA.U1_LD = db1.PK;
			var globalPriceUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, globalServicePriceItem.L7_Code, periodStart, lic1.LA_LC, 1);
			globalPriceUsage.U1_Parent = globalService.PK;
			globalPriceUsage.U1_LD = db1.PK;

			var wiseCloudUsage = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, wiseCloudCode, periodStart, lic1.LA_LC, 1);
			wiseCloudUsage.U1_Parent = db1.PK;
			wiseCloudUsage.U1_LD = db1.PK;

			Factory.Save();

			var billing = new PremiumServiceBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2013, 1, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billing.LoadStlRawUsage(context);
			AssertEquals(new ZDateTime(2013, 1, 1), rawUsage.PeriodStart);
			AssertEquals("Service", rawUsage.Summary.Header.Column1);
			AssertEquals("Reference", rawUsage.Summary.Header.Column2);
			AssertEquals("Unit Count", rawUsage.Summary.Header.Column3);

			AssertEquals(4, rawUsage.Summary.Lines.Count);
			AssertEquals("Item BBB", rawUsage.Summary.Lines[0].Column1);
			AssertEquals("Your ref 1", rawUsage.Summary.Lines[0].Column2);
			AssertEquals("5", rawUsage.Summary.Lines[0].Column3);

			AssertEquals("Item BBB", rawUsage.Summary.Lines[1].Column1);
			AssertEquals("Your ref 2", rawUsage.Summary.Lines[1].Column2);
			AssertEquals("13", rawUsage.Summary.Lines[1].Column3);

			AssertEquals("Item AAA", rawUsage.Summary.Lines[2].Column1);
			AssertEquals("", rawUsage.Summary.Lines[2].Column2);
			AssertEquals("8", rawUsage.Summary.Lines[2].Column3);

			AssertEquals("Golden Tax Service", rawUsage.Summary.Lines[3].Column1);
			AssertEquals("Your global", rawUsage.Summary.Lines[3].Column2);
			AssertEquals("1", rawUsage.Summary.Lines[3].Column3);

			string expectedCsvResult =
@"""Service"",""Reference"",""Unit Count""
""Item BBB"",""Your ref 1"",""5""
""Item BBB"",""Your ref 2"",""13""
""Item AAA"","""",""8""
""Golden Tax Service"",""Your global"",""1""
";

			var builder = new ZStringBuilder();
			billing.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billing.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Jan-13 00:00"","""","""","""",""Item BBB Your ref 1 5"","""","""",""1""
""01-Jan-13 00:00"","""","""","""",""Item BBB Your ref 2 13"","""","""",""1""
""01-Jan-13 00:00"","""","""","""",""Item AAA  8"","""","""",""1""
""01-Jan-13 00:00"","""","""","""",""Golden Tax Service Your global 1"","""","""",""1""
", writer.ToString());

			builder.Clear();
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2013, 1, 1), db1.PK, wiseCloudPrice.PK, ZGuid.Empty);
			billing.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(@"""Service"",""Reference"",""Unit Count""
""WiseCloudProductionLicence"",""Server AA1"",""1""
", builder.ToString());
		}

		public void TestCreateSystemUsages_Multiple_LC()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "C01", "DB1");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EN1", "C02", "DB1");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "EN1", "C03", "DB1");

			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var priceB = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			var priceA = BillingTestHelper.AddPriceItem(prices1, "AAA", BillingConstants.FeeType.PerDevicePerMonth, "", 7m);

			var db1 = lic1.Database;
			db1.LD_OH_BillingParty = ZGuid.Empty;

			var premiumService1 = BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2017, 1, 1), new ZDateTime(2017, 12, 31));
			premiumService1.CPS_LCC = lic3.ClientCompany.PK;

			var usageLic3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.Service, "BBB", new ZDateTime(2017, 1, 15), lic3.LA_LC, 5);
			usageLic3.U1_Parent = premiumService1.PK;
			usageLic3.U1_LD = db1.PK;

			Factory.Save();

			AssertEquals(lic1.Company, db1.UsageOwnerOrFirstLicence.Company);
			AssertEquals(lic3.Company, premiumService1.UsageOwner.Org.LicCompany);

			var billing = new PremiumServiceBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2017, 1, 31));
			var bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills loaded", 1, bills.Length);
			var bill1 = bills.First(x => x.OrganisationPK == lic3.Company.LC_OH);
			var premiumServiceUsage = bill1.SystemUsages.OfType<PremiumServiceUsage>().Single();
			AssertEquals(lic3.Company, premiumServiceUsage.LicCompany);
			AssertEquals(usageLic3.PK, premiumServiceUsage.ChargeableUsagePKs.Single());
			AssertEquals(lic3.Company.PK, usageLic3.U1_LC);
		}

		public void TestDummyBillingPremiumTypes()
		{
			var registryValue = new CodeDescriptionPairList();
			registryValue.AddPairIfNotExist("#AA", "DESC_#AA");
			EDIDataRegistry.Instance.UnregisteredDevicePremiumTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AA1");
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);
			var prices1 = BillingTestHelper.CreatePriceList(lic1.Company);
			var priceB = BillingTestHelper.AddPriceItem(prices1, "BBB", BillingConstants.FeeType.PerDevicePerMonth, "", 10m);
			var priceA = BillingTestHelper.AddPriceItem(prices1, "AAA", BillingConstants.FeeType.PerDevicePerMonth, "", 7m);

			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "AA2");
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranch.PK);
			BillingTestHelper.SetInvoicingTo(lic2, lic1.Company);
			var db1 = lic1.Database;

			var licInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA3");
			licInactive.LA_IsActive = false;

			var licOrgInactive = BillingTestHelper.CreateAnotherLicence(lic1, "AA4");
			licOrgInactive.Company.Header.OH_IsActive = false;

			var lic3 = BillingTestHelper.CreateLicence(Factory, "BBB");
			BillingTestHelper.SetInvoicing(lic3, Env.CurrentBranch.PK);
			var db3 = lic3.Database;

			var licDbInactive = BillingTestHelper.CreateLicence(Factory, "CCC");
			licDbInactive.Database.LD_IsActive = false;

			var licFuture = BillingTestHelper.CreateLicence(Factory, "FFF");

			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var org3 = lic3.Company.Header;

			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2013, 2, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db1, "BBB", new ZDateTime(2012, 1, 1), new ZDateTime(2012, 12, 31));
			BillingTestHelper.CreatePremiumService(db3, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			BillingTestHelper.CreatePremiumService(db1, "AAA", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(db1, "#AA", new ZDateTime(2013, 1, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(db3, "AAA", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));
			BillingTestHelper.CreatePremiumService(db3, "#AA", new ZDateTime(2013, 1, 1), new ZDateTime(2013, 12, 31));

			BillingTestHelper.CreatePremiumService(licFuture.Database, "BBB", new ZDateTime(2013, 2, 1), ZDateTime.Empty);
			BillingTestHelper.CreatePremiumService(licDbInactive.Database, "BBB", new ZDateTime(2013, 1, 1), ZDateTime.Empty);

			Factory.Save();

			var billing = new PremiumServiceBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31));
			var bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("bills loaded", 2, bills.Length);
			var bill1 = bills.First(x => x.OrganisationPK == org1.PK);
			var bill2 = bills.First(x => x.OrganisationPK == org3.PK);

			AssertEquals("org1 has the usage as the DB owner", 2, bill1.SystemUsages.Count);
			AssertEquals("org3 pays for itself", 2, bill2.SystemUsages.Count);

			foreach (var bill in bills)
			{
				foreach (SystemUsage systemUsage in bill.SystemUsages)
				{
					AssertEquals(true, systemUsage is PremiumServiceUsage);
				}
			}

			AssertEquals(true, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(false, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "#AA"));
			AssertEquals(true, bill1.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "AAA"));
			AssertEquals(false, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "#AA"));
			AssertEquals(true, bill2.SystemUsages.Any(x => x.SystemCode == BillingConstants.BillingSystem.Service && x.SubCode == "BBB"));

			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31), org1.PK);
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("Bill for Context.Organisation loaded", 1, bills.Length);

			billing = new PremiumServiceBillingSystem();
			context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2013, 1, 31), org3.PK);
			bills = billing.LoadSystemBills(context).Cast<PremiumServiceBill>().ToArray();
			AssertEquals("Bill for Context.Organisation loaded", 1, bills.Length);

			var services = PremiumServiceBillingSystem.GetDue(Factory, new ZDateTime(2013, 1, 1), new[] { db1.PK.ToGuid(), db3.PK.ToGuid() }, null);
			AssertEquals(true, services.Any(x => x.CPS_Type == "AAA"));
			AssertEquals(true, services.Any(x => x.CPS_Type == "BBB"));
			AssertEquals(false, services.Any(x => x.CPS_Type == "#AA"));
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
