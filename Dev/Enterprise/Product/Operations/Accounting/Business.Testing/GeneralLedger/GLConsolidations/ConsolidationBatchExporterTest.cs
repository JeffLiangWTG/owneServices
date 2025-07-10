using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations.Testing
{
	public class ConsolidationBatchExporterTest : TestCaseWithFactory
	{
		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestAllTransactionsAggregation()
		{
			TransactionCreator.CreateTransactions();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestReceipts()
		{
			TransactionCreator.CreateReceipts();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestTimeOutOfExportConsolidationBatch()
		{
			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();
			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			var exporter = new ConsolidationBatchExporter(batchPKs, false);
			AssertEquals(1800, exporter.GetCommandForReadingBatchContents_ExposedForTest().CommandTimeout);
		}

		public void TestPayments()
		{
			TransactionCreator.CreatePayments();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[SuspendCriticalValidation]
		public void TestDiscounts()
		{
			TransactionCreator.CreateDiscounts();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[SuspendCriticalValidation]
		public void TestOverpayments()
		{
			TransactionCreator.CreateOverpayments();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestARAPJournals()
		{
			TransactionCreator.CreateARAPJournals();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestGLJournals()
		{
			TransactionCreator.CreateGLJournals();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2014, 04, 24)]
		public void TestManualEliminationJournalsAreReBatchedInParentGroups()
		{
			var eliminationJournal = TestObjectCreator.CreateGLJournal(ZArchitecture.Core.TransactionTypes.GLStandardJournal, new ZDateTime(2008, 2, 15), new ZDateTime(2008, 2, 28, 23, 59, 0), null);
			eliminationJournal.AH_TransactionCategory = eliminationCategory;
			var glHeader1 = TestObjectCreator.GLHeader1;
			glHeader1.AG_AccountNum = "GLHeader1";
			var glHeader2 = TestObjectCreator.GLHeader2;
			glHeader2.AG_AccountNum = "GLHeader2";
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 1m, DebitCredit.DR, glHeader2.PK).AL_OH = Org1.PK;
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 1m, DebitCredit.CR, glHeader1.PK).AL_OH = Org1.PK;
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 10m, DebitCredit.DR, glHeader2.PK).AL_OH = Org2.PK;
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 10m, DebitCredit.CR, glHeader1.PK).AL_OH = Org2.PK;
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 100m, DebitCredit.DR, glHeader2.PK).AL_OH = Org3.PK;
			TestObjectCreator.CreateGLJournalLine(eliminationJournal, 100m, DebitCredit.CR, glHeader1.PK).AL_OH = Org3.PK;
			BusinessObjectFactory.SaveTogether(Factory, new AggregateWrapper(eliminationJournal, eliminationJournal));

			AssertMultilineASCIIEquals("PRECONDITION: Normal Aggregation (no transaction category", "", TakeUpSubLedgersAndReturnDebitCreditString(""));
			AssertMultilineASCIIEquals("PRECONDITION: Aggregation with eliminations transaction category", "200802 GLHeader1 -111.00\r\n200802 GLHeader2 111.00", TakeUpSubLedgersAndReturnDebitCreditString(eliminationCategory));

			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();
			var group1ExpectedValues = @"
200802 GLHeader1 BNE BRN AUSTRALIA            EDI              AUD  -110.00 AUD  -110.00
200802 GLHeader1 BNE BRN AUSTRALIA            EDI ZOrg1        AUD    -1.00 AUD    -1.00
200802 GLHeader2 BNE BRN AUSTRALIA            EDI              AUD   110.00 AUD   110.00
200802 GLHeader2 BNE BRN AUSTRALIA            EDI ZOrg1        AUD     1.00 AUD     1.00";
			AssertMultilineASCIIEquals("Group1 Export", group1ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));

			var group2ExpectedValues = @"
200802 GLHeader1 BNE BRN AUSTRALIA            EDI              AUD  -100.00 AUD  -100.00
200802 GLHeader1 BNE BRN AUSTRALIA            EDI ZOrg1        AUD    -1.00 AUD    -1.00
200802 GLHeader1 BNE BRN OCEANIA              EDI ZOrg2        AUD   -10.00 AUD   -10.00
200802 GLHeader2 BNE BRN AUSTRALIA            EDI              AUD   100.00 AUD   100.00
200802 GLHeader2 BNE BRN AUSTRALIA            EDI ZOrg1        AUD     1.00 AUD     1.00
200802 GLHeader2 BNE BRN OCEANIA              EDI ZOrg2        AUD    10.00 AUD    10.00";
			new ConsolidationBatchCreator(group2.PK).ReadDataAndCreateBatches();
			AssertMultilineASCIIEquals("Group2 Export", group2ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));

			var group3ExpectedValues = @"
200802 GLHeader1 BNE BRN AUSTRALIA            EDI ZOrg1        AUD    -1.00 AUD    -1.00
200802 GLHeader1 BNE BRN GLOBAL               EDI ZOrg3        AUD  -100.00 AUD  -100.00
200802 GLHeader1 BNE BRN OCEANIA              EDI ZOrg2        AUD   -10.00 AUD   -10.00
200802 GLHeader2 BNE BRN AUSTRALIA            EDI ZOrg1        AUD     1.00 AUD     1.00
200802 GLHeader2 BNE BRN GLOBAL               EDI ZOrg3        AUD   100.00 AUD   100.00
200802 GLHeader2 BNE BRN OCEANIA              EDI ZOrg2        AUD    10.00 AUD    10.00";
			new ConsolidationBatchCreator(group3.PK).ReadDataAndCreateBatches();
			AssertMultilineASCIIEquals("Group3 Export", group3ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));
		}

		string GetExportContentsAsString(IEnumerable<ConsolidationBatchDetailsRow> batchDetailsRows)
		{
			var result = new List<string>();
			foreach (var batchDetailsRow in batchDetailsRows)
			{
				result.Add(batchDetailsRow.ToString());
			}
			result.Sort();
			return string.Join("\r\n", result);
		}

		public void TestManualGLEliminationJournalsAreIncludedInDataExport()
		{
			TransactionCreator.CreateGLJournals();
			TransactionCreator.CreateEliminationGLJournals(eliminationCategory);
			Factory.Save();
			CreateBatchesAndExport();
			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			AssertAggregation(new ConsolidationBatchExporter(batchPKs, true).Export(), eliminationCategory);
		}

		public void TestAutoGLEliminationJournalsAreIncludedInDataExport()
		{
			TransactionCreator.CreateGLJournals();
			TransactionCreator.CreateEliminationGLJournals(eliminationCategory);
			Factory.Save();

			CreateBatchesAndExport();

			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			AssertAggregation(new ConsolidationBatchExporter(batchPKs, true).Export(), eliminationCategory);
		}

		public void TestManualGLEliminationJournalsAreNotIncludedInEliminationJournalCalculation()
		{
			TransactionCreator.CreateGLJournals();
			TransactionCreator.CreateEliminationGLJournals(eliminationCategory);
			Factory.Save();
			CreateBatchesAndExport();

			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			AssertEquals("Should include manual elimination journals", true, new ConsolidationBatchExporter(batchPKs, true).Export().Any(x => !x.TransactionOrganisationCode.IsEmpty));
			AssertEquals("Should not include manual elimination journals", false, new ConsolidationBatchExporter(batchPKs, false).Export().Any(x => !x.TransactionOrganisationCode.IsEmpty));
			var journalCreator = new EliminationJournalCreator();
			journalCreator.Create(batchPKs);
			AssertEquals("no elimination journals should be created", 0, Factory.Load<AccConsolidationBatch>(new ZQuery(AccConsolidationBatchSchema.YB_AH_EliminationJournal, SQLComparisonOperator.NotEqual, null)).Length);
		}

		public void TestARAPTransfers()
		{
			TransactionCreator.CreateARAPTransfers();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[SuspendCriticalValidation]
		public void TestARAPExchangeDifferences()
		{
			TransactionCreator.CreateARAPExchangeDifferences();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestContras()
		{
			TransactionCreator.CreateContras();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[SuspendCriticalValidation]
		public void TestInvoices()
		{
			TransactionCreator.CreateInvoices();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2013, 07, 03)]
		[SuspendCriticalValidation]
		public void TestLargeBatchInsert()
		{
			var apInvoicePositive = TestObjectCreator.CreateAPInvoice<APInvoice>("111", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, Org1);
			apInvoicePositive.Lines[0].AL_AC = TestObjectCreator.CC1.PK;
			apInvoicePositive.AH_PostDate = June2008;
			apInvoicePositive.AH_ComplianceSubType = PeruComplianceInfo.ComplianceSubTypeCodes.TXI;

			for (int i = 1; i <= 1234; i++)
			{
				var line = TestObjectCreator.CreateAPInvoiceLine(apInvoicePositive, null, TestObjectCreator.CC1, TestObjectCreator.AUD, 1.0m, "Desc", -10m);
				line.AL_AC = TestObjectCreator.CC1.PK;
			}

			Factory.Save();

			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestCashBasisVATRecords()
		{
			var pendingInputTaxAccount = TestObjectCreator.CreateInputTaxReceivablePendingAccount();
			var pendingOutputTaxAccount = TestObjectCreator.CreateOutputTaxPayablePendingAccount();
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingInputTaxAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pendingOutputTaxAccount.PK.ToGuid());

			var company = Factory.Load<GlbCompany>(Env.CurrentCompany.PK);
			company.GC_IsGSTCashBasis = true;

			var invoice = (APInvoice)TestObjectCreator.CreateInvoiceWithCashVATLine(typeof(APInvoice), 100, 10);
			var matchLink = TestObjectCreator.CreateMatchLinkToPayAPInvoice(invoice, ZDateTime.Today.AddMonths(1));
			var payment = Factory.LoadTop1<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment));
			payment.AH_AB = BankAccountAUD.PK;
			Factory.Save();

			var wip = Factory.LoadTop1<WIP>(new ZQuery(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.WIP));
			wip.RelatedJobCharge.ReverseWIP(wip.AL_PostDate);
			Factory.Save();

			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2008, 06, 15)]
		[SuspendCriticalValidation]
		public void TestCreditNotes()
		{
			TransactionCreator.CreateCreditNotes();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[SuspendCriticalValidation]
		public void TestAdjustmentNotes()
		{
			TransactionCreator.CreateAdjustmentNotes();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestCBTransfers()
		{
			TransactionCreator.CreateCBTransfers();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestCBExchangeDifferences()
		{
			TransactionCreator.CreateCBExchangeDifferences();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestCBDirectPayments()
		{
			TransactionCreator.CreateCBDirectPayments();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestCBDirectReceipts()
		{
			TransactionCreator.CreateCBDirectReceipts();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		public void TestJobCostingJournals()
		{
			TransactionCreator.CreateJobCostingJournals();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2008, 06, 15)]
		public void TestWIPsAndAccruals()
		{
			TransactionCreator.CreateWIPsAndAccruals();
			Factory.Save();
			AssertAggregation(CreateBatchesAndExport());

			var before = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			CreateBatchesAndExport();
			var after = Factory.GetDatabaseCount(typeof(AccConsolidationBatch));
			AssertEquals("There should be no new consolidation batches when re-exporting", before, after);
		}

		[TestDate(2008, 02, 15)]
		public void TestJobRevenueJournals()
		{
			var jobRevenueJournalControlAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_Description, "Job Revenue Journal Control Account")) ?? TestObjectCreator.CreateJobRevenueJournalControlAccount();

			TestObjectCreator.CC1.AC_AG_RevenueAccount = TestObjectCreator.CreateGLHeader("TEST111REV").PK;
			TestObjectCreator.CC1.AC_AG_CostAccount = TestObjectCreator.CreateGLHeader("TEST22COST").PK;

			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.SetJobRevenueJournalsLineTypeBasedonCostRevenueGLAccountused.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var jrj1 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 10m);
				jrj1.AH_OH = Org1.PK;
				jrj1.Lines[0].AL_OH = Org1.PK;
				jrj1.Lines[1].AL_OH = Org1.PK;

				var jrj2 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100m);
				jrj2.AH_OH = Org2.PK;
				jrj2.Lines[0].AL_OH = Org2.PK;
				jrj2.Lines[1].AL_OH = Org2.PK;

				var jrj3 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 1000m);
				jrj3.AH_OH = Org3.PK;
				jrj3.Lines[0].AL_OH = Org3.PK;
				jrj3.Lines[1].AL_OH = Org3.PK;
				Factory.Save();
				AssertMultilineASCIIEquals("Precondition: Normal Aggregation", $@"200802 TEST111REV -1,110.00
200802 TEST22COST 1,110.00", TakeUpSubLedgersAndReturnDebitCreditString(""));

				var group1ExpectedValues = @"
200802 TEST111REV BNE CES AUSTRALIA            EDI              AUD -1110.00 AUD -1110.00
200802 TEST22COST BNE CES AUSTRALIA            EDI              AUD  1110.00 AUD  1110.00";
				new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();
				AssertMultilineASCIIEquals("Group1 Export", group1ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));

				var group2ExpectedValues = @"
200802 TEST111REV BNE CES AUSTRALIA            EDI              AUD -1010.00 AUD -1010.00
200802 TEST111REV BNE CES OCEANIA              EDI              AUD  -100.00 AUD  -100.00
200802 TEST22COST BNE CES AUSTRALIA            EDI              AUD  1010.00 AUD  1010.00
200802 TEST22COST BNE CES OCEANIA              EDI              AUD   100.00 AUD   100.00";
				new ConsolidationBatchCreator(group2.PK).ReadDataAndCreateBatches();
				AssertMultilineASCIIEquals("Group2 Export", group2ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));

				var group3ExpectedValues = @"
