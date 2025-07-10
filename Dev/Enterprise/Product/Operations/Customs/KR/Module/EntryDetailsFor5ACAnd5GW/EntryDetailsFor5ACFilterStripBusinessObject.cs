using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ZArchitecture.Business.ModuleDateFilter;

namespace Enterprise.Customs.KR.Module
{
	public class EntryDetailsFor5ACFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Supplier = "Supplier";
			public const string SupplierCompanyName = "Supplier Company Name";
			public const string EntryCreatedDate = "Entry Created Date";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var declarationDateFilter = result.AddDateFilter(Schema.EntryCreatedDate, KREntryHeaderDetailsViewSchema.KEH_EntryCreatedLocalTime);
			declarationDateFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5ACFilterStripBusinessObject|DeclarationTime", Schema.EntryCreatedDate);
			declarationDateFilter.Category = FilterCategories.Dates;
			declarationDateFilter.FilterOption = DateRangeSearchTexts.Today;
			declarationDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			declarationDateFilter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;

			var supplierFilter = result.AddGuidFilter(Schema.Supplier, ModuleIDs.Organisation, GetSupplierQuery, new OrgHeaderCollection(Factory));
			supplierFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5ACFilterStripBusinessObject|Supplier", Schema.Supplier);
			supplierFilter.Category = FilterCategories.Organisations;

			var supplierCompanyNameFilter = result.AddTextFilter(Schema.SupplierCompanyName, KREntryHeaderDetailsViewSchema.KEH_SupplierName);
			supplierCompanyNameFilter.MultilingualDescription = ResString.GetMultilingualString("EntryDetailsFor5ACFilterStripBusinessObject|SupplierCompanyName", Schema.SupplierCompanyName);
			supplierCompanyNameFilter.Category = FilterCategories.TextSearch;
			supplierCompanyNameFilter.MaxLength = 100;
			supplierCompanyNameFilter.ComparisonOperator_List.Clear();
			supplierCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			supplierCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			supplierCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			supplierCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			supplierCompanyNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			supplierCompanyNameFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;

			return result;
		}

		ZQuery GetSupplierQuery(ZGuid value)
		{
			var header = Factory.Load<OrgHeader>(value);

			if (header != null)
			{
				return new ZQuery(KREntryHeaderDetailsViewSchema.KEH_OA_SupplierAddress, header.Addresses.Select(x => x.PK).ToArray());
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}
	}
}
