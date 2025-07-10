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
	public class GBCustomsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new GBCustomsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.GBCustoms, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var billingSystem = new GBCustomsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 4, 30));

			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDAAASYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";

			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "GBC", BillingConstants.FeeType.Transactional, "", 0.03m);
			priceItem.L7_Description = "Messaging Service Bureau - Management, Retention and Compliance - GB";
			priceItem.L7_RX_NKCurrency = "GBP";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GBCustoms, "AWB", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GBCustoms, "GTM", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 30);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.GBCustoms, "CUE", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 20);

			Factory.Save();

			var bill = billingSystem.LoadSystemBills(context).First();
			AssertEquals(BillingConstants.BillingSystem.GBCustoms, bill.SystemCode);

			var lines = new List<SystemBill.BillLine>();
			bill.CreateInvoiceLines(lines, ZDateTime.Now);

			AssertEquals(1, lines.Count);
			CombineAssertions(() =>
			{
				AssertEquals("GB Customs - 100 Transactions at GBP 0.03 per Transaction", lines[0].Description);
			});
		}

		public void TestLoadOdplRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "FFF", "III", "BRN");

			var db1 = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db1.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db1.PK, org1.PK, "", "");

			var db2 = org2.LicCompany.ActiveOrAllLicDatabases[0];
			db2.LD_DatabaseNumber = 5555;
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db2.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db1.DatabaseId + ".ABC";
			var clientNumber2 = db2.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "61898196346", "PS001972", "CUK", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "GTM", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "TRX", "CUKFFW98000DBC", "CUKFFW98000ZEB", "Text message (TXT)", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "CUE", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "SLHR012353", "IMPIMAAIR", "6GB800547754000-SLHR012353", "120-001278R", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "11212196951", "L1314850", "CUK", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "GTM", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "RCV", "CUKSYS98COMMDB", "CUKFFW98000DBC", "Fallback invocation/revocation (BCM)", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "CUE", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "SLGW029247", "EXPEXDAIR", "6GB800547754000-SLGW029247", "120-A07546V", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new GBCustomsBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.SummarySections[0].Lines[0], "DDDABCSYD", "61898196346", "PS001972", "CUK");
				AssertOdplRawUsage(rawUsage.SummarySections[1].Lines[0], "DDDABCSYD", "Text message (TXT)", "TRX", "CUKFFW98000DBC", "CUKFFW98000ZEB");
				AssertOdplRawUsage(rawUsage.SummarySections[2].Lines[0], "DDDABCSYD", "SLHR012353", "IMPIMAAIR", "6GB800547754000-SLHR012353", "120-001278R");
			});

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			CombineAssertions(() =>
			{
				AssertOdplRawUsage(rawUsage.SummarySections[0].Lines[0], "FFFIIIBRN", "11212196951", "L1314850", "CUK");
				AssertOdplRawUsage(rawUsage.SummarySections[0].Lines[1], "FFFIIIBRN", "", "", "");
				AssertOdplRawUsage(rawUsage.SummarySections[1].Lines[0], "FFFIIIBRN", "Fallback invocation/revocation (BCM)", "RCV", "CUKSYS98COMMDB", "CUKFFW98000DBC");
				AssertOdplRawUsage(rawUsage.SummarySections[2].Lines[0], "FFFIIIBRN", "SLGW029247", "EXPEXDAIR", "6GB800547754000-SLGW029247", "120-A07546V");
			});

			string expectedCsvResult =
