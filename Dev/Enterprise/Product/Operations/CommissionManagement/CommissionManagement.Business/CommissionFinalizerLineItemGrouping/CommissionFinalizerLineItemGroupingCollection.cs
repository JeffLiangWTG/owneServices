using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFinalizerLineItemGroupingCollection : CommissionLineGroupingCollection<CommissionFinalizerLineItemGrouping, CommissionFinalizerLineItem>
	{
		#region Constructor

		public CommissionFinalizerLineItemGroupingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region AddNew

		protected override CommissionFinalizerLineItemGrouping CreateNew(ViewCommissionLineGrouper<CommissionFinalizerLineItem>[] subGroupers)
		{
			return new CommissionFinalizerLineItemGrouping(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionFinalizerLineItemGrouping(Factory);
		}

		#endregion
	}
}
