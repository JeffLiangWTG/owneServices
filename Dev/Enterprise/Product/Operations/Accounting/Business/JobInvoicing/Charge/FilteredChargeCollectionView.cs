using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
namespace Enterprise.Accounting.Business
{
	public sealed class FilteredChargeCollectionView : BusinessObjectCollectionView<Charge>
	{
		public FilteredChargeCollectionView(ChargeCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override void RebuildOnConstruction()
		{
			var jobs = ((ChargeCollection)collectionToFilter)
				.Select(charge => charge.InvoicingJob)
				.Distinct()
				.ToArray();
			JobHeader.GetOrgPKsFromChargesInDb(Factory, jobs);
			using (Factory.SetTempContext(BusinessContext.UseCacheToGetOrgsFromJobChargesInDb))
			{
				base.RebuildOnConstruction();
			}
		}

		#region IsThisPartOfTheCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return ((Charge)element).IsAllowedToViewThisCharge;
		}

		#endregion
	}
}