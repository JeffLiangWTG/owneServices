using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.MX.Business
{
	public static class MXRefCusCodeListTypes
	{
		public static ZZRefCusCodeListCombinedCollection GetCustomsFacilities(BusinessObjectFactory factory)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(factory, ECC.CountryCodes.Mexico, ECC.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, ZDateTime.Today);
		}
	}
}
