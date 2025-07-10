using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestsSubclassesOf(
		 typeof(EInvoicingBatchCreatorBase),
		 ExcludeClientDlls = true)]
	public abstract class EInvoicingBatchCreatorBaseTest : TestCaseWithFactory
	{
		[UseSnapshotProtection]
		[TestDate(2016, 5, 10)]
		public void TestTransactionRollBacksWhenExceptionHappens()
		{
			for (int i = 0; i < 6; i++)
			{
				var transaction = ObjectCreator.CreateARInvoice<ARInvoice>(FormattableString.Invariant($"100{i}"), ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
				ObjectCreator.CreateEInvoicingTransactionPivot(transaction, status: EInvoicingPivotState.Queued);
			}
			Factory.Save();

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("No invoice batches in DB.", 0, invoiceBatches.Length);
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("6 transactions ready for batching.", 6, transactionPivots.Length);

			var errorOccured = false;
			var logger = new DetailedLoggerForTest();
			var processor = new EInvoiceBatchProcessorWithException(GlbCompany.CurrentCompany);
			try
			{
				processor.PerformBatching(logger);
			}
			catch (Exception)
			{
				errorOccured = true;
			}

			Assert("Exception should be thrown", errorOccured);
			invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("Invoice batch is not created.", 0, invoiceBatches.Length);
			transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("6 transactions are not batched.", 6, transactionPivots.Length);
		}

		public virtual void TestBatching_ExcludesPendingEInvoicingPivots()
		{
			var transaction1 = ObjectCreator.CreateARInvoice<ARInvoice>("1000", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(transaction1, status: EInvoicingPivotState.Queued);
			var transaction2 = ObjectCreator.CreateARInvoice<ARInvoice>("1001", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot2 = ObjectCreator.CreateEInvoicingTransactionPivot(transaction2, status: EInvoicingPivotState.Pending);
			Factory.Save();

			var batchingResult = GetBatchProcessor(GlbCompany.CurrentCompany).PerformBatching(new DetailedLoggerForTest());
			AssertEquals("Batching should create a batch", true, batchingResult);

			pivot1.Reload();
			pivot2.Reload();
			AssertEquals("Previously queued pivot should now be batched", EInvoicingPivotState.Batched, pivot1.AIP_Status);
			AssertEquals("Previously pending pivot should still be pending ", EInvoicingPivotState.Pending, pivot2.AIP_Status);

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, EInvoicingBatchState.Ready);
			AssertEquals("Invoice Batch was created", 1, invoiceBatches.Length);
		}

		public virtual void TestDefaultPivotStatus()
		{
			var transaction = ObjectCreator.CreateARInvoice<ARInvoice>("1000", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(transaction, status: EInvoicingPivotState.Queued);
			Factory.Save();
			var batchingResult = GetBatchProcessor(GlbCompany.CurrentCompany).PerformBatching(new DetailedLoggerForTest());
			AssertEquals("Batching should create a batch", true, batchingResult);

			pivot.Reload();
			AssertEquals("Previously queued pivot should now be batched", EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		#region Helpers

		protected EInvoicingTestHelper Helper => helper ?? (helper = new EInvoicingTestHelper(ObjectCreator));
		EInvoicingTestHelper helper;

		protected TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;

		protected void AssertTransactionGroupingWithTheSameUniqueKeys(string country, int expectedBatchCount)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				var company = GlbCompany.CurrentCompany;
				var branch = GlbBranch.CurrentBranch;
				var transactionCreator = new TransactionCreator();

				AssertEquals("Precondition: no batched pivots", 0, EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched).Length);
				AssertEquals("Precondition: no batches", 0, EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready).Length);

				var pivot1ForAP = CreateTestTransactionPivot("AP100", 1);
				var pivot2ForAP = CreateTestTransactionPivot("AP102", 1);
				var pivot3ForAP = CreateTestTransactionPivot("AP102", 2);

				var batchingResult = GetBatchProcessor(GlbCompany.CurrentCompany).PerformBatching(new DetailedLoggerForTest());
				AssertEquals("Batching should create a batch", true, batchingResult);

				var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(
					company.PK, EInvoicingBatchState.Ready);

				var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched);

				AssertEquals("All pivots are batched", 2, pivots.Length);
				AssertEquals($"{expectedBatchCount} batch(s) are created", expectedBatchCount, batches.Length);

				AssertNotInSameGroup(pivots,
					pivot2ForAP,
					pivot3ForAP);
			}

			#region LocalMethods

			AccEInvoicingTransactionPivot CreateTestTransactionPivot(string transactionNum, int transactionCount)
			{
				var company = GlbCompany.CurrentCompany;
				var branch = GlbCompany.CurrentCompany.Branches[0];
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				invoice.AH_GC = company.PK;
				invoice.AH_GB = branch.PK;
				invoice.AH_TransactionCount = (ZByte)transactionCount;
				invoice.AH_OH = branch.GB_OH_OrgProxy;
				invoice.IsManuallySetTransactionNumber_ForTestOnly = true;
				invoice.AH_TransactionNum = transactionNum;
				Factory.Save();

				var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(
					Factory.Load<InvoicingBase>(invoice.PK));
				Factory.Save();

				return pivot;
			}

			void AssertNotInSameGroup(AccEInvoicingTransactionPivot[] allPivots, params AccEInvoicingTransactionPivot[] assertItems)
			{
				var expectedPks = assertItems.Select(a => a.PK).ToHashSet();
				var actualPks = allPivots.First().Batch.TransactionPivots
					.Cast<AccEInvoicingTransactionPivot>()
					.Select(p => p.PK).ToArray();

				Assert(!expectedPks.SetEquals(actualPks));
			}

			#endregion
		}

		protected void CreateFakePivots(int count, GlbCompany company)
		{
			var transactions = new List<AccTransactionHeader>();

			for (int i = 0; i < count; i++)
			{
				transactions.Add(CreateTestTransaction(i));
			}
			Factory.Save();

			foreach (var transaction in transactions)
			{
				var sql = $@"INSERT INTO {AccEInvoicingTransactionPivotSchema.Constants.SqlSchemaName}.{AccEInvoicingTransactionPivotSchema.Constants.TableName}
({AccEInvoicingTransactionPivotSchema.Constants.PK},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_GC},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_ParentID},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_ParentTableCode},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_SystemCreateTimeUtc},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_SystemCreateUser},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_SystemLastEditTimeUtc},
	{AccEInvoicingTransactionPivotSchema.Constants.AIP_SystemLastEditUser})
