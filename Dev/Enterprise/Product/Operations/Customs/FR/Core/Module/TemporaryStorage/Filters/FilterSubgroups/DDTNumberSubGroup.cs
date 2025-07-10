using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Module;

sealed class DDTNumberSubGroup : EU.TemporaryStorage.Module.FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var query = new ZDBOnlyQuery(typeof(CusTempStorageJobHeader));
		var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.France.DDT);
		subQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.Country.Code);
		subQuery.AddToFilter(filter);
		query.AddSubQuery(CusTempStorageJobHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
