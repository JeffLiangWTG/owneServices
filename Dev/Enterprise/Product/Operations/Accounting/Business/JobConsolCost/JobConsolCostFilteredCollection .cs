using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class JobConsolCostFilteredCollection : BusinessObjectCollectionView<JobConsolCost>
	{
		public JobConsolCostFilteredCollection(JobConsolCostCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var jobConsolCost = element as JobConsolCost;
			if (jobConsolCost.IsInDatabase && jobConsolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().Any(c => !c.IsAllowedToViewCosts && c.JR_IsUsedForApportionment))
			{
				return false;
			}

			return true;
		}

		#endregion

		protected override bool AllowRemoveCore
		{
			get { return CollectionToFilter.AllowRemove; }
		}
	}
}

#if DEBUG

namespace Enterprise.Accounting.Business
{
}

#endif
