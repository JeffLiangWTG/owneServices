using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using static Enterprise.Integration.Customs.Shared;

namespace Enterprise.Customs.Common
{
	public static class EntryStatusListHelper
	{
		public static IEntryStatusListProvider GetEntryStatusListProvider(ZString countryCode)
		{
			var allProviders = ObjectFactory.Get<Hashtable>("EntryStatusListProviders");
			var countryCodeKey = countryCode.ToString();
			ObjectHandle providerHandle = (allProviders[countryCodeKey]
										   ?? (ObjectFactory.Get<IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCodeKey) ? allProviders["EUN"] : null)
										   ?? (ObjectFactory.Get<IAsycudaCustomsCountryProvider>().IsAsycudaCustomsCountry(countryCodeKey) ? allProviders["Asycuda"] : null)
										   ?? allProviders["Shared"]) as ObjectHandle;

			return (IEntryStatusListProvider)providerHandle.GetObject();
		}

		public static ICodeDescriptionPairList EntryStatusList(BusinessObjectFactory factory, ZString countryCode, ZString messageType) => GetEntryStatusListProvider(countryCode).EntryStatusList(factory, countryCode, messageType);

		public static ICodeDescriptionPairList EntryStatusListForShipments(BusinessObjectFactory factory, ZString countryCode) => GetEntryStatusListProvider(countryCode).EntryStatusListForShipments(factory, countryCode);
	}
}
