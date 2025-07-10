using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStorageRegisterLinesFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string TSDNumber = "TSD Number";
			public const string JobReference = "Job Reference";
			public const string PreviousRefType = "Previous Reference Type";
			public const string PreviousRefNumber = "Previous Reference Number";
			public const string Status = "Status";
			public const string RemainingPackagesQuantity = "Remaining Packages Quantity";
			public const string PackageType = "Packing Type";
			public const string Premises = "Premises";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			var ddtNumberTextFilter = result.AddTextFilter(Schema.TSDNumber, (comparisonOperator, filterValue) => GetHeaderFilteredQuery(CusTempStorageRegHeaderSchema.SRH_Reference, comparisonOperator, filterValue));
			ddtNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			ddtNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|TSDNumber", Schema.TSDNumber);

			var jobReferenceTextFilter = result.AddTextFilter(Schema.JobReference, (comparisonOperator, filterValue) => GetHeaderFilteredQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, comparisonOperator, filterValue));
			jobReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|JobReference", Schema.JobReference);

			var previousRefTypeTextFilter = result.AddTextFilter(Schema.PreviousRefType, (comparisonOperator, filterValue) => GetHeaderFilteredQuery(CusTempStorageRegHeaderSchema.SRH_PreviousReferenceType, comparisonOperator, filterValue), Lookups.PreviousReferenceTypeList);
			previousRefTypeTextFilter.Category = FilterCategories.NumbersAndReferences;
			previousRefTypeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|PreviousRefType", Schema.PreviousRefType);
			previousRefTypeTextFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			var previousRefNumberTextFilter = result.AddTextFilter(Schema.PreviousRefNumber, (comparisonOperator, filterValue) => GetHeaderFilteredQuery(CusTempStorageRegHeaderSchema.SRH_PreviousReference, comparisonOperator, filterValue));
			previousRefNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			previousRefNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|PreviousRefNumber", Schema.PreviousRefNumber);

			var statusTextFilter = result.AddTextFilter(Schema.Status, (comparisonOperator, filterValue) => GetHeaderFilteredQuery(CusTempStorageRegHeaderSchema.SRH_Status, comparisonOperator, filterValue), Lookups.StatusList);
			statusTextFilter.Category = FilterCategories.StatusAndFlags;
			statusTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|Status", Schema.Status);
			statusTextFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			var remainingPackagesQuantityNumberfilter = result.AddNumberRangeFilter(Schema.RemainingPackagesQuantity, GetRemainingPackagesQuantityQuery);
			remainingPackagesQuantityNumberfilter.PropertyType = ZCalcEditPropertyType.Int;
			remainingPackagesQuantityNumberfilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|RemainingPackagesQuantity", Schema.RemainingPackagesQuantity);

			var packageTypeTextFilter = result.AddTextFilter(Schema.PackageType, CusTempStorageRegLineSchema.SRL_PackageType, Lookups.PackTypeList);
			packageTypeTextFilter.Category = FilterCategories.NumbersAndReferences;
			packageTypeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|PackageType", Schema.PackageType, Lookups.PackTypeList);

			return result;
		}

		protected void AddPremisesFilter(ModuleFilterCollection filters)
		{
			PremisesModuleFilter premisesFilter = new PremisesModuleFilter(Schema.Premises, GetPremisesFilterQuery);
			premisesFilter.Category = FilterCategories.NumbersAndReferences;
			premisesFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterLinesFilter|Premises", Schema.Premises);
			filters.AddCustomFilter(premisesFilter);
		}

		ZQuery GetPremisesFilterQuery(ZQuery premisesHeaderQuery)
		{
			var headerFilter = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegLineSchema.SRL_SRH);
			headerFilter.AddToFilter(premisesHeaderQuery);

			var lineFilter = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			lineFilter.AddSubQuery(headerFilter, JoinCondition.And);
			return lineFilter;
		}

		ZQuery GetRemainingPackagesQuantityQuery(INumericZType value, INumericZType value2)
		{
			var lineFilter = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			lineFilter.AddToFilter(CusTempStorageRegLineSchema.SRL_PackagesRemaining, SQLComparisonOperator.GreaterThanOrEqualTo, value.ToZInt());
			lineFilter.AddToFilter(CusTempStorageRegLineSchema.SRL_PackagesRemaining, SQLComparisonOperator.LessThanOrEqualTo, value2.ToZInt());
			return lineFilter;
		}

		ZQuery GetHeaderFilteredQuery(SchemaStringColumn headerColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var headerFilter = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegLineSchema.SRL_SRH);
			headerFilter.AddToFilter(headerColumn, comparisonOperator, value.KeepAlphanumericCharacters());

			var lineFilter = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
			lineFilter.AddSubQuery(headerFilter, JoinCondition.And);
			return lineFilter;
		}

		CusTempStorageRegHeaderLookups Lookups => lookups ?? (lookups = GetCusTempStorageRegHeaderForLookups().Lookups);
		CusTempStorageRegHeaderLookups lookups;
		protected virtual CusTempStorageRegHeader GetCusTempStorageRegHeaderForLookups() => Factory.GetNull<CusTempStorageRegHeader>();
	}
}
