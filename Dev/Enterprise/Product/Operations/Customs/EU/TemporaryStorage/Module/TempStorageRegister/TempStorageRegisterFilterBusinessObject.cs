using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module
{
	public class TempStorageRegisterFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string DDTNumber = "TSD Number";
			public const string JobReference = "Job Reference";
			public const string ArrivalDate = "Arrival Date";
			public const string PresentationDate = "Presentation Date";
			public const string PreviousRefType = "Previous Reference Type";
			public const string PreviousRefNumber = "Previous Reference Number";
			public const string Status = "Status";
			public const string RemainingPackagesQuantity = "Remaining Packages Quantity";
			public const string CustomsOffice = "Customs Office";
			public const string PackageType = "Packing Type";
			public const string Premises = "Premises";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddReferenceFilter(result);

			var jobReferenceTextFilter = result.AddTextFilter(Schema.JobReference, CusTempStorageRegHeaderSchema.SRH_InternalReference);
			jobReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
			jobReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|JobReference", Schema.JobReference);

			var arrivalDateDateFilter = result.AddDateFilter(Schema.ArrivalDate, CusTempStorageRegHeaderSchema.SRH_ArrivalDate);
			arrivalDateDateFilter.Category = FilterCategories.Dates;
			arrivalDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|ArrivalDate", Schema.ArrivalDate);

			var presentationDateDateFilter = result.AddDateFilter(Schema.PresentationDate, CusTempStorageRegHeaderSchema.SRH_PresentationDate);
			presentationDateDateFilter.Category = FilterCategories.Dates;
			presentationDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|PresentationDate", Schema.PresentationDate);

			AddPreviousReferenceTypeFilter(result);
			AddPreviousReferenceFilter(result);

			var statusTextFilter = result.AddTextFilter(Schema.Status, CusTempStorageRegHeaderSchema.SRH_Status, Lookups.StatusList);
			statusTextFilter.Category = FilterCategories.StatusAndFlags;
			statusTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|Status", Schema.Status);
			statusTextFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			AddRemainingPackagesQuantityFilter(result);

			var customsOfficeTextFilter = result.AddNkFilter(Schema.CustomsOffice, CusTempStorageRegHeaderSchema.SRH_CustomsOffice, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOfficeList);
			customsOfficeTextFilter.Category = FilterCategories.NumbersAndReferences;
			customsOfficeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|CustomsOffice", Schema.CustomsOffice);

			AddPackageTypeFilter(result);

			return result;
		}

		protected virtual void AddReferenceFilter(ModuleFilterCollection filters)
		{
			var ddtNumberTextFilter = filters.AddTextFilter(Schema.DDTNumber, CusTempStorageRegHeaderSchema.SRH_Reference);
			ddtNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			ddtNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|DDTNumber", Schema.DDTNumber);
		}

		protected virtual void AddPreviousReferenceFilter(ModuleFilterCollection filters)
		{
			var previousRefNumberTextFilter = filters.AddTextFilter(Schema.PreviousRefNumber, CusTempStorageRegHeaderSchema.SRH_PreviousReference);
			previousRefNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			previousRefNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|PreviousRefNumber", Schema.PreviousRefNumber);
		}

		protected virtual void AddPreviousReferenceTypeFilter(ModuleFilterCollection filters)
		{
			var previousRefTypeTextFilter = filters.AddTextFilter(Schema.PreviousRefType, CusTempStorageRegHeaderSchema.SRH_PreviousReferenceType, Lookups.PreviousReferenceTypeList);
			previousRefTypeTextFilter.Category = FilterCategories.NumbersAndReferences;
			previousRefTypeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|PreviousRefType", Schema.PreviousRefType);
			previousRefTypeTextFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
		}

		protected virtual void AddRemainingPackagesQuantityFilter(ModuleFilterCollection filters)
		{
			var remainingPackagesQuantityNumberfilter = filters.AddNumberRangeFilter(Schema.RemainingPackagesQuantity, GetRemainingPackagesQuantityQuery);
			remainingPackagesQuantityNumberfilter.PropertyType = ZCalcEditPropertyType.Int;
			remainingPackagesQuantityNumberfilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|RemainingPackagesQuantity", Schema.RemainingPackagesQuantity);
		}

		protected virtual void AddPackageTypeFilter(ModuleFilterCollection filters)
		{
			var packageTypeTextFilter = filters.AddTextFilter(Schema.PackageType, GetPackageTypeQuery, Lookups.PackTypeList);
			packageTypeTextFilter.Category = FilterCategories.NumbersAndReferences;
			packageTypeTextFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|PackageType", Schema.PackageType, Lookups.PackTypeList);
		}

		ZQuery GetRemainingPackagesQuantityQuery(INumericZType value, INumericZType value2)
		{
			var result = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));

			var additionalSql = FormattableString.Invariant($@"{CusTempStorageRegHeaderSchema.Constants.PK} in (
									SELECT {CusTempStorageRegHeaderSchema.Constants.PK} FROM {CusTempStorageRegHeaderSchema.Constants.SqlSchemaName}.{CusTempStorageRegHeaderSchema.Constants.TableName}
									INNER JOIN {CusTempStorageRegLineSchema.Constants.SqlSchemaName}.{CusTempStorageRegLineSchema.Constants.TableName} ON {CusTempStorageRegLineSchema.Constants.SRL_SRH} = {CusTempStorageRegHeaderSchema.Constants.PK} 
									GROUP BY {CusTempStorageRegHeaderSchema.Constants.PK}
									HAVING SUM({CusTempStorageRegLineSchema.Constants.SRL_PackagesRemaining}) BETWEEN {value} AND {value2})");

			result.AddFilterAndZSQLParameterCollection(additionalSql, null);

			return result;
		}

		ZQuery GetPackageTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));

			var additionalSql = FormattableString.Invariant($@"{CusTempStorageRegHeaderSchema.Constants.PK} in (
									SELECT {CusTempStorageRegHeaderSchema.Constants.PK} FROM {CusTempStorageRegHeaderSchema.Constants.SqlSchemaName}.{CusTempStorageRegHeaderSchema.Constants.TableName}
									INNER JOIN {CusTempStorageRegLineSchema.Constants.SqlSchemaName}.{CusTempStorageRegLineSchema.Constants.TableName} ON {CusTempStorageRegLineSchema.Constants.SRL_SRH} = {CusTempStorageRegHeaderSchema.Constants.PK} 
									WHERE {CusTempStorageRegLineSchema.Constants.SRL_PackageType} = '{value}')");

			result.AddFilterAndZSQLParameterCollection(additionalSql, null);

			return result;
		}

		protected void AddPremisesFilter(ModuleFilterCollection filters)
		{
			PremisesModuleFilter premisesFilter = new PremisesModuleFilter(Schema.Premises);
			premisesFilter.Category = FilterCategories.NumbersAndReferences;
			premisesFilter.MultilingualDescription = ResString.GetMultilingualString("TempStorageRegisterFilter|Premises", Schema.Premises);
			filters.AddCustomFilter(premisesFilter);
		}

		CusTempStorageRegHeaderLookups Lookups => lookups ?? (lookups = GetCusTempStorageRegHeaderForLookups().Lookups);
		CusTempStorageRegHeaderLookups lookups;

		protected virtual CusTempStorageRegHeader GetCusTempStorageRegHeaderForLookups() => Factory.GetNull<CusTempStorageRegHeader>();
	}
}
