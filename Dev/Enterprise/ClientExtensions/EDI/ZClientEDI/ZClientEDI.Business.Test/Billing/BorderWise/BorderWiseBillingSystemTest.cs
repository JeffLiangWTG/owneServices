using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Billing.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Billing.BorderWise.Test
{
	sealed class BorderWiseBillingSystemTest : TestCaseWithFactory
	{
		public void TestSystemCode()
		{
			var billing = new BorderWiseBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.BorderWise, billing.SystemCode);
		}

		public void TestCreateSystemUsages_ChargeableUsageNotOnDemand()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);

			var licCompany1 = BillingTestHelper.CreateLicenceCompany(Factory, "EN1", "CO1");
			var usageWithoutDatabase = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, "BW1", periodStart, licCompany1.PK, 10);

			var licBORStl = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1");
			var usageSTL = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, "BW1", periodStart, licBORStl.Company.PK, 10);

			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));
			var bills = billingSystem.LoadSystemBills(context);
			AssertEquals(0, bills.Length);
		}

		public void TestCreateSystemBill_CurrencyPerItem()
		{
			var periodStart = new ZDateTime(2018, 7, 1);
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);

			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, licBOR.LA_LC, 13);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.GlobalStandalonePriceCode, periodStart, licBOR.LA_LC, 7);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var bills = billingSystem.LoadSystemBills(context).ToList();
			AssertEquals(2, bills.Count);
			var billAUD = bills.Single(x => x.CurrencyCode == "AUD");
			var billUSD = bills.Single(x => x.CurrencyCode == "USD");
		}

		public void TestCreateSystemBill_LicenceUnits()
		{
			var periodStart = new ZDateTime(2018, 10, 1);
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany, 10);
			var priceItem = borderWisePrices.Items.FindByCode(BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode);
			priceItem.L7_Price = 75m;
			priceItem.L7_LicenceUnits = 750m;

			var licBOR = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode, periodStart, licBOR.LA_LC, 13);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var bills = billingSystem.LoadSystemBills(context).ToList();
			AssertEquals(1, bills.Count);
			var bill1 = bills[0];
			var usage1 = bill1.SystemUsages[0];
			AssertEquals("LicenceUnitsAmount", 13 * 750m / 50m, usage1.LicenceUnitsAmount);
			var summarySections = usage1.GetGeneralSummarySections();
			AssertEquals(1, summarySections.Length);
			AssertEquals("15.0", summarySections[0].Lines[0].LicenceUnits);
			AssertEquals("195.0", summarySections[0].Lines[0].LicenceUnitsAmount);
		}

		public void TestLoadOdplRawUsage_201807_Edition()
		{
			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			var periodStart = new ZDateTime(2018, 7, 1);
			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var lic2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			var cw1Licence = BillingTestHelper.CreateAnotherDatabase(lic1, "PRD");
			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var ftaMember = org1.Memberships.AddNew();
			ftaMember.EOR_MembershipType = "FTA";
			ftaMember.EOR_ValidFrom = periodStart.Date;
			var ftaMemberDupe = org1.Memberships.AddNew();
			ftaMemberDupe.EOR_MembershipType = "FTA";
			ftaMemberDupe.EOR_ValidFrom = periodStart.Date.AddMonths(-1);
			ftaMemberDupe.EOR_ValidTo = periodStart.Date.AddMonths(1).AddDays(-1);
			var cbaffMember = org2.Memberships.AddNew();
			cbaffMember.EOR_MembershipType = "CBAFF";
			cbaffMember.EOR_ValidFrom = periodStart.Date;
			{
				var contact1 = (EDIOrgContact)org1.Contacts[0];
				contact1.OC_RN_NKNationality = "NZ";
				CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart, "Machine 1a", "Global", "");
				// Usage not in the month
				CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart.AddMonths(1), "Machine 1b", "Global", "");
				CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart.AddMonths(-1), "Machine 1b", "Global", "");
			}
			{
				var contact2 = (EDIOrgContact)org1.Contacts.AddNew();
				contact2.OC_Email = "user2@test.com";
				contact2.OC_ContactName = "User Two";

				CreateBorderWiseUsage(infoList, lic1, contact2, "AU3", periodStart.AddMinutes(5), "Machine 1b", "Single AU Pro", "AU");
			}
			{
				var contact3 = (EDIOrgContact)org1.Contacts.AddNew();
				contact3.OC_Email = "user3@test.com";
				contact3.OC_ContactName = "User Three";
			}
			{
				// Free trial for whole month, after they registered
				var contact4 = (EDIOrgContact)org2.Contacts.AddNew();
				contact4.OC_Email = "user4@test.com";
				contact4.OC_ContactName = "User Four";
				contact4.OC_RN_NKNationality = "NZ";
				var countryFreeTrialCertificate = contact4.Certificates.AddNew();
				countryFreeTrialCertificate.XZ_Comment = "BorderWise Country Free Trial";
				countryFreeTrialCertificate.XZ_Type = "MSC";
				countryFreeTrialCertificate.XZ_IssueDate = periodStart;
				countryFreeTrialCertificate.XZ_ExpiryOrDueDate = periodStart.AddMonths(1).AddDays(-1);

				CreateBorderWiseUsage(infoList, lic2, contact4, "NZS", periodStart, "Machine 4", "Single NZ", "NZ");
			}
			{
				// Free trial that begins part way through this month, and after they registered
				var contact5 = (EDIOrgContact)org2.Contacts.AddNew();
				contact5.OC_Email = "user5@test.com";
				contact5.OC_ContactName = "User Five";
				contact5.OC_RN_NKNationality = "AU";
				var freeTrialCertificate = contact5.Certificates.AddNew();
				freeTrialCertificate.XZ_Comment = "BorderWise Promo Free Trial";
				freeTrialCertificate.XZ_Type = "MSC";
				freeTrialCertificate.XZ_IssueDate = periodStart.AddDays(25);
				freeTrialCertificate.XZ_ExpiryOrDueDate = periodStart.AddMonths(2).AddDays(-1);

				CreateBorderWiseUsage(infoList, lic2, contact5, "AUS", periodStart.AddDays(5), "Machine 5", "Single AU", "AU");
			}
			{
				// Free trial that begins part way through last month, and after they registered
				var contact6 = (EDIOrgContact)org2.Contacts.AddNew();
				contact6.OC_Email = "user6@test.com";
				contact6.OC_ContactName = "User Six";
				contact6.OC_RN_NKNationality = "NZ";
				var freeTrialCertificate = contact6.Certificates.AddNew();
				freeTrialCertificate.XZ_Comment = "BorderWise Promo Free Trial";
				freeTrialCertificate.XZ_Type = "MSC";
				freeTrialCertificate.XZ_IssueDate = periodStart.AddMonths(-1).AddDays(25);
				freeTrialCertificate.XZ_ExpiryOrDueDate = periodStart.AddMonths(1).AddDays(-1);

				CreateBorderWiseUsage(infoList, lic2, contact6, "AUS", periodStart, "Machine 6", "Single AU", "AU");
			}
			{
				// Free trial that begins part way through this month, and on same day they registered
				var contact7 = (EDIOrgContact)org2.Contacts.AddNew();
				contact7.OC_Email = "user7@test.com";
				contact7.OC_ContactName = "User Seven";
				contact7.OC_RN_NKNationality = "AU";
				var freeTrialCertificate = contact7.Certificates.AddNew();
				freeTrialCertificate.XZ_Comment = "BorderWise Promo Free Trial";
				freeTrialCertificate.XZ_Type = "MSC";
				freeTrialCertificate.XZ_IssueDate = periodStart.AddDays(25);
				freeTrialCertificate.XZ_ExpiryOrDueDate = periodStart.AddMonths(2).AddDays(-1);

				CreateBorderWiseUsage(infoList, lic2, contact7, "AUS", periodStart.AddDays(25), "Machine 7", "Single AU", "AU");
			}

			var contactToIgnore = (EDIOrgContact)org1.Contacts.AddNew();
			contactToIgnore.OC_Email = "user@tradefoxinc.com";
			contactToIgnore.OC_ContactName = "User Tradefox";

			CreateBorderWiseUsage(infoList, lic1, contactToIgnore, "AUS", periodStart.AddDays(5), "Machine 1b", "Single AU", "AU");
			EServicesBillingTestHelper.AddTransactions(infoList);

			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, lic1.LA_LC, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			var expectedSectionAsText = @"Organization,User,Edition,Machine,Registered,Special Conditions,User Country,Membership,CargoWiseOne,
