using CargoWise.EntityFramework;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class CusTempStorageRegLineTransactionSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var regLineTransactionQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLineTransaction), CusTempStorageRegLineTransactionSchema.SRT_SRL);
		regLineTransactionQuery.AddToFilter(filter);

		var regLineQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);
		regLineQuery.AddSubQuery(CusTempStorageRegLineSchema.PK, regLineTransactionQuery, JoinCondition.And);

		var regHeaderQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
		regHeaderQuery.AddSubQuery(CusTempStorageRegHeaderSchema.PK, regLineQuery, JoinCondition.And);

		return regHeaderQuery;
	}
}
