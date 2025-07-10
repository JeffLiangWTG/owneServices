using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class E2EBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new E2EBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.E2E, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDCOMSYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = "DDD";

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2015, 1, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			BillingTestHelper.AddPriceItem(priceHeader, "E2E", BillingConstants.FeeType.Transactional, "", 0.10m).L7_Description = "E2E Messaging";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.E2E, "E2E", new ZDateTime(2015, 11, 1), org.LicCompany.PK, 30);
			Factory.Save();

			var billingSystem = new E2EBillingSystem();
			BillingRunContext context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2015, 11, 30));

			var bill = billingSystem.LoadSystemBills(context).First();
			AssertEquals(BillingConstants.BillingSystem.E2E, bill.SystemCode);

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("E2E Messaging - 30 Transactions at AUD 0.10 per Transaction", lines[0].Description);
			});
		}

		public void TestLoadOdplRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");

			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1a", "ref2a", "ref3a", "ref4a", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1b", "ref2b", "ref3b", "ref4b", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1c", "ref2c", "ref3c", "ref4c", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new E2EBillingSystem();
			var licence = org.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 11, 1), org.PK, clientCompany.PK, licence.Company.PK, licence.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as SystemCodeRawUsage;
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.Summary.Lines[0], "DDDABCSYD", "ref1a");
				AssertOdplRawUsage(rawUsage.Summary.Lines[1], "DDDABCSYD", "ref1b");
				AssertOdplRawUsage(rawUsage.Summary.Lines[2], "DDDABCSYD", "ref1c");
				AssertOdplRawUsage(rawUsage.Summary.Lines[3], "DDDABCSYD", "");
			});

			string expectedCsvResult =
@"'Client ID','Sender','WIP','Top Job','Job 1','Job 2 / Event','Job 3','Tracking ID','Message Time (UTC)'
'DDDABCSYD','ref1a','N','ref2a','ref3a','ref4a','','','01-Nov-15 10:00'
'DDDABCSYD','ref1b','N','ref2b','ref3b','ref4b','','','02-Nov-15 10:00'
'DDDABCSYD','ref1c','N','ref2c','ref3c','ref4c','','','03-Nov-15 10:00'
'DDDABCSYD','','N','','','','','','30-Nov-15 00:00'
".Replace('\'', '"');

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""01-Nov-15 10:00"",""DDDABCSYD"",""B10"",""S10"",""ref1a ref2a ref3a ref4a"","""","""",""1""
""02-Nov-15 10:00"",""DDDABCSYD"",""B11"",""S11"",""ref1b ref2b ref3b ref4b"","""","""",""1""
""03-Nov-15 10:00"",""DDDABCSYD"",""B12"",""S12"",""ref1c ref2c ref3c ref4c"","""","""",""1""
""30-Nov-15 00:00"",""DDDABCSYD"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		public void TestLoadStlRawUsage()
		{
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");

			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 4809;
			var clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");

			Factory.Save();

			var clientNumber = db.DatabaseId + ".ABC";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 1, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1a", "ref2a", "ref3a", "ref4a", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 2, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1b", "ref2b", "ref3b", "ref4b", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 3, 10, 0, 0), "DDDABCSYD", clientNumber, db.DatabaseId, clientCompany.PK, "ref1c", "ref2c", "ref3c", "ref4c", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2015, 11, 30, 0, 0, 0), "DDDABCSYD", null, db.DatabaseId, clientCompany.PK, "", null, null, null, DateTime.Now, "HUB", 1));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new E2EBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2015, 11, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context);
			AssertEquals(4, rawUsage.Summary.Lines.Count);
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.Summary.Lines[0], "ABC", "ref1a");
				AssertStlRawUsage(rawUsage.Summary.Lines[1], "ABC", "ref1b");
				AssertStlRawUsage(rawUsage.Summary.Lines[2], "ABC", "ref1c");
				AssertStlRawUsage(rawUsage.Summary.Lines[3], "ABC", "");
			});

			string expectedCsvResult =