EN1CO1,EN1@test.com,Global,Machine 1a,2018-07-01 00:00,,NZ,,Y,
EN1CO1,user2@test.com,Single AU Pro,Machine 1b,2018-07-01 00:05,,AU,FTA,Y,";
			AssertSummarySection(rawUsage.Summary, expectedSectionAsText);

			context = new BillingLoadRawUsageContext(Factory, periodStart, org2.PK, ZGuid.Empty, lic2.LA_LC, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			string expectedCsvResult =
@"""Organization"",""User"",""Edition"",""Machine"",""Registered"",""Special Conditions"",""User Country"",""Membership"",""CargoWiseOne""
""EN2CO2"",""user4@test.com"",""Single NZ"",""Machine 4"",""2018-07-01 00:00"",""BorderWise Country Free Trial"",""NZ"",""CBAFF"",""N""
""EN2CO2"",""user5@test.com"",""Single AU"",""Machine 5"",""2018-07-06 00:00"","""",""AU"","""",""N""
""EN2CO2"",""user6@test.com"",""Single AU"",""Machine 6"",""2018-07-01 00:00"",""BorderWise Promo Free Trial"",""NZ"",""CBAFF"",""N""
""EN2CO2"",""user7@test.com"",""Single AU"",""Machine 7"",""2018-07-26 00:00"",""BorderWise Promo Free Trial"",""AU"","""",""N""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		public void TestLoadOdplRawUsage()
		{
			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			var periodStart = new ZDateTime(2018, 12, 1);
			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var lic2 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BOR");
			var cw1Licence = BillingTestHelper.CreateAnotherDatabase(lic1, "PRD", false);
			var org1 = lic1.Company.Header;
			var org2 = lic2.Company.Header;
			var ftaMember = org1.Memberships.AddNew();
			ftaMember.EOR_MembershipType = "FTA";
			ftaMember.EOR_ValidFrom = periodStart.Date;
			var cbaffMember = org2.Memberships.AddNew();
			cbaffMember.EOR_MembershipType = "CBAFF";
			cbaffMember.EOR_ValidFrom = periodStart.Date;

			var contact1 = (EDIOrgContact)org1.Contacts[0];
			contact1.OC_RN_NKNationality = "NZ";
			var contact2 = (EDIOrgContact)org1.Contacts.AddNew();
			contact2.OC_Email = "user2@test.com";
			contact2.OC_ContactName = "User Two";
			var contact3 = (EDIOrgContact)org1.Contacts.AddNew();
			contact3.OC_Email = "user3@test.com";
			contact3.OC_ContactName = "User Three";

			var contact4 = (EDIOrgContact)org2.Contacts.AddNew();
			contact4.OC_Email = "user4@test.com";
			contact4.OC_ContactName = "User Four";
			contact4.OC_RN_NKNationality = "NZ";
			var tradefoxCert = contact4.Certificates.AddNew();
			tradefoxCert.XZ_Comment = "TradeFox1";
			tradefoxCert.XZ_Type = "MSC";

			var contactToIgnore = (EDIOrgContact)org1.Contacts.AddNew();
			contactToIgnore.OC_Email = "user@tradefoxinc.com";
			contactToIgnore.OC_ContactName = "User Tradefox";

			CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart, "Machine 1a", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic1, contact2, "AUS", periodStart.AddMinutes(5), "Machine 1a", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic2, contact4, "AUS", periodStart, "Machine 4", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic1, contactToIgnore, "AUS", periodStart, "Machine 1b", "Single AU", "AU");

			// Usage not in the month
			CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart.AddMonths(1), "Machine 1c", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart.AddMonths(-1), "Machine 1d", "Single AU", "AU");

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, lic1.LA_LC, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1: rawUsage.Summary: Lines", 2, rawUsage.Summary.Lines.Count);
			var expectedSectionAsText = @"Organization,User,Edition,Machine,Registered,Special Conditions,User Country,Membership,CargoWiseOne,
