using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class ARCreditNoteApprovalRequestPostingErrorEmailTest : ARCreditNoteApprovalRequestEmailTest
	{
		readonly string errorMessage = "Test Error Message";

		protected override TransactionApprovalRequestEmail GetNewEmail(ARCreditNoteApprovalRequest approvalRequest) => new ARCreditNoteApprovalRequestPostingErrorEmail(approvalRequest, errorMessage);

		protected override string[] GetExpectedBody(ARCreditNoteApprovalRequest approvalRequest, string link = null, string expectedApprovers = "New test user")
		{
			var expectedBody = new string[]
				{
					$@"<p>The following AR Credit Note approval request was approved, but could not be automatically posted:</p>",
					$@"<p><a href=""edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARCreditNoteApproval&BusinessEntityPK={approvalRequest.PK}",
					$@"Credit Note Approval - {approvalRequest.JobNumber}</a></p>"
				};

			if (link != null)
			{
				expectedBody.Append(link);
			}

			expectedBody.Append($@"
<p>Reason the transaction could not be posted automatically:</p>
<p>Test Error Message</p>
<p>Please post this credit note manually.</p>"
				);

			return expectedBody;
		}

		protected override string GetExpectedEmailSubjectForTest(bool approved = false, string jobNumber = null, bool firstCheck = false)
		{
			return $"Error posting approved AR Credit Note - {jobNumber}";
		}
	}
}
