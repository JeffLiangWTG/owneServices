using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public static class UniversalReferenceHelper
{
	public static IEnumerable<ZString> GetSupplementaryQuantityUOMs(TariffView universalTariff)
	{
		Argument.NotNull(universalTariff, nameof(universalTariff));
		Argument.NotNull(universalTariff.UnitsOfMeasure, nameof(universalTariff.UnitsOfMeasure));
		return universalTariff.UnitsOfMeasure.Where(x => x.ZZ8_Type == Constants.UnitOfMeasureTypes.AdditionalUOMType).Select(x => x.ZZ8_UOM).Distinct();
	}

	public static ZString GetThirdQuantityUOM(RateView universalRate)
	{
		Argument.NotNull(universalRate, nameof(universalRate));
		Argument.NotNull(universalRate.CusTariff, nameof(universalRate.CusTariff));
		Argument.NotNull(universalRate.UnitsOfMeasure, nameof(universalRate.UnitsOfMeasure));

		var thirdQuantityUOMs = universalRate.UnitsOfMeasure.Select(x => x.ZXG_UOM);
		var supplementaryUoms = GetSupplementaryQuantityUOMs(universalRate.CusTariff);
		var weightUOMs = new ZString[] { Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Gram, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne };
		return thirdQuantityUOMs.Except(supplementaryUoms).Except(weightUOMs).FirstOrDefault();
	}

	public static CodeDescriptionPairList GetPortTaxRateList(BusinessObjectFactory factory) => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Italy, UniversalReferenceConstants.RefCusCodeListTypes.HarbourCommodityCode, ZDateTime.Today);

	public static CodeDescriptionPairList GetEuropeanUnionEUNCustomsUQCodeList(BusinessObjectFactory factory)
	{
		Argument.NotNull(factory, nameof(factory));

		return RefCusCodeListTypes.GetCachedList(factory,
			Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
			ZDateTime.Today);
	}
}
