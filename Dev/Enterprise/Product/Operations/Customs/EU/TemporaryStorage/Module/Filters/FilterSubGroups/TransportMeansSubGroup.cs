using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class TransportMeansSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var transportMeansQuery = new ZDBOnlySubQuery(typeof(CusTransportMeans), CusTransportMeansSchema.TPM_ParentID);
		transportMeansQuery.AddToFilter(filter);

		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, transportMeansQuery, JoinCondition.And);

		return query;
	}
}
