using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionApprovalRequestItemGrouping : SelectableCommissionLineGrouping<CommissionApprovalRequestItemGrouping, AccCommissionApprovalRequestItem>
	{
		#region Constructors

		public CommissionApprovalRequestItemGrouping(BusinessObjectFactory factory, ViewCommissionLineGrouper<AccCommissionApprovalRequestItem>[] subGroupers = null)
			: base(factory, subGroupers)
		{
		}

		#endregion

		#region Properties

		protected bool IsSelected_ReadOnly
		{
			get { return CommissionLineProviders.Any() && CommissionLineProviders.First().CRI_IsSelected_ReadOnly; }
		}

		#endregion

		#region SubGroupings

		protected override CommissionLineGroupingCollection<CommissionApprovalRequestItemGrouping, AccCommissionApprovalRequestItem> GetNewSubGroupingCollection(BusinessObjectFactory factory)
		{
			return new CommissionApprovalRequestItemGroupingCollection(factory);
		}

		public new CommissionApprovalRequestItemGroupingCollection SubGroupingCollection
		{
			get { return (CommissionApprovalRequestItemGroupingCollection)base.SubGroupingCollection; }
		}

		public new IEnumerable<CommissionApprovalRequestItemGrouping> SubGroupings
		{
			get { return base.SubGroupings.Cast<CommissionApprovalRequestItemGrouping>(); }
		}

		#endregion
	}
}
