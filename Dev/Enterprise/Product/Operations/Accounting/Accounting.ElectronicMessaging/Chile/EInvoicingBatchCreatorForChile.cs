using System;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Chile
{
	public class EInvoicingBatchCreatorForChile : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForChile(GlbCompany company) : base(company)
		{
		}

		protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(_, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case EInvoicingPivotState.Succeed:
				case EInvoicingPivotState.Failed:
					return true;
				case "":
				case EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				default:
					return false;
			}
		}

		protected override bool AmendingPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(_, string originalPivotState, _) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case EInvoicingPivotState.Succeed:
				case EInvoicingPivotState.Failed:
					return true;
				case "":
					var (originalTransactionNumber, originalInvoiceDate) = GetManualOriginalTransactionValues(parentId);
					if (!string.IsNullOrEmpty(originalTransactionNumber) || originalInvoiceDate != default)
					{
						return true;
					}
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				case EInvoicingPivotState.Discarded:
					UpdatePivotStateAndBatchState(pivotId);
					return false;
				default:
					return false;
			}
		}
	}
}
