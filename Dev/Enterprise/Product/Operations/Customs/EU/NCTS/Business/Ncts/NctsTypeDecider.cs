using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business.Interfaces;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsTypeDecider
	{
		public static Type GetNctsAdditionalInfoType(BusinessObjectFactory factory, string countryCode) => GetProvider(factory, countryCode).NctsAdditionalInfoType;
		public static Type GetNctsArrivalCargoDescType(BusinessObjectFactory factory, string countryCode) => GetProvider(factory, countryCode).NctsArrivalCargoDescType;
		public static Type GetNctsDepartureCargoDescType(BusinessObjectFactory factory, string countryCode) => GetProvider(factory, countryCode).NctsDepartureCargoDescType;

		static INctsTypesProvider GetProvider(BusinessObjectFactory factory, string countryCode)
		{
			var customsCountryOfJurisdiction = Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode);
			return factory.GetCachedValue(FormattableString.Invariant($"INctsTypesProvider_{customsCountryOfJurisdiction}"), () =>
			{
				INctsTypesProvider provider = null;
				var providers = ObjectFactory.Get<Hashtable>("NCTS.NctsTypesProvider");
				if (!string.IsNullOrEmpty(customsCountryOfJurisdiction))
				{
					var objectHandle = (ObjectHandle)providers[customsCountryOfJurisdiction];
					provider = (INctsTypesProvider)objectHandle?.GetObject();
				}
				return provider ?? new NctsTypesProvider();
			});
		}
	}
}
