using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public class EInvoicingBatchCreatorForUruguay : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForUruguay(GlbCompany company)
			: base(company)
		{
		}

		protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case Constants.EInvoicingPivotState.Failed:
					return true;
				case Constants.EInvoicingPivotState.Discarded:
				case "":
					UpdatePivotStateAndBatchState(pivotId);
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
				case Constants.EInvoicingPivotState.Failed:
					return true;
				case Constants.EInvoicingPivotState.Discarded:
				case "":
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				default:
					return false;
			}
		}
	}
}
