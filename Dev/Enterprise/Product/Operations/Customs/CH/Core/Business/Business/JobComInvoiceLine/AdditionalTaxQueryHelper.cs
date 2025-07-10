using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

static class AdditionalTaxQueryHelper
{
	public static string GetAdditionalTaxTariffsCacheKey(this JobComInvoiceLine invoiceLine) =>
		$"{invoiceLine.TariffWithoutCustomsFavourCode}_{invoiceLine.EffectiveCountryOfOrigin}_{invoiceLine.EffectiveAssessmentDate.Date}_{invoiceLine.UniversalTariff?.ZZ1_ZZI_TariffType}";

	public static TariffView[] GetApplicableAdditionalTaxTariffs(this JobComInvoiceLine invoiceLine)
	{
		Argument.NotNull(invoiceLine, nameof(invoiceLine));

		var tariffWithoutCustomsFavourCode = invoiceLine.TariffWithoutCustomsFavourCode;
		var countryCode = invoiceLine.EffectiveCountryOfOrigin;
		var tariffType = invoiceLine.UniversalTariff?.ZZ1_ZZI_TariffType ?? ZGuid.Empty;
		var date = invoiceLine.EffectiveAssessmentDate;
		var factory = invoiceLine.Factory;

		return factory.GetCachedValue(GetAdditionalTaxTariffsCacheKey(invoiceLine), () => GetApplicableAdditionalTaxTariffs(factory, tariffWithoutCustomsFavourCode, tariffType, countryCode, date));
	}

	static TariffView[] GetApplicableAdditionalTaxTariffs(BusinessObjectFactory factory, ZString tariffWithoutCustomsFavourCode, ZGuid tariffType, ZString countryCode, ZDateTime date)
	{
		var query = new ZDBOnlyQuery(typeof(TariffView));

		if (!date.IsValidSmallDateTime || countryCode.IsEmpty || tariffWithoutCustomsFavourCode.IsEmpty || !tariffType.IsValid)
		{
			query.IsNoResultQuery = true;
		}
		else
		{
			var rateQuery = new ZDBOnlyQuery(typeof(TariffView));
			rateQuery.AddSubQuery(GetRateWithAdditionalCodeQuery(date, tariffWithoutCustomsFavourCode, countryCode, tariffType), JoinCondition.And);

			query.AddEffectiveDateFilter(date, TariffViewSchema.ZZ1_StartDate, TariffViewSchema.ZZ1_EndDate);
			query.AddSubQuery(GetTariffTypeQuery(), JoinCondition.And);
			query.AddToFilter(rateQuery, JoinCondition.And);
			query.OrderBy = TariffView.Schema.ZZ1_TariffCode;
		}

		return factory.Load<TariffView>(query);
	}

	static void AddEffectiveDateFilter(this ZDBOnlyQuery query, ZDateTime date, SchemaDateTimeColumn startDateColumn, SchemaDateTimeColumn endDateColumn)
	{
		query.AddToFilter(startDateColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, date);
		query.AddToFilter(endDateColumn, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, date);
	}

	static ZDBOnlySubQuery GetTariffTypeQuery()
	{
		var refCusTariffTypeQuery = new ZDBOnlySubQuery(typeof(RefCusTariffType), TariffViewSchema.ZZ1_ZZI_TariffType);
		refCusTariffTypeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Switzerland);
		refCusTariffTypeQuery.AddToFilter(RefCusTariffTypeSchema.ZZI_TariffType, UniversalReferenceConstants.TariffTypes.AdditionalTaxesTariff);

