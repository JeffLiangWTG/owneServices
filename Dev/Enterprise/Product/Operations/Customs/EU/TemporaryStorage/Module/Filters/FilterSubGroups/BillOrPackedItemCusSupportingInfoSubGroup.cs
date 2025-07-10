using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class BillOrPackedItemCusSupportingInfoSubGroup(string filterType) : FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var packedItemQuery = GetCusSupportingInfoPackedItemQuery(filter, filterType);
		var billQuery = GetCusSupportingInfoBillQuery(filter, filterType);
		billQuery.AddSubQuery(AsycudaBillSchema.PK, packedItemQuery, JoinCondition.Or);

		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, billQuery, JoinCondition.And);

		return query;
	}
}
