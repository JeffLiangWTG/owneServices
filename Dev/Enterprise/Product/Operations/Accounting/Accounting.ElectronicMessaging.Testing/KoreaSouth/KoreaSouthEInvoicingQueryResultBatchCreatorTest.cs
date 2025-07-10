using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthEInvoicingQueryResultBatchCreator))]
	public class KoreaSouthEInvoicingQueryResultBatchCreatorTest : KoreaSouthEInvoicingBatchCreatorTest
	{
		public void TestPerformBatching_QueryResultOnly()
		{
			var batchCreatorSTA = GetBatchProcessor(GlbCompany.CurrentCompany);
			var pivotSTA = CreatePivot(EInvoicingPivotActionType.StatusCheck);
			var pivotSUB = CreatePivot(EInvoicingPivotActionType.Submit);
			ResetBatchStatus();
			Factory.Save();

			batchCreatorSTA.PerformBatching(new DetailedLoggerForTest());
			AssertBatchedPivoits("KoreaSouthEInvoicingQueryResultBatchCreator shoul only get 'StatusCheck' pivots.", pivotSTA.PK);
		}

		public void TestPerformBatching_MultipleBranch()
		{
			AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, 2);

			var pivot1_NonCurrentBranch = CreatePivot(ActionType);
			pivot1_NonCurrentBranch.ParentTransactionHeader.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			var pivot2 = CreatePivot(ActionType);
			var pivot3 = CreatePivot(ActionType);

			Factory.Save();

			var batchCreator = GetBatchProcessor(GlbCompany.CurrentCompany);
			batchCreator.PerformBatching(new DetailedLoggerForTest());

			var batches = Factory.Load<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.AIB_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals(3, batches.Length);
			batches.ToList().ForEach(x => x.TransactionPivots.Reload(true));
			batches.Single(x => x.TransactionPivots.Count == 1 && x.TransactionPivots[0] == pivot1_NonCurrentBranch);
			batches.Single(x => x.TransactionPivots.Count == 1 && x.TransactionPivots[0] == pivot2);
			batches.Single(x => x.TransactionPivots.Count == 1 && x.TransactionPivots[0] == pivot3);
		}

		public override void TestBatching_ExcludesPendingEInvoicingPivots()
		{
			var transaction1 = ObjectCreator.CreateARInvoice<ARInvoice>("1000", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot1 = ObjectCreator.CreateEInvoicingTransactionPivot(transaction1, ActionType, status: EInvoicingPivotState.Queued);
			var transaction2 = ObjectCreator.CreateARInvoice<ARInvoice>("1001", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot2 = ObjectCreator.CreateEInvoicingTransactionPivot(transaction2, ActionType, status: EInvoicingPivotState.Pending);
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

		public override void TestDefaultPivotStatus()
		{
			var transaction = ObjectCreator.CreateARInvoice<ARInvoice>("1000", ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
			var pivot = ObjectCreator.CreateEInvoicingTransactionPivot(transaction, ActionType, status: EInvoicingPivotState.Queued);
			Factory.Save();
			var batchingResult = GetBatchProcessor(GlbCompany.CurrentCompany).PerformBatching(new DetailedLoggerForTest());
			AssertEquals("Batching should create a batch", true, batchingResult);

			pivot.Reload();
			AssertEquals("Previously queued pivot should now be batched", EInvoicingPivotState.Batched, pivot.AIP_Status);
		}

		protected override string ActionType => EInvoicingPivotActionType.StatusCheck;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new KoreaSouthEInvoicingQueryResultBatchCreator(company);
		}
	}
}
