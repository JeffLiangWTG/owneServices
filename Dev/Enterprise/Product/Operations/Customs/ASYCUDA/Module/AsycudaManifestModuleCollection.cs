using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Schema;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class AsycudaManifestModuleCollection : BusinessObjectCollection<AsycudaManifestHeader>, Integration.Customs.ASYCUDA.IAsycudaManifestModuleCollection
	{
		public AsycudaManifestModuleCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public AsycudaManifestModuleCollection(BusinessObjectFactory factory, ZString country)
			: base(factory)
		{
			this.country = country;
		}
		readonly ZString country;

		public bool IsCountrySpecific => !country.IsEmpty;

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			var headerQuery = new ZDBOnlyQuery(typeof(AsycudaManifestHeader));
			if (IsCountrySpecific)
			{
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, country);
			}
			else
			{
				headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_RN_NKCountry, SQLComparisonOperator.NotEqual, Core.Constants.CountryCodes.Singapore);
			}
			headerQuery.AddToFilter(AsycudaManifestHeaderSchema.AMA_ApplicationCode, new[] { ApplicationCodeTypeList.Codes.Consolidator, ApplicationCodeTypeList.Codes.ShippingLine });

			CountryHelper.AddManifestCompanyFilterInSpecifiedCountry(headerQuery);
			result.AddToFilter(headerQuery);

			return result;
		}
	}
}
