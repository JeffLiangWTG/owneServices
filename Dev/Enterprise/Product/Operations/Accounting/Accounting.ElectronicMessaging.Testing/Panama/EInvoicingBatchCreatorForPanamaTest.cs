using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Panama.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForPanama))]
	public class EInvoicingBatchCreatorForPanamaTest : EInvoicingDependentBatchCreatorTest
	{
		protected override string Country => CountryCodes.Panama;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForPanama(company);

		#region Amending

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Batch amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard amending
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				//Discard both
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForAmendings => new[] { EInvoicingPivotState.Batched, EInvoicingPivotState.Queued, EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending };

		protected override bool SupportsWaitForOriginalTransactionForAmending => true;

		#endregion

		#region Cancellations

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				//Discard both
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded)
			};

		#endregion
	}
}
