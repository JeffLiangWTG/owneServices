using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina
{
	public class EInvoicingBatchCreatorForArgentina : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForArgentina(GlbCompany company)
			: base(company)
		{
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
				case Constants.EInvoicingPivotState.Batched:
				case Constants.EInvoicingPivotState.Queued:
				case Constants.EInvoicingPivotState.BatchedWithError:
					UpdatePivotStateAndBatchState(pivotId);
					UpdatePivotStateAndBatchState(originalPivotId);
					return false;
				default:
					return false;
			}
		}

		protected override bool AmendingPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case "":
					return true;
				case Constants.EInvoicingPivotState.Failed:
				case Constants.EInvoicingPivotState.BatchedWithError:
					UpdatePivotStateAndBatchState(pivotId);
					UpdatePivotStateAndBatchState(originalPivotId);
					return false;
				case Constants.EInvoicingPivotState.Queued:
				case Constants.EInvoicingPivotState.Batched:
					return false;
				case Constants.EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				default:
					return false;
			}
		}
	}
}
