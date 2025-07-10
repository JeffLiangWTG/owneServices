using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForUruguay))]
	public class EInvoicingBatchCreatorForUruguayTest : EInvoicingDependentBatchCreatorTest
	{
		protected override string Country => CountryCodes.Uruguay;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForUruguay(company);

		#region Cancellations

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForCancellations => new[] { EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending };

		protected override string ExpectedDependentCancelTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;

		#endregion

		#region Amendings

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Batch amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForAmendings => new[] { EInvoicingPivotState.Batched, EInvoicingPivotState.Queued, EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending };

		protected override string ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction => EInvoicingPivotState.Discarded;

		protected override string ExpectedDependentAmendmentTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;

		protected override bool SupportsWaitForOriginalTransactionForAmending => true;

		#endregion
	}
}
