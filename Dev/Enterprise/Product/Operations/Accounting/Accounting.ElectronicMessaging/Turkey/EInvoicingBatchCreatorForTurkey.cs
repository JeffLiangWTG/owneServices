using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Turkey
{
	public class EInvoicingBatchCreatorForTurkey : EInvoicingDependentBatchCreator
	{
		public EInvoicingBatchCreatorForTurkey(GlbCompany company)
			: base(company)
		{
		}

		protected override ZDateTime LatestCallTimeToRetryStatusCheck =>
			ZDateTime.UtcNow.AddMinutes(-AccountingMasterFilesRegistry.Instance.TaxInvoiceStatusUpdateAutomatedRequestSchedule.GetValueWithoutFallback(CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

		protected override ZBool EnableRetryReadyStatusPivots => true;

		protected override string[] GetActionTypesWithTransactionID() => new string[] { Constants.EInvoicingPivotActionType.ConfirmTransactionReceived, Constants.EInvoicingPivotActionType.Approve, Constants.EInvoicingPivotActionType.Reject };
	}
}
