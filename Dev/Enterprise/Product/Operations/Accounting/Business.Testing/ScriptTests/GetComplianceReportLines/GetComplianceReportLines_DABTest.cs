using System;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetComplianceReportLines_DABTest : GetComplianceReportLines_BaseTest
	{
		public override void TestGetComplianceReportLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, TablePrefix, GroupByCode, reportLineOrdering: ReportLineGroupingListCodes.Organisation);

			var glHeader1 = TestObjectCreator.CreateGLHeader("11111111");
			var glHeader2 = TestObjectCreator.CreateGLHeader("99999999");

			var invoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("ABI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.ABIGAS);
			Assert("Has Lines", invoice1.Lines.Count > 0);
			var line11 = invoice1.Lines[0];
			line11.AL_AT = TestObjectCreator.GST1.PK;
			line11.AL_AG = glHeader1.PK;
			var line12 = TestObjectCreator.CreateInvoiceLine(invoice1, TestObjectCreator.AUD, 1m, 200m);
			line12.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line12.AL_AG = glHeader2.PK;

			var invoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("SHI0001", TestObjectCreator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.AALSHI);
			Assert("Has Lines", invoice2.Lines.Count > 0);
			var line21 = invoice2.Lines[0];
			line21.AL_AT = TestObjectCreator.GST1.PK;
			line21.AL_AG = glHeader2.PK;
			var line22 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1m, 200m);
			line22.AL_AT = TestObjectCreator.GSTFREE1.PK;
			line22.AL_AG = glHeader2.PK;

			Factory.Save();

			TestObjectCreator.CreateComplianceReportQueueEntry(report, invoice1, invoice2);
			TestObjectCreator.CreateComplianceReportQueueEntry(report, line11, line12, line21, line22);
			report.GenerateFromQueue();

			var rows = RunScript(report, ReportName, TablePrefix).AsEnumerable().ToArray();

			AssertEquals(6, rows.Length);

			var result = rows.Where(x => x.Field<Guid>("AH_PK") == invoice1.PK).OrderBy(x => x["AG_AccountNum"]).ToList();
			AssertEquals(4, result.Count);
			AssertEquals(glHeader1.AccountNum, result[0].Field<string>("AG_AccountNum"));
			AssertEquals(glHeader1.AccountNum, result[1].Field<string>("AG_AccountNum"));
			AssertEquals(glHeader2.AccountNum, result[2].Field<string>("AG_AccountNum"));
			AssertEquals(glHeader2.AccountNum, result[3].Field<string>("AG_AccountNum"));

			result = rows.Where(x => x.Field<Guid>("AH_PK") == invoice2.PK).ToList();
			AssertEquals(2, result.Count);

			AssertEquals(glHeader2.AccountNum, result[0].Field<string>("AG_AccountNum"));
			AssertEquals(glHeader2.AccountNum, result[1].Field<string>("AG_AccountNum"));
		}

		[TestDate(2023, 01, 01)]
		[SuspendCriticalValidation]
		public void TestGetComplianceReportLines_Base_GLD()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var testDate = new ZDateTime(2023, 01, 01);

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, testDate, TestObjectCreator.ABIGAS.PK);

			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			Factory.Save();

			TestObjectCreator.MockNudgeGLDProcessData([apJournal]);

			var newFactory = Factory.CreateNewFactory();
			var report = newFactory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report,
				baseTablePrefix: AccGeneralLedgerDataSchema.Constants.Prefix, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();

			report.GenerateFromQueue();

			var rows1 = RunScript(report, ReportName, ReportBaseTablePrefixListCodes.AllTransactions).AsEnumerable().ToArray();
			var rows2 = RunScript(report, ReportName, ReportBaseTablePrefixListCodes.GeneralLedgerData).AsEnumerable().ToArray();
			AssertEquals(0, rows1.Length);
			AssertEquals(2, rows2.Length);
		}

		protected override string ReportName => "GetComplianceReportLines_DAB";
		protected override string GroupByCode => ReportLineGroupingListCodes.DayBook;
		protected override string TablePrefix => ReportBaseTablePrefixListCodes.AllTransactions;
	}
}
