using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
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

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class ViewComplianceReportLine_GLDTest : TestCaseWithFactory
	{
		[TestDate(2023, 09, 30)]
		[SuspendCriticalValidation]
		public void TestViewComplianceReportLine_GLD_NoGrouping()
		{
			var pkList = GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.NoGrouping);
			var sql = "SELECT * FROM dbo.ViewComplianceReportLine_GLD ORDER by ACL_ReportSequence";
			var pivotTableRows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			AssertEquals(24, pivotTableRows.Count());
			Assert(pivotTableRows.Take(6).All(row => row.Field<string>("AH_PK") == null));
			Assert(pivotTableRows.Skip(6).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[0]));
			Assert(pivotTableRows.Skip(8).Take(12).All(row => row.Field<Guid>("AH_PK") == pkList[1]));
			Assert(pivotTableRows.Skip(20).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[2]));
			Assert(pivotTableRows.Skip(22).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[3]));
		}

		[TestDate(2023, 09, 30)]
		[SuspendCriticalValidation]
		public void TestViewComplianceReportLine_GLD_GroupByDAB()
		{
			var pkList = GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
			var sql = "SELECT * FROM dbo.ViewComplianceReportLine_GLD ORDER by ACL_ReportSequence";
			var pivotTableRows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			AssertEquals(12, pivotTableRows.Count());
			Assert(pivotTableRows.Take(3).All(row => row.Field<string>("AH_PK") == null && row.Field<int>("ACL_ReportSequence") == 1));
			Assert(pivotTableRows.Skip(3).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[0] && row.Field<int>("ACL_ReportSequence") == 2));
			Assert(pivotTableRows.Skip(5).Take(4).All(row => row.Field<Guid>("AH_PK") == pkList[1] && row.Field<int>("ACL_ReportSequence") == 3));
			Assert(pivotTableRows.Skip(9).Take(1).All(row => row.Field<Guid>("AH_PK") == pkList[2] && row.Field<int>("ACL_ReportSequence") == 4));
			Assert(pivotTableRows.Skip(10).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[3] && row.Field<int>("ACL_ReportSequence") == 5));
		}

		[TestDate(2023, 09, 30)]
		[SuspendCriticalValidation]
		public void TestViewComplianceReportLine_GLD_GroupByDBW()
		{
			var pkList = GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
			var sql = "SELECT * FROM dbo.ViewComplianceReportLine_GLD ORDER by ACL_ReportSequence";
			var pivotTableRows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();

			AssertEquals(12, pivotTableRows.Count());
			Assert(pivotTableRows.Take(3).All(row => row.Field<string>("AH_PK") == null && row.Field<int>("ACL_ReportSequence") == 1));
			Assert(pivotTableRows.Skip(3).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[0] && row.Field<int>("ACL_ReportSequence") == 2));
			Assert(pivotTableRows.Skip(5).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[1] && row.Field<int>("ACL_ReportSequence") == 3));
			Assert(pivotTableRows.Skip(7).Take(2).All(row => row.Field<Guid>("AH_PK") == pkList[1] && row.Field<int>("ACL_ReportSequence") == 4));

			Assert(pivotTableRows.Skip(9).Take(1).All(row => row.Field<Guid>("AH_PK") == pkList[2] && row.Field<int>("ACL_ReportSequence") == 5));
			Assert(pivotTableRows.Skip(10).Take(1).All(row => row.Field<Guid>("AH_PK") == pkList[3] && row.Field<int>("ACL_ReportSequence") == 6));
			Assert(pivotTableRows.Skip(11).Take(1).All(row => row.Field<Guid>("AH_PK") == pkList[3] && row.Field<int>("ACL_ReportSequence") == 7));
		}

		ZGuid[] GenerateAccComplianceReportTransactionPivot(string groupBy)
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var testDate = new ZDateTime(2023, 09, 30);

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M,
				10M, 10M, 10M, 10M);
			aRInvoice.Lines[0].AL_PostDate = testDate;
			aRInvoice.Lines[0].AL_ReverseDate = testDate;
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			TestObjectCreator.CreateInvoiceLine(aRInvoice, GlbCompany.CurrentCompany.LocalCurrency, 1, 100, 10, 0, TestObjectCreator.GLHeader1.PK);

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, testDate, TestObjectCreator.ABIGAS.PK);

			var cashBasisVAT = TestObjectCreator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001",
				GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasisVAT.TransactionLine.AL_AT = TestObjectCreator.GST1.PK;

			var job = TestObjectCreator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			var wip = TestObjectCreator.CreateWIP(job);
			wip.AL_LineAmount = 20m;
			wip.AL_OSAmount = 20m;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			var acr = TestObjectCreator.CreateAccrual(charge1);

			var gLGJL = TestObjectCreator.CreateGLJournal("GJL", testDate, testDate);
			var gLGJLline1 = TestObjectCreator.CreateGLJournalLine(gLGJL, 250M, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			var gLGJLline2 = TestObjectCreator.CreateGLJournalLine(gLGJL, 250M, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			Factory.Save();

			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();
			((INeedRow)aRInvoice.Lines[1]).Row.SetAdded();

			TestObjectCreator.MockNudgeGLDProcessData([((INeedRow)aRInvoice.Lines[0]).Row, ((INeedRow)aRInvoice.Lines[1]).Row, ((INeedRow)cashBasisVAT).Row, ((INeedRow)apJournal).Row, ((INeedRow)wip).Row, ((INeedRow)acr).Row, ((INeedRow)gLGJLline1).Row, ((INeedRow)gLGJLline2).Row]);

			var newFactory = Factory.CreateNewFactory();
			var report = newFactory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report,
				baseTablePrefix: AccGeneralLedgerDataSchema.Constants.Prefix, reportLineGrouping: groupBy,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();

			report.GenerateFromQueue();
			return new[] { apJournal.PK, aRInvoice.PK, cashBasisVAT.TransactionLine.AL_AH, gLGJL.PK };
		}

		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