EN1CO1,EN1@test.com,Single AU,Machine 1a,2018-12-01 00:00,,NZ,,Y,
EN1CO1,user2@test.com,Single AU,Machine 1a,2018-12-01 00:05,,AU,FTA,Y,";
			AssertSummarySection(rawUsage.Summary, expectedSectionAsText);

			context = new BillingLoadRawUsageContext(Factory, periodStart, org2.PK, ZGuid.Empty, lic2.LA_LC, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org2: rawUsage.Summary: Lines", 1, rawUsage.Summary.Lines.Count);

			string expectedCsvResult =
@"""Organization"",""User"",""Edition"",""Machine"",""Registered"",""Special Conditions"",""User Country"",""Membership"",""CargoWiseOne""
""EN2CO2"",""user4@test.com"",""Single AU"",""Machine 4"",""" + periodStart.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + @""","""",""NZ"",""CBAFF"",""N""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		public void TestLoadOdplRawUsage_Student()
		{
			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();

			var periodStart = new ZDateTime(2018, 12, 1);
			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var org1 = lic1.Company.Header;

			var contact1 = (EDIOrgContact)org1.Contacts[0];

			var student1 = (EDIOrgContact)org1.Contacts.AddNew();
			student1.OC_Email = "userstudent1@test.com";
			student1.OC_ContactName = "Student One";

			var studentCert = student1.Certificates.AddNew();
			studentCert.XZ_Comment = "Student - CBFCA";
			studentCert.XZ_Type = "MSC";

			CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart, "Machine 1a", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic1, student1, "AUS", periodStart.AddDays(6), "Student Machine 1a", "Single AU", "AU");

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, lic1.LA_LC, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1: rawUsage.Summary: Lines", 2, rawUsage.Summary.Lines.Count);
			var expectedSectionAsText = @"Organization,User,Edition,Machine,Registered,Special Conditions,User Country,Membership,CargoWiseOne,
EN1CO1,EN1@test.com,Single AU,Machine 1a,2018-12-01 00:00,,AU,,N,
EN1CO1,userstudent1@test.com,Single AU,Student Machine 1a,2018-12-07 00:00,Student - CBFCA,AU,,N,";
			AssertSummarySection(rawUsage.Summary, expectedSectionAsText);

			string expectedCsvResult =
@"""Organization"",""User"",""Edition"",""Machine"",""Registered"",""Special Conditions"",""User Country"",""Membership"",""CargoWiseOne""
""EN1CO1"",""EN1@test.com"",""Single AU"",""Machine 1a"",""2018-12-01 00:00"","""",""AU"","""",""N""
""EN1CO1"",""userstudent1@test.com"",""Single AU"",""Student Machine 1a"",""2018-12-07 00:00"",""Student - CBFCA"",""AU"","""",""N""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		public void TestLoadOdplRawUsage_MultipleOrgsShareDatabase()
		{
			EServicesBillingTestHelper.CreateTable();
			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			var periodStart = new ZDateTime(2018, 7, 1);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BOR");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2");
			var licCompany1 = lic1.Company;
			var licCompany2 = lic2.Company;
			var org1 = licCompany1.Header;
			var org2 = licCompany2.Header;

			var contact1 = (EDIOrgContact)org1.Contacts[0];

			var contact4 = (EDIOrgContact)org2.Contacts.AddNew();
			contact4.OC_Email = "user4@test.com";
			contact4.OC_ContactName = "User Four";
			var tradefoxCert = contact4.Certificates.AddNew();
			tradefoxCert.XZ_Comment = "TradeFox1";
			tradefoxCert.XZ_Type = "MSC";

			CreateBorderWiseUsage(infoList, lic1, contact1, "AUS", periodStart, "Machine 1a", "Single AU", "AU");
			CreateBorderWiseUsage(infoList, lic2, contact4, "AUS", periodStart, "Machine 4", "Single AU", "AU");

			EServicesBillingTestHelper.AddTransactions(infoList);
			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, periodStart, org1.PK, ZGuid.Empty, licCompany1.PK, ZGuid.Empty);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org1: rawUsage.Summary: Lines", 2, rawUsage.Summary.Lines.Count);
			var expectedSectionAsText = @"Organization,User,Edition,Machine,Registered,Special Conditions,User Country,Membership,CargoWiseOne,
