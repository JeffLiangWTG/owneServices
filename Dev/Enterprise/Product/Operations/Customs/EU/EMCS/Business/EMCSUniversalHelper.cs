using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public static class EMCSUniversalHelper
	{
		public static CodeDescriptionPairList GetEMCSPackTypeList(this BusinessObjectFactory factory, ZString languageCode) => RefCusCodeListTypes.GetCachedList(factory
			, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN
			, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes
			, ZDateTime.Today
			, languageCode: languageCode);

		public static ZZRefCusCodeListCombined[] GetExciseProductCodes(this BusinessObjectFactory factory, ZString dataGroupingCode)
		{
			return factory.GetCachedValue("EU|GetExciseProductCodes|" + dataGroupingCode, () =>
			{
				return ZZRefCusCodeListCombined.Loader.Load(factory, dataGroupingCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, ZDateTime.Now);
			});
		}

		public static bool IsCountable(this EMCSPackage package)
		{
			var unitOfQuantity = package.B5_UnitType;

			return package.Factory.GetCachedValue("EU|CountablePackUnit|" + unitOfQuantity, () =>
			{
				return RefCusCodeListAttributeTypes.GetAttributeValuesFor(package.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, ZDateTime.Now, unitOfQuantity, RefCusCodeListAttributeTypes.Codes.Countable).Length > 0;
			});
		}
	}
}
