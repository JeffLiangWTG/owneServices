using Enterprise.Accounting.Business.ARAP.Invoicing;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public class ARCreditNoteApprovalRequestPostingErrorEmail : ARCreditNoteApprovalRequestEmail
	{
		public ARCreditNoteApprovalRequestPostingErrorEmail(ARCreditNoteApprovalRequest approvalRequest, string errorMessage)
			: base(approvalRequest)
		{
			ErrorMessage = errorMessage;
		}

		string ErrorMessage { get; }

		protected override string GetSubject()
		{
			return Invariant($"Error posting approved AR Credit Note - {ApprovalRequest.JobNumber}");
		}

		protected override string GetBody()
		{
			return Invariant($@"<p>The following AR Credit Note approval request was approved, but could not be automatically posted:</p>
{GetLinkForMoreDetails()}
<p>Reason the transaction could not be posted automatically:</p>
<p>{ErrorMessage.TrimEnd()}</p>
<p>Please post this credit note manually.</p>");
		}
	}
}