EN1CO1,EN1@test.com,Single AU,Machine 1a,2018-07-01 00:00,,AU,,N,
EN1CO2,user4@test.com,Single AU,Machine 4,2018-07-01 00:00,,AU,,N,";
			AssertSummarySection(rawUsage.Summary, expectedSectionAsText);

			context = new BillingLoadRawUsageContext(Factory, periodStart, org2.PK, ZGuid.Empty, licCompany2.PK, ZGuid.Empty);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals("org2: rawUsage.Summary: Lines", 2, rawUsage.Summary.Lines.Count);
		}

		void AssertSummarySection(SummarySection summarySection, string expectedSectionAsText)
		{
			var linesAsText = string.Join("\r\n", new[] { summarySection.Header }.Concat(summarySection.Lines.OfType<SummaryLine>()).Select(x => x.CodeColumns1To10));
			AssertEquals(expectedSectionAsText, linesAsText);
		}

		EServicesBillingTestHelper.RawUsageInfo CreateBorderWiseUsage(List<EServicesBillingTestHelper.RawUsageInfo> infoList,
			LicenceHeader lic,
			EDIOrgContact contact,
			string priceCode,
			ZDateTime serviceTimeUtc,
			string machineId,
			string editionName,
			string editionCountry)
		{
			var usage = new EServicesBillingTestHelper.RawUsageInfo("BOR", priceCode, serviceTimeUtc, lic.LicenceCode, lic.Database.DatabaseId, lic.Database.DatabaseId,
				ZGuid.Empty, lic.Company.LC_OH.ToString(), contact.PK.ToString(), machineId, editionName, editionCountry, null, billableCount: 1);
			infoList.Add(usage);
			return usage;
		}

		public void TestLoadStlRawUsage()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "EEE", "LOL", "MEL");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "FFF", "III", "BRN");

			var db1 = lic1.Database;
			var db2 = lic2.Database;
			var db3 = lic3.Database;

			Factory.Save();

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2014, 2, 1), db1.PK, ZGuid.Empty, ZGuid.Empty);
			AssertExceptionThrown<InvalidOperationException>(() => billingSystem.LoadStlRawUsage(context));
			var writer = new CsvUsageReportWriterForTest();
			AssertExceptionThrown<InvalidOperationException>(() => billingSystem.LoadRawUsageInCsv(context, true, writer));
		}

		public void TestCreateSystemBill_NoCurrencyPerPriceItem()
		{
			var periodStart = new ZDateTime(2018, 6, 1);
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2", false);
			BillingTestHelper.SetInvoicing(lic2, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicingTo(lic2, lic1);

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, lic1.LA_LC, 7);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.ExtraMachinePriceCode, periodStart, lic1.LA_LC, 3);

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart.AddMonths(-1), lic1.LA_LC, 5);

			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BorderWiseBillingSystem.UserPriceCode, periodStart, lic2.LA_LC, 9);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billingSystem = new BorderWiseBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

			var bill = (BorderWiseSystemBill)billingSystem.LoadSystemBills(context).First();
			bill.ValidateAll(bill);
			AssertNoNotifications(bill);
			var usages = bill.SystemUsages.Cast<UniversalPriceSystemUsage>().ToList();
			AssertEquals(3, usages.Count);
			var usage1 = usages.First(x => x.ChargeableUsagePKs[0] == u1.PK);
			var usage2 = usages.First(x => x.ChargeableUsagePKs[0] == u2.PK);
			var usage3 = usages.First(x => x.ChargeableUsagePKs[0] == u3.PK);
			AssertEquals(lic1, usage1.User.UsageOwnerLicence);
			AssertEquals(lic1, usage2.User.UsageOwnerLicence);
			AssertEquals(lic2, usage3.User.UsageOwnerLicence);
			AssertEquals((7 + 9) * 200m + 3 * 30m, bill.Amount);
		}

		public void TestCreateSystemBill_PurchasedLicences()
		{
			var periodStart = new ZDateTime(2018, 7, 1);
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			lic1.Database.LD_OH_BillingParty = lic1.Company.LC_OH;
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "CO2", false);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");
			BillingTestHelper.SetInvoicingTo(lic2, lic1);

			var code1 = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			var code2 = BillingConstants.BorderWise.AUSingleWindowPartnerOnlyPriceCode;
			var code3 = BillingConstants.BorderWise.AUSingleWindowCW1OnlyPriceCode;

			var purchaseSetting1 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting1.LS9_ValidFrom = periodStart;
			purchaseSetting1.LS9_LD = lic1.LA_LD;
			purchaseSetting1.PriceCode = code1;
			purchaseSetting1.LicenceCount = 4;
			var purchaseSetting2 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting2.LS9_ValidFrom = periodStart;
			purchaseSetting2.LS9_LD = lic1.LA_LD;
			purchaseSetting2.PriceCode = code2;
			purchaseSetting2.LicenceCount = 2;
			lic1.Database.LicenceSettings.Add(purchaseSetting2);

			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart, lic1.LA_LC, 7);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart, lic2.LA_LC, 5);
			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code2, periodStart, lic1.LA_LC, 29);
			var u4 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code3, periodStart, lic1.LA_LC, 3);

			// next month they had less users than purchased licences
			var uNext1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart.AddMonths(1), lic1.LA_LC, 1);
			var uNext2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, code1, periodStart.AddMonths(1), lic2.LA_LC, 2);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			var price1 = borderWisePrices.Items.FindByCode(code1).L7_Price;
			var price2 = borderWisePrices.Items.FindByCode(code2).L7_Price;
			var price3 = borderWisePrices.Items.FindByCode(code3).L7_Price;
			Assert(price1 != 0m);
			Assert(price2 != 0m);
			Assert(price3 != 0m);

			var billingSystem = new BorderWiseBillingSystem();
			{
				var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

				var bills = billingSystem.LoadSystemBills(context).Cast<BorderWiseSystemBill>().ToArray();
				AssertEquals(1, bills.Length);
				var bill = bills[0];
				bill.ValidateAll(bill);
				AssertNoNotifications(bill);
				var usages = bill.SystemUsages.Cast<UniversalPriceSystemUsage>().ToList();
				AssertEquals(4, usages.Count);
				var usage1 = usages.First(x => x.ChargeableUsagePKs[0] == u1.PK);
				var usage2 = usages.First(x => x.ChargeableUsagePKs[0] == u2.PK);
				var usage3 = usages.First(x => x.ChargeableUsagePKs[0] == u3.PK);
				var usage4 = usages.First(x => x.ChargeableUsagePKs[0] == u4.PK);
				AssertEquals(lic1, usage1.User.UsageOwnerLicence);
				AssertEquals(lic2, usage2.User.UsageOwnerLicence);
				AssertEquals(lic1, usage3.User.UsageOwnerLicence);

				AssertEquals(7, usage1.TotalUnitCount);
				AssertEquals(3, usage1.UnitCount);
				AssertEquals(4, usage1.IncludedUnitCount);

				AssertEquals(5, usage2.TotalUnitCount);
				AssertEquals(5, usage2.UnitCount);
				AssertEquals(0, usage2.IncludedUnitCount);

				AssertEquals(29, usage3.TotalUnitCount);
				AssertEquals(27, usage3.UnitCount);
				AssertEquals(2, usage3.IncludedUnitCount);

				AssertEquals(3, usage4.TotalUnitCount);
				AssertEquals(3, usage4.UnitCount);
				AssertEquals(0, usage4.IncludedUnitCount);

				AssertEquals((7 + 5 - 4) * price1 + (29 - 2) * price2 + 3 * price3, bill.Amount);
			}

			// month 2
			{
				var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(2).AddDays(-1));
				var bills = billingSystem.LoadSystemBills(context).Cast<BorderWiseSystemBill>().ToArray();
				AssertEquals(1, bills.Length);

				var bill = bills[0];
				bill.ValidateAll(bill);
				AssertNoNotifications(bill);
				var usages = bill.SystemUsages.Cast<UniversalPriceSystemUsage>().ToList();
				AssertEquals(2, usages.Count);
				var usage4 = usages.First(x => x.ChargeableUsagePKs[0] == uNext1.PK);
				var usage5 = usages.First(x => x.ChargeableUsagePKs[0] == uNext2.PK);

				AssertEquals(1, usage4.TotalUnitCount);
				AssertEquals(0, usage4.UnitCount);
				AssertEquals(1, usage4.IncludedUnitCount);

				AssertEquals(2, usage5.TotalUnitCount);
				AssertEquals(0, usage5.UnitCount);
				AssertEquals(3, usage5.IncludedUnitCount);
			}
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateSystemBill_PurchasedLicencesGroup()
		{
			var periodStart = BillingTestHelper.MonthToday;

			var globalUserCode = BillingConstants.BorderWise.GlobalStandalonePriceCode;
			var globalProPackCode = BillingConstants.BorderWise.GlobalProPackPriceCode;
			var countryUserCode1 = BillingConstants.BorderWise.AUSingleWindowStandalonePriceCode;
			var countryUserCode2 = BillingConstants.BorderWise.AUSingleWindowPartnerCW1PriceCode;
			var countryProPackCode = BillingConstants.BorderWise.AUProPackPriceCode;

			BillingTestHelper.CreateExchangeRate(Factory, "USD", 2);

			var priceCodeGroup = new CodeDescriptionPairList();
			priceCodeGroup.AddPair("GALL", string.Join(", ", globalUserCode, countryUserCode1, countryUserCode2));
			priceCodeGroup.AddPair("GPRO", string.Join(", ", globalProPackCode, countryProPackCode));
			EDIDataRegistry.Instance.BorderWisePurchasedGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, priceCodeGroup);

			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateBorderWisePriceList(stdLicCompany);

			var licBOR1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN2", "CO2", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			licBOR1.Database.LD_OH_BillingParty = licBOR1.Company.LC_OH;
			var licBOR2 = BillingTestHelper.CreateAnotherLicence(licBOR1, "CO3", false);
			BillingTestHelper.SetInvoicingTo(licBOR2, licBOR1);

			var purchaseSetting1 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting1.LS9_ValidFrom = periodStart;
			purchaseSetting1.LS9_LD = licBOR1.LA_LD;
			purchaseSetting1.LicenceCount = 20;
			purchaseSetting1.PriceCode = "GALL";
			licBOR1.Database.LicenceSettings.Add(purchaseSetting1);

			var purchaseSetting2 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting2.LS9_ValidFrom = periodStart;
			purchaseSetting2.LS9_LD = licBOR1.LA_LD;
			purchaseSetting2.LicenceCount = 10;
			purchaseSetting2.PriceCode = "GPRO";

			var purchaseSetting3 = Factory.New<BorderWisePurchasedLicenceSetting>();
			purchaseSetting3.LS9_ValidFrom = periodStart;
			purchaseSetting3.LS9_LD = licBOR1.LA_LD;
			purchaseSetting3.LicenceCount = 1;
			purchaseSetting3.PriceCode = countryUserCode2;

			// 12 global users, 6 country user code1, 4 country user code2
			// 7 global pro-packs, 5 country user pro packs
			var u1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart, licBOR1.LA_LC, 7);
			var u2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart, licBOR2.LA_LC, 5);
			var u3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode1, periodStart, licBOR1.LA_LC, 1);
			var u4 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode1, periodStart, licBOR2.LA_LC, 5);
			var u5 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart, licBOR1.LA_LC, 2);
			var u6 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart, licBOR2.LA_LC, 2);

			var u7 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalProPackCode, periodStart, licBOR1.LA_LC, 4);
			var u8 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalProPackCode, periodStart, licBOR2.LA_LC, 3);
			var u9 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryProPackCode, periodStart, licBOR2.LA_LC, 5);

			// next month global users consume all licences
			var v1 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart.AddMonths(1), licBOR1.LA_LC, 15);
			var v2 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, globalUserCode, periodStart.AddMonths(1), licBOR2.LA_LC, 6);
			var v3 = BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, countryUserCode2, periodStart.AddMonths(1), licBOR1.LA_LC, 2);

			BillingTestHelper.CreateChargeCodesForBranch(Factory, GlbBranch.CurrentBranch);

			Factory.Save();
			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);
			BillingTestHelper.LoadClientSpecificDocuments();

			var billingSystem = new BorderWiseBillingSystem();
			{
				var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(1).AddDays(-1));

				var bills = billingSystem.LoadSystemBills(context).Cast<BorderWiseSystemBill>().ToList();
				foreach (var bill in bills)
				{
					bill.ValidateAll(bill);
					AssertNoNotifications(bill);
				}
				AssertEquals(4, bills.Count);
				var usages = bills.SelectMany(x => x.SystemUsages.Cast<UniversalPriceSystemUsage>()).ToList();
				AssertEquals(9, usages.Count);
				var usage1 = usages.First(x => x.ChargeableUsagePKs[0] == u1.PK);
				var usage2 = usages.First(x => x.ChargeableUsagePKs[0] == u2.PK);
				var usage3 = usages.First(x => x.ChargeableUsagePKs[0] == u3.PK);
				var usage4 = usages.First(x => x.ChargeableUsagePKs[0] == u4.PK);
				var usage5 = usages.First(x => x.ChargeableUsagePKs[0] == u5.PK);
				var usage6 = usages.First(x => x.ChargeableUsagePKs[0] == u6.PK);
				var usage7 = usages.First(x => x.ChargeableUsagePKs[0] == u7.PK);
				var usage8 = usages.First(x => x.ChargeableUsagePKs[0] == u8.PK);
				var usage9 = usages.First(x => x.ChargeableUsagePKs[0] == u9.PK);
				AssertEquals(licBOR1, usage1.User.UsageOwnerLicence);
				AssertEquals(licBOR2, usage2.User.UsageOwnerLicence);
				AssertEquals(licBOR1, usage3.User.UsageOwnerLicence);
				AssertEquals(licBOR2, usage4.User.UsageOwnerLicence);
				AssertEquals(licBOR1, usage5.User.UsageOwnerLicence);
				AssertEquals(licBOR2, usage6.User.UsageOwnerLicence);
				AssertEquals(licBOR1, usage7.User.UsageOwnerLicence);
				AssertEquals(licBOR2, usage8.User.UsageOwnerLicence);
				AssertEquals(licBOR2, usage9.User.UsageOwnerLicence);

				AssertEquals(7, usage1.TotalUnitCount);
				AssertEquals(0, usage1.UnitCount);
				AssertEquals(7, usage1.IncludedUnitCount);

				AssertEquals(5, usage2.TotalUnitCount);
				AssertEquals(0, usage2.UnitCount);
				AssertEquals(5, usage2.IncludedUnitCount);

				AssertEquals(1, usage3.TotalUnitCount);
				AssertEquals(0, usage3.UnitCount);
				AssertEquals(1, usage3.IncludedUnitCount);

				AssertEquals(5, usage4.TotalUnitCount);
				AssertEquals(0, usage4.UnitCount);
				AssertEquals(5, usage4.IncludedUnitCount);

				AssertEquals(2, usage5.TotalUnitCount);
				AssertEquals(0, usage5.UnitCount);
				AssertEquals(2, usage5.IncludedUnitCount);

				AssertEquals(2, usage6.TotalUnitCount);
				AssertEquals(1, usage6.UnitCount);
				AssertEquals(1, usage6.IncludedUnitCount);

				AssertEquals(4, usage7.TotalUnitCount);
				AssertEquals(0, usage7.UnitCount);
				AssertEquals(4, usage7.IncludedUnitCount);

				AssertEquals(3, usage8.TotalUnitCount);
				AssertEquals(0, usage8.UnitCount);
				AssertEquals(3, usage8.IncludedUnitCount);

				AssertEquals(5, usage9.TotalUnitCount);
				AssertEquals(2, usage9.UnitCount);
				AssertEquals(3, usage9.IncludedUnitCount);
			}

			// month 2
			{
				var context = new BillingRunContext(Factory, ZDateTime.Today, periodStart.AddMonths(2).AddDays(-1));
				var bills = billingSystem.LoadSystemBills(context).Cast<BorderWiseSystemBill>().ToArray();
				foreach (var bill in bills)
				{
					bill.ValidateAll(bill);
					AssertNoNotifications(bill);
				}
				AssertEquals(3, bills.Length);

				var usages = bills.SelectMany(x => x.SystemUsages.Cast<UniversalPriceSystemUsage>()).ToList();
				AssertEquals(3, usages.Count);
				var usage1 = usages.First(x => x.ChargeableUsagePKs[0] == v1.PK);
				var usage2 = usages.First(x => x.ChargeableUsagePKs[0] == v2.PK);
				var usage3 = usages.First(x => x.ChargeableUsagePKs[0] == v3.PK);

				AssertEquals(15, usage1.TotalUnitCount);
				AssertEquals(0, usage1.UnitCount);
				AssertEquals(15, usage1.IncludedUnitCount);

				AssertEquals(6, usage2.TotalUnitCount);
				AssertEquals(1, usage2.UnitCount);
				AssertEquals(5, usage2.IncludedUnitCount);

				AssertEquals(2, usage3.TotalUnitCount);
				AssertEquals(1, usage3.UnitCount);
				AssertEquals(1, usage3.IncludedUnitCount);
			}
		}

		public void TestCreateSystemBill_StudentMix()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode, periodStart, lic1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode2, periodStart, lic1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.UserPriceCode3, periodStart, lic1.LA_LC, 7);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.ExtraMachinePriceCode, periodStart, lic1.LA_LC, 3);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode2, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode3, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentExtraMachinePriceCode, periodStart, lic1.LA_LC, 1);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billing = new MonthlyUsageBilling(Factory);
			var borderWiseBilling = new BorderWiseBillingSystem();
			billing.BillingSystems.Clear();
			billing.BillingSystems.Add(borderWiseBilling);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(1, billing.OrganisationBills.Count);
			var bill = billing.OrganisationBills[0];
			AssertNoErrors(bill);
			AssertEquals(7 * (200m + 50m + 40m) + 3 * 30m, bill.Amount);
		}

		public void TestCreateSystemBill_StudentOnly()
		{
			var periodStart = BillingTestHelper.MonthToday;
			var stdLicHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdLicHeader.Company;
			var borderWisePrices = BillingTestHelper.CreateLegacyBorderWisePriceList(stdLicCompany);

			var lic1 = BillingTestHelper.CreateBorderWiseLicence(Factory, "EN1", "CO1", "BW1", LicenceAdvStdOthList.Codes.OnDemand);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranchPK, "AUD");

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode2, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentUserPriceCode3, periodStart, lic1.LA_LC, 2);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.BorderWise, BillingConstants.BorderWise.StudentExtraMachinePriceCode, periodStart, lic1.LA_LC, 1);

			Factory.Save();

			ClientLicencePriceHeaderCollectionTest.SetStandardPriceCompanyLicence(stdLicHeader);

			var billing = new MonthlyUsageBilling(Factory);
			var borderWiseBilling = new BorderWiseBillingSystem();
			billing.BillingSystems.Clear();
			billing.BillingSystems.Add(borderWiseBilling);
			billing.DateTo = periodStart.AddMonths(1).AddDays(-1);
			billing.GenerateReport(null);
			AssertEquals(0, billing.OrganisationBills.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}
	}
}
