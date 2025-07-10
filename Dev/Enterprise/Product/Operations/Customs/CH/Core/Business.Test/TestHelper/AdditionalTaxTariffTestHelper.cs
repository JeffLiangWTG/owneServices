using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business.Testing;

internal class AdditionalTaxTariffTestHelper
{
	public const string DefaultTradeGroupCountry = Core.Constants.CountryCodes.Austria;
	public const string DefaultExcludedTradeGroupCountry = Core.Constants.CountryCodes.France;

	public AdditionalTaxTariffTestHelper(BusinessObjectFactory factory)
	{
		this.factory = factory;

		universalReferenceTestDataHelper = new UniversalReferenceTestDataHelper(factory);
		tariffTestHelper = new RefCusTariffTestHelper(factory);
	}

	readonly UniversalReferenceTestDataHelper universalReferenceTestDataHelper;
	readonly RefCusTariffTestHelper tariffTestHelper;
	readonly BusinessObjectFactory factory;

	public TariffView CreateAdditionalTaxTariffWithRelationship(TariffView parentTariff, string tariffCode = "280-000",
		string childTariffCode = "12345678912", string tradeGroupCountry = DefaultTradeGroupCountry,
		string excludedTradeGroupCountry = DefaultExcludedTradeGroupCountry, string rateFormula = null)
	{
		var additionalTaxTariff = CreateAdditionalTaxTariffWithAdditionalCode(tariffCode, null, tradeGroupCountry, excludedTradeGroupCountry, rateFormula);

		var tariffRelationship = universalReferenceTestDataHelper.CreateTariffRelationship(additionalTaxTariff.PK, parentTariff.CusTariffType.PK, childTariffCode);
		tariffRelationship.ZZH_ZZ1_LinkedTariffOrNationalCode = additionalTaxTariff.PK;
		factory.Save();

		return additionalTaxTariff;
	}

	public TariffView CreateAdditionalTaxTariffWithAdditionalCode(string tariffCode = "290-000", string additionalCode = "12345678912",
		string tradeGroupCountry = DefaultTradeGroupCountry, string excludedTradeGroupCountry = DefaultExcludedTradeGroupCountry,
		string rateFormula = null, string tariffDescription = null, (string type, string uom) tariffUom = default)
	{
		var tradeGroupName = "TradeGroupAdditionalTax";
		var tradeGroup = factory.LoadTop1<CusRefTradeGroupView>(new ZQuery(CusRefTradeGroupViewSchema.ZZA_TradeGroup, tradeGroupName));
		if (tradeGroup is null)
		{
			tradeGroup = universalReferenceTestDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Switzerland, tradeGroupName, ZDateTime.MinSmallDateTimeValue,
			ZDateTime.MaxSmallDateTime);
			universalReferenceTestDataHelper.AddCountry(tradeGroup, tradeGroupCountry);
		}
		var tariffType = universalReferenceTestDataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RateTypes.AdditionalTaxes);
		factory.Save();
		var additionalTaxTariff = universalReferenceTestDataHelper.CreateTariff(Core.Constants.CountryCodes.Switzerland, tariffType.PK, tariffCode,
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, tariffDescription);

		var rateType = universalReferenceTestDataHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Switzerland,
			UniversalReferenceConstants.RateTypes.AdditionalTaxes);

		var rateCode = tariffCode.Substring(0, 3);
		var rateCodeView = factory.LoadTop1<CusRefRateCodeView>(new ZQuery(CusRefRateCodeViewSchema.ZY1_RateCode, rateCode))
			?? universalReferenceTestDataHelper.CreateCusRateCode(factory, rateCode, rateType.PK);

		var tariff1Rate = universalReferenceTestDataHelper.CreateRate(additionalTaxTariff, rateCodeView.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, rateFormula);
		var applicability = universalReferenceTestDataHelper.CreateCusApplicability(tariff1Rate, tradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode);

		var excludedTradeGroupName = "ExcludedTradeGroupAdditionalTax";
		var excludedTradeGroup = factory.LoadTop1<CusRefTradeGroupView>(new ZQuery(CusRefTradeGroupViewSchema.ZZA_TradeGroup, excludedTradeGroupName));
		if (excludedTradeGroup is null)
		{
			excludedTradeGroup = universalReferenceTestDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.Switzerland, excludedTradeGroupName,
				ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalReferenceTestDataHelper.AddCountry(excludedTradeGroup, excludedTradeGroupCountry);
			universalReferenceTestDataHelper.CreateExcludedTradeGroup(excludedTradeGroup, applicability);
		}

		if (tariffUom.type != null && tariffUom.uom != null)
		{
			universalReferenceTestDataHelper.CreateTariffUOM(additionalTaxTariff, tariffUom.type, tariffUom.uom, Core.Constants.CountryCodes.Switzerland);
		}

		factory.Save();

		return additionalTaxTariff;
	}

	internal void CreateAttributeForTariff(TariffView tariff, string attributeName, params string[] attributeValues)
	{
		foreach (var attributeValue in attributeValues)
		{
			universalReferenceTestDataHelper.CreateTariffAttribute(attributeName, attributeValue, tariff);
		}
	}

	public TariffView CreateParentImportTariff(string tariffCode = "12345678000912") => tariffTestHelper.CreateImportTariff(tariffCode);

	public string GetTariffCodeWithoutCustomsFavourCode(ZString tariff) => $"{tariff.SubstringSafe(0, 8)}{tariff.SubstringSafe(11, 3)}";
}
