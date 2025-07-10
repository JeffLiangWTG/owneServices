using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_TXRTest : GetComplianceReportLines_BaseTest
	{
		public void TestGetComplianceReportLinesWithComplianceSequence()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();

				var reportConfigurations = new ComplianceReportConfigurationCollection(Factory);
				var config = TestObjectCreator.CreateConfigurationForComplianceReport(report, reportConfigurations);
				TestObjectCreator.CreateConfigurationSettingsForComplianceReport(config, LedgerTypes.AccountsPayable, "", complianceSubType: "API");
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.TaxReporting);

				var menuPK = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "ARInvoice PE Factura")).PK;
				var sequence = TestObjectCreator.CreateNewComplianceSequence(menuPK, "API", 1, 100, 25);
				sequence.XD_Prefix = "API";
				sequence.XD_Code = "API";
				sequence.XD_Description = "API2021";
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;
				sequence.XD_GC_Company = GlbCompany.CurrentCompany.PK;
				Factory.Save();

				var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count, TestObjectCreator.EUR, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
				invoice.AH_XD_ComplianceBook = sequence.PK;

				Assert("Has Lines", invoice.Lines.Count > 0);
				var line1 = invoice.Lines[0];
				line1.AL_AT = TestObjectCreator.GST1.PK;
				var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1m, 200m);
				line2.AL_AT = TestObjectCreator.GSTFREE1.PK;
				var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1m, 300m);
				line3.AL_AT = TestObjectCreator.VATSPV.PK;
				var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1m, 400m);
				line4.AL_AT = TestObjectCreator.GST1.PK;
				var line5 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.EUR, 1m, 770.22m);
				line5.AL_AT = TestObjectCreator.VATSPV.PK;
				Factory.Save();

				TestObjectCreator.CreateComplianceReportQueueEntry(report, line1, line2, line3, line4, line5);

				report.GenerateFromQueue();

				var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable();

				AssertEquals(5, rows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());

				var row = rows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.GST1.AT_Code);
				AssertNotNull(row);
				AssertEquals(10M, row.Field<decimal>("AL_TaxRate"));
				AssertEquals(0M, Convert.ToDecimal(row["SPVTaxAmount"]));
				AssertEquals("RAT", row.Field<string>("AT_Type"));
				AssertEquals("API - API2021", row.Field<string>("ComplianceSequence"));

				row = rows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.GSTFREE1.AT_Code);
				AssertNotNull(row);
				AssertEquals(0M, row.Field<decimal>("AL_TaxRate"));
				AssertEquals(0M, Convert.ToDecimal(row["SPVTaxAmount"]));
				AssertEquals("RAT", row.Field<string>("AT_Type"));
				AssertEquals("API - API2021", row.Field<string>("ComplianceSequence"));

				row = rows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.VATSPV.AT_Code);
				AssertNotNull(row);
				AssertEquals(22M, row.Field<decimal>("AL_TaxRate"));
				AssertEquals("RAT", row.Field<string>("AT_Type"));
				AssertEquals("API - API2021", row.Field<string>("ComplianceSequence"));
			}
		}

		public void TestGetComplianceReportLines_RateFromLine()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, reportLineGrouping: ReportLineGroupingListCodes.TaxReporting);

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("I0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = TestObjectCreator.GST1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 200m);
			line2.AL_AT = TestObjectCreator.VATSPV.PK;
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 300m);
			line3.AL_AT = TestObjectCreator.VATSPV.PK;
			var line4 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 400m);
			line4.AL_AT = TestObjectCreator.GST1.PK;
			var line5 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 500m);
			line5.AL_AT = TestObjectCreator.VATSPV.PK;
			var line6 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 600m);
			line6.AL_AT = TestObjectCreator.GST1.PK;
			var line7 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 700m);
			line7.AL_AT = TestObjectCreator.VATSPV.PK;
			var line8 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1m, 800m);
			line8.AL_AT = TestObjectCreator.GST1.PK;

			SetLineData(line1, 8, 2);
			SetLineData(line2, 18, 3);
			SetLineData(line3, 16, 2, 14, 2);
			SetLineData(line4, 8, 2);
			SetLineData(line5, 18, 3);
			SetLineData(line6, 8, 2);
			SetLineData(line7, 16, 2, 14, 2);
			SetLineData(line8, 8, 2);

			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, line1, line2, line3, line4, line5);
			report.GenerateFromQueue();

			var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable();

			var row = rows.FirstOrDefault(x => x.Field<decimal>("AL_TaxRate") == 4M);
			AssertNotNull(row);
			AssertEquals(0M, Convert.ToDecimal(row["SPVTaxAmount"]));

			row = rows.FirstOrDefault(x => x.Field<decimal>("AL_TaxRate") == 6M);
			AssertNotNull(row);
			AssertEquals(42M, Convert.ToDecimal(row["SPVTaxAmount"]));

			row = rows.FirstOrDefault(x => x.Field<decimal>("AL_TaxRate") == 8M);
			AssertNotNull(row);
			AssertEquals(24M, Convert.ToDecimal(row["SPVTaxAmount"]));

			void SetLineData(InvoicingLineBase line, int numerator, int denominator, int extraRateNumerator = 0, int extraRateDenominator = 1)
			{
				line.AL_TaxRateNumerator = numerator;
				line.AL_TaxRateDenominator = denominator;
				line.AL_TaxExtraRateNumerator = extraRateNumerator;
				line.AL_TaxExtraRateDenominator = extraRateDenominator;
			}
		}

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

			AssertEquals(3, dataRows.GroupBy(x => x.Field<int>("ACL_ReportSequence")).Count());

			var row = dataRows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.GST1.AT_Code);
			AssertNotNull(row);
			AssertEquals(10M, row.Field<decimal>("AL_TaxRate"));
			AssertEquals(0M, row.Field<decimal>("SPVTaxAmount"));
			AssertEquals("RAT", row.Field<string>("AT_Type"));
			AssertNullOrEmpty(row.Field<string>("ComplianceSequence"));

			row = dataRows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.GSTFREE1.AT_Code);
			AssertNotNull(row);
			AssertEquals(0M, row.Field<decimal>("AL_TaxRate"));
			AssertEquals(0M, row.Field<decimal>("SPVTaxAmount"));
			AssertEquals("RAT", row.Field<string>("AT_Type"));
			AssertNullOrEmpty(row.Field<string>("ComplianceSequence"));

			row = dataRows.FirstOrDefault(x => x.Field<string>("AT_Code") == TestObjectCreator.VATSPV.AT_Code);
			AssertNotNull(row);
			AssertEquals(22M, row.Field<decimal>("AL_TaxRate"));
			AssertEquals("RAT", row.Field<string>("AT_Type"));
			AssertNullOrEmpty(row.Field<string>("ComplianceSequence"));
		}

		protected override string ReportName => "GetComplianceReportLines_TXR";
		protected override string GroupByCode => ReportLineGroupingListCodes.TaxReporting;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.TransactionLine;
	}
}