@"'Company Code','Sender','WIP','Top Job','Job 1','Job 2 / Event','Job 3','Tracking ID','Message Time (UTC)'
'ABC','ref1a','N','ref2a','ref3a','ref4a','','','01-Nov-15 10:00'
'ABC','ref1b','N','ref2b','ref3b','ref4b','','','02-Nov-15 10:00'
'ABC','ref1c','N','ref2c','ref3c','ref4c','','','03-Nov-15 10:00'
'ABC','','N','','','','','','30-Nov-15 00:00'
".Replace('\'', '"');

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Nov-15 10:00"",""ABC"",""B10"",""S10"",""ref1a ref2a ref3a ref4a"","""","""",""1""
""02-Nov-15 10:00"",""ABC"",""B11"",""S11"",""ref1b ref2b ref3b ref4b"","""","""",""1""
""03-Nov-15 10:00"",""ABC"",""B12"",""S12"",""ref1c ref2c ref3c ref4c"","""","""",""1""
""30-Nov-15 00:00"",""ABC"",""B13"",""S13"","""","""","""",""1""
", writer.ToString());
		}

		public void TestLoadRawUsageE2E_E2W()
		{
			var licReceiver = BillingTestHelper.CreateLicence(Factory, "EN0", "CM0", "SV0", true);
			var dbReceiver = licReceiver.Database;
			dbReceiver.LD_DatabaseNumber = 1900;

			var prices = BillingTestHelper.CreateStlPriceList(licReceiver.Company, "E2E", "E2W");
			var e2ePriceItem = prices.Items[0];
			var e2wPriceItem = prices.Items[1];

			BillingTestHelper.CreatePriceLink(licReceiver.Database, prices, new ZDateTime(2017, 1, 1));

			var licSender1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "SV1", true);
			var dbSender1 = licSender1.Database;
			dbSender1.LD_DatabaseNumber = 1901;

			var licSender2 = BillingTestHelper.CreateLicence(Factory, "EN2", "CM2", "SV2", true);
			var dbSender2 = licSender2.Database;
			dbSender2.LD_DatabaseNumber = 1902;

			var licSender3 = BillingTestHelper.CreateLicence(Factory, "EN3", "CM3", "SV3", true);
			var dbSender3 = licSender3.Database;
			dbSender3.LD_DatabaseNumber = 1903;

			var orgWIP = Factory.NewWithValidTestData<EDIOrgHeader>();
			orgWIP.OH_IsUserFlag24 = true; //Marketing Option of  "WiseIndustry Partner" 

			dbSender1.ClientCompanies[0].Org.SetRelatedParty(orgWIP, "WRP", "CM");
			dbSender1.ClientCompanies[0].Org.AllRelatedParties[0].PR_SystemCreateTimeUtc = new ZDateTime(2017, 1, 1);

			Factory.Save();

			var infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 2), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, licReceiver.ClientCompany.PK, "EN0CM0SV0", "", null, null, null, "STL", 2));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 3), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, licReceiver.ClientCompany.PK, "EN1CM1SV1", "", null, null, null, "STL", 4));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 4), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, licReceiver.ClientCompany.PK, "EN2CM2SV2", "", null, null, null, "STL", 8));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 5), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, licReceiver.ClientCompany.PK, "EN3CM3SV3", "", null, null, null, "STL", 16));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("E2E", "E2E", new ZDateTime(2017, 5, 6), "EN0CM0SV0", "EN0CM0SV0", dbReceiver.DatabaseId, licReceiver.ClientCompany.PK, "EN9CM9SV9", "", null, null, null, "STL", 32));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new E2EBillingSystem();
			var odplContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2017, 5, 1), licReceiver.Company.LC_OH, licReceiver.ClientCompany.PK, licReceiver.LA_LC, licReceiver.LA_LD);
			var odplRawUsage = billingSystem.LoadOdplRawUsage(odplContext) as SystemCodeRawUsage;

			var nonwipContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2017, 5, 1), licReceiver.LA_LD, e2ePriceItem.PK, licReceiver.ClientCompany.PK);
			var wipContext = new BillingLoadRawUsageContext(Factory, new ZDateTime(2017, 5, 1), licReceiver.LA_LD, e2wPriceItem.PK, licReceiver.ClientCompany.PK);

			var nonwipUsage = billingSystem.LoadStlRawUsage(nonwipContext);
			var wipUsage = billingSystem.LoadStlRawUsage(wipContext);

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(odplContext, false, (csv) => { builder.AppendLine(csv); });

			AssertEquals(5, odplRawUsage.Summary.Lines.Count);
			AssertEquals(4, nonwipUsage.Summary.Lines.Count);
			AssertEquals(1, wipUsage.Summary.Lines.Count);

			CombineAssertions(() =>
			{
				AssertOdplRawUsage(odplRawUsage.Summary.Lines[0], "EN0CM0SV0", "EN0CM0SV0");
				AssertOdplRawUsage(odplRawUsage.Summary.Lines[1], "EN0CM0SV0", "EN1CM1SV1 (WIP)");
				AssertOdplRawUsage(odplRawUsage.Summary.Lines[2], "EN0CM0SV0", "EN2CM2SV2");
				AssertOdplRawUsage(odplRawUsage.Summary.Lines[3], "EN0CM0SV0", "EN3CM3SV3");
				AssertOdplRawUsage(odplRawUsage.Summary.Lines[4], "EN0CM0SV0", "EN9CM9SV9");
			});

			CombineAssertions(() =>
			{
				AssertStlRawUsage(nonwipUsage.Summary.Lines[0], "CM0", "EN0CM0SV0");
				AssertStlRawUsage(nonwipUsage.Summary.Lines[1], "CM0", "EN2CM2SV2");
				AssertStlRawUsage(nonwipUsage.Summary.Lines[2], "CM0", "EN3CM3SV3");
				AssertStlRawUsage(nonwipUsage.Summary.Lines[3], "CM0", "EN9CM9SV9");
			});

			CombineAssertions(() =>
			{
				AssertStlRawUsage(wipUsage.Summary.Lines[0], "CM0", "EN1CM1SV1");
			});

			string expectedCsvResult =
@"'Client ID','Sender','WIP','Top Job','Job 1','Job 2 / Event','Job 3','Tracking ID','Message Time (UTC)'
'EN0CM0SV0','EN0CM0SV0','N','','','','','','02-May-17 00:00'
'EN0CM0SV0','EN1CM1SV1','Y','','','','','','03-May-17 00:00'
'EN0CM0SV0','EN2CM2SV2','N','','','','','','04-May-17 00:00'
'EN0CM0SV0','EN3CM3SV3','N','','','','','','05-May-17 00:00'
'EN0CM0SV0','EN9CM9SV9','N','','','','','','06-May-17 00:00'
".Replace('\'', '"');
			AssertEquals(expectedCsvResult, builder.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string senderId)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Sender", senderId, summaryLine.Column2);
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string consol)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Consol", consol, summaryLine.Column2);
		}
	}
}