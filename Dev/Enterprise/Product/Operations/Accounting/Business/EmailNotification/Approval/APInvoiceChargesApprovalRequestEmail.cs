using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class APInvoiceChargesApprovalRequestEmail : InvoicingBaseApprovalRequestEmail<APInvoiceChargesApprovalRequestDetails>
	{
		public APInvoiceChargesApprovalRequestEmail(APInvoiceChargesApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
		}

		protected override string GetApprovalBizoName()
		{
			return string.Format((NoResString)"AP Invoice (Creditor: '{0}' and number: '{1}')", ApprovalRequest.PostingDetails.Creditor, ApprovalRequest.PostingDetails.TransactionNumber);
		}
	}
}
