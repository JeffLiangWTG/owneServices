using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.Customs.KR.Module
{
	public class EntryDetailsFor5GWFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Payer = "Payer";
			public const string PayerCompanyName = "Payer Company Name";
			public const string EntryCreatedDate = "Entry Created Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var declarationDateFilter = result.AddDateFilter(Schema.EntryCreatedDate, KREntryHeaderDetailsViewSchema.KEH_EntryCreatedLocalTime);
			declarationDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5GWFilterStripBusinessObject|DeclarationTime", Schema.EntryCreatedDate);
			declarationDateFilter.Category = FilterCategories.Dates;
			declarationDateFilter.FilterOption = DateRangeSearchTexts.Today;
			declarationDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			declarationDateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;

			var payerFilter = result.AddGuidFilter(Schema.Payer, ModuleIDs.Organisation, KREntryHeaderDetailsViewSchema.KEH_OH_Payer, new OrgHeaderCollection(Factory));
			payerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5GWFilterStripBusinessObject|Payer", Schema.Payer);
			payerFilter.Category = FilterCategories.Organisations;

			var payerCompanyNameFilter = result.AddTextFilter(Schema.PayerCompanyName, KREntryHeaderDetailsViewSchema.KEH_PayerName);
			payerCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5GWFilterStripBusinessObject|PayerCompanyName", Schema.PayerCompanyName);
			payerCompanyNameFilter.Category = FilterCategories.TextSearch;
			payerCompanyNameFilter.MaxLength = 100;
			payerCompanyNameFilter.ComparisonOperator_List.Clear();
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			payerCompanyNameFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;

			return result;
		}
	}
}
