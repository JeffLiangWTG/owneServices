using System.Data;
using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_TPATest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, TablePrefix, GroupByCode, reportLineOrdering: ReportLineGroupingListCodes.Organisation);

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("ABI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			invoice1.AH_PostDate = invoice1.AH_PostDate.AddDays(-1);
			Assert("Has Lines", invoice1.Lines.Count > 0);
			var line11 = invoice1.Lines[0];
			line11.AL_AT = TestObjectCreator.GST1.PK;
			var line12 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1m, 200m);
			line12.AL_AT = TestObjectCreator.GSTFREE1.PK;
			var matchLink = TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice1, invoice1.AH_PostDate, -155m);
			TestObjectCreator.CreateLineMatchLink(matchLink, line11, -55m);
			TestObjectCreator.CreateLineMatchLink(matchLink, line12, -100m);

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("SHI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line21 = invoice2.Lines[0];
			line21.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice2, invoice2.AH_PostDate);

			var invoice3 = TestObjectCreator.CreateAPInvoice<APInvoice>("SHI0002", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			invoice3.AH_PostDate = invoice2.AH_PostDate.AddDays(-1);
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice3, invoice3.AH_PostDate, -36.63m);

			var invoice4 = TestObjectCreator.CreateAPInvoice<APInvoice>("SHI0003", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.AALSHI);
			invoice4.AH_PostDate = invoice2.AH_PostDate.AddDays(-2);
			Assert("Has Lines", invoice4.Lines.Count > 0);
			var line41 = invoice4.Lines[0];
			line41.AL_AT = TestObjectCreator.GSTFREE1.PK;
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice4, invoice4.AH_PostDate, -50m);

			var invoice5 = TestObjectCreator.CreateAPInvoice<APInvoice>("ABI0002", TestObjectCreator.AUD, 1m, 100m, 0m, 0m, 100m, 0m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice5.Lines.Count > 0);
			var line51 = invoice5.Lines[0];
			line51.AL_AT = TestObjectCreator.GSTFREE1.PK;
			TestObjectCreator.CreateAndMatchAPPaymentForAPInvoice(invoice5, invoice5.AH_PostDate);

			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, invoice1, invoice2, invoice3, invoice4, invoice5);

			AssertEquals("ReportLines.Count", 0, report.ReportLines.Count);
			report.GenerateFromQueue();

			var rows = RunScript(report, ReportName, TablePrefix, reportDateFrom: report.ACR_DateFrom, reportDateTo: report.ACR_DateTo).AsEnumerable().ToArray();
			AssertEquals("ReportLines.Count", 5, rows.Length);

			Assert("AH_InvoiceAmount is populated", rows.All(x => x.Field<decimal>("AH_InvoiceAmount") != 0m));
			AssertEquals("AH_GSTAmount is populated on 3 out of 5", 3, rows.Count(x => x.Field<decimal>("AH_GSTAmount") != 0m));

			AssertEquals("invoice3.AH_InvoiceAmount", -100m, rows[0].Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("invoice3.AH_GSTAmount", 0m, rows[0].Field<decimal>("AH_GSTAmount"));

			AssertEquals("invoice3.AH_InvoiceAmount", -100m, rows[1].Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("invoice3.AH_GSTAmount", -10m, rows[1].Field<decimal>("AH_GSTAmount"));

			AssertEquals("invoice2.AH_InvoiceAmount", -100m, rows[2].Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("invoice2.AH_GSTAmount", -10m, rows[2].Field<decimal>("AH_GSTAmount"));

			AssertEquals("invoice1.AH_InvoiceAmount", -300m, rows[3].Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("invoice1.AH_GSTAmount", -10m, rows[3].Field<decimal>("AH_GSTAmount"));

			AssertEquals("invoice3.AH_InvoiceAmount", -100m, rows[4].Field<decimal>("AH_InvoiceAmount"));
			AssertEquals("invoice3.AH_GSTAmount", 0m, rows[4].Field<decimal>("AH_GSTAmount"));

			AssertEquals("Partially paid invoice3.TotalExTaxAmount", 50m, rows[0].Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Partislly paid invoice3.TotalTaxAmount", 0m, rows[0].Field<decimal>("TotalTaxAmount"));

			AssertEquals("Partially paid invoice3.TotalExTaxAmount", 33.3m, rows[1].Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Partislly paid invoice3.TotalTaxAmount", 3.33m, rows[1].Field<decimal>("TotalTaxAmount"));

			AssertEquals("Fully paid invoice2.TotalExTaxAmount", 100m, rows[2].Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Fully paid invoice2.TotalTaxAmount", 10m, rows[2].Field<decimal>("TotalTaxAmount"));

			AssertEquals("Partially paid by lines invoice1.TotalExTaxAmount", 150m, rows[3].Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Partially paid by lines invoice1.TotalTaxAmount", 5m, rows[3].Field<decimal>("TotalTaxAmount"));

			AssertEquals("Fully paid invoice2.TotalExTaxAmount", 100m, rows[4].Field<decimal>("TotalExTaxAmount"));
			AssertEquals("Fully paid invoice2.TotalTaxAmount", 0m, rows[4].Field<decimal>("TotalTaxAmount"));

			Assert("Goods Amounts are not populated", rows.All(x => x.Field<int>("GoodsExTaxAmount") == 0m && x.Field<int>("GoodsTaxAmount") == 0m));
			Assert("Service Amounts are not populated", rows.All(x => x.Field<int>("ServiceExTaxAmount") == 0m && x.Field<int>("ServiceTaxAmount") == 0m));
		}

		protected override string ReportName => "GetComplianceReportLines_TPA";
		protected override string GroupByCode => ReportLineGroupingListCodes.PaymentTimesAll;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.TransactionHeader;
	}
}
