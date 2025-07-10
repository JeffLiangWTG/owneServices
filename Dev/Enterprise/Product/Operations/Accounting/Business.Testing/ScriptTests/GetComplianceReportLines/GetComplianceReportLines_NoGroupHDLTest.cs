using System.Data;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_NoGroupHDLTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: TablePrefix, reportLineGrouping: GroupByCode);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = TestObjectCreator.GST1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 200m);
			line2.AL_AT = TestObjectCreator.GSTFREE1.PK;
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 300m);
			line3.AL_AT = TestObjectCreator.VATSPV.PK;
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 400m);
			line4.AL_AT = TestObjectCreator.GST1.PK;
			var line5 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 770.22m);
			line5.AL_AT = TestObjectCreator.VATSPV.PK;
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line1, line2, line3, line4, line5);
			report.GenerateFromQueue();

			var dataRows = RunScript(report, ReportName, TablePrefix).AsEnumerable();

			AssertEquals(5, dataRows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());

			var rows = dataRows.Where(x => x.Field<string>("AT_Code") == TestObjectCreator.GST1.AT_Code).OrderBy(x => x.Field<decimal>("AL_TaxRate")).ToList();
			AssertEquals(2, rows.Count);
			AssertEquals(10M, rows[0].Field<decimal>("AL_TaxRate"));
			AssertEquals(0, rows[0].Field<int>("SPVTaxAmount"));
			AssertEquals("RAT", rows[0].Field<string>("AT_Type"));
			AssertNullOrEmpty(rows[0].Field<string>("ComplianceSequence"));

			AssertEquals(10M, rows[1].Field<decimal>("AL_TaxRate"));
			AssertEquals(0, rows[1].Field<int>("SPVTaxAmount"));
			AssertEquals("RAT", rows[1].Field<string>("AT_Type"));
			AssertNullOrEmpty(rows[1].Field<string>("ComplianceSequence"));

			rows = dataRows.Where(x => x.Field<string>("AT_Code") == TestObjectCreator.GSTFREE1.AT_Code).ToList();
			AssertEquals(1, rows.Count);
			AssertEquals(0M, rows[0].Field<decimal>("AL_TaxRate"));
			AssertEquals(0, rows[0].Field<int>("SPVTaxAmount"));
			AssertEquals("RAT", rows[0].Field<string>("AT_Type"));
			AssertNullOrEmpty(rows[0].Field<string>("ComplianceSequence"));

			rows = dataRows.Where(x => x.Field<string>("AT_Code") == TestObjectCreator.VATSPV.AT_Code).OrderBy(x => x.Field<decimal>("AL_TaxRate")).ToList();
			AssertEquals(2, rows.Count);
			AssertEquals(22M, rows[0].Field<decimal>("AL_TaxRate"));
			AssertEquals(0, rows[0].Field<int>("SPVTaxAmount"));
			AssertEquals("RAT", rows[0].Field<string>("AT_Type"));
			AssertNullOrEmpty(rows[0].Field<string>("ComplianceSequence"));

			AssertEquals(22M, rows[1].Field<decimal>("AL_TaxRate"));
			AssertEquals(0, rows[1].Field<int>("SPVTaxAmount"));
			AssertEquals("RAT", rows[1].Field<string>("AT_Type"));
			AssertNullOrEmpty(rows[0].Field<string>("ComplianceSequence"));
		}

		protected override string ReportName => "GetComplianceReportLines_NoGroupHDL";
		protected override string GroupByCode => ReportLineGroupingListCodes.NoGrouping;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.TransactionLine;
	}
}
