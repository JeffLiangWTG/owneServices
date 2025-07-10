using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class PreviousDocumentLookups : CusSupportingInfoLookups
	{
		public PreviousDocumentLookups(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		public new ZZRefCusCodeListCombinedCollection CodeList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
									Factory,
									Core.Constants.CountryCodes.Israel,
									new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILEntryStyle },
									ZDateTime.Today,
									null,
									includeParentDataGroupings: false
								);
				var listTypeFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, (NoResString)"Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILEntryStyle, isRemovable: false);
				var countryFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, (NoResString)"Property", (ZString)Core.Constants.CountryCodes.Israel, isRemovable: false);
				result.FilterBusinessObjectDefaults.Add(listTypeFilter);
				result.FilterBusinessObjectDefaults.Add(countryFilter);
				return result;
			}
		}

		public new ZZRefCusCodeListCombinedCollection UnitOfQuantityList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
									Factory,
									Core.Constants.CountryCodes.Israel,
									new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCustomsUnitsOfQuantity },
									ZDateTime.Today,
									null,
									includeParentDataGroupings: false
								);
				var listTypeFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, (NoResString)"Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCustomsUnitsOfQuantity, isRemovable: false);
				var countryFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, (NoResString)"Property", (ZString)Core.Constants.CountryCodes.Israel, isRemovable: false);
				result.FilterBusinessObjectDefaults.Add(listTypeFilter);
				result.FilterBusinessObjectDefaults.Add(countryFilter);
				return result;
			}
		}
		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;
	}
}
