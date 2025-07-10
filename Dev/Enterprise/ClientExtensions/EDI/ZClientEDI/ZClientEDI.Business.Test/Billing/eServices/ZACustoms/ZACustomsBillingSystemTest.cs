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
	public class ZACustomsBillingSystemTest : EServicesBillingSystemTestCase
	{
		public void TestSystemCode()
		{
			var billingSystem = new ZACustomsBillingSystem();
			AssertEquals(BillingConstants.BillingSystem.ZACustoms, billingSystem.SystemCode);
		}

		public void TestCreateSystemBill()
		{
			var billingSystem = new ZACustomsBillingSystem();
			var context = new BillingRunContext(Factory, ZDateTime.Today, new ZDateTime(2016, 4, 30));

			var org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDAAASYD";
			org.CreateAndLoadLicenceForOrg();
			org.LicCompany.LicEnterprise.LE_EnterpriseCode = org.OH_Code.Substring(0, 3);

			var priceHeader = org.LicCompany.PriceHeaders.AddNew();
			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = new ZDateTime(2016, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";

			var priceItem = BillingTestHelper.AddPriceItem(priceHeader, "ZAC", BillingConstants.FeeType.Transactional, "", 0.12m);
			priceItem.L7_Description = "ZAC - 0.12m";
			priceItem.L7_RX_NKCurrency = "ZAR";

			var priceItemZX1 = BillingTestHelper.AddPriceItem(priceHeader, "ZX1", BillingConstants.FeeType.Transactional, "ZAC", 0.13m);
			priceItemZX1.L7_Description = "ZX1 - 0.13m";
			priceItemZX1.L7_RX_NKCurrency = "ZAR";

			var priceItemZX2 = BillingTestHelper.AddPriceItem(priceHeader, "ZX2", BillingConstants.FeeType.Transactional, "ZAC", 0.14m);
			priceItemZX2.L7_Description = "ZX2 - 0.14m";
			priceItemZX2.L7_RX_NKCurrency = "ZAR";

			var priceItemZX3 = BillingTestHelper.AddPriceItem(priceHeader, "ZX3", BillingConstants.FeeType.Transactional, "ZAC", 0.15m);
			priceItemZX3.L7_Description = "ZX3 - 0.15m";
			priceItemZX3.L7_RX_NKCurrency = "ZAR";

			var invoiceDelivery = org.LicCompany.InvoiceDeliveries.AddNew();
			invoiceDelivery.L9_GB_InvoicingBranch = Env.CurrentBranch.PK;

			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ZACustoms, "ZX1", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 50);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ZACustoms, "ZX2", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 30);
			BillingTestHelper.CreateChargeableUsage(Factory, BillingConstants.BillingSystem.ZACustoms, "ZX3", new ZDateTime(2016, 4, 1), org.LicCompany.PK, 20);

			Factory.Save();

			var bill = billingSystem.LoadSystemBills(context).First();
			AssertEquals(BillingConstants.BillingSystem.ZACustoms, bill.SystemCode);
			AssertEquals(bill.Amount, 50m * 0.13m + 30m * 0.14m + 20m * 0.15m);
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Carrier#11", "MasterBill#11", "FlightVoyage#11", "JOB#11"));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX2", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Carrier#12", "[EMPTY]", "FlightVoyage#12", "JOB#12", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX3", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db1.DatabaseId, clientCompany1.PK, "Carrier#13", "BillDate#13", "FlightVoyage#13", "JOB#13", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "Carrier#21", "MasterBill#21", "FlightVoyage#21", "JOB#21", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX2", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "Carrier#22", "[EMPTY]", "FlightVoyage#22", "JOB#22", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX3", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db2.DatabaseId, clientCompany2.PK, "Carrier#23", "BillDate#23", "FlightVoyage#23", "JOB#23", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db2.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ZACustomsBillingSystem();
			var licence1 = org1.LicCompany.LicHeadersForAllDatabases[0];
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org1.PK, clientCompany1.PK, licence1.Company.PK, licence1.Database.PK);
			var rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;
			AssertOdplRawUsage(rawUsage.SummarySections[0].Lines[0], "DDDABCSYD", "Carrier#11", "MasterBill#11", "FlightVoyage#11", "JOB#11");
			AssertOdplRawUsage(rawUsage.SummarySections[1].Lines[0], "DDDABCSYD", "Carrier#12", "FlightVoyage#12", "JOB#12");
			AssertOdplRawUsage(rawUsage.SummarySections[2].Lines[0], "DDDABCSYD", "Carrier#13", "BillDate#13", "FlightVoyage#13", "JOB#13");

			var licence2 = org2.LicCompany.LicHeadersForAllDatabases[0];
			context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), org2.PK, clientCompany2.PK, licence2.Company.PK, licence2.Database.PK);
			rawUsage = billingSystem.LoadOdplRawUsage(context) as MultiSectionSystemRawUsage;

			AssertOdplRawUsage(rawUsage.SummarySections[0].Lines[0], "FFFIIIBRN", "Carrier#21", "MasterBill#21", "FlightVoyage#21", "JOB#21");
			AssertOdplRawUsage(rawUsage.SummarySections[1].Lines[0], "FFFIIIBRN", "Carrier#22", "FlightVoyage#22", "JOB#22");
			AssertOdplRawUsage(rawUsage.SummarySections[2].Lines[0], "FFFIIIBRN", "Carrier#23", "BillDate#23", "FlightVoyage#23", "JOB#23");

			string expectedCsvResult =
