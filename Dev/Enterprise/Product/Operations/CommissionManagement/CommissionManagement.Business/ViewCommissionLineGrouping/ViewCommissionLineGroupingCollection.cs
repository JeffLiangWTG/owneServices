using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class ViewCommissionLineGroupingCollection : CommissionLineGroupingCollection<ViewCommissionLineGrouping, ViewCommissionLine>
	{
		#region Constructor

		public ViewCommissionLineGroupingCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region AddNew

		protected override ViewCommissionLineGrouping CreateNew(ViewCommissionLineGrouper<ViewCommissionLine>[] subGroupers)
		{
			return new ViewCommissionLineGrouping(Factory, subGroupers);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ViewCommissionLineGrouping(Factory);
		}

		#endregion
	}
}
