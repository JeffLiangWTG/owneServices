using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class PresentationCustomsOfficeSubGroup : FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		var subQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
		subQuery.AddToFilter(CusCodeDataSchema.CY_Type, SQLComparisonOperator.Equal, CusCodeDataTypeList.Codes.OfficeCode);
		subQuery.AddToFilter(CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, EuOfficeCodesTypes.Codes.OfficeOfPresentation);
		subQuery.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, SQLComparisonOperator.Equal, AsycudaManifestHeaderSchema.Constants.Prefix);
		subQuery.AddToFilter(filter);
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
