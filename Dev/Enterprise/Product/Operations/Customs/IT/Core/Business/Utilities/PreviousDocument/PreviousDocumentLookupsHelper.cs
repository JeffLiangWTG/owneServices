using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class PreviousDocumentLookupsHelper
{
	public static CodeDescriptionPairList GetKilogramUomList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("IT.GetUnitOfQuantityList", () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(Core.Constants.Weight.Kilograms, Res.GetString("E0E06951-24A2-43D1-8F1D-5082F1074F57", "Kilogram"));
			return list;
		});
	}

	public static CodeDescriptionPairList GetPackageTypeList(BusinessObjectFactory factory)
	{
		Argument.NotNull(factory, nameof(factory));

		return RefCusCodeListTypes.GetCachedList(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			ZDateTime.Today);
	}

	public static CodeDescriptionPairList GetKGMUomList(BusinessObjectFactory factory)
	{
		return factory.GetCachedValue("IT.GetKGMUnitOfQuantityList", () =>
		{
			var list = new CodeDescriptionPairList();
			var kilogramCodeList = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(
				factory,
				Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
				ZDateTime.Today);

			if (kilogramCodeList != null)
			{
				list.AddPair(kilogramCodeList.ZZD_Code, kilogramCodeList.ZZD_Description);
			}
			return list;
		});
	}
}
