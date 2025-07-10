using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.DataTransfer.CreditControlledDocumentApproval
{
	class CreditApprovalEventInfo : IEventInfo
	{
		public CreditApprovalEventInfo(CreditControlledDocumentsApproval approvalRequest)
		{
			ApprovalRequest = approvalRequest;
		}

		CreditControlledDocumentsApproval ApprovalRequest { get; set; }
		public ZString EventReference
		{
			get
			{
				ZString reference = ZString.Empty;

				if (ApprovalRequest.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested
					|| ApprovalRequest.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					reference = CreditControlledDocumentsApproval.AddTypeToEventReference(ApprovalRequest.XP_ApprovalStatus);
				}

				if (!reference.IsEmpty && !ApprovalRequest.XP_ReasonDescription.IsEmpty)
				{
					reference += CreditControlledDocumentsApproval.AppendReasonToEventReference(ApprovalRequest.XP_ReasonDescription);
				}

				return reference;
			}
		}

		public IUser EventUser => ApprovalRequest.CreatedUser;

		public IBranch EventBranch => ApprovalRequest.RequestingBranch;

		public IDepartment EventDepartment => ApprovalRequest.JobDepartment;
	}
}
