using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class ContainerSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var cusCodeDataQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
		cusCodeDataQuery
			.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusTempStorageRegLineSchema.Constants.Prefix)
			.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.TemporaryStorageContainer)
			.AddToFilter(filter);

		var regLineQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);
		regLineQuery.AddSubQuery(CusTempStorageRegLineSchema.PK, cusCodeDataQuery, JoinCondition.And);

		var regHeaderQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
		regHeaderQuery.AddSubQuery(CusTempStorageRegHeaderSchema.PK, regLineQuery, JoinCondition.And);

		return regHeaderQuery;
	}
}
