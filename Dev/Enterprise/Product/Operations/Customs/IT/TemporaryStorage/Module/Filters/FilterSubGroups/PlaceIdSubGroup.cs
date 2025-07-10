using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class PlaceIdSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var subQuery = new ZDBOnlySubQuery(typeof(CusGoodsLocation), CusGoodsLocationSchema.CGL_ParentID);
		subQuery.AddToFilter(filter);
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
