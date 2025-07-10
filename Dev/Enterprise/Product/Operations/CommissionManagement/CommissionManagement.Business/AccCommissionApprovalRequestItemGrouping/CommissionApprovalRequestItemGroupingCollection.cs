using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionApprovalRequestItemGroupingCollection : CommissionLineGroupingCollection<CommissionApprovalRequestItemGrouping, AccCommissionApprovalRequestItem>
	{
		#region Constructor

		public CommissionApprovalRequestItemGroupingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region AddNew

		protected override CommissionApprovalRequestItemGrouping CreateNew(ViewCommissionLineGrouper<AccCommissionApprovalRequestItem>[] subGroupers)
		{
			return new CommissionApprovalRequestItemGrouping(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionApprovalRequestItemGrouping(Factory);
		}

		#endregion
	}
}
