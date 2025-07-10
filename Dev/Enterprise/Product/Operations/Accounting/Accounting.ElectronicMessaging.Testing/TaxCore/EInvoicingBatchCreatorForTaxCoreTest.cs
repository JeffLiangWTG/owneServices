using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForTaxCore))]
	class EInvoicingBatchCreatorForTaxCoreTest : EInvoicingDependentBatchCreatorTest
	{
		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForTaxCore(company);

		protected override string Country => CountryCodes.Fiji;

		protected override bool ExpectPopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber => false;

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

		protected override string ExpectedDependentTransactionStatusWhenOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled => EInvoicingPivotState.Batched;
	}
}
