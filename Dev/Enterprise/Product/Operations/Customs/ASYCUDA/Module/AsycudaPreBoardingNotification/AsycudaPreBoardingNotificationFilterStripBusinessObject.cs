using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaPreBoardingNotificationFilterStripBusinessObject : AsycudaFilterStrip
	{
		public AsycudaPreBoardingNotificationFilterStripBusinessObject() : base()
		{
		}

		public AsycudaPreBoardingNotificationFilterStripBusinessObject(bool enableCountryFilter = true) : base(enableCountryFilter)
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filterCollection = base.GetModuleFiltersCore();

			var localReferenceNumberFilter = filterCollection[FilterConstants.LocalReferenceNumber];
			if (localReferenceNumberFilter != null)
			{
				filterCollection.RemoveFilter(localReferenceNumberFilter);
				localReferenceNumberFilter = filterCollection.AddTextFilter(FilterConstants.LocalReferenceNumber, GetLocalReferenceNumberQuery)
					.WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
				localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
				localReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AsycudaPreBoardingNotificationFilterStripBusinessObject|LocalReferenceNumberFilter", FilterConstants.LocalReferenceNumber);
			}

			return filterCollection;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new AsycudaPreBoardingNotificationFilterStripBusinessObject(shouldAddCountryFilter);
	}
}
