using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class APInvoiceChargesApprovalRequestEmailTest : InvoicingBaseApprovalRequestEmailTest<APInvoiceChargesApprovalRequest, APInvoiceChargesApprovalRequestDetails>
	{
		protected override TransactionApprovalRequestEmail GetNewEmail(APInvoiceChargesApprovalRequest approvalRequest)
		{
			approvalRequest.PostingDetails.Creditor = "ORG";
			approvalRequest.PostingDetails.TransactionNumber = "INV1";

			var charge = approvalRequest.PostingDetails.Charges.AddNew();
			charge.LocalCostAmount = 11;

			charge = approvalRequest.PostingDetails.Charges.AddNew();
			charge.LocalCostAmount = 22;
			return new APInvoiceChargesApprovalRequestEmail(approvalRequest);
		}

		protected override string GetApprovalBizoName()
		{
			return "AP Invoice (Creditor: 'ORG' and number: 'INV1')";
		}

		protected override string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false)
		{
			if (firstCheck)
			{
				return "AP Invoice (Creditor: 'ORG' and number: 'INV1') approval request for Job Number '' was Requested";
			}
			else
			{
				return "AP Invoice (Creditor: 'ORG' and number: 'INV1') approval request for Job Number '" + jobNumber + "' was " + (approved ? "Approved" : "Rejected");
			}
		}
	}
}
