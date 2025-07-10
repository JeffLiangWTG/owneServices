using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Panama
{
	public class EInvoicingBatchCreatorForPanama : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForPanama(GlbCompany company) : base(company) { }

		protected override bool AmendingPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);
			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case "":
					return true;
				case Constants.EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				case Constants.EInvoicingPivotState.BatchedWithError:
				case Constants.EInvoicingPivotState.Failed:
					UpdatePivotStateAndBatchState(pivotId);
					UpdatePivotStateAndBatchState(originalPivotId);
					return false;
				default:
					return false;
			}
		}
	protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case "":
					return true;
				case Constants.EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				case Constants.EInvoicingPivotState.Failed:
				case Constants.EInvoicingPivotState.BatchedWithError:
				case Constants.EInvoicingPivotState.Batched:
				case Constants.EInvoicingPivotState.Queued:
					UpdatePivotStateAndBatchState(pivotId);
					UpdatePivotStateAndBatchState(originalPivotId);
					return false;
				default:
					return false;
			}
		}
	}
}
