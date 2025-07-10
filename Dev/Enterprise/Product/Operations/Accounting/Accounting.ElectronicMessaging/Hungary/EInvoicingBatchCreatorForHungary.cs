using System;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Hungary
{
	public sealed class EInvoicingBatchCreatorForHungary : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForHungary(GlbCompany company) : base(company)
		{
		}

		protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, bool eInvoicingWasEnabledForParent) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case EInvoicingPivotState.Succeed:
				case EInvoicingPivotState.Delivered:
				case "":  // No original pivot
					return true;

				case EInvoicingPivotState.Failed:
					UpdatePivotStateAndBatchState(pivotId);
					return false;

				case EInvoicingPivotState.Discarded:
					if (eInvoicingWasEnabledForParent)
					{
						UpdatePivotStateAndBatchState(pivotId);
						return false;
					}
					else
					{
						return true;
					}

				case EInvoicingPivotState.Batched:
				case EInvoicingPivotState.BatchedWithError:
				case EInvoicingPivotState.Queued:
					UpdatePivotStateAndBatchState(pivotId);
					UpdatePivotStateAndBatchState(originalPivotId);
					return false;

				default:
					return false;
			}
		}
	}
}
