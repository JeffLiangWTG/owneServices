using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFinalizerLineItemGrouping : SelectableCommissionLineGrouping<CommissionFinalizerLineItemGrouping, CommissionFinalizerLineItem>
	{
		#region Constructors

		public CommissionFinalizerLineItemGrouping(BusinessObjectFactory factory, ViewCommissionLineGrouper<CommissionFinalizerLineItem>[] subGroupers = null)
			: base(factory, subGroupers)
		{
		}

		#endregion

		#region SubGroupings

		protected override CommissionLineGroupingCollection<CommissionFinalizerLineItemGrouping, CommissionFinalizerLineItem> GetNewSubGroupingCollection(BusinessObjectFactory factory)
		{
			return new CommissionFinalizerLineItemGroupingCollection(factory);
		}

		public new CommissionFinalizerLineItemGroupingCollection SubGroupingCollection
		{
			get { return (CommissionFinalizerLineItemGroupingCollection)base.SubGroupingCollection; }
		}

		public new IEnumerable<CommissionFinalizerLineItemGrouping> SubGroupings
		{
			get { return base.SubGroupings.Cast<CommissionFinalizerLineItemGrouping>(); }
		}

		#endregion
	}
}
