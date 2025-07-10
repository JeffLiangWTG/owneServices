using System;
using System.Data;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_OBLTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: TablePrefix, reportLineGrouping: GroupByCode);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I100" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			var line01 = invoice.Lines[0];
			line01.AL_AT = TestObjectCreator.GST1.PK;

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("I200" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			var line11 = invoice1.Lines[0];
			line11.AL_AT = TestObjectCreator.GST1.PK;

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("I300" + report.ReportLines.Count, TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			var line21 = invoice2.Lines[0];
			line21.AL_AT = TestObjectCreator.GST1.PK;
			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line01, line11, line21);
			report.GenerateFromQueue();

			var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable();

			AssertEquals(2, rows.Count());

			var row = rows.FirstOrDefault(x => x.Field<Guid>("AH_OH") == TestObjectCreator.AALSHI.PK);
			AssertNotNull(row);
			AssertEquals(100m, row.Field<decimal>("ServiceExTaxAmount"));
			AssertEquals(10m, row.Field<decimal>("ServiceTaxAmount"));
			AssertEquals(0, row.Field<int>("SPVTaxAmount"));
			AssertEquals(0, row.Field<int>("GeneralLedgerAmount"));
			AssertEquals(1, row.Field<int>("TotalTransactionCount"));

			row = rows.FirstOrDefault(x => x.Field<Guid>("AH_OH") == TestObjectCreator.ABIGAS.PK);
			AssertNotNull(row);
			AssertEquals(200m, row.Field<decimal>("ServiceExTaxAmount"));
			AssertEquals(20m, row.Field<decimal>("ServiceTaxAmount"));
			AssertEquals(0, row.Field<int>("SPVTaxAmount"));
			AssertEquals(0, row.Field<int>("GeneralLedgerAmount"));
			AssertEquals(2, row.Field<int>("TotalTransactionCount"));
		}

		protected override string ReportName => "GetComplianceReportLines_OBL";
		protected override string GroupByCode => ReportLineGroupingListCodes.OrganisationBLCode;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.TransactionLine;
	}
}
