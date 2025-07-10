using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Module;

public sealed class CustomsSummaryFilterStripBusinessObject : FilterStripBusinessObject
{
	public static class Schema
	{
		public const string AccountNumber = "Account Number";
		public const string SummaryDate = "Summary Date";
		public const string SummaryNumber = "Summary Number";
		public const string EvvDocumentReceiveStatus = "eVV Document Receive Status";
		public const string EntryNumber = "Entry Number";
		public const string TraderReference = "Trader Reference";
		public const string EvvDocumentType = "eVV Document Type";
		public const string Amount = "Amount";
	}

	CustomsSummaryFilterStripLookups lookups;

	public CustomsSummaryFilterStripLookups Lookups => lookups ?? (lookups = new CustomsSummaryFilterStripLookups(this));

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var result = new ModuleFilterCollection();
		AddAccountNumberFilter(result);
		AddSummaryDateFilter(result);
		AddSummaryNumberFilter(result);
		AddEntryNumberFilter(result);
		AddTraderReferenceFilter(result);
		AddEvvDocumentRecieveFilter(result);
		AddEvvDocumentTypeFilter(result);
		AddAmountFilter(result);
		return result;
	}

	protected override void AddInitialAuditFilters(ModuleFilterCollection filters)
	{
		base.AddInitialAuditFilters(filters);

		if (filters[FilterDescriptions.CreatedTime] is ModuleDateFilter createdTimeFilter)
		{
			createdTimeFilter.Visibility = FilterVisibility.AlwaysVisible;
			createdTimeFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
		}
	}

	public void AddAccountNumberFilter(ModuleFilterCollection filters)
	{
		var accountNumberFilter = filters.AddTextFilter(Schema.AccountNumber, GetAccountNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusStatementHeaderSchema.B2_AccountNo);
		accountNumberFilter.Category = FilterCategories.NumbersAndReferences;
		accountNumberFilter.MultilingualDescription = ResString.GetMultilingualString("BCB1A2B6-75E3-497D-B209-90A1E3C7875D", Schema.AccountNumber);
	}

	public ZQuery GetAccountNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var headerFilter = new ZDBOnlySubQuery(typeof(CustomsSummaryHeader), CusStatementLineSchema.B3_B2);
		headerFilter.AddToFilter(CusStatementHeaderSchema.B2_AccountNo, comparisonOperator, value.KeepAlphanumericCharacters());

		var lineFilter = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

		return lineFilter;
	}

	public void AddSummaryDateFilter(ModuleFilterCollection filters)
	{
		var summaryDateFilter = filters.AddDateFilter(Schema.SummaryDate, GetSummaryDateQuery);
		summaryDateFilter.Category = FilterCategories.Dates;
		summaryDateFilter.MultilingualDescription = ResString.GetMultilingualString("10D1056B-F073-4181-A1B8-1EE013152D95", Schema.SummaryDate);
	}

	public ZQuery GetSummaryDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
	{
		var headerFilter = new ZDBOnlySubQuery(typeof(CustomsSummaryHeader), CusStatementLineSchema.B3_B2);
		AddDateTimeRange(headerFilter, comparisonOperator, JoinCondition.And, CusStatementHeaderSchema.B2_ProcessDate, value1, value2);

		var lineFilter = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

		return lineFilter;
	}

	public void AddSummaryNumberFilter(ModuleFilterCollection filters)
	{ 
		var summaryNumberFilter = filters.AddTextFilter(Schema.SummaryNumber, GetSummaryNumberQuery).WithMaxLengthOf<ModuleTextFilter>(CusStatementHeaderSchema.B2_StatementNumber);
		summaryNumberFilter.Category = FilterCategories.NumbersAndReferences;
		summaryNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
		summaryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("651695D5-AB82-4819-8A39-9425B3BA2B55", Schema.SummaryNumber);
	}

	public ZQuery GetSummaryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var headerFilter = new ZDBOnlySubQuery(typeof(CustomsSummaryHeader), CusStatementLineSchema.B3_B2);
		headerFilter.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, comparisonOperator, value.KeepAlphanumericCharacters());

		var lineFilter = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		lineFilter.AddSubQuery(headerFilter, JoinCondition.And);

		return lineFilter;
	}

	public void AddEvvDocumentRecieveFilter(ModuleFilterCollection filters)
	{
		var evvDocRecieveFilter = filters.AddTextFilter(Schema.EvvDocumentReceiveStatus, CusStatementLineSchema.B3_Status, Lookups.BordereauReceivedStatusList).WithMaxLengthOf<ModuleTextFilter>(CusStatementLineSchema.B3_Status);
		evvDocRecieveFilter.Category = FilterCategories.StatusAndFlags;
		evvDocRecieveFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
		evvDocRecieveFilter.MultilingualDescription = ResString.GetMultilingualString("C2F75EE2-D5B5-41F3-A274-FC0C45B43FE7", Schema.EvvDocumentReceiveStatus);
	}

	public void AddEntryNumberFilter(ModuleFilterCollection filters)
	{
		var entryNumberFilter = filters.AddTextFilter(Schema.EntryNumber, CusStatementLineSchema.B3_EntryNum);
		entryNumberFilter.Category = FilterCategories.NumbersAndReferences;
		entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("34D51793-30B1-4B2C-8325-13BD1C9461C9", Schema.EntryNumber);
	}

	public void AddTraderReferenceFilter(ModuleFilterCollection filters)
	{
		var traderReferenceFilter = filters.AddTextFilter(Schema.TraderReference, CusStatementLineSchema.B3_BrokerReference);
		traderReferenceFilter.Category = FilterCategories.NumbersAndReferences;
		traderReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("1B9F95A3-0AA9-44DB-9F41-59D65B8CAD14", Schema.TraderReference);
	}

	public void AddEvvDocumentTypeFilter(ModuleFilterCollection filters)
	{
		var traderReferenceFilter = filters.AddTextFilter(Schema.EvvDocumentType, GetEvvDocumentTypeQuery, Lookups.BordereauChargeTypeList).WithMaxLengthOf<ModuleTextFilter>(CusStatementLineChargeSchema.B4_ChargeType);
		traderReferenceFilter.Category = FilterCategories.ModesAndTypes;
		traderReferenceFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
		traderReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("C92A3451-D740-4AB1-8D05-ABB8D4C7551B", Schema.EvvDocumentType);
	}

	public ZQuery GetEvvDocumentTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var lineQuery = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		var lineChargeQuery = new ZDBOnlySubQuery(typeof(CustomsSummaryLineCharge), CusStatementLineChargeSchema.PK);
		lineChargeQuery.AddToFilter(CusStatementLineChargeSchema.B4_ChargeType, comparisonOperator, value.KeepAlphanumericCharacters());
		lineQuery.AddSubQuery(CusStatementLineSchema.PK, CusStatementLineChargeSchema.B4_B3, lineChargeQuery, JoinCondition.And);

		return lineQuery;
	}

	public void AddAmountFilter(ModuleFilterCollection filters)
	{
		var traderReferenceFilter = filters.AddNumberRangeFilter(Schema.Amount, GetAmountQuery);
		traderReferenceFilter.Category = ModelViewConstants.AmountFilter;
		traderReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("796BF41B-63DA-46CD-A036-2FFB37E2DBA3", Schema.Amount);
	}

	public ZQuery GetAmountQuery(INumericZType value, INumericZType value2)
	{
		var lineQuery = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
		var lineChargeQuery = new ZDBOnlySubQuery(typeof(CustomsSummaryLineCharge), CusStatementLineChargeSchema.PK);
		lineChargeQuery.AddToFilter(CusStatementLineChargeSchema.B4_ChargeAmount, SQLComparisonOperator.GreaterThanOrEqualTo, value)
			.AddToFilter(CusStatementLineChargeSchema.B4_ChargeAmount, SQLComparisonOperator.LessThanOrEqualTo, value2);
		lineQuery.AddSubQuery(CusStatementLineSchema.PK, CusStatementLineChargeSchema.B4_B3, lineChargeQuery, JoinCondition.And);

		return lineQuery;
	}
}