		return refCusTariffTypeQuery;
	}

	static ZDBOnlySubQuery GetTariffRelationshipQuery(ZString parentTariffCode, ZGuid parentTariffType)
	{
		var tariffRelationshipViewQuery = new ZDBOnlySubQuery(typeof(TariffRelationshipView), TariffRelationshipViewSchema.ZZH_ZZ1_LinkedTariffOrNationalCode, RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode);
		tariffRelationshipViewQuery.AddToFilter(TariffRelationshipViewSchema.ZZH_TariffCode, parentTariffCode);
		tariffRelationshipViewQuery.AddToFilter(TariffRelationshipViewSchema.ZZH_ZZI_TariffType, parentTariffType);

		return tariffRelationshipViewQuery;
	}

	static ZDBOnlySubQuery GetRateWithAdditionalCodeQuery(ZDateTime date, ZString additionalCode, ZString countryCode, ZGuid tariffType)
	{
		var refCusRateQuery = new ZDBOnlySubQuery(typeof(RateView), RateViewSchema.ZZ2_ZZ1_ParentTariffOrNationalCode);
		refCusRateQuery.AddToFilter(RateViewSchema.ZZ2_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Switzerland);
		refCusRateQuery.AddEffectiveDateFilter(date, RateViewSchema.ZZ2_StartDate, RateViewSchema.ZZ2_EndDate);
		refCusRateQuery.AddSubQuery(GetApplicabilityQuery(date, additionalCode, countryCode, tariffType), JoinCondition.And);

		return refCusRateQuery;
	}

	static ZDBOnlySubQuery GetExcludedTradeGroupQuery(ZDateTime date, ZString countryCode)
	{
		var refCusTradeGroupQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), RefCusExcludedTradeGroupSchema.ZZC_ZZA_TradeGroup);
		refCusTradeGroupQuery.AddSubQuery(GetTradeGroupCountryQuery(date, countryCode), JoinCondition.And);
		refCusTradeGroupQuery.AddEffectiveDateFilter(date, CusRefTradeGroupViewSchema.ZZA_StartDate, CusRefTradeGroupViewSchema.ZZA_EndDate);

		var refCusExcludedTradeGroupQuery = new ZDBOnlySubQuery(typeof(RefCusExcludedTradeGroup), RefCusExcludedTradeGroupSchema.ZZC_ZZT_Applicability, true);
		refCusExcludedTradeGroupQuery.AddSubQuery(refCusTradeGroupQuery, JoinCondition.And);

		return refCusExcludedTradeGroupQuery;
	}

	static ZDBOnlySubQuery GetIncludedTradeGroupQuery(ZDateTime date, ZString countryCode)
	{
		var refCusTradeGroupQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupView), CusRefApplicabilityViewSchema.ZZT_ZZA_TradeGroup);
		refCusTradeGroupQuery.AddToFilter(CusRefTradeGroupViewSchema.ZZA_ZZZ_NKDataGrouping, Core.Constants.CountryCodes.Switzerland);
		refCusTradeGroupQuery.AddEffectiveDateFilter(date, CusRefTradeGroupViewSchema.ZZA_StartDate, CusRefTradeGroupViewSchema.ZZA_EndDate);
		refCusTradeGroupQuery.AddSubQuery(GetTradeGroupCountryQuery(date, countryCode), JoinCondition.And);

		return refCusTradeGroupQuery;
	}

	static ZDBOnlySubQuery GetTradeGroupCountryQuery(ZDateTime date, ZString countryCode)
	{
		var refCusTradeGroupCountryQuery = new ZDBOnlySubQuery(typeof(CusRefTradeGroupCountryView), CusRefTradeGroupCountryViewSchema.ZZB_ZZA_TradeGroup);
		refCusTradeGroupCountryQuery.AddToFilter(CusRefTradeGroupCountryViewSchema.ZZB_RN_NKTradeGroupCountryCode, countryCode);
		refCusTradeGroupCountryQuery.AddEffectiveDateFilter(date, CusRefTradeGroupCountryViewSchema.ZZB_StartDate, CusRefTradeGroupCountryViewSchema.ZZB_EndDate);

		return refCusTradeGroupCountryQuery;
	}

	static ZDBOnlySubQuery GetApplicabilityQuery(ZDateTime date, ZString additionalCode, ZString countryCode, ZGuid tariffType)
	{
		var applicabilityQuery = new ZDBOnlySubQuery(typeof(CusRefApplicabilityView), CusRefApplicabilityViewSchema.ZZT_ZZ2_Rate);
		applicabilityQuery.AddToFilter(CusRefApplicabilityViewSchema.ZZT_AdditionalCode, new ZString[] { additionalCode, ZString.Empty });
		applicabilityQuery.AddEffectiveDateFilter(date, CusRefApplicabilityViewSchema.ZZT_StartDate, CusRefApplicabilityViewSchema.ZZT_EndDate);

		applicabilityQuery.AddToFilter(JoinCondition.And, CusRefApplicabilityViewSchema.ZZT_AdditionalCode, SQLComparisonOperator.NotEqual, ZString.Empty);
		applicabilityQuery.AddSubQuery(GetTariffRelationshipQuery(additionalCode, tariffType), JoinCondition.Or);

		applicabilityQuery.AddSubQuery(GetIncludedTradeGroupQuery(date, countryCode), JoinCondition.And);
		applicabilityQuery.AddSubQuery(GetExcludedTradeGroupQuery(date, countryCode), JoinCondition.And);

		return applicabilityQuery;
	}
}