@"""Message Type"",""Client ID"",""Carrier Code"",""Master Bill Number"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [COH and HAB]"",""FFFIIIBRN"",""Carrier#21"",""MasterBill#21"",""FlightVoyage#21"",""JOB#21"",""03-Mar-16 12:00""
""Outbound CUSCAR Messages [COH and HAB]"",""FFFIIIBRN"","""","""","""","""",""31-Mar-16 00:00""
""Message Type"",""Client ID"",""Carrier Code"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [FFM, ECL, RFM, BBB and RMA]"",""FFFIIIBRN"",""Carrier#22"",""FlightVoyage#22"",""JOB#22"",""04-Mar-16 10:00""
""Message Type"",""Client ID"",""Carrier Code"",""Bill Date"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [COM and FWB]"",""FFFIIIBRN"",""Carrier#23"",""BillDate#23"",""FlightVoyage#23"",""JOB#23"",""09-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, false, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, false, writer);
			AssertEquals(@"""03-Mar-16 12:00"",""FFFIIIBRN"",""B13"",""S13"",""Carrier#21 MasterBill#21 FlightVoyage#21 JOB#21"","""","""",""1""
""31-Mar-16 00:00"",""FFFIIIBRN"",""B16"",""S16"","""","""","""",""1""
""04-Mar-16 10:00"",""FFFIIIBRN"",""B14"",""S14"",""Carrier#22 [EMPTY] FlightVoyage#22 JOB#22"","""","""",""1""
""09-Mar-16 10:00"",""FFFIIIBRN"",""B15"",""S15"",""Carrier#23 BillDate#23 FlightVoyage#23 JOB#23"","""","""",""1""
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
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 1, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "Carrier#11", "MasterBill#11", "FlightVoyage#11", "JOB#11", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX2", new ZDateTime(2016, 3, 2, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "Carrier#12", "[EMPTY]", "FlightVoyage#12", "JOB#12", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX3", new ZDateTime(2016, 3, 3, 10, 0, 0), "DDDABCSYD", clientNumber1, db.DatabaseId, clientCompany1.PK, "Carrier#13", "BillDate#13", "FlightVoyage#13", "JOB#13", null));

			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 3, 12, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "Carrier#21", "MasterBill#21", "FlightVoyage#21", "JOB#21", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX2", new ZDateTime(2016, 3, 4, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "Carrier#22", "[EMPTY]", "FlightVoyage#22", "JOB#22", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX3", new ZDateTime(2016, 3, 9, 10, 0, 0), "FFFIIIBRN", clientNumber2, db.DatabaseId, clientCompany2.PK, "Carrier#23", "BillDate#23", "FlightVoyage#23", "JOB#23", null));
			infoList.Add(new EServicesBillingTestHelper.RawUsageInfo("ZAC", "ZX1", new ZDateTime(2016, 3, 31, 0, 0, 0), "FFFIIIBRN", null, db.DatabaseId, clientCompany2.PK, "", null, null, null, DateTime.Now, "ENT"));

			EServicesBillingTestHelper.RawUsageInfo.FillBranchAndStaff(infoList);
			EServicesBillingTestHelper.AddTransactions(infoList);

			var billingSystem = new ZACustomsBillingSystem();
			var context = new BillingLoadRawUsageContext(Factory, new ZDateTime(2016, 3, 1), db.PK, ZGuid.Empty, ZGuid.Empty);
			var rawUsage = billingSystem.LoadStlRawUsage(context) as MultiSectionStlRawUsage;

			AssertStlRawUsage(rawUsage.SummarySections[0].Lines[0], "ABC", "Carrier#11", "MasterBill#11", "FlightVoyage#11", "JOB#11");
			AssertStlRawUsage(rawUsage.SummarySections[1].Lines[0], "ABC", "Carrier#12", "FlightVoyage#12", "JOB#12");
			AssertStlRawUsage(rawUsage.SummarySections[2].Lines[0], "ABC", "Carrier#13", "BillDate#13", "FlightVoyage#13", "JOB#13");
			AssertStlRawUsage(rawUsage.SummarySections[0].Lines[1], "III", "Carrier#21", "MasterBill#21", "FlightVoyage#21", "JOB#21");
			AssertStlRawUsage(rawUsage.SummarySections[1].Lines[1], "III", "Carrier#22", "FlightVoyage#22", "JOB#22");
			AssertStlRawUsage(rawUsage.SummarySections[2].Lines[1], "III", "Carrier#23", "BillDate#23", "FlightVoyage#23", "JOB#23");
			AssertStlRawUsage(rawUsage.SummarySections[0].Lines[2], "III", "", "", "", "");

			string expectedCsvResult =
@"""Message Type"",""Company Code"",""Carrier Code"",""Master Bill Number"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [COH and HAB]"",""ABC"",""Carrier#11"",""MasterBill#11"",""FlightVoyage#11"",""JOB#11"",""01-Mar-16 10:00""
""Outbound CUSCAR Messages [COH and HAB]"",""III"",""Carrier#21"",""MasterBill#21"",""FlightVoyage#21"",""JOB#21"",""03-Mar-16 12:00""
""Outbound CUSCAR Messages [COH and HAB]"",""III"","""","""","""","""",""31-Mar-16 00:00""
""Message Type"",""Company Code"",""Carrier Code"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [FFM, ECL, RFM, BBB and RMA]"",""ABC"",""Carrier#12"",""FlightVoyage#12"",""JOB#12"",""02-Mar-16 10:00""
""Outbound CUSCAR Messages [FFM, ECL, RFM, BBB and RMA]"",""III"",""Carrier#22"",""FlightVoyage#22"",""JOB#22"",""04-Mar-16 10:00""
""Message Type"",""Company Code"",""Carrier Code"",""Bill Date"",""Flight / Voyage"",""Job Number"",""Message Time (UTC)""
""Outbound CUSCAR Messages [COM and FWB]"",""ABC"",""Carrier#13"",""BillDate#13"",""FlightVoyage#13"",""JOB#13"",""03-Mar-16 10:00""
""Outbound CUSCAR Messages [COM and FWB]"",""III"",""Carrier#23"",""BillDate#23"",""FlightVoyage#23"",""JOB#23"",""09-Mar-16 10:00""
";

			var builder = new ZStringBuilder();
			billingSystem.LoadRawUsageInCsv(context, true, (csv) => { builder.AppendLine(csv); });
			AssertEquals(expectedCsvResult, builder.ToString());

			var writer = new CsvUsageReportWriterForTest();
			billingSystem.LoadRawUsageInCsv(context, true, writer);
			AssertEquals(@"""01-Mar-16 10:00"",""ABC"",""B10"",""S10"",""Carrier#11 MasterBill#11 FlightVoyage#11 JOB#11"","""","""",""1""
""03-Mar-16 12:00"",""III"",""B13"",""S13"",""Carrier#21 MasterBill#21 FlightVoyage#21 JOB#21"","""","""",""1""
""31-Mar-16 00:00"",""III"",""B16"",""S16"","""","""","""",""1""
""02-Mar-16 10:00"",""ABC"",""B11"",""S11"",""Carrier#12 [EMPTY] FlightVoyage#12 JOB#12"","""","""",""1""
""04-Mar-16 10:00"",""III"",""B14"",""S14"",""Carrier#22 [EMPTY] FlightVoyage#22 JOB#22"","""","""",""1""
""03-Mar-16 10:00"",""ABC"",""B12"",""S12"",""Carrier#13 BillDate#13 FlightVoyage#13 JOB#13"","""","""",""1""
""09-Mar-16 10:00"",""III"",""B15"",""S15"",""Carrier#23 BillDate#23 FlightVoyage#23 JOB#23"","""","""",""1""
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