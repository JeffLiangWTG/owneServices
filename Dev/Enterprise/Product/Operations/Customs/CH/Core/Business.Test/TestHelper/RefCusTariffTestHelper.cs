using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;
using RateView = Enterprise.Customs.Universal.RateView;

namespace Enterprise.Customs.CH.Business.Testing;

public class RefCusTariffTestHelper
{
	internal const string ImportTariffCodeWildTurkey = "01051200000000";
	internal const string ImportTariffCodeDuck = "01051300000000";
	internal const string ImportTariffCodeGoose = "01051400000000";
	public const string ImportTariffBycycle = "87120000000000";
	public const string ImportTariffBeverages = "22083020000801";
	public const string ImportTariffMineralOilOthers = "27090090000000";
	public const string ImportTariffMineralOilForUseAsFuel = "27090010000000";

	public const string ExportTariffGardenUmbrellas = "66011000000";
	public const string ExportTariffHay = "12149011000";
	public const string ExportTariffFruitJuice = "20096119000";
	internal const string ExportTariffStraw = "12130091000";
	internal const string ExportTariffEthylAlcohol = "22089010000";
	internal const string ExportTariffNonAlcoholicBeer = "22029100000";

	internal const decimal TareSupplementDuck = 3;
	internal const decimal TareSupplementGoose = 5;

	internal const string UomBycycleCU3 = "NAR";
	internal const string UomBeveragesCU3 = "LTR";

	internal const string UomCU3NAR = "NAR";

	internal const string Datagrouping = Core.Constants.CountryCodes.Switzerland;
	public const string Country = Core.Constants.CountryCodes.France;

	public RefCusTariffTestHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;
		Helper = new UniversalReferenceTestDataHelper(factory);
	}
	public readonly UniversalReferenceTestDataHelper Helper;
	readonly BusinessObjectFactory factory;

	internal TariffView CreateImportTariff(string tariffCode) => CreateTariff(tariffCode, Universal.Constants.TariffTypes.Import);

	public TariffView CreateExportTariff(string tariffCode) => CreateTariff(tariffCode, Universal.Constants.TariffTypes.Export);

	internal TariffView CreateTariff(string tariffCode, string typeCode, string datagrouping = Datagrouping)
	{
		var tariffType = Helper.CreateNewOrGetExistingTariffType(datagrouping, typeCode);
		factory.Save();

		return Helper.CreateTariff(datagrouping, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
	}

	internal TariffView CreateImportTariffWithAttribute(string tariffCode, string attributeName, params string[] attributeValues) => CreateTariffWithAttribute(Universal.Constants.TariffTypes.Import, tariffCode, attributeName, Datagrouping, attributeValues);

	public TariffView CreateExportTariffWithAttribute(string tariffCode, string attributeName, params string[] attributeValues) => CreateTariffWithAttribute(Universal.Constants.TariffTypes.Export, tariffCode, attributeName, Datagrouping, attributeValues);

	internal TariffView CreateTariffWithAttribute(string typeCode, string tariffCode, string attributeName, string datagrouping, params string[] attributeValues)
	{
		var tariff = CreateTariff(tariffCode, typeCode, datagrouping);

		foreach (var attributeValue in attributeValues)
		{
			Helper.CreateTariffAttribute(attributeName, attributeValue, tariff);
		}
		return tariff;
	}

	internal TariffView CreateImportTariffWithUOMs(string tariffCode, params string[] uoms) => CreateTariffWithUOMs(tariffCode, Universal.Constants.TariffTypes.Import, uoms);

	internal TariffView CreateExportTariffWithUOMs(string tariffCode, params string[] uoms) => CreateTariffWithUOMs(tariffCode, Universal.Constants.TariffTypes.Export, uoms);

	internal TariffView CreateTariffWithUOMs(string tariffCode, string typeCode, params string[] uoms)
	{
		var tariff = CreateTariff(tariffCode, typeCode);

		for (int i = 0; i < uoms.Length; i++)
		{
			Helper.CreateTariffUOM(tariff, $"CU{i + 1}", uoms[i], Datagrouping);
		}
		factory.Save();
		return tariff;
	}

	internal TariffView CreateImportTariffWithConditionClass(string tariffCode, string conditionType, string conditionValue, string conditionComment = null, string cu3UOM = null, string[] countries = null, string[] excludedCountries = null, string[] attributes = null)
	{
		return CreateImportTariffWithCondition(tariffCode, RefCusConditionTypes.ConditionClass.Class, conditionType, RefCusConditionValueTypes.Codes.Formula, conditionValue, conditionComment, cu3UOM, countries: countries, excludedCountries: excludedCountries, attributes: attributes);
	}

	internal TariffView CreateExportTariffWithConditionClass(string tariffCode, string conditionType, string conditionValue, string conditionComment = null, string cu3UOM = null, string[] countries = null, string[] excludedCountries = null, string[] attributes = null)
	{
		return CreateExportTariffWithCondition(tariffCode, RefCusConditionTypes.ConditionClass.Class, conditionType, RefCusConditionValueTypes.Codes.Formula, conditionValue, conditionComment, cu3UOM, countries: countries, excludedCountries: excludedCountries, attributes: attributes);
	}

	internal TariffView CreateImportTariffWithCondition(string tariffCode, string conditionClass, string conditionType, string valueType, string conditionValue, string conditionComment = null, string cu3UOM = null, string[] countries = null, string[] excludedCountries = null, string[] attributes = null)
	{
		return CreateTariffWithCondition(tariffCode, TariffTypes.ImportTariff, conditionClass, conditionType, valueType, conditionValue, conditionComment: conditionComment, cu3UOM: cu3UOM, countries: countries, excludedCountries: excludedCountries, attributes: attributes);
	}

	internal TariffView CreateExportTariffWithCondition(string tariffCode, string conditionClass, string conditionType, string valueType, string conditionValue, string conditionComment = null, string cu3UOM = null, string[] countries = null, string[] excludedCountries = null, string[] attributes = null)
	{
		return CreateTariffWithCondition(tariffCode, TariffTypes.ExportTariff, conditionClass, conditionType, valueType, conditionValue, conditionComment: conditionComment, cu3UOM: cu3UOM, countries: countries, excludedCountries: excludedCountries, attributes: attributes);
	}

	internal TariffView CreateTariffWithCondition(string tariffCode, string tariffType, string conditionClass, string conditionType, string valueType, string conditionValue, string conditionComment = null, string cu3UOM = null, string cu4UOM = null, string[] countries = null, string[] excludedCountries = null, string[] attributes = null)
	{
		var tariff = CreateTariff(tariffCode, tariffType);

		Helper.CreateTariffUOM(tariff, "CU1", SwissCustomsConstants.MeasurementUnits.GrossWeightUOM, Datagrouping);
		Helper.CreateTariffUOM(tariff, "CU2", SwissCustomsConstants.MeasurementUnits.NetWeightUOM, Datagrouping);
		if (cu3UOM != null)
		{
			Helper.CreateTariffUOM(tariff, "CU3", cu3UOM, Datagrouping);
		}
		if (cu4UOM != null)
		{
			Helper.CreateTariffUOM(tariff, "CU4", cu4UOM, Datagrouping);
		}

		var cusConditionType = Helper.CreateOrGetExistingRefCusConditionType(Datagrouping, conditionClass, conditionType);
		var cusValueType = Helper.CreateOrGetExistingRefCusConditionValueType(Datagrouping, valueType, afterCreate: f => f.ZX4_IsFormula = valueType == Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.Formula);
		var cusCondition = Helper.CreateOrGetExistingRefCusCondition(Datagrouping, cusConditionType.PK, tariff.PK, conditionComment ?? $"ConditionType {conditionType}", true, true, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);

		if (conditionValue != null)
		{
			Helper.CreateOrGetExistingRefCusConditionValue(cusValueType.PK, cusCondition.PK, conditionValue);
		}

		if (countries != null || excludedCountries != null)
		{
			var tradeGroup = countries == null ? null : CreateTradeGroup(countries);
			var applicability = Helper.CreateCusApplicability(cusCondition, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			if (excludedCountries != null)
			{
				var excludedTradeGroup = CreateTradeGroup(excludedCountries);
				Helper.CreateExcludedTradeGroup(excludedTradeGroup, applicability);
			}
		}

		if (attributes != null)
		{
			for (int a = 0; a < attributes.Length; a += 2)
			{
				Helper.CreateTariffAttribute(attributes[a], attributes[a + 1], tariff);
			}
		}

		factory.Save();
		return tariff;
	}

	int tradeGroupCounter;

	CusRefTradeGroupView CreateTradeGroup(params string[] countries)
	{
		var tradeGroupCode = $"TradeGroup#{++tradeGroupCounter}";
		var tradeGroup = Helper.CreateTradeGroup(Datagrouping, tradeGroupCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		foreach (var country in countries.Where(c => !tradeGroup.TradeGroupCountries.Any(x => x.ZZB_RN_NKTradeGroupCountryCode == c)))
		{
			Helper.AddCountry(tradeGroup, country);
		}
		factory.Save();
		return tradeGroup;
	}

	public TariffView CreateImportTariffWithSingleRate(string tariffCode, string preferenceCode = PrimaryPreferenceCodes.PreferentialTariff, string rateFormula = "3.06 * [KGMG]")
	{
		var tariff = CreateImportTariffWithUOMs(tariffCode, new[] { SwissCustomsConstants.MeasurementUnits.GrossWeightUOM, SwissCustomsConstants.MeasurementUnits.NetWeightUOM });
		AddRate(tariff, preferenceCode, rateFormula);
		return tariff;
	}

	public RateView AddRate(TariffView tariff, string preference, string rateFormula, ZDateTime? startDate = null, ZDateTime? endDate = null)
	{
		var effStartDate = startDate ?? ZDateTime.MinSmallDateTimeValue;
		var effEndDate = endDate ?? ZDateTime.MaxSmallDateTime;

		var rateType = Helper.CreateNewOrGetExistingRateType(Datagrouping, Constants.RateTypes.Duty);
		var rateCode = Helper.LoadOrCreateNewCusRateCode(factory, Constants.RateTypes.Duty, rateType.PK);

		var preferencePR = Helper.CreatePreferenceForCountry(preference, "Preference " + preference, Datagrouping);
		var tradeGroup = CreateTradeGroup(Country);

		var rate = Helper.CreateRate(tariff, rateCode.PK, effStartDate, effEndDate, rateFormula: rateFormula, preferencePk: preferencePR.PK, dataGrouping: Datagrouping);
		Helper.CreateCusApplicability(rate, tradeGroup, effStartDate, effEndDate);

		factory.Save();
		return rate;
	}

	public TariffView CreateImportTariffWithMultipleRates(string tariffCode, string[] rateFormulas = null)
	{
		if (rateFormulas == null)
		{
			rateFormulas = new string[]
			{
					"1.04 * [KGMG]",
					"3.06 * [KGMG]"
			};
		}

		var rateType = Helper.CreateNewOrGetExistingRateType(Datagrouping, Universal.Constants.RateTypes.Duty);
		var rateCode = Helper.LoadOrCreateNewCusRateCode(factory, Universal.Constants.RateTypes.Duty, rateType.PK);

		var preferencePR = Helper.CreatePreferenceForCountry(PrimaryPreferenceCodes.PreferentialTariff, "Preferential Tariff", Datagrouping);

		var tariff = CreateImportTariffWithUOMs(tariffCode, new[] { SwissCustomsConstants.MeasurementUnits.GrossWeightUOM, SwissCustomsConstants.MeasurementUnits.NetWeightUOM });
		foreach (string rateFormula in rateFormulas)
		{
			var tradeGroup = CreateTradeGroup(Country);
			var rate = Helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula: rateFormula, preferencePk: preferencePR.PK, dataGrouping: Datagrouping);
			Helper.CreateCusApplicability(rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: GetNewAdditionalCode());
		}

		factory.Save();
		return tariff;
	}

	int additionalCodeCounter;

	string GetNewAdditionalCode()
	{
		return $"AC{++additionalCodeCounter:000}";
	}
}
