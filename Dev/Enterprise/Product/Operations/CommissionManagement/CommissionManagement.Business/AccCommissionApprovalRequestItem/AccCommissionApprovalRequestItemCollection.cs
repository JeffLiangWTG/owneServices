using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionApprovalRequestItemCollection : ActiveBusinessObjectCollection<AccCommissionApprovalRequestItem>
	{
		public AccCommissionApprovalRequestItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AccCommissionApprovalRequestItemCollection(AccCommissionApprovalRequest approvalRequest)
			: base(approvalRequest)
		{
		}

		public AccCommissionApprovalRequestItemCollection(AccCommissionLine commissionLine)
			: base(commissionLine)
		{
		}

		#region Allowed Actions

		protected override bool AllowNew
		{
			get { return false; }
		}

		#endregion
	}
}
