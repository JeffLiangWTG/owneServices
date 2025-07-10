using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public static class LocationsHelper
	{
		public static CodeDescriptionPairList GetESLocations(BusinessObjectFactory factory)
			=> Universal.RefCusCodeListTypes.GetCachedList(factory,
					Core.Constants.CountryCodes.Spain,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType,
					ZDateTime.Today,
					includeParentDataGrouping: false);

		public static ZZRefCusCodeListCombinedCollection GetESLocationsCusCodeList(BusinessObjectFactory factory)
			=> ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory,
								Core.Constants.CountryCodes.Spain,
								new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType },
								ZDateTime.Today,
								System.Array.Empty<RefCusCodeListAttributeFilter>(),
								false);
	}
}
