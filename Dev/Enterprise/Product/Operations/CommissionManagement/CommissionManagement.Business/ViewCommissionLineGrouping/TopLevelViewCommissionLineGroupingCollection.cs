using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class TopLevelViewCommissionLineGroupingCollection : TopLevelCommissionLineGroupingCollection<ViewCommissionLineGrouping, ViewCommissionLine>
	{
		#region Constructor

		public TopLevelViewCommissionLineGroupingCollection(ViewCommissionLineCollection innerCollection, ViewCommissionLineGrouper<ViewCommissionLine>[] groupers)
			: base(innerCollection, groupers)
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
