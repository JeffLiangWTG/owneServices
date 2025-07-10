using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	static class UniversalReferenceExtensions
	{
		public static CodeDescriptionPairList GetPackagesTypesList(this BusinessObjectFactory factory, string languageCode = "")
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(
			Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, ZDateTime.Today, languageCode: languageCode));

			list.AddRange(
			Universal.RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILManifestPackageTypes, ZDateTime.Today, languageCode: languageCode));
			list.SortByDescriptionAndCombineIfSameCode();
			return list;
		}

		public static IEnumerable<CusRefRateCodeView> GetCachedRatesByType(this BusinessObjectFactory factory, ZString dataGrouping, params ZString[] rateTypes)
		{
			var cacheKey = FormattableString.Invariant($"IL.RefCusRateCodeList.{dataGrouping}.{string.Join("_", rateTypes)}");

			return factory.GetCachedValue(cacheKey, delegate
			{
				var result = CusRefRateCodeView.Loader.Load(factory, dataGrouping, new RateCodeLoadCriteria() { RateTypesToInclude = rateTypes });
				return result;
			});
		}
	}
}
