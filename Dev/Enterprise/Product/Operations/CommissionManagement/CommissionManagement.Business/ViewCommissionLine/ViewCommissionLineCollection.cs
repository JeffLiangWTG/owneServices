using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineCollection : ActiveBusinessObjectCollection<ViewCommissionLine>
	{
		public ViewCommissionLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ViewCommissionLineCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public ViewCommissionLineCollection(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest, typeof(AccCommissionApprovalRequestItem), new ZQuery(), AccCommissionApprovalRequestItemSchema.CRI_CRQ, AccCommissionApprovalRequestItemSchema.CRI_CL0)
		{
		}
	}
}
