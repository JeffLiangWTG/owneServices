using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Messaging.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	public class KoreaSouthGEIFailedMessageHandlerTest : TestCaseWithFactory
	{
		public void TestUpdatePivot_WhenSubmitBatchAndStatusCheckBatchAreNull()
		{
			var batch = Factory.NewWithValidTestData<AccEInvoicingBatch>();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = EInvoicingPivotActionType.Cancel;
			pivot.AIP_AIB = batch.PK;

			var finder = new KoreaSouthEInvoicingDataFinder();
			AssertNull("Precondition", finder.FindSubmitBatch(batch));
			AssertNull("Precondition", finder.FindStatusCheckBatch(batch));

			var handler = new KoreaSouthGEIFailedMessageHandler();
			AssertNoExceptionThrown(() => handler.UpdatePivot(pivot));
		}

		public void TestUpdateFailedMessage_WhenEInvoicingPivotActionTypeIsSubmit()
		{
			var expectedMessage = "Unexpected error when sending the 'GEN - Generate Invoice Submission' type Message. Please check the Notes 'eHub Server Error' on this EDI Message for more details";

			var batchSUB = KREInvoicingTestHelper.SetUpSubmitBatch();
			AssertEquals("Precondition", EInvoicingBatchState.Sent, batchSUB.AIB_Status);
			AssertEquals("Precondition", 2, batchSUB.TransactionPivots.Count);
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_Status == EInvoicingPivotState.Delivered));
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ErrorDescription.IsEmpty));

			var collection = new EDIMessageCollection(batchSUB);
			collection.Load();
			AssertEquals("Precondition", 1, collection.Count);
			var handler = new KoreaSouthGEIFailedMessageHandler();
			handler.UpdateFailedMessage(collection[0].Interchange);

			AssertEquals("Submit batch Should still contain 2 pivots", 2, batchSUB.TransactionPivots.Count);
			AssertEquals("AIB_Status of submit batch should not be changed", EInvoicingBatchState.Sent, batchSUB.AIB_Status);
			AssertEquals("AIP_Status of all submit pivots should be updated to failed", true,
				batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_Status == EInvoicingPivotState.Failed));
			AssertEquals($"AIP_ErrorDescription of all submit pivots should be updated to {expectedMessage}", true,
				batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ErrorDescription == expectedMessage));
		}

		public void TestUpdateFailedMessage_WhenEInvoicingPivotActionTypeIsStatusCheck()
		{
			var expectedMessage = "Unexpected error when sending the 'GEQ - Query Invoice Request' type Message.Please check the Notes 'eHub Server Error' on this EDI Message for more details.";

			var (batchSUB, batchSTA) = KREInvoicingTestHelper.SetUpQueryBatch();

			AssertEquals("Precondition", EInvoicingBatchState.Sent, batchSUB.AIB_Status);
			AssertEquals("Precondition", 2, batchSUB.TransactionPivots.Count);
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_Status == EInvoicingPivotState.Delivered));
			AssertEquals("Precondition", true, batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ErrorDescription.IsEmpty));

			AssertEquals("Precondition", EInvoicingBatchState.Sent, batchSTA.AIB_Status);
			AssertEquals("Precondition", 1, batchSTA.TransactionPivots.Count);
			AssertEquals("Precondition", true, batchSTA.TransactionPivots[0].AIP_Status == EInvoicingPivotState.Delivered);
			AssertEquals("Precondition", true, batchSTA.TransactionPivots[0].AIP_ErrorDescription.IsEmpty);

			var collection = new EDIMessageCollection(batchSTA);
			collection.Load();
			AssertEquals("Precondition", 1, collection.Count);
			var handler = new KoreaSouthGEIFailedMessageHandler();
			handler.UpdateFailedMessage(collection[0].Interchange);

			AssertEquals("Submit batch Should still contain 2 pivots", 2, batchSUB.TransactionPivots.Count);
			AssertEquals("AIB_Status of submit batch should not be changed", EInvoicingBatchState.Sent, batchSUB.AIB_Status);
			AssertEquals("AIP_Status of all submit pivots should be updated to failed", true,
				batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_Status == EInvoicingPivotState.Failed));
			AssertEquals($"AIP_ErrorDescription of all submit pivots should be updated to {expectedMessage}", true,
				batchSUB.TransactionPivots.OfType<AccEInvoicingTransactionPivot>().All(x => x.AIP_ErrorDescription == expectedMessage));

			AssertEquals("All STA pivots should be deleted", 0, batchSTA.TransactionPivots.Count);
		}

		KoreaSouthEInvoicingTestHelper KREInvoicingTestHelper => krEInvoicingTestHelper ?? (krEInvoicingTestHelper = new KoreaSouthEInvoicingTestHelper(TestObjectCreator));
		KoreaSouthEInvoicingTestHelper krEInvoicingTestHelper;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
