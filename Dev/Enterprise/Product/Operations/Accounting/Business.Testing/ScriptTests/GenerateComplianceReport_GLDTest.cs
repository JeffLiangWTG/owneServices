using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class GenerateComplianceReport_GLDTest : TestCaseWithFactory
	{
		[TestDate(2023, 09, 01)]
		[SuspendCriticalValidation]
		public void TestGenerateComplianceReport_NoGrouping()
		{
			GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.NoGrouping);
			var sqlToGetGLDPK = "SELECT GLD_PK,DENSE_RANK() OVER(ORDER BY ISNULL(AH_ComplianceSubType, ''), ISNULL(AH_Ledger, ''), ISNULL(AH_TransactionReference, ''), ISNULL(AH_TransactionNum, ''), AH_PK, GLD_PK) as sequence\r\nfrom AccGeneralLedgerData LEFT JOIN AccTransactionHeader on GLD_AH_TransactionHeader = AH_PK LEFT JOIN AccTransactionLines ATL on AccTransactionHeader.AH_PK = ATL.AL_AH\r\nGroup by ISNULL(AH_ComplianceSubType, ''), ISNULL(AH_Ledger, ''), ISNULL(AH_TransactionReference, ''), ISNULL(AH_TransactionNum, ''), AH_PK, GLD_PK, CASE WHEN GLD_Type = 'CBV' THEN GLD_YC_CashBasisVAT ELSE ISNULL(GLD_AL_TransactionLine, GLD_AH_TransactionHeader) END";
			AssertSequencesEqual(sqlToGetGLDPK);
		}

		[TestDate(2023, 09, 01)]
		[SuspendCriticalValidation]
		public void TestGenerateComplianceReport_GroupByDAB()
		{
			GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
			var sqlToGetGLDPK = "select GLD_PK,DENSE_RANK() OVER(ORDER BY GLD_PostDate,  CASE WHEN AH_TransactionType = 'CTR' THEN AH_TransactionType ELSE AH_Ledger END, AH_TransactionType DESC, ISNULL(AH_TransactionReference, ''), ISNULL(AH_TransactionNum, ''), AH_PK\r\n) AS sequence from AccGeneralLedgerData\r\nLEFT JOIN AccTransactionHeader ATH on AccGeneralLedgerData.GLD_AH_TransactionHeader = ATH.AH_PK\r\nLEFT JOIN AccTransactionLines ATL on ATH.AH_PK = ATL.AL_AH\r\ngroup by GLD_PostDate, CASE WHEN AH_TransactionType = 'CTR' THEN AH_TransactionType ELSE AH_Ledger END, AH_TransactionType, AH_TransactionReference, AH_TransactionNum, AH_PK, GLD_PK,CASE WHEN GLD_Type = 'CBV' THEN GLD_YC_CashBasisVAT ELSE ISNULL(GLD_AL_TransactionLine, GLD_AH_TransactionHeader) END, GLD_GC_Company\r\n";
			AssertSequencesEqual(sqlToGetGLDPK);
		}

		[TestDate(2023, 09, 01)]
		[SuspendCriticalValidation]
		public void TestGenerateComplianceReport_GroupByDBW()
		{
			GenerateAccComplianceReportTransactionPivot(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping);
			var sqlToGetGLDPK = "SELECT GLD_PK,dense_rank() over (ORDER BY GLD_PostDate,CASE WHEN AH_TransactionType = 'CTR' THEN AH_TransactionType ELSE ISNULL(AH_Ledger, '') END , ISNULL(AH_TransactionReference, ''), ISNULL(AH_TransactionNum, ''), AH_PK,  CASE WHEN AH_Ledger = 'JC' OR AL_LineType ='WIP' OR AL_LineType ='ACR' THEN NULL ELSE AL_Sequence END, CASE WHEN AH_Ledger = 'JC' OR AL_LineType ='WIP' OR AL_LineType ='ACR' THEN ISNULL(AH_OH, AL_OH) ELSE CASE WHEN GLD_Type = 'CBV' THEN GLD_YC_CashBasisVAT ELSE ISNULL(GLD_AL_TransactionLine, GLD_AH_TransactionHeader) END end)  as sequence\r\nfrom AccGeneralLedgerData\r\nLEFT JOIN AccTransactionHeader on GLD_AH_TransactionHeader = AH_PK\r\nLEFT JOIN AccTransactionLines ON GLD_AL_TransactionLine = AL_PK\r\n";
			AssertSequencesEqual(sqlToGetGLDPK);
		}

		[TestDate(2023, 09, 01)]
		[SuspendCriticalValidation]
		public void TestGenerateComplianceReport_GroupByDBP()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var gLPresentationJournalCategoriesList = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value;
			var closingCategory = gLPresentationJournalCategoriesList.AddNew();
			closingCategory.Code = "CLO";
			closingCategory.Description = (NoResString)"Closing";
			closingCategory.Bool = true;
			closingCategory.Bool2 = false;
			closingCategory.Bool3 = true;
			closingCategory.Bool4 = false;
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, gLPresentationJournalCategoriesList);

			List<DataRow> dataRows = new List<DataRow>();
			CreateJournalLine(dataRows, TransactionTypes.GLStandardJournal, "ELM", ZDateTime.Empty);
			CreateJournalLine(dataRows, TransactionTypes.GLStandardJournal, closingCategory.Code, ZDateTime.Empty);
			CreateJournalLine(dataRows, TransactionTypes.GLReversingJournal, "", ZDateTime.Now.AddMonths(1));
			CreateJournalLine(dataRows, TransactionTypes.GLAutoJournal, "", ZDateTime.Now.AddMonths(2));
			CreateJournalLine(dataRows, TransactionTypes.GLAutoJournal, closingCategory.Code, ZDateTime.Now.AddMonths(2));

			var reportNoGroup = CreateComplianceReport(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.NoGrouping);
			var reportDAB = CreateComplianceReport(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook);
			var reportDBP = CreateComplianceReport(ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithPresentation);

			Factory.Save();

			TestObjectCreator.MockNudgeGLDProcessData(dataRows.ToArray());

			reportNoGroup.GenerateFromQueue();
			reportDAB.GenerateFromQueue();
			reportDBP.GenerateFromQueue();

			AssertPivot(reportNoGroup, 4, new[] { string.Empty });
			AssertPivot(reportDAB, 6, new[] { string.Empty, closingCategory.Code.ToString() });
			AssertPivot(reportDBP, 10, new[] { string.Empty, "ELM", closingCategory.Code.ToString() });

			AccComplianceReport CreateComplianceReport(string groupBy)
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_DateFrom = new ZDate(2023, 09, 01);
				report.ACR_DateTo = new ZDate(2023, 09, 30);

				TestObjectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: AccGeneralLedgerDataSchema.Constants.Prefix, groupBy, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);

				return report;
			}

			void CreateJournalLine(List<DataRow> dataRows, string transactionType, string transactionCategory, ZDateTime dueDate)
			{
				var journal = Factory.NewWithValidTestData<GLJournal>();
				journal.AH_TransactionNum = "Test AJL1";
				journal.AH_TransactionCategory = transactionCategory;
				journal.AH_DueDate = dueDate;
				journal.AH_TransactionType = transactionType;
				var line1 = TestObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
				var line2 = TestObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
				dataRows.Add(((IBusinessObjectInternals)line1).Row);
				dataRows.Add(((IBusinessObjectInternals)line2).Row);
			}
			void AssertPivot(AccComplianceReport complianceReport, int count, IEnumerable<string> categoryList)
			{
				var sql = @$"
SELECT DISTINCT AH_TransactionCategory, ACL_PK
FROM dbo.AccComplianceReportTransactionPivot
		 JOIN dbo.AccGeneralLedgerData ON GLD_PK = ACL_ParentID
		 JOIN dbo.AccTransactionHeader ON GLD_AH_TransactionHeader = AH_PK
WHERE ACL_ACR_Report = '{complianceReport.PK}'
";
				var pivotTableRows = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
				var categoryRows = pivotTableRows.Rows.Cast<DataRow>().Select(x => x["AH_TransactionCategory"]);
				AssertEquals(count, pivotTableRows.Rows.Count);
				AssertEquals(true, categoryRows.All(x => categoryList.Contains(x)));
			}
		}

		ZGuid[] GenerateAccComplianceReportTransactionPivot(string groupBy)
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2023);

			var testDate = new ZDateTime(2023, 09, 01);

			var aRInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "121", TestObjectCreator.AUD, 1M, 10M, 10M, 10M, 10M);
			aRInvoice.Lines[0].AL_PostDate = testDate;
			aRInvoice.Lines[0].AL_ReverseDate = testDate;
			aRInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;

			var apJournal = TestObjectCreator.CreateJournal<APJournal>(100m, testDate, TestObjectCreator.ABIGAS.PK);

			var cashBasisVAT = TestObjectCreator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001", GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasisVAT.TransactionLine.AL_AT = TestObjectCreator.GST1.PK;

			var job = TestObjectCreator.InsertJobHeader(GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			var wip = TestObjectCreator.CreateWIP(job);
			wip.AL_LineAmount = 20m;
			wip.AL_OSAmount = 20m;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;
			var acr = TestObjectCreator.CreateAccrual(charge1);

			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			Factory.Save();

			((INeedRow)aRInvoice.Lines[0]).Row.SetAdded();

			TestObjectCreator.MockNudgeGLDProcessData([((INeedRow)aRInvoice.Lines[0]).Row, ((INeedRow)cashBasisVAT).Row, ((INeedRow)apJournal).Row, ((INeedRow)wip).Row, ((INeedRow)acr).Row]);

			var newFactory = Factory.CreateNewFactory();
			var report = newFactory.NewWithValidTestData<AccComplianceReport>();
			TestObjectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: AccGeneralLedgerDataSchema.Constants.Prefix, reportLineGrouping: groupBy, reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);
			newFactory.Save();

			report.GenerateFromQueue();
			return new[] { apJournal.PK, wip.PK, acr.PK, aRInvoice.PK, cashBasisVAT.TransactionLine.AL_AH };
		}

		void AssertSequencesEqual(string sqlToGetGLDPK)
		{
			var gldRows = DataUtils.GetDataTableFromQuery(Db.Connection, sqlToGetGLDPK).AsEnumerable();
			var gldPKandSequence = gldRows.ToDictionary(
				row => row["GLD_PK"],
				row => row["sequence"]);
			var sql = "SELECT ACL_ParentID, ACL_ReportSequence FROM dbo.AccComplianceReportTransactionPivot ORDER by ACL_ReportSequence";
			var pivotTableRows = DataUtils.GetDataTableFromQuery(Db.Connection, sql).AsEnumerable();
			AssertEquals(gldRows.Count(), pivotTableRows.Count());
			for (var i = 0; i < pivotTableRows.Count(); i++)
			{
				var gldPK = pivotTableRows.ElementAt(i)["ACL_ParentID"];
				var reportSequence = pivotTableRows.ElementAt(i)["ACL_ReportSequence"];
				AssertEquals(reportSequence, Convert.ToInt32(gldPKandSequence[gldPK]));
			}
		}

		TestObjectCreator TestObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
