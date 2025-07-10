using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class CommissionRegeneratorTest : CommissionCreatorTestCase
	{
		#region Tests

		public void TestRegenerateCommissions_Transaction()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions();

			AssertEquals("Pre-condition", 1, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			helper.Invoice.RegenerateCommissions(Factory);

			Factory.Save();

			AssertCorrectNumberOfCommissionLines("1 original + 1 reversal + 1 regenerated lines.", 1, 1, 1);
		}

		public void TestRegenerateCommission_Job()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions(createLinesForJobOnly: true, closeJob: true);

			AssertEquals("Pre-condition", 2, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			helper.Job.RegenerateCommissions(Factory);

			Factory.Save();

			AssertCorrectNumberOfCommissionLines("2 original + 2 reversal + 2 regenerated lines.", 2, 2, 2);
		}

		public void TestCancelledInvoiceMessageLogged()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions();

			AssertEquals("Pre-condition", 1, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			helper.Invoice.AH_IsCancelled = true;

			var logger = new SimpleLogger();
			helper.Invoice.RegenerateCommissions(Factory, logger);

			AssertEquals("Regenerating cancelled transaction should log a message.", "00001001 - This transaction has been canceled, therefore commissions cannot be regenerated for it.\r\n", logger.ToString());
		}

		public void TestPreviouslyCancelledAndOverridenLinesNotRegenerated()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions();

			AssertEquals("Pre-condition", 1, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			helper.Invoice.RegenerateCommissions(Factory);

			// Factory.GetDatabaseCount() only checks the database so we need to save the data first.
			Factory.Save();

			AssertCorrectNumberOfCommissionLines("1 original + 1 reversal + 1 regenerated", 1, 1, 1);

			helper.Invoice.RegenerateCommissions(Factory);

			Factory.Save();

			AssertCorrectNumberOfCommissionLines("1 original + 1 original reversal + 1 original regenerated + 1 regenerated reversal + 1 regenerated regenerated", 2, 2, 1);
		}

		public void TestReversedInvoicesCorrectlyHandled_Job()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions(createLinesForJobOnly: true, closeJob: true);

			var lines = Factory.Load<ViewCommissionLine>(new ZQuery());
			AssertEquals("Pre-condition", 2, lines.Length);

			AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader), new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote)));

			helper.TestObjectCreator.ReverseTransaction(helper.JobInvoice, out _);

			Factory.Save();

			var reverseTransaction = helper.JobInvoice.ReverseInvoice;
			lines.ForEach(line => line.Reload());

			Assert("Credit Note should be created on reversal.", reverseTransaction is ARCreditNote);
			Assert("Existing commission line should be cancelled.", lines[0].IsCancelled);
			Assert("Existing commission line should be cancelled.", lines[1].IsCancelled);

			lines = Factory.Load<ViewCommissionLine>(new ZQuery());
			AssertEquals("2 original + 2 credit note line", 4, lines.Length);

			var creditNoteLines = lines.Where(x => x.VCL_AH == reverseTransaction.PK).ToArray();
			AssertEquals("2 credit not lines should be created.", 2, creditNoteLines.Length);
			Assert("Credit note lines should be cancelled too.", creditNoteLines[0].IsCancelled);
			Assert("Credit note lines should be cancelled too.", creditNoteLines[1].IsCancelled);

			helper.Job.RegenerateCommissions(Factory);

			Factory.Save();

			var cancelledButNotOverridenLinesQuery = new ZQuery(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, SQLComparisonOperator.NotEqual, null);
			cancelledButNotOverridenLinesQuery.AddToFilter(new ZQuery(AccCommissionLineSchema.CL0_OverridenDateTimeUtc, null), JoinCondition.And);

			AssertEquals("Lines should not be regenerated for cancelled invoice.", 4, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			AssertEquals("Original lines + credit note lines should be ONLY reversed (cancelled but not overriden)", 4, Factory.GetDatabaseCount(typeof(AccCommissionLine), cancelledButNotOverridenLinesQuery));
		}

		public void TestReversedInvoicesCorrectlyHandled_Transaction()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions();

			var lines = Factory.Load<ViewCommissionLine>(new ZQuery());
			AssertEquals("Pre-condition", 1, lines.Length);

			var invoiceLine = lines[0];
			AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(AccTransactionHeader), new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote)));

			helper.TestObjectCreator.ReverseTransaction(helper.Invoice, out _);

			Factory.Save();

			var reverseTransaction = helper.Invoice.ReverseInvoice;
			invoiceLine.Reload();

			Assert("Credit Note should be created on reversal.", reverseTransaction is ARCreditNote);
			Assert("Existing commission line should be cancelled.", invoiceLine.IsCancelled);

			lines = Factory.Load<ViewCommissionLine>(new ZQuery());
			AssertEquals("1 original + 1 credit note line", 2, lines.Length);

			var creditNoteLine = lines.First(x => x.VCL_AH == reverseTransaction.PK);
			AssertNotNull(creditNoteLine);
			Assert("Credit note should be cancelled too.", creditNoteLine.IsCancelled);

			helper.Invoice.RegenerateCommissions(Factory);

			Factory.Save();

			var cancelledButNotOverridenLinesQuery = new ZQuery(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, SQLComparisonOperator.NotEqual, null);
			cancelledButNotOverridenLinesQuery.AddToFilter(new ZQuery(AccCommissionLineSchema.CL0_OverridenDateTimeUtc, null), JoinCondition.And);

			AssertEquals("Lines should not be regenerated for cancelled invoice.", 2, Factory.GetDatabaseCount(typeof(AccCommissionLine)));
			AssertEquals("Original lines should be ONLY reversed (cancelled but not overriden)", 2, Factory.GetDatabaseCount(typeof(AccCommissionLine), cancelledButNotOverridenLinesQuery));
		}

		public void TestJobParentSetIfNull()
		{
			var helper = new CommissionTestObjectCreator(Factory);
			helper.SetupTransactionsAndCommissions(true, true);

			helper.Job.Parent = null;
			AssertNull(helper.Job.Parent);

			helper.Job.RegenerateCommissions(Factory);
			AssertNotNull(helper.Job.Parent);
		}

		public void TestQueueSourceItemsForRegeneration()
		{
			var commissionObjectCreator = new CommissionTestObjectCreator(Factory);
			commissionObjectCreator.SetupTransactionsAndCommissions(false, true);

			var groupings = CommissionTestObjectCreator.GetGroupings(Factory);

			AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));
			CommissionRegenerator.QueueSourceItemsForRegeneration(Factory, groupings);

			AssertEquals("2 queue items should have been created", 2, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));
		}

		public void TestQueueSourceItemsForRegeneration_DuplicateItemsOverwritten()
		{
			var commissionObjectCreator = new CommissionTestObjectCreator(Factory);
			commissionObjectCreator.SetupTransactionsAndCommissions(false, true);

			var groupings = CommissionTestObjectCreator.GetGroupings(Factory);
			var jobGrouping = groupings.First(grouping => grouping.SourceTableCode == JobHeaderSchema.Constants.Prefix);
			AssertNotNull(nameof(jobGrouping), jobGrouping);

			var existingJobQueueItem = Factory.New<OrgCommissionCalculationQueue>();
			existingJobQueueItem.CAQ_Operation = OrgCommissionCalculationQueueOperationCodeList.Codes.Regeneration;
			existingJobQueueItem.CAQ_JH = jobGrouping.SourceId;

			Factory.Save();

			AssertEquals("Pre-condition", 1, Factory.GetDatabaseCount(typeof(OrgCommissionCalculationQueue)));
			CommissionRegenerator.QueueSourceItemsForRegeneration(Factory, groupings);

			var queueItems = Factory.Load<OrgCommissionCalculationQueue>(new ZQuery());
			var newJobQueueItem = queueItems.First(queueItem => !queueItem.CAQ_JH.IsEmpty);
			AssertNotNull(nameof(newJobQueueItem), newJobQueueItem);
			AssertEquals("Only 2 queue items should exist", 2, queueItems.Length);
			Assert("Queue item for job should be new", existingJobQueueItem.PK != newJobQueueItem.PK);
			Assert("Old queue item should be deleted", existingJobQueueItem.IsDeleted);
		}

		#endregion

		#region Implementation

		void AssertCorrectNumberOfCommissionLines(string message, int originalCancelledAndOverridenLines, int numReversalLines, int numRegeneratedLines)
		{
			AssertEquals(message, originalCancelledAndOverridenLines + numReversalLines + numRegeneratedLines, Factory.GetDatabaseCount(typeof(AccCommissionLine)));

			var originalOverridenCancelledLinesQuery = new ZQuery(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, SQLComparisonOperator.NotEqual, null);
			originalOverridenCancelledLinesQuery.AddToFilter(AccCommissionLineSchema.CL0_OverridenDateTimeUtc, SQLComparisonOperator.NotEqual, null);
			originalOverridenCancelledLinesQuery.AddToFilter(AccCommissionLineSchema.CL0_BelongsToGroup, null);

			var reversalLinesQuery = new ZQuery(AccCommissionLineSchema.CL0_BelongsToGroup, SQLComparisonOperator.NotEqual, null);

			var regeneratedLinesQuery = new ZQuery(AccCommissionLineSchema.CL0_CancelledDateTimeUtc, null);
			regeneratedLinesQuery.AddToFilter(AccCommissionLineSchema.CL0_OverridenDateTimeUtc, null);

			CombineAssertions(() =>
			{
				AssertEquals($"{originalCancelledAndOverridenLines} original lines should be cancelled AND overriden.", originalCancelledAndOverridenLines, Factory.GetDatabaseCount(typeof(AccCommissionLine), originalOverridenCancelledLinesQuery));
				AssertEquals($"{numReversalLines} reversal lines should be created.", numReversalLines, Factory.GetDatabaseCount(typeof(AccCommissionLine), reversalLinesQuery));
				AssertEquals($"{numRegeneratedLines} lines should be regenerated", numRegeneratedLines, Factory.GetDatabaseCount(typeof(AccCommissionLine), regeneratedLinesQuery));
			});
		}
		#endregion
	}
}
