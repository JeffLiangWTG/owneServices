using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.Customs.KR.Module
{
	public class EntryCustomsBillsFor5ULFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Payer = "Payer";
			public const string EntryNum = "Entry Number";
			public const string AcceptedDate = "Accepted Date";
			public const string PaymentDate = "Payment Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var payerFilter = result.AddGuidFilter(Schema.Payer, ModuleIDs.Organisation, KREntryCustomsBillsViewSchema.KEB_OH_Importer, new ConsigneeCollection(Factory));
			payerFilter.MultilingualDescription = ResString.GetMultilingualString("EntryCustomsBillsFor5ULFilterStripBusinessObject|Payer", Schema.Payer);
			payerFilter.Category = FilterCategories.Organisations;
			payerFilter.ComparisonOperator_List.Clear();
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			payerFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));
			payerFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;

			var importEntryNumberFilter = result.AddNumberFilter(Schema.EntryNum, KREntryCustomsBillsViewSchema.KEB_ImportEntryNum);
			importEntryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("EntryCustomsBillsFor5ULFilterStripBusinessObject|EntryNum", Schema.EntryNum);
			importEntryNumberFilter.Category = FilterCategories.NumbersAndReferences;
			importEntryNumberFilter.ComparisonOperator_List.Clear();
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotStartsWith));
			importEntryNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotContain));
			importEntryNumberFilter.ComparisonOperator_List.DefaultCode = ModuleNumberFilter.ComparisonConstants.StartsWith;

			var acceptedDateFilter = result.AddDateFilter(Schema.AcceptedDate, KREntryCustomsBillsViewSchema.KEB_ImportIssueDate);
			acceptedDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryCustomsBillsFor5ULFilterStripBusinessObject|AcceptedDate", Schema.AcceptedDate);
			acceptedDateFilter.Category = FilterCategories.Dates;
			acceptedDateFilter.FilterOption = DateRangeSearchTexts.Today;

			var paymentDateFilter = result.AddDateFilter(Schema.PaymentDate, KREntryCustomsBillsViewSchema.KEB_PaymentAuthorizationDate);
			paymentDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryCustomsBillsFor5ULFilterStripBusinessObject|PaymentDate", Schema.PaymentDate);
			paymentDateFilter.Category = FilterCategories.Dates;
			paymentDateFilter.FilterOption = DateRangeSearchTexts.Today;

			return result;
		}
	}
}
