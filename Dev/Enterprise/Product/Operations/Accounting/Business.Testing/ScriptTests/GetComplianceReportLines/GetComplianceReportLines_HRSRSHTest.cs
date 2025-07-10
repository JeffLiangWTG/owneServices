using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_HRSRSHTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, TablePrefix, GroupByCode);

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var postDate = new ZDate(2021, 12, 10);
			var line1 = CreateARInvoiceLine(invoice, amount: 200m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40m, postDate: postDate);
			var line2 = CreateARInvoiceLine(invoice, amount: 200m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40m, postDate: postDate);
			var line3 = CreateARInvoiceLine(invoice, amount: 300m, accTaxRate: TestObjectCreator.GST2, taxAmount: 60m, postDate: postDate);
			var line4 = CreateARInvoiceLine(invoice, amount: 400m, accTaxRate: TestObjectCreator.GST2, taxAmount: 80m, postDate: postDate);
			var line5 = CreateARInvoiceLine(invoice, amount: 500m, accTaxRate: TestObjectCreator.GST2, taxAmount: 100m, postDate: postDate);
			var line6 = CreateARInvoiceLine(invoice, amount: 600m, accTaxRate: TestObjectCreator.GST1, taxAmount: 60m, postDate: postDate);
			var line7 = CreateARInvoiceLine(invoice, amount: 700m, accTaxRate: TestObjectCreator.GST1, taxAmount: 70m, postDate: postDate);
			var line8 = CreateARInvoiceLine(invoice, amount: 800m, accTaxRate: TestObjectCreator.GST1, taxAmount: 80m, postDate: postDate);

			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var line21 = CreateARInvoiceLine(invoice2, amount: 200m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line22 = CreateARInvoiceLine(invoice2, amount: 200m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line23 = CreateARInvoiceLine(invoice2, amount: 300m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line24 = CreateARInvoiceLine(invoice2, amount: 400m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);

			var invoiceap = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.EUR, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			invoiceap.Lines[0].AL_AT = TestObjectCreator.GST2.PK;
			invoiceap.Lines[0].AL_GSTVAT = -20m;
			var lineap1 = invoiceap.Lines[0];
			var lineap2 = TestObjectCreator.CreateInvoiceLine(invoiceap, TestObjectCreator.EUR, 1m, 200m);
			lineap2.AL_AT = TestObjectCreator.GST2.PK;
			lineap2.AL_GSTVAT = -40m;
			var lineap3 = TestObjectCreator.CreateInvoiceLine(invoiceap, TestObjectCreator.EUR, 1m, 300m);
			lineap3.AL_AT = TestObjectCreator.GST2.PK;
			lineap3.AL_GSTVAT = -60m;

			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR003", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var line31 = CreateARInvoiceLine(invoice3, amount: 200.6248m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40.12496m, postDate: postDate);
			var line32 = CreateARInvoiceLine(invoice3, amount: 200.6248m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40.12496m, postDate: postDate);

			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, "81_NA_NA", null, line1, line2, line3, line4, line5, line31, line32);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "86_NA_NA", null, line6, line7, line8);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "66_NA_NA", null, lineap1, lineap2, lineap3);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, "46_47_67", null, line21, line22, line23, line24);
			report.GenerateFromQueue();

			var sql = "SELECT  ACL_ParentID, ACL_ReportSequence, ACL_ReportSubCode FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("AccComplianceReportTransactionPivot should have 17 rows", 17, rows.Count());

			rows = RunScript(report, ReportName, TablePrefix, reverseSign: false).AsEnumerable();

			AssertEquals(5, rows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());
			AssertEquals(4, rows.GroupBy(x => x.Field<string>("ReportSubCode")).Count());

			var row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == "81_NA_NA" && x.Field<Guid>("AH_PK") == invoice.PK);
			AssertNotNull(row);
			AssertEquals("Wrong Net Amount", 1600m, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", 320m, row.Field<decimal>("TotalTaxAmount"));

			row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == "86_NA_NA" && x.Field<Guid>("AH_PK") == invoice.PK);
			AssertNotNull(row);
			AssertEquals("Wrong Net Amount", 2100m, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", 210m, row.Field<decimal>("TotalTaxAmount"));

			row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == "46_47_67" && x.Field<Guid>("AH_PK") == invoice2.PK);
			AssertNotNull(row);
			AssertEquals("Wrong Net Amount", 1100m, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", 0m, row.Field<decimal>("TotalTaxAmount"));

			row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == "66_NA_NA" && x.Field<Guid>("AH_PK") == invoiceap.PK);
			AssertNotNull(row);
			AssertEquals("Wrong Net Amount", -600m, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", -120m, row.Field<decimal>("TotalTaxAmount"));

			row = rows.FirstOrDefault(x => x.Field<string>("ReportSubCode") == "81_NA_NA" && x.Field<Guid>("AH_PK") == invoice3.PK);
			AssertNotNull(row);
			AssertEquals("Wrong Net Amount", 401.24m, row.Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Wrong Tax Amount", 80.24m, row.Field<decimal>("TotalTaxAmount"));
		}

		public void TestGetComplianceReportLines_TypeU11WithPEoT()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader);
			report.ACR_DateFrom = new ZDate(2021, 01, 01);
			report.ACR_DateTo = new ZDate(2021, 12, 31);
			Factory.Save();

			var glAccount = TestObjectCreator.CreateGLHeader("1234.56.78");
			var bankAccount = TestObjectCreator.CreateBankAccount("BANK1", "Bank Account", TestObjectCreator.EUR, glAccount);

			// UST 1/11
			var cbDpy111 = CreateACashbookQueueEntry(2021, 02, 10, 333, bankAccount.PK);
			// USTVA's posted with 2 month delay
			var cbDpy1 = CreateACashbookQueueEntry(2021, 03, 31, 100, bankAccount.PK);
			var cbDpy2 = CreateACashbookQueueEntry(2021, 04, 30, 200, bankAccount.PK);
			var cbDpy3 = CreateACashbookQueueEntry(2021, 05, 15, 300, bankAccount.PK);
			var cbDrc4 = CreateACashbookQueueEntry(2021, 06, 15, -400, bankAccount.PK);
			var cbDpy51 = CreateACashbookQueueEntry(2021, 07, 15, 500, bankAccount.PK);
			var cbDpy52 = CreateACashbookQueueEntry(2021, 07, 25, 150, bankAccount.PK);
			var cbDpy6 = CreateACashbookQueueEntry(2021, 08, 15, 600, bankAccount.PK);
			var cbDpy7 = CreateACashbookQueueEntry(2021, 09, 15, 700, bankAccount.PK);
			var cbDpy8 = CreateACashbookQueueEntry(2021, 10, 15, 800, bankAccount.PK);
			var cbDpy9 = CreateACashbookQueueEntry(2021, 11, 15, 900, bankAccount.PK);
			var cbDpy10 = CreateACashbookQueueEntry(2021, 12, 15, 1000, bankAccount.PK);
			var cbDpy11 = CreateACashbookQueueEntry(2022, 01, 15, 1100, bankAccount.PK);

			// single transactions for December
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("AR001", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			var postDate = new ZDate(2021, 12, 10);
			invoice.AH_PostDate = postDate;
			var line1 = CreateARInvoiceLine(invoice, amount: 200m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40m, postDate: postDate);
			var line2 = CreateARInvoiceLine(invoice, amount: 200m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40m, postDate: postDate);
			var line3 = CreateARInvoiceLine(invoice, amount: 300m, accTaxRate: TestObjectCreator.GST2, taxAmount: 60m, postDate: postDate);
			var line4 = CreateARInvoiceLine(invoice, amount: 400m, accTaxRate: TestObjectCreator.GST2, taxAmount: 80m, postDate: postDate);
			var line5 = CreateARInvoiceLine(invoice, amount: 500m, accTaxRate: TestObjectCreator.GST2, taxAmount: 100m, postDate: postDate);
			var line6 = CreateARInvoiceLine(invoice, amount: 600m, accTaxRate: TestObjectCreator.GST1, taxAmount: 60m, postDate: postDate);
			var line7 = CreateARInvoiceLine(invoice, amount: 700m, accTaxRate: TestObjectCreator.GST1, taxAmount: 70m, postDate: postDate);
			var line8 = CreateARInvoiceLine(invoice, amount: 800m, accTaxRate: TestObjectCreator.GST1, taxAmount: 80m, postDate: postDate);

			var invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR002", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			postDate = new ZDate(2021, 12, 07);
			invoice2.AH_PostDate = postDate;
			var line21 = CreateARInvoiceLine(invoice2, amount: 200m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line22 = CreateARInvoiceLine(invoice2, amount: 200m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line23 = CreateARInvoiceLine(invoice2, amount: 300m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);
			var line24 = CreateARInvoiceLine(invoice2, amount: 400m, accTaxRate: TestObjectCreator.RVS1, taxAmount: 0m, postDate: postDate);

			var invoiceap = TestObjectCreator.CreateAPInvoice<APInvoice>("AP0001", TestObjectCreator.EUR, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			postDate = new ZDate(2021, 12, 08);
			invoiceap.AH_PostDate = postDate;
			var lineap1 = invoiceap.Lines[0];
			lineap1.AL_AT = TestObjectCreator.GST2.PK;
			lineap1.AL_GSTVAT = -20m;
			lineap1.AL_PostDate = postDate;
			var lineap2 = CreateAPLine(invoiceap, 200m, -40m, postDate);
			var lineap3 = CreateAPLine(invoiceap, 300m, -60m, postDate);

			var invoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("AR003", TestObjectCreator.EUR, 1m, TestObjectCreator.LocalClient);
			postDate = new ZDate(2021, 12, 12);
			invoice3.AH_PostDate = postDate;
			var line31 = CreateARInvoiceLine(invoice3, amount: 200.6248m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40.12496m, postDate: postDate);
			var line32 = CreateARInvoiceLine(invoice3, amount: 200.6248m, accTaxRate: TestObjectCreator.GST2, taxAmount: 40.12496m, postDate: postDate);

			Factory.Save();

			CreateComplianceReportQueueEntry(2021, 02, 28, 0, "UST 1/11", report, cbDpy111);
			CreateComplianceReportQueueEntry(2021, 03, 31, -2, "USTVA", report, cbDpy1);
			CreateComplianceReportQueueEntry(2021, 04, 30, -2, "USTVA", report, cbDpy2);
			CreateComplianceReportQueueEntry(2021, 05, 15, -2, "USTVA", report, cbDpy3);
			CreateComplianceReportQueueEntry(2021, 06, 15, -2, "USTVA", report, cbDrc4);
			CreateComplianceReportQueueEntry(2021, 07, 15, -2, "USTVA", report, cbDpy51);
			CreateComplianceReportQueueEntry(2021, 07, 25, -2, "USTVA", report, cbDpy52);
			CreateComplianceReportQueueEntry(2021, 08, 15, -2, "USTVA", report, cbDpy6);
			CreateComplianceReportQueueEntry(2021, 09, 15, -2, "USTVA", report, cbDpy7);
			CreateComplianceReportQueueEntry(2021, 10, 15, -2, "USTVA", report, cbDpy8);
			CreateComplianceReportQueueEntry(2021, 11, 15, -2, "USTVA", report, cbDpy9);
			CreateComplianceReportQueueEntry(2021, 12, 15, -2, "USTVA", report, cbDpy10);
			CreateComplianceReportQueueEntry(2022, 01, 15, -2, "USTVA", report, cbDpy11);
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, "USTVA - 202112", null, line1, line2, line3, line4, line5, line6, line7, line8, line21, line22, line23, line24, line31, line32, lineap1, lineap2, lineap3);

			report.GenerateFromQueue();

			var sql = "SELECT ACL_ParentID, ACL_ReportSequence, ACL_ReportSubCode FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("AccComplianceReportTransactionPivot should have 30 rows", 30, rows.Count());

			rows = RunScript(report, ReportName, TablePrefix, reverseSign: false).AsEnumerable();

			AssertEquals(17, rows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());
			AssertEquals(13, rows.GroupBy(x => x.Field<string>("ReportSubCode")).Count());

			AssertRow(rows, "UST 1/11 - 202102", cbDpy111.AL_AH, -333, 1, 0);
			AssertRow(rows, "USTVA - 202101", cbDpy1.AL_AH, -100, 2, 0);
			AssertRow(rows, "USTVA - 202102", cbDpy2.AL_AH, -200, 3, 0);
			AssertRow(rows, "USTVA - 202103", cbDpy3.AL_AH, -300, 4, 0);
			AssertRow(rows, "USTVA - 202104", cbDrc4.AL_AH, 400, 5, 0);
			AssertRow(rows, "USTVA - 202105", cbDpy51.AL_AH, -500, 6, 0);
			AssertRow(rows, "USTVA - 202105", cbDpy52.AL_AH, -150, 7, 0);
			AssertRow(rows, "USTVA - 202106", cbDpy6.AL_AH, -600, 8, 0);
			AssertRow(rows, "USTVA - 202107", cbDpy7.AL_AH, -700, 9, 0);
			AssertRow(rows, "USTVA - 202108", cbDpy8.AL_AH, -800, 10, 0);
			AssertRow(rows, "USTVA - 202109", cbDpy9.AL_AH, -900, 11, 0);
			AssertRow(rows, "USTVA - 202110", cbDpy10.AL_AH, -1000, 12, 0);
			AssertRow(rows, "USTVA - 202111", cbDpy11.AL_AH, -1100, 13, 0);
			AssertRow(rows, "USTVA - 202112", invoiceap.PK, -600, 14, -120);
			AssertRow(rows, "USTVA - 202112", invoice2.PK, 1100, 15, 0);
			AssertRow(rows, "USTVA - 202112", invoice.PK, 3700, 16, 530);
			AssertRow(rows, "USTVA - 202112", invoice3.PK, 401.24m, 17, 80.24m);
		}

		public void TestGetComplianceReportLines_TypeU11WithoutPEoT()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.ReportSubCodeAndTransactionHeader);
			report.ACR_DateFrom = new ZDate(2021, 01, 01);
			report.ACR_DateTo = new ZDate(2021, 12, 31);
			Factory.Save();

			var glAccount = TestObjectCreator.CreateGLHeader("1234.56.78");
			var bankAccount = TestObjectCreator.CreateBankAccount("BANK1", "Bank Account", TestObjectCreator.EUR, glAccount);

			// USTVA's posted with 1 month delay
			var cbDpy1 = CreateACashbookQueueEntry(2021, 02, 28, 100, bankAccount.PK);
			var cbDpy2 = CreateACashbookQueueEntry(2021, 03, 31, 200, bankAccount.PK);
			var cbDpy3 = CreateACashbookQueueEntry(2021, 04, 30, 300, bankAccount.PK);
			var cbDpy4 = CreateACashbookQueueEntry(2021, 05, 15, 400, bankAccount.PK);
			var cbDrc5 = CreateACashbookQueueEntry(2021, 06, 15, -500, bankAccount.PK);
			var cbDpy6 = CreateACashbookQueueEntry(2021, 07, 15, 600, bankAccount.PK);
			var cbDpy71 = CreateACashbookQueueEntry(2021, 08, 15, 700, bankAccount.PK);
			var cbDpy72 = CreateACashbookQueueEntry(2021, 08, 23, 170, bankAccount.PK);
			var cbDpy8 = CreateACashbookQueueEntry(2021, 09, 15, 800, bankAccount.PK);
			var cbDpy9 = CreateACashbookQueueEntry(2021, 10, 15, 900, bankAccount.PK);
			var cbDpy10 = CreateACashbookQueueEntry(2021, 11, 15, 1000, bankAccount.PK);
			var cbDpy11 = CreateACashbookQueueEntry(2021, 12, 15, 1100, bankAccount.PK);
			var cbDpy12 = CreateACashbookQueueEntry(2022, 01, 15, 1200, bankAccount.PK);
			Factory.Save();
			CreateComplianceReportQueueEntry(2021, 02, 28, -1, "USTVA", report, cbDpy1);
			CreateComplianceReportQueueEntry(2021, 03, 31, -1, "USTVA", report, cbDpy2);
			CreateComplianceReportQueueEntry(2021, 04, 30, -1, "USTVA", report, cbDpy3);
			CreateComplianceReportQueueEntry(2021, 05, 15, -1, "USTVA", report, cbDpy4);
			CreateComplianceReportQueueEntry(2021, 06, 15, -1, "USTVA", report, cbDrc5);
			CreateComplianceReportQueueEntry(2021, 07, 15, -1, "USTVA", report, cbDpy6);
			CreateComplianceReportQueueEntry(2021, 08, 15, -1, "USTVA", report, cbDpy71);
			CreateComplianceReportQueueEntry(2021, 08, 23, -1, "USTVA", report, cbDpy72);
			CreateComplianceReportQueueEntry(2021, 09, 15, -1, "USTVA", report, cbDpy8);
			CreateComplianceReportQueueEntry(2021, 10, 15, -1, "USTVA", report, cbDpy9);
			CreateComplianceReportQueueEntry(2021, 11, 15, -1, "USTVA", report, cbDpy10);
			CreateComplianceReportQueueEntry(2021, 12, 15, -1, "USTVA", report, cbDpy11);
			CreateComplianceReportQueueEntry(2022, 01, 15, -1, "USTVA", report, cbDpy12);
			Factory.Save();

			report.GenerateFromQueue();

			var sql = "SELECT ACL_ParentID, ACL_ReportSequence, ACL_ReportSubCode FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence";
			var rows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals("AccComplianceReportTransactionPivot should have 13 rows", 13, rows.Count());

			rows = RunScript(report, ReportName, TablePrefix, reverseSign: false).AsEnumerable();

			AssertEquals(13, rows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());
			AssertEquals(12, rows.GroupBy(x => x.Field<string>("ReportSubCode")).Count());

			AssertRow(rows, "USTVA - 202101", cbDpy1.AL_AH, -100, 1, 0);
			AssertRow(rows, "USTVA - 202102", cbDpy2.AL_AH, -200, 2, 0);
			AssertRow(rows, "USTVA - 202103", cbDpy3.AL_AH, -300, 3, 0);
			AssertRow(rows, "USTVA - 202104", cbDpy4.AL_AH, -400, 4, 0);
			AssertRow(rows, "USTVA - 202105", cbDrc5.AL_AH, 500, 5, 0);
			AssertRow(rows, "USTVA - 202106", cbDpy6.AL_AH, -600, 6, 0);
			AssertRow(rows, "USTVA - 202107", cbDpy71.AL_AH, -700, 7, 0);
			AssertRow(rows, "USTVA - 202107", cbDpy72.AL_AH, -170, 8, 0);
			AssertRow(rows, "USTVA - 202108", cbDpy8.AL_AH, -800, 9, 0);
			AssertRow(rows, "USTVA - 202109", cbDpy9.AL_AH, -900, 10, 0);
			AssertRow(rows, "USTVA - 202110", cbDpy10.AL_AH, -1000, 11, 0);
			AssertRow(rows, "USTVA - 202111", cbDpy11.AL_AH, -1100, 12, 0);
			AssertRow(rows, "USTVA - 202112", cbDpy12.AL_AH, -1200, 13, 0);
		}

		protected override string ReportName => "GetComplianceReportLines_HRSRSH";
		protected override string GroupByCode => ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.TransactionLine;
	}
}
