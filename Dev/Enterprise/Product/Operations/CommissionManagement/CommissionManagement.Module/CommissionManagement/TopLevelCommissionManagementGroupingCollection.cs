using Enterprise.CommissionManagement.Business;

namespace Enterprise.CommissionManagement.Module
{
	public class TopLevelCommissionManagementGroupingCollection : TopLevelViewCommissionLineGroupingCollection
	{
		#region Constructor

		public TopLevelCommissionManagementGroupingCollection(ViewCommissionLineCollection innerCollection)
			: base(innerCollection, GetNewGroupers())
		{
		}

		#endregion

		#region Grouping Delegates

		static ViewCommissionLineGrouper<ViewCommissionLine>[] GetNewGroupers()
		{
			return new ViewCommissionLineGrouper<ViewCommissionLine>[]
			{
				new ViewCommissionLineRecipientAndLocalCompanyAndSourceGrouper<ViewCommissionLine>(),
				new ViewCommissionLineAllCurrenciesGrouper<ViewCommissionLine>()
			};
		}

		#endregion
	}
}
