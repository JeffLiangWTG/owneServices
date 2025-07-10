using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class BillNumberSubGroup : FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var asycudaBillQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA);
		asycudaBillQuery.AddToFilter(filter);

		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, asycudaBillQuery, JoinCondition.And);

		return query;
	}
}
