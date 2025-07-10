using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public class EInvoicingBatchCreatorForTaxCore : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForTaxCore(GlbCompany company)
			: base(company)
		{
		}

		protected override bool CancelPivotCanBeBatched(Guid pivotId, Guid parentId, Guid originalParentId)
		{
			(Guid originalPivotId, string originalPivotState, bool eInvoicingWasEnabledForParent) = GetOriginalTransactionPivotPKAndState(originalParentId);

			switch (originalPivotState)
			{
				case Constants.EInvoicingPivotState.Succeed:
				case "":
					return true;

				case Constants.EInvoicingPivotState.Discarded:
					if (eInvoicingWasEnabledForParent)
					{
						UpdatePivotStateAndBatchState(pivotId);
						return false;
					}
					else
					{
						return true;
					}

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

		protected override ZBool PopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber => false;
	}
}
