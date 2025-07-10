using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public abstract class InvoicingBaseApprovalModule<RequestType, DetailsType> : TransactionApprovalModule<InvoicingBase, RequestType, DetailsType>
		where RequestType : InvoicingBaseApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public InvoicingBaseApprovalModule()
		{
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InvoicingBaseApprovalFilterBusinessObject();
		}
	}
}
