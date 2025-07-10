using CargoWise.EntityFramework;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class AuthorizationNumberSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var subGroup = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
		subGroup.AddToFilter(filter);
		var subQuery = new ZDBOnlySubQuery(typeof(CusGoodsLocation), CusGoodsLocationSchema.CGL_ParentID);
		subQuery.AddSubQuery(CusGoodsLocationSchema.PK, subGroup, JoinCondition.And);
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
