using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ConsolCosting
{
	public class ApportionmentSplitChargeFilteredCollection : BusinessObjectCollectionView<ApportionSplitCharge>
	{
		public ApportionmentSplitChargeFilteredCollection(ApportionmentSplitChargeCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var charge = element as ApportionSplitCharge;
			return charge.IsAllowedToViewCosts;
		}

		#endregion

		#region Implementation

		public override bool ReadOnly
		{
			get { return this.CollectionToFilter.ReadOnly; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}

