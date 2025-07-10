using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class ImportFromSumARegisterFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string RegistrationNumber = "Registration Number";
			public const string CustomerReference = "Customer Reference";
			public const string LineNumber = "Line #";
			public const string LineOwnerReferenceNumber = "Owner Reference Number";
			public const string LineOwnerReferenceType = "Owner Reference Type";
			public const string LineGoodsDescription = "Goods Description";
		}

		ImportFromSumARegisterFilterStripBusinessObjectLookups lookups;

		public ImportFromSumARegisterFilterStripBusinessObjectLookups Lookups => lookups ?? (lookups = new ImportFromSumARegisterFilterStripBusinessObjectLookups(this));

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddHeaderFilters(result);
			AddLineFilters(result);
			return result;
		}

		void AddHeaderFilters(ModuleFilterCollection filterCollection)
		{
			var regHeaderFilterSubGroup = new RegHeaderFilterSubGroup();

			var registrationNumberFilter = filterCollection.AddTextFilter(Schema.RegistrationNumber, (comparisonOperator, value) => GetQuery(CusTempStorageRegHeaderSchema.SRH_Reference, comparisonOperator, value));
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("48CC3625-7178-492F-A84B-2D8B0BC96882", Schema.RegistrationNumber);
			registrationNumberFilter.MaxLength = CusTempStorageRegHeaderSchema.SRH_Reference.MaxLength;
			registrationNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
			registrationNumberFilter.SubGroup = regHeaderFilterSubGroup;

			var customerReferenceFilter = filterCollection.AddTextFilter(Schema.CustomerReference, (comparisonOperator, value) => GetQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, comparisonOperator, value));
			customerReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			customerReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("FCC4C2CF-EF41-41AA-94F5-92F6743FD95C", Schema.CustomerReference);
			customerReferenceFilter.MaxLength = CusTempStorageRegHeaderSchema.SRH_InternalReference.MaxLength;
			customerReferenceFilter.Visibility = FilterVisibility.AlwaysVisible;
			customerReferenceFilter.SubGroup = regHeaderFilterSubGroup;
		}

		void AddLineFilters(ModuleFilterCollection filterCollection)
		{
			var regLineFilterSubGroup = new RegLineFilterSubGroup();

			var lineNumberFilter = filterCollection.AddNumberRangeFilter(Schema.LineNumber, CusTempStorageRegLineSchema.SRL_LineNumber);
			lineNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineNumberFilter.MultilingualDescription = ResString.GetMultilingualString("F4879A36-2A27-4EEE-8881-DF9D08DFFDA2", Schema.LineNumber);
			lineNumberFilter.MaxLength = CusTempStorageRegLineSchema.SRL_LineNumber.MaxLength;
			lineNumberFilter.SubGroup = regLineFilterSubGroup;
			lineNumberFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;

			var lineOwnerReferenceNumberFilter = filterCollection.AddTextFilter(Schema.LineOwnerReferenceNumber, (comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_OwnerReference, comparisonOperator, value));
			lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AEBE2CFD-EAA3-4652-8E20-BD4DC1A3F9E0", Schema.LineOwnerReferenceNumber);
			lineOwnerReferenceNumberFilter.MaxLength = CusTempStorageRegLineSchema.SRL_OwnerReference.MaxLength;
			lineOwnerReferenceNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
			lineOwnerReferenceNumberFilter.SubGroup = regLineFilterSubGroup;

			var lineOwnerReferenceTypeFilter = filterCollection.AddTextFilter(
				Schema.LineOwnerReferenceType,
				(comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_OwnerReferenceType, comparisonOperator, value),
				() => Lookups.OwnerReferenceTypeList);
			lineOwnerReferenceTypeFilter.Category = FilterCategories.ModesAndTypes;
			lineOwnerReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("3269D435-1C35-471E-AC8B-7ACA4421A0A2", Schema.LineOwnerReferenceType);
			lineOwnerReferenceTypeFilter.MaxLength = CusTempStorageRegLineSchema.SRL_OwnerReferenceType.MaxLength;
			lineOwnerReferenceTypeFilter.SubGroup = regLineFilterSubGroup;

			var lineGoodsDescriptionFilter = filterCollection.AddTextFilter(Schema.LineGoodsDescription, (comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_GoodsDescription, comparisonOperator, value));
			lineGoodsDescriptionFilter.Category = FilterCategories.TextSearch;
			lineGoodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("AE39F734-D49F-41EE-A8D2-845556231FDE", Schema.LineGoodsDescription);
			lineGoodsDescriptionFilter.MaxLength = CusTempStorageRegLineSchema.SRL_GoodsDescription.MaxLength;
			lineGoodsDescriptionFilter.SubGroup = regLineFilterSubGroup;
		}

		ZQuery GetQuery(CargoWise.Schema.SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value) => new ZQuery(column, comparisonOperator, value);

		class RegHeaderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
				var regHeaderQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegHeader), CusTempStorageRegHeaderSchema.PK);
				regHeaderQuery.AddToFilter(filter);
				result.AddSubQuery(CusTempStorageRegLineSchema.SRL_SRH, regHeaderQuery, JoinCondition.And);
				return result;
			}
		}

		class RegLineFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
				lineQuery.AddToFilter(filter);
				return lineQuery;
			}
		}
	}
}