200802 TEST111REV BNE CES AUSTRALIA            EDI              AUD   -10.00 AUD   -10.00
200802 TEST111REV BNE CES GLOBAL               EDI              AUD -1000.00 AUD -1000.00
200802 TEST111REV BNE CES OCEANIA              EDI              AUD  -100.00 AUD  -100.00
200802 TEST22COST BNE CES AUSTRALIA            EDI              AUD    10.00 AUD    10.00
200802 TEST22COST BNE CES GLOBAL               EDI              AUD  1000.00 AUD  1000.00
200802 TEST22COST BNE CES OCEANIA              EDI              AUD   100.00 AUD   100.00";
				new ConsolidationBatchCreator(group3.PK).ReadDataAndCreateBatches();
				AssertMultilineASCIIEquals("Group3 Export", group3ExpectedValues.TrimStart(), GetExportContentsAsString(new ConsolidationBatchExporter(GetAllBatchPKs(), true).Export()));
			}
		}

		#region Implementation

		IEnumerable<ConsolidationBatchDetailsRow> CreateBatchesAndExport()
		{
			new ConsolidationBatchCreator(group1.PK).ReadDataAndCreateBatches();
			var batchPKs = Factory.Load<AccConsolidationBatch>(new ZQuery()).Select(x => x.PK);
			return new ConsolidationBatchExporter(batchPKs, false).Export();
		}

		void AssertAggregation(IEnumerable<ConsolidationBatchDetailsRow> batchRows, string transactionCategory = "")
		{
			var expected = TakeUpSubLedgersAndReturnDebitCreditString(transactionCategory);
			var actual = GetDebitCreditsString(getDebitsCredits(batchRows));
			AssertMultilineASCIIEquals("Comparison of GL Aggregation (expected) vs Consolidation Batch Files (actual)", expected, actual);
		}

		IEnumerable<KeyValuePair<string, decimal>> getDebitsCredits(IEnumerable<ConsolidationBatchDetailsRow> batchRows)
		{
			var debitsCredits = new Dictionary<string, decimal>();

			foreach (var detailsRow in batchRows)
			{
				addAmount(debitsCredits, detailsRow.Period, detailsRow.GLAccount, detailsRow.AmountInPostingCompanyCurrency);
			}

			return debitsCredits.Where(x => x.Value != 0m);
		}

		string GetDebitCreditsString(IEnumerable<KeyValuePair<string, decimal>> debitsCredits)
		{
			List<string> exportedValues = new List<string>();
			foreach (var dbcr in debitsCredits)
			{
				exportedValues.Add(string.Format("{0} {1}\r\n", dbcr.Key, dbcr.Value.ToString("N2")));
			}
			exportedValues.Sort();
			return string.Join("", exportedValues);
		}

		string TakeUpSubLedgersAndReturnDebitCreditString(string transactionCategory)
		{
			takeUpLedgers();

			string sql = string.Format(@"SELECT AA_Period, ISNULL(AG_AccountNum, '*Empty*') AG_AccountNum, SUM(AA_Amount) AA_Amount
						FROM dbo.AccGLAggregate
						LEFT OUTER JOIN dbo.AccGLHeader
							ON AA_AG = AG_PK
						WHERE AA_TransactionCategory IN ('','{0}')
						GROUP BY AA_Period, AG_AccountNum
                        HAVING SUM(AA_Amount) <> 0
                        ORDER BY AA_Period, AG_AccountNum", transactionCategory);

			var aggregateValues = new StringBuilder();

			using (var reader = TestConnection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					string period = Convert.ToString(reader["AA_Period"]);
					string accountNum = (string)reader["AG_AccountNum"];
					decimal amount = (decimal)reader["AA_Amount"];
					aggregateValues.AppendFormat("{0} {1} {2}\r\n", period, accountNum, amount.ToString("N2"));
				}
			}
			return aggregateValues.ToString();
		}

		void takeUpLedgers()
		{
			string sql = "DELETE FROM dbo.AccChargeCode WHERE AC_Code IN ('BOND', 'CLAIM')";
			TestConnection.ExecuteNonQuery(sql);

			sql = "EXEC TakeUpSubledgers @Company, 'TST'";
			using (DbCommand command = TestConnection.Command(sql))
			{
				command.AddParameter("@Company", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		void addAmount(Dictionary<string, decimal> debitsCredits, int postingPeriod, string account, decimal amount)
		{
			decimal currentValue;
			string key = postingPeriod + " " + account;
			if (debitsCredits.TryGetValue(key, out currentValue))
			{
				currentValue += amount;
				debitsCredits[key] = currentValue;
			}
			else
			{
				debitsCredits.Add(key, amount);
			}
		}

		TestObjectCreator TestObjectCreator;
		TestBulkTransactionCreator TransactionCreator;
		AccConsolidationGroup group1, group2, group3;
		ZString eliminationCategory;
		OrgHeader Org1, Org2, Org3;

		IEnumerable<ZGuid> GetAllBatchPKs()
		{
			var query = new ZQuery();
			query.ReLoadExistingRows = true;
			return Factory.Load<AccConsolidationBatch>(query).Select(x => x.PK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator = new TestObjectCreator(Factory);
			TransactionCreator = new TestBulkTransactionCreator(Factory, TestObjectCreator);
			TransactionCreator.Setup();

			Org1 = TransactionCreator.Org1;
			Org2 = TransactionCreator.Org2;
			Org3 = TransactionCreator.Org3;

			eliminationCategory = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).EliminationCategory.Code;

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			group1 = Factory.New<AccConsolidationGroup>();
			group1.YR_Code = "AUSTRALIA";
			group1.YR_Description = "AUSTRALIA GROUP";
			group1.GroupMembers.AddNew().YM_GC_Company = GlbCompany.CurrentCompany.PK;
			group1.GroupMembers.AddNew().YM_OH_Organisation = Org1.PK;

			group2 = Factory.New<AccConsolidationGroup>();
			group2.YR_Code = "OCEANIA";
			group2.YR_Description = "OCEANIA GROUP";
			group2.GroupMembers.AddNew().YM_OH_Organisation = Org2.PK;

			group3 = Factory.New<AccConsolidationGroup>();
			group3.YR_Code = "GLOBAL";
			group3.YR_Description = "GLOBALGROUP";
			group3.GroupMembers.AddNew().YM_OH_Organisation = Org3.PK;

			group1.YR_YR_ConsolidationGroup = group2.PK;
			group2.YR_YR_ConsolidationGroup = group3.PK;

			Factory.Save();
		}

		ZDateTime June2008
		{
			get { return new ZDateTime(2008, 06, 15); }
		}

		protected virtual TimeSpan ExpectedHighWaterMarkBuffer
		{
			get { return new TimeSpan(48, 0, 0); }
		}

		AccBankAccount BankAccountAUD
		{
			get { return TestObjectCreator.AUDBankAccount; }
		}

		#endregion
	}
}