VALUES (
	NEWID(),
	'{company.PK}',
	'{transaction.PK}',
	'{AccTransactionHeaderSchema.Constants.Prefix}',
	'{EInvoicingPivotState.Queued}',
	'{company.GC_RN_NKCountryCode}',
	GetUtcDate(),
	'~BP',
	GetUtcDate(),
	'~BP'
)";
				TestConnection.ExecuteNonQuery(sql);
			}

			AccTransactionHeader CreateTestTransaction(int transactionSeq)
			{
				var transaction = Factory.NewWithValidTestData<AccTransactionHeader>();
				transaction.AH_TransactionNum = "INV000000" + transactionSeq;
				transaction.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				transaction.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Invoice;
				transaction.AH_OH = GlbBranch.CurrentBranch.OrgProxy.PK;
				return transaction;
			}
		}

		protected GlbCompany CreateCompanyAndAssertPreconditionsForPivotsAndBatches()
		{
			var company = ObjectCreator.CreateCompanyAndBranch("AUSYD");
			Factory.Save();

			AssertEquals("Precondition: no queued pivots", 0, EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Queued).Length);
			AssertEquals("Precondition: no batched pivots", 0, EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched).Length);
			AssertEquals("Precondition: no batches", 0, EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready).Length);

			return company;
		}

		protected void AssertPivotsAndBatches(GlbCompany company, int expectedPivots, int expectedBatches)
		{
			var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready);
			AssertEquals($"{expectedBatches} batch(s) are created", expectedBatches, batches.Length);

			var pivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched);
			AssertEquals("All pivots are batched", expectedPivots, pivots.Length);
			Assert("All Pivots have a batch", pivots.All(p => p.AIP_AIB.IsValid));
			var batchPks = batches.Select(b => b.PK).ToHashSet();
			Assert("All Pivots are part of the created batches", pivots.All(p => batchPks.Contains(p.AIP_AIB)));
		}

		#endregion

		class EInvoiceBatchProcessorWithException : EInvoicingBatchCreatorBase
		{
			public EInvoiceBatchProcessorWithException(GlbCompany company) : base(company)
			{
			}

			protected override IEnumerable<QueuedPivotPKs> GroupTransactionForBatching(DynamicBusinessObjectCollection transactions)
			{
				throw new NotImplementedException();
			}

			protected override void UpdateTransactionPivots(IEnumerable<Guid> transactionPivotPks, Guid batchPk, string defaultPivotStatus = EInvoicingPivotState.Batched)
			{
				throw new NotImplementedException("This is an exception created for testing");
			}
		}

		protected abstract EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company);
	}
}
