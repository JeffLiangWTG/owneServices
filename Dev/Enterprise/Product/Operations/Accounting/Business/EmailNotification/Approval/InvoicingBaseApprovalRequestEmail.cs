using CargoWise.Application;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.EmailNotification
{
	public abstract class InvoicingBaseApprovalRequestEmail<DetailsType> : TransactionApprovalRequestEmail
		where DetailsType : ApprovalRequestDetails
	{
		public InvoicingBaseApprovalRequestEmail(TransactionApprovalRequest<DetailsType> approvalRequest)
			: base(approvalRequest)
		{
		}

		protected new InvoicingBaseApprovalRequest<DetailsType> ApprovalRequest => base.ApprovalRequest as InvoicingBaseApprovalRequest<DetailsType>;

		protected override string GetLinkForMoreDetails()
		{
			var link = string.Empty;
			if (ApprovalRequest.XP_ParentTableCode == JobConsolSchema.Constants.Prefix)
			{
				link = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.JobConsol, ApprovalRequest.XP_ParentID.ToGuid());
			}
			else
			{
				var job = Factory.Load<Job>(ApprovalRequest.XP_ParentID);
				var consumerControllerID = job?.GenericJobView?.GetConsumerController();
				if (consumerControllerID != null)
				{
					link = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(consumerControllerID, job.JH_ParentID.ToGuid());
				}
			}
			return string.IsNullOrWhiteSpace(link) ? string.Empty : Invariant($@"<p>Refer to <a href=""{link}"">{ApprovalRequest.JobNumber}</a></p>");
		}

		protected override string GetRequestID()
		{
			return Invariant($"for Job Number '{ApprovalRequest.JobNumber}'");
		}
	}
}
