using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CA.Business
{
	public static class ZZRefCarrierCombinedCollectionExtension
	{
		public static ZZRefCarrierCombinedCollection GetCachedCollection(BusinessObjectFactory factory, ZString transportMode)
		{
			return factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "CACarrierCombinedCollection_{0}", transportMode), () =>
			{
				var result = new ZZRefCarrierCombinedCollection(factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, ZString.Empty);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.Country, "Property", (ZString)Core.Constants.CountryCodes.Canada, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.CarrierType, "Property", (ZString)Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, false));
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCarrierFilters.TransportMode, "Property", transportMode));
				return result;
			});
		}
	}
}
