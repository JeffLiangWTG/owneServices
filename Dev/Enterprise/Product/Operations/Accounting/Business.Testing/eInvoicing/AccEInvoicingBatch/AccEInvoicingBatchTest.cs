using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(AccEInvoicingBatch))]
	class AccEInvoicingBatchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHasDataContextManager()
		{
			object manager = null;
			AssertNoExceptionThrown(() => { manager = batch.GetUniversalDataContextManager(); });
			AssertNotNull("batch should have [UniversalDataContext(AccEInvoicingBatch.)] attribute", manager);
		}

		public void TestGetInvoicesInBatchByState()
		{
			var transactions = batch.GetInvoicesWithStatus(Core.Constants.EInvoicingPivotState.Batched);
			AssertEquals(2, transactions.Length);
			AssertContainsExactElementsInAnyOrder(new InvoicingBase[] { arInvoice, arCreditNote }, transactions);
			transactions = batch.GetInvoicesWithStatus(Core.Constants.EInvoicingPivotState.Succeed);
			AssertEquals(0, transactions.Length);

			var arInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice2, Core.Constants.EInvoicingPivotState.BatchedWithError);
			var arInvoice3 = Factory.NewWithValidTestData<ARInvoice>();
			var pivot3 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice3, Core.Constants.EInvoicingPivotState.Succeed);
			Factory.Save();

			transactions = batch.GetInvoicesWithStatus(Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotState.BatchedWithError);
			AssertEquals(3, transactions.Length);
			AssertContainsExactElementsInAnyOrder(new InvoicingBase[] { arInvoice, arCreditNote, arInvoice2 }, transactions);

			transactions = batch.GetInvoicesWithStatus();
			AssertEquals(4, transactions.Length);
			AssertContainsExactElementsInAnyOrder(new InvoicingBase[] { arInvoice, arCreditNote, arInvoice2, arInvoice3 }, transactions);
		}

		public void TestGetComplianceDocumentsInBatchByState()
		{
			var batch = TestObjectCreator.CreateEInvoicingBatch(101, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var complianceDocuments = batch.GetComplianceDocumentsWithStatus(Core.Constants.EInvoicingPivotState.Batched);
			AssertEquals(0, complianceDocuments.Length);

			var complianceDocument1 = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocument1.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument1, Core.Constants.EInvoicingPivotState.Batched);
			var complianceDocument2 = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocument2.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument2, Core.Constants.EInvoicingPivotState.BatchedWithError);
			var complianceDocument3 = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			complianceDocument3.ADH_GC_Company = GlbCompany.CurrentCompany.PK;
			TestObjectCreator.CreateEInvoicingTransactionPivot(batch, complianceDocument3, Core.Constants.EInvoicingPivotState.Succeed);
			Factory.Save();

			complianceDocuments = batch.GetComplianceDocumentsWithStatus(Core.Constants.EInvoicingPivotState.Batched, Core.Constants.EInvoicingPivotState.BatchedWithError);
			AssertEquals(2, complianceDocuments.Length);
			AssertContainsExactElementsInAnyOrder(new ARComplianceDocumentHeader[] { complianceDocument1, complianceDocument2 }, complianceDocuments);

			complianceDocuments = batch.GetComplianceDocumentsWithStatus();
			AssertEquals(3, complianceDocuments.Length);
			AssertContainsExactElementsInAnyOrder(new ARComplianceDocumentHeader[] { complianceDocument1, complianceDocument2, complianceDocument3 }, complianceDocuments);
		}

		public void TestTransactionPivots()
		{
			AssertNotNull(batch.TransactionPivots);
			AssertEquals(2, batch.TransactionPivots.Count);
			AssertContainsExactElementsInAnyOrder(new[] { pivot1, pivot2 }, batch.TransactionPivots);
		}

		public void TestUpdatePivotState()
		{
			AssertEquals(2, batch.TransactionPivots.Count);
			batch.TransactionPivots[0].AIP_Status = Enterprise.Core.Constants.EInvoicingPivotState.Batched;
			batch.TransactionPivots[1].AIP_Status = Enterprise.Core.Constants.EInvoicingPivotState.BatchedWithError;
			var invoices = new InvoicingBase[] { arInvoice };
			AssertEquals(Core.Constants.EInvoicingPivotState.Batched, batch.TransactionPivots[0].AIP_Status);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[1].AIP_Status);

			var actionDateTime = CargoWise.Types.ZDateTime.UtcNow;
			batch.UpdatePivotsStatus(Core.Constants.EInvoicingPivotState.Failed, invoices.Select(x => x.PK), "test error message", actionDateTime);
			AssertEquals(actionDateTime, batch.TransactionPivots[0].AIP_LastResponseReceivedUtc);
			AssertEquals(ZDateTime.Empty, batch.TransactionPivots[1].AIP_LastResponseReceivedUtc);
			batch.UpdatePivotsLastSentTimeUtc(actionDateTime, invoices.Select(x => x.PK));
			AssertEquals(Core.Constants.EInvoicingPivotState.Failed, batch.TransactionPivots[0].AIP_Status);
			AssertEquals(Core.Constants.EInvoicingPivotState.BatchedWithError, batch.TransactionPivots[1].AIP_Status);
			AssertEquals(actionDateTime, batch.TransactionPivots[0].AIP_LastSentTimeUtc);
			AssertEquals(ZDateTime.Empty, batch.TransactionPivots[1].AIP_LastSentTimeUtc);
			AssertEquals("test error message", batch.TransactionPivots[0].AIP_ErrorDescription);
			AssertEquals("", batch.TransactionPivots[1].AIP_ErrorDescription);
		}

		public void TestRemovePivotAndDiscardBatchIfEmpty_WhenNonDiscardedPivots()
		{
			AssertEquals("Precondition", 2, batch.TransactionPivots.Count);
			AssertEquals("Precondition", Core.Constants.EInvoicingBatchState.Ready, batch.AIB_Status);

			batch.RemovePivotAndDiscardBatchIfEmpty(pivot1);
			AssertEquals("Pivot is removed from collection", 1, batch.TransactionPivots.Count);
			AssertEquals("Pivot is disconnected from batch", ZGuid.Empty, pivot1.AIP_AIB);
			AssertEquals("Batch remains ready", Core.Constants.EInvoicingBatchState.Ready, batch.AIB_Status);

			batch.RemovePivotAndDiscardBatchIfEmpty(pivot2);
			AssertEquals("Pivot is removed from collection", 0, batch.TransactionPivots.Count);
			AssertEquals("Pivot is disconnected from batch", ZGuid.Empty, pivot2.AIP_AIB);
			AssertEquals("Batch is discarded when empty", Core.Constants.EInvoicingBatchState.Discarded, batch.AIB_Status);
		}

		public void TestRemovePivotAndDiscardBatchIfEmpty_WhenDiscardedPivots()
		{
			AssertEquals("Precondition", 2, batch.TransactionPivots.Count);
			AssertEquals("Precondition", Core.Constants.EInvoicingBatchState.Ready, batch.AIB_Status);
			pivot2.AIP_Status = Core.Constants.EInvoicingPivotState.Discarded;

			batch.RemovePivotAndDiscardBatchIfEmpty(pivot1);
			AssertEquals("Pivot is removed from collection", 1, batch.TransactionPivots.Count);
			AssertEquals("Pivot is disconnected from batch", ZGuid.Empty, pivot1.AIP_AIB);
			AssertEquals("Batch is discarded when all pivots are discarded", Core.Constants.EInvoicingBatchState.Discarded, batch.AIB_Status);

			batch.RemovePivotAndDiscardBatchIfEmpty(pivot2);
			AssertEquals("Pivot is removed from collection", 0, batch.TransactionPivots.Count);
			AssertEquals("Pivot is disconnected from batch", ZGuid.Empty, pivot2.AIP_AIB);
			AssertEquals("Batch is discarded when empty", Core.Constants.EInvoicingBatchState.Discarded, batch.AIB_Status);
		}

		public void TestAIB_QueryTimes()
		{
			var newBatch = TestObjectCreator.CreateEInvoicingBatch(200, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			AssertNull(GetQueryTimesGenAddOnColumn(newBatch));
			AssertEquals(0, newBatch.AIB_QueryTimes);
			Factory.Save();

			AssertNull(GetQueryTimesGenAddOnColumn(newBatch));
			AssertEquals(0, newBatch.AIB_QueryTimes);

			newBatch.AIB_QueryTimes = 2;
			AssertEquals(2, newBatch.AIB_QueryTimes);
			AssertNotNull(GetQueryTimesGenAddOnColumn(newBatch));

			newBatch.AIB_QueryTimes = 5;
			Factory.Save();
			AssertEquals(5, newBatch.AIB_QueryTimes);
			AssertNotNull(GetQueryTimesGenAddOnColumn(newBatch));

			newBatch.AIB_QueryTimes = 0;
			Factory.Save();
			AssertEquals(0, newBatch.AIB_QueryTimes);
			AssertNull(GetQueryTimesGenAddOnColumn(newBatch));
		}

		public void TestClearQueryTimes()
		{
			var newBatch = TestObjectCreator.CreateEInvoicingBatch(200, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);

			AssertNull(GetQueryTimesGenAddOnColumn(newBatch));

			newBatch.AIB_QueryTimes = 2;
			newBatch.ClearQueryTimes();
			Factory.Save();
			AssertNull("QueryTimes was added then removed in a new batch", GetQueryTimesGenAddOnColumn(newBatch));

			newBatch.AIB_QueryTimes = 2;
			Factory.Save();
			AssertNotNull("QueryTimes added", GetQueryTimesGenAddOnColumn(newBatch));

			newBatch.ClearQueryTimes();
			Factory.Save();
			AssertEquals(0, newBatch.AIB_QueryTimes);
			AssertNull("QueryTimes was removed from a saved batch", GetQueryTimesGenAddOnColumn(newBatch));
		}

		public void TestRemoveQueryTimesWhenDiscardBatch()
		{
			batch.AIB_QueryTimes = 2;
			batch.AIB_Status = EInvoicingBatchState.Ready;
			Factory.Save();

			AssertEquals(2, batch.AIB_QueryTimes);
			AssertNotNull(GetQueryTimesGenAddOnColumn(batch));

			batch.AIB_Status = EInvoicingBatchState.Discarded;
			Factory.Save();

			AssertEquals(0, batch.AIB_QueryTimes);
			AssertNull("QueryTimes was removed", GetQueryTimesGenAddOnColumn(batch));
		}

		#region Implementation

		AccEInvoicingBatch batch;
		ARInvoice arInvoice;
		ARCreditNote arCreditNote;
		AccEInvoicingTransactionPivot pivot1, pivot2;

		GenAddOnColumn GetQueryTimesGenAddOnColumn(AccEInvoicingBatch batch)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, batch.PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, batch.TablePrefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, AccEInvoicingBatch.Schema.AIB_QueryTimes);

			var result = Factory.Load<GenAddOnColumn>(query);
			if (result.Length == 0)
			{
				return null;
			}

			AssertEquals(1, result.Length);
			return result[0];
		}

		protected override void SetUp()
		{
			base.SetUp();
			batch = TestObjectCreator.CreateEInvoicingBatch(100, Core.Constants.EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, Core.Constants.EInvoicingPivotState.Batched);
			arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arCreditNote, Core.Constants.EInvoicingPivotState.Batched);
			Factory.Save();
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
