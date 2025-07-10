using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_DBWTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, TablePrefix, GroupByCode, reportLineOrdering: ReportLineGroupingListCodes.Organisation);

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("ABI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice1.Lines.Count > 0);
			var line11 = invoice1.Lines[0];
			line11.AL_AT = TestObjectCreator.GST1.PK;
			line11.AL_AG = TestObjectCreator.GLHeader1.PK;
			var line12 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1m, 200m);
			line12.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line12.AL_AG = TestObjectCreator.GLHeader2.PK;

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("SHI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line21 = invoice2.Lines[0];
			line21.AL_AT = TestObjectCreator.GST1.PK;
			line21.AL_AG = TestObjectCreator.GLHeader2.PK;
			var line22 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1m, 200m);
			line22.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line22.AL_AG = TestObjectCreator.GLHeader2.PK;

			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, invoice1, invoice2);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, line11, line12, line21, line22);
			report.GenerateFromQueue();

			var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable().ToArray();

			AssertEquals(7, rows.Length);

			var result = rows.Where(x => x.Field<Guid>("AH_PK") == invoice1.PK).OrderBy(x => x.Field<int>("ACL_ReportSequence")).ToList();
			AssertEquals(4, result.Count);

			AssertContainsExactElementsInAnyOrder(new List<string> { TestObjectCreator.GLHeader1.AccountNum, TestObjectCreator.GLHeader2.AccountNum },
				new List<string> { result[0].Field<string>("AG_AccountNum"), result[1].Field<string>("AG_AccountNum") });
			AssertEquals(DBNull.Value, result[0]["AL_AT"]);
			AssertEquals(DBNull.Value, result[1]["AL_AT"]);

			AssertEquals(TestObjectCreator.GLHeader1.AccountNum, result[2].Field<string>("AG_AccountNum"));
			AssertEquals(TestObjectCreator.GST1.PK, result[2]["AL_AT"]);

			AssertEquals(TestObjectCreator.GLHeader2.AccountNum, result[3].Field<string>("AG_AccountNum"));
			AssertEquals(TestObjectCreator.GSTFREE1.PK, result[3]["AL_AT"]);

			result = rows.Where(x => x.Field<Guid>("AH_PK") == invoice2.PK).OrderBy(x => x.Field<int>("ACL_ReportSequence")).ToList();
			AssertEquals(3, result.Count);

			AssertEquals(TestObjectCreator.GLHeader2.AccountNum, result[0].Field<string>("AG_AccountNum"));
			AssertEquals(DBNull.Value, result[0]["AL_AT"]);

			AssertEquals(TestObjectCreator.GLHeader2.AccountNum, result[1].Field<string>("AG_AccountNum"));
			AssertEquals(TestObjectCreator.GST1.PK, result[1]["AL_AT"]);

			AssertEquals(TestObjectCreator.GLHeader2.AccountNum, result[2].Field<string>("AG_AccountNum"));
			AssertEquals(TestObjectCreator.GSTFREE1.PK, result[2]["AL_AT"]);
		}

		protected override string ReportName => "GetComplianceReportLines_DBW";
		protected override string GroupByCode => ReportLineGroupingListCodes.DayBookWithoutGrouping;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.AllTransactions;
	}
}
