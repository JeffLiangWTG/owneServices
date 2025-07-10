using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

public class ImportFromTemporaryStorageRegisterFilterStripBusinessObject : FilterStripBusinessObject
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

	ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups lookups;

	public ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups Lookups => lookups ??= new ImportFromTemporaryStorageRegisterFilterStripBusinessObjectLookups(this);

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
		registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("957E3243-C491-4B4E-8B93-7318A5293C3E", Schema.RegistrationNumber);
		registrationNumberFilter.MaxLength = CusTempStorageRegHeaderSchema.SRH_Reference.MaxLength;
		registrationNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
		registrationNumberFilter.SubGroup = regHeaderFilterSubGroup;

		var customerReferenceFilter = filterCollection.AddTextFilter(Schema.CustomerReference, (comparisonOperator, value) => GetQuery(CusTempStorageRegHeaderSchema.SRH_InternalReference, comparisonOperator, value));
		customerReferenceFilter.Category = FilterCategories.NumbersAndReferences;
		customerReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("20E971F7-E6DC-4FD7-BC28-E512268DD348", Schema.CustomerReference);
		customerReferenceFilter.MaxLength = CusTempStorageRegHeaderSchema.SRH_InternalReference.MaxLength;
		customerReferenceFilter.Visibility = FilterVisibility.AlwaysVisible;
		customerReferenceFilter.SubGroup = regHeaderFilterSubGroup;
	}

	void AddLineFilters(ModuleFilterCollection filterCollection)
	{
		var regLineFilterSubGroup = new RegLineFilterSubGroup();

		var lineNumberFilter = filterCollection.AddNumberRangeFilter(Schema.LineNumber, CusTempStorageRegLineSchema.SRL_LineNumber);
		lineNumberFilter.Category = FilterCategories.NumbersAndReferences;
		lineNumberFilter.MultilingualDescription = ResString.GetMultilingualString("E9FE7EAA-9B2F-4AC0-8D26-B2B78D3D5B54", Schema.LineNumber);
		lineNumberFilter.MaxLength = CusTempStorageRegLineSchema.SRL_LineNumber.MaxLength;
		lineNumberFilter.SubGroup = regLineFilterSubGroup;
		lineNumberFilter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;

		var lineOwnerReferenceNumberFilter = filterCollection.AddTextFilter(Schema.LineOwnerReferenceNumber, (comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_OwnerReference, comparisonOperator, value));
		lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("67E90CE9-CA58-448C-95F1-A0E0A04A4ABB", Schema.LineOwnerReferenceNumber);
		lineOwnerReferenceNumberFilter.MaxLength = CusTempStorageRegLineSchema.SRL_OwnerReference.MaxLength;
		lineOwnerReferenceNumberFilter.Visibility = FilterVisibility.AlwaysVisible;
		lineOwnerReferenceNumberFilter.SubGroup = regLineFilterSubGroup;

		var lineOwnerReferenceTypeFilter = filterCollection.AddTextFilter(
			Schema.LineOwnerReferenceType,
			(comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_OwnerReferenceType, comparisonOperator, value),
			() => Lookups.OwnerReferenceTypeList);
		lineOwnerReferenceTypeFilter.Category = FilterCategories.ModesAndTypes;
		lineOwnerReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("F077180A-BC69-4225-B811-8DA03498016F", Schema.LineOwnerReferenceType);
		lineOwnerReferenceTypeFilter.MaxLength = CusTempStorageRegLineSchema.SRL_OwnerReferenceType.MaxLength;
		lineOwnerReferenceTypeFilter.SubGroup = regLineFilterSubGroup;

		var lineGoodsDescriptionFilter = filterCollection.AddTextFilter(Schema.LineGoodsDescription, (comparisonOperator, value) => GetQuery(CusTempStorageRegLineSchema.SRL_GoodsDescription, comparisonOperator, value));
		lineGoodsDescriptionFilter.Category = FilterCategories.TextSearch;
		lineGoodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("E2DBB50C-2A93-494D-A537-58D2057EC8B5", Schema.LineGoodsDescription);
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