@"""Message Type"",""Client ID"",""MAWB"",""HAWB"",""Application Code"",""Message Time (UTC)""
""Air Waybills"",""FFFIIIBRN"",""11212196951"",""L1314850"",""CUK"",""03-Mar-16 12:00""
""Air Waybills"",""FFFIIIBRN"","""","""","""",""31-Mar-16 00:00""
""Message Type"",""Client ID"",""Message Purpose"",""Direction"",""Sender"",""Recipient"",""Message Time (UTC)""
""General Text Messages"",""FFFIIIBRN"",""Fallback invocation/revocation (BCM)"",""RCV"",""CUKSYS98COMMDB"",""CUKFFW98000DBC"",""04-Mar-16 10:00""
""Message Type"",""Client ID"",""Job Number"",""Job Type"",""BGM Reference"",""Entry Numbe"",""Message Time (UTC)""
""Customs Entries"",""FFFIIIBRN"",""SLGW029247"",""EXPEXDAIR"",""6GB800547754000-SLGW029247"",""120-A07546V"",""09-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Mar-16 12:00"",""FFFIIIBRN"",""B13"",""S13"",""11212196951 L1314850 CUK"","""","""",""1""
""31-Mar-16 00:00"",""FFFIIIBRN"",""B16"",""S16"","""","""","""",""1""
""04-Mar-16 10:00"",""FFFIIIBRN"",""B14"",""S14"",""RCV CUKSYS98COMMDB CUKFFW98000DBC Fallback invocation/revocation (BCM)"","""","""",""1""
""09-Mar-16 10:00"",""FFFIIIBRN"",""B15"",""S15"",""SLGW029247 EXPEXDAIR 6GB800547754000-SLGW029247 120-A07546V"","""","""",""1""
", writer.ToString());
		}

		void AssertOdplRawUsage(SummaryLine summaryLine, string clientId, string column2, string column3, string column4, string column5 = null)
		{
			AssertEquals("Client ID", clientId, summaryLine.Column1);
			AssertEquals("Column 2", column2, summaryLine.Column2);
			AssertEquals("Column 3", column3, summaryLine.Column3);
			AssertEquals("Column 4", column4, summaryLine.Column4);
			if (column5 != null)
			{
				AssertEquals("Column 5", column5, summaryLine.Column5);
			}
		}

		public void TestLoadStlRawUsage()
		{
			var org1 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var org2 = BillingTestHelper.CreateOrganisation(Factory, "DDD", "III", "SYD");

			var db = org1.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 342;
			var clientCompany1 = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org1.PK, "", "");
			var clientCompany2 = ClientCompany.FindOrCreate(Factory, "III", db.PK, org2.PK, "", "");

			Factory.Save();

			var clientNumber1 = db.DatabaseId + ".ABC";
			var clientNumber2 = db.DatabaseId + ".III";

			List<EServicesBillingTestHelper.RawUsageInfo> infoList = new List<EServicesBillingTestHelper.RawUsageInfo>();
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "61898196346", "PS001972", "CUK", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "GTM", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "TRX", "CUKFFW98000DBC", "CUKFFW98000ZEB", "Text message (TXT)", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "CUE", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "SLHR012353", "IMPIMAAIR", "6GB800547754000-SLHR012353", "120-001278R", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "11212196951", "L1314850", "CUK", null, null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "GTM", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "RCV", "CUKSYS98COMMDB", "CUKFFW98000DBC", "Fallback invocation/revocation (BCM)", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "CUE", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "SLGW029247", "EXPEXDAIR", "6GB800547754000-SLGW029247", "120-A07546V", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("GBC", "AWB", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new GBCustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;
			CombineAssertions(() =>
			{
				AssertStlRawUsage(rawUsage.SummarySections[0].Lines[0], "ABC", "61898196346", "PS001972", "CUK");
				AssertStlRawUsage(rawUsage.SummarySections[0].Lines[1], "III", "11212196951", "L1314850", "CUK");
				AssertStlRawUsage(rawUsage.SummarySections[0].Lines[2], "III", "", "", "");

				AssertStlRawUsage(rawUsage.SummarySections[1].Lines[0], "ABC", "Text message (TXT)", "TRX", "CUKFFW98000DBC", "CUKFFW98000ZEB");
				AssertStlRawUsage(rawUsage.SummarySections[1].Lines[1], "III", "Fallback invocation/revocation (BCM)", "RCV", "CUKSYS98COMMDB", "CUKFFW98000DBC");

				AssertStlRawUsage(rawUsage.SummarySections[2].Lines[0], "ABC", "SLHR012353", "IMPIMAAIR", "6GB800547754000-SLHR012353", "120-001278R");
				AssertStlRawUsage(rawUsage.SummarySections[2].Lines[1], "III", "SLGW029247", "EXPEXDAIR", "6GB800547754000-SLGW029247", "120-A07546V");
			});

			string expectedCsvResult =
@"""Message Type"",""Company Code"",""MAWB"",""HAWB"",""Application Code"",""Message Time (UTC)""
""Air Waybills"",""ABC"",""61898196346"",""PS001972"",""CUK"",""01-Mar-16 10:00""
""Air Waybills"",""III"",""11212196951"",""L1314850"",""CUK"",""03-Mar-16 12:00""
""Air Waybills"",""III"","""","""","""",""31-Mar-16 00:00""
""Message Type"",""Company Code"",""Message Purpose"",""Direction"",""Sender"",""Recipient"",""Message Time (UTC)""
""General Text Messages"",""ABC"",""Text message (TXT)"",""TRX"",""CUKFFW98000DBC"",""CUKFFW98000ZEB"",""02-Mar-16 10:00""
""General Text Messages"",""III"",""Fallback invocation/revocation (BCM)"",""RCV"",""CUKSYS98COMMDB"",""CUKFFW98000DBC"",""04-Mar-16 10:00""
""Message Type"",""Company Code"",""Job Number"",""Job Type"",""BGM Reference"",""Entry Numbe"",""Message Time (UTC)""
""Customs Entries"",""ABC"",""SLHR012353"",""IMPIMAAIR"",""6GB800547754000-SLHR012353"",""120-001278R"",""03-Mar-16 10:00""
""Customs Entries"",""III"",""SLGW029247"",""EXPEXDAIR"",""6GB800547754000-SLGW029247"",""120-A07546V"",""09-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Mar-16 10:00"",""ABC"",""B10"",""S10"",""61898196346 PS001972 CUK"","""","""",""1""
""03-Mar-16 12:00"",""III"",""B13"",""S13"",""11212196951 L1314850 CUK"","""","""",""1""
""31-Mar-16 00:00"",""III"",""B16"",""S16"","""","""","""",""1""
""02-Mar-16 10:00"",""ABC"",""B11"",""S11"",""TRX CUKFFW98000DBC CUKFFW98000ZEB Text message (TXT)"","""","""",""1""
""04-Mar-16 10:00"",""III"",""B14"",""S14"",""RCV CUKSYS98COMMDB CUKFFW98000DBC Fallback invocation/revocation (BCM)"","""","""",""1""
""03-Mar-16 10:00"",""ABC"",""B12"",""S12"",""SLHR012353 IMPIMAAIR 6GB800547754000-SLHR012353 120-001278R"","""","""",""1""
""09-Mar-16 10:00"",""III"",""B15"",""S15"",""SLGW029247 EXPEXDAIR 6GB800547754000-SLGW029247 120-A07546V"","""","""",""1""
", writer.ToString());
		}

		void AssertStlRawUsage(SummaryLine summaryLine, string companyCode, string column2, string column3, string column4, string column5 = null)
		{
			AssertEquals("Company Code", companyCode, summaryLine.Column1);
			AssertEquals("Column 2", column2, summaryLine.Column2);
			AssertEquals("Column 3", column3, summaryLine.Column3);
			AssertEquals("Column 4", column4, summaryLine.Column4);
			if (column5 != null)
			{
				AssertEquals("Column 5", column5, summaryLine.Column5);
			}
		}
	}
}