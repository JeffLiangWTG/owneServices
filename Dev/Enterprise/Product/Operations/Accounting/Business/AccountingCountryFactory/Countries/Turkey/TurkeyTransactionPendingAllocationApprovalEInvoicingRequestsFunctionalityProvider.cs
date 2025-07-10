using System.Linq;
using CargoWise.Application;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TurkeyTransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider : ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider
	{
		bool ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider.IsTransactionEligibleToCreateApprovalRequest(AccTransactionHeader transaction) =>
			IsTransactionEligibleToCreateElectronicRequest(transaction);

		bool ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider.IsTransactionEligibleToCreateRejectionRequest(AccTransactionHeader transaction) =>
			IsTransactionEligibleToCreateElectronicRequest(transaction);

		bool IsTransactionEligibleToCreateElectronicRequest(AccTransactionHeader transaction) =>
			transaction.IsInDatabase
			&& transaction.AH_ComplianceSubType == TurkeyComplianceInfo.ComplianceSubTypeCodes.PIC
			&& transaction.HasEInvoicingPivot(EInvoicingPivotActionType.ConfirmTransactionReceived)
			&& !transaction.HasEInvoicingPivot(EInvoicingPivotActionType.Approve)
			&& !transaction.HasEInvoicingPivot(EInvoicingPivotActionType.Reject);

		bool ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider.IsTransactionEditable(AccTransactionHeader transaction)
		{
			var approvalStatus = (transaction as InvoicingBase)?.TransactionRelatedApprovalRequest?.XP_ApprovalStatus;
			return !(approvalStatus.HasValue && NotEditableApprovalStatuses.Contains(approvalStatus.Value.ToString()));
		}

		readonly string[] NotEditableApprovalStatuses = new[]
		{
			GenApprovalRequestApprovalStatus.Approved,
			GenApprovalRequestApprovalStatus.ApprovalRequested,
			GenApprovalRequestApprovalStatus.RejectionRequested,
		};
	}

	#region Extension

	static class Extension
	{
		public static bool HasEInvoicingPivot(this AccTransactionHeader transaction, string actionType) =>
			ObjectFactory.Get<IEInvoicingHelper>()?.HasActiveEInvoicingTransactionPivot(transaction, actionType) ?? false;
	}

	#endregion
}
