using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalBulk : TransactionApprovalBulk<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationApprovalBulk(BusinessObjectFactory factory, ISecurityOverrideProvider interactiveSecurityOverrideProvider, params TransactionPendingAllocationApprovalRequest[] approvalRequests)
			: base(factory, interactiveSecurityOverrideProvider, approvalRequests)
		{
			EInvoicingRequestCountryCompliance = TransactionPendingAllocationApprovalHelper.GetITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider(Factory, approvalRequests.FirstOrDefault()?.LinkedTransaction?.Company.GC_RN_NKCountryCode ?? string.Empty);
		}

		readonly ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider EInvoicingRequestCountryCompliance;

		protected override bool CheckLevelSecurityRights(TransactionPendingAllocation transaction)
		{
			return TransactionPendingAllocationLevelAuthorizationWithApprovalRequest.CheckLevelSecurityRights(transaction);
		}

		protected override string GetApplicableApprovalStatus(TransactionPendingAllocationApprovalRequest approvalRequest, string status)
			=> (EInvoicingRequestCountryCompliance?.IsTransactionEligibleToCreateApprovalRequest(approvalRequest.LinkedTransaction) ?? false) && status == Constants.GenApprovalRequestApprovalStatus.Approved
			? Constants.GenApprovalRequestApprovalStatus.ApprovalRequested
			: status;
	}
}
