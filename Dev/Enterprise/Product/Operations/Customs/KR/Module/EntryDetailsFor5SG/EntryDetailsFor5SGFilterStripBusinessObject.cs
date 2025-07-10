using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.Customs.KR.Module
{
	public class EntryDetailsFor5SGFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Payer = "Payer";
			public const string PayerCompanyName = "Payer Company Name";
			public const string Importer = "Importer";
			public const string ImporterCompanyName = "Importer Company Name";
			public const string EstimatedDateOfFinalPrice = "Estimated Date Of Final Price";
			public const string ClearedDate = "Cleared Date";
			public const string AcceptedDate = "Accepted Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var payerFilter = result.AddGuidFilter(Schema.Payer, ModuleIDs.Organisation, KREntryHeaderDetailsViewSchema.KEH_OH_Payer, new ConsigneeCollection(Factory));
			payerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|Payer", Schema.Payer);
			payerFilter.Category = FilterCategories.Organisations;
			payerFilter.ComparisonOperator_List.Clear();
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));
			payerFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var payerCompanyNameFilter = result.AddTextFilter(Schema.PayerCompanyName, KREntryHeaderDetailsViewSchema.KEH_PayerName);
			payerCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|PayerCompanyName", Schema.PayerCompanyName);
			payerCompanyNameFilter.Category = FilterCategories.TextSearch;
			payerCompanyNameFilter.MaxLength = 100;
			payerCompanyNameFilter.ComparisonOperator_List.Clear();
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			payerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			payerCompanyNameFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;

			var importerFilter = result.AddGuidFilter(Schema.Importer, ModuleIDs.Organisation, KREntryHeaderDetailsViewSchema.KEH_OH_Importer, new ConsigneeCollection(Factory));
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|Importer", Schema.Importer);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.ComparisonOperator_List.Clear();
			importerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			importerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			importerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			importerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			importerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));
			importerFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var importerCompanyNameFilter = result.AddTextFilter(Schema.ImporterCompanyName, KREntryHeaderDetailsViewSchema.KEH_ImporterName);
			importerCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|ImporterCompanyName", Schema.ImporterCompanyName);
			importerCompanyNameFilter.Category = FilterCategories.TextSearch;
			importerCompanyNameFilter.MaxLength = 100;
			importerCompanyNameFilter.ComparisonOperator_List.Clear();
			importerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			importerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			importerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			importerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			importerCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			importerCompanyNameFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;

			var estimatedDateFilter = result.AddDateFilter(Schema.EstimatedDateOfFinalPrice, KREntryHeaderDetailsViewSchema.KEH_EstimatedDateOfFinalPrice);
			estimatedDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|EstimatedDateOfFinalPrice", Schema.EstimatedDateOfFinalPrice);
			estimatedDateFilter.Category = FilterCategories.Dates;
			estimatedDateFilter.FilterOption = DateRangeSearchTexts.Today;
			estimatedDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			estimatedDateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Next14Days;

			var clearedDateFilter = result.AddDateFilter(Schema.ClearedDate, KREntryHeaderDetailsViewSchema.KEH_EntryReleaseDate);
			clearedDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|ClearedDate", Schema.ClearedDate);
			clearedDateFilter.Category = FilterCategories.Dates;
			clearedDateFilter.FilterOption = DateRangeSearchTexts.Today;

			var acceptedDateFilter = result.AddDateFilter(Schema.AcceptedDate, KREntryHeaderDetailsViewSchema.KEH_EntryNumIssueDate);
			acceptedDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5SGFilterStripBusinessObject|AcceptedDate", Schema.AcceptedDate);
			acceptedDateFilter.Category = FilterCategories.Dates;
			acceptedDateFilter.FilterOption = DateRangeSearchTexts.Today;

			return result;
		}
	}
}
