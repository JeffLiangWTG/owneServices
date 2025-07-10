using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestCollection : ActiveBusinessObjectCollection<AccCommissionApprovalRequest>
	{
		public AccCommissionApprovalRequestCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccCommissionApprovalRequestCollection(AccCommissionLine commissionLine)
			: base(commissionLine, typeof(AccCommissionApprovalRequestItem), new ZQuery(), AccCommissionApprovalRequestItemSchema.CRI_CL0, AccCommissionApprovalRequestItemSchema.CRI_CRQ)
		{
		}

		public AccCommissionApprovalRequestCollection(ViewCommissionLine commissionLine)
			: base(commissionLine, typeof(AccCommissionApprovalRequestItem), new ZQuery(), AccCommissionApprovalRequestItemSchema.CRI_CL0, AccCommissionApprovalRequestItemSchema.CRI_CRQ)
		{
		}
	}
}
