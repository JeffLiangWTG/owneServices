using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
	{
		public JobDeclarationLookups(JobDeclaration parent)
			: base(parent)
		{
		}

		public new CodeDescriptionPairList MessageTypeList => Factory.GetCachedValue<ILDeclarationMessageTypeList>();

		public new CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<ILDeclarationMessageSubTypeList>();

		public new CodeDescriptionPairList TransportMeansList => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILCargoIdentifierType);

		public override IBusinessObjectCollection LocationOfGoodsCollection
		{
			get
			{
				ZString country = CountryCodes.Israel;
				ZString listType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					country,
					listType,
					ZDateTime.Today);

				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, "Property", country, false));
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, "Property", listType, false));
				return collection;
			}
		}

		public OrganisationsFindBoxCollection RepresentativeOfficeList => new OrganisationsFindBoxCollection(Factory);

		public OrganisationsFindBoxCollection SellerOfficeList => new OrganisationsFindBoxCollection(Factory);

		public new ZZRefCusCodeListCombinedCollection CustomsOfficeList
		{
			get
			{
				var result = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
					Factory,
					Core.Constants.CountryCodes.Israel,
					new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice },
					ZDateTime.Today,
					null,
					includeParentDataGroupings: false
				);
				var listTypeFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.ListType, (NoResString)"Property", (ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, isRemovable: false);
				var countryFilter = new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.CountryOrGrouping, (NoResString)"Property", (ZString)Core.Constants.CountryCodes.Israel, isRemovable: false);
				result.FilterBusinessObjectDefaults.Add(listTypeFilter);
				result.FilterBusinessObjectDefaults.Add(countryFilter);
				return result;
			}
		}
	}
}
