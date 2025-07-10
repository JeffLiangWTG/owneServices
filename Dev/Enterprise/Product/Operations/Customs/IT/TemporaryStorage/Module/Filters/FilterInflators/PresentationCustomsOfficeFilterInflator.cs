using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusCodeDataTypeList = Enterprise.Customs.EU.Business.CusCodeDataTypeList;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class PresentationCustomsOfficeFilterInflator : EU.TemporaryStorage.Module.PresentationCustomsOfficeFilterInflator
{
	public PresentationCustomsOfficeFilterInflator(FilterStripBusinessObject bizObj) : base(bizObj)
	{
	}

	public override void InflateFilter(ModuleFilterCollection filterCollection)
	{
		var customsOfficeCodeList = TemporaryStorageHeader.Lookups.CustomsOfficeCodeList;
		var presentationCustomsOfficeFilter = filterCollection.AddNkFilter(FilterDescription, GetPresentationCustomsOfficeFilterByCode, ModuleIDs.Customs.Universal.ZZRefCusCodeList, customsOfficeCodeList);
		presentationCustomsOfficeFilter.Category = FilterCategories.Locations;
		presentationCustomsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("4E4835E8-FC9B-4E42-B864-EF054E1E8533", FilterDescription);
	}

	ZQuery GetPresentationCustomsOfficeFilterByCode(SQLComparisonOperator comparisonOperator, ZString value)
	{
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		var subQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
		subQuery.AddToFilter(CusCodeDataSchema.CY_Type, SQLComparisonOperator.Equal, CusCodeDataTypeList.Codes.OfficeCode);
		subQuery.AddToFilter(CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		subQuery.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, SQLComparisonOperator.Equal, AsycudaManifestHeaderSchema.Constants.Prefix);
		subQuery.AddToFilter(CusCodeDataSchema.CY_Data, comparisonOperator, value);

		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
