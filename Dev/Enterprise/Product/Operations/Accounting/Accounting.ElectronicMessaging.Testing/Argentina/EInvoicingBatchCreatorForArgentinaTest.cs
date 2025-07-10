using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForArgentina))]
	public class EInvoicingBatchCreatorForArgentinaTest : EInvoicingDependentBatchCreatorTest
	{
		protected override string Country => CountryCodes.Argentina;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForArgentina(company);

		#region Cancellations

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				//Discard both
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForCancellations => new[] { EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending };

		#endregion

		#region Amendings

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Batch amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				//Discard amending
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				//Discard both
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForAmendings => new[] { EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending, EInvoicingPivotState.Batched, EInvoicingPivotState.Queued };

		protected override bool SupportsWaitForOriginalTransactionForAmending => true;

		#endregion
	}
}
