using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business
{
	public class FilteredChargeCollection : BusinessObjectCollectionView<Charge>
	{
		public FilteredChargeCollection(ChargeCollection collectionToFilter)
			: base(collectionToFilter) { }

		public FilteredChargeCollection(FilteredChargeCollectionView collectionToFilter)
			: base(collectionToFilter) { }

		#region Enable/Disable Cost Reference Filter

		public void EnableCostReferenceFilter(ZString filterByCostReference)
		{
			if (!IsFilterByCostReferenceEnabled || FilterByCostReference != filterByCostReference)
			{
				FilterByCostReference = filterByCostReference;
				IsFilterByCostReferenceEnabled = true;
				Rebuild();
			}
		}

		public void DisableCostReferenceFilter()
		{
			if (IsFilterByCostReferenceEnabled)
			{
				IsFilterByCostReferenceEnabled = false;
				Rebuild();
			}
		}

		bool IsFilterByCostReferenceEnabled;
		ZString FilterByCostReference;

		#endregion

		#region Related Job Filter

		public HashSet<ZString> RelatedJobFilter
		{
			get => relatedJobFilter ?? (relatedJobFilter = new HashSet<ZString>());
			set
			{
				if (!RelatedJobFilter.SetEquals(value))
				{
					relatedJobFilter = value;
					Rebuild();
				}
			}
		}
		HashSet<ZString> relatedJobFilter;

		#endregion

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var charge = ((Charge)element);
			return (!IsFilterByCostReferenceEnabled || FilterByCostReference == charge.JR_CostReference)
				&& (!RelatedJobFilter.Any() || RelatedJobFilter.Contains(charge.JR_Calc_RelatedJobNumber));
		}

		#endregion

		public override IDisposable SuspendAdditionallyForImport()
		{
			ChargeCollection charges = null;

			if (CollectionToFilter is FilteredChargeCollectionView chargeCollectionView)
			{
				charges = chargeCollectionView.CollectionToFilter as ChargeCollection;
			}
			else if (CollectionToFilter is ChargeCollection chargeCollection)
			{
				charges = chargeCollection;
			}

			return charges?.UpdateTotalSuspender.GetSuspender();
		}
	}
}
