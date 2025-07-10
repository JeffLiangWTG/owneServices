using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class OrganisationSubGroup(SchemaColumn fieldName) : EU.TemporaryStorage.Module.FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var zDBOnlySubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
		_ = zDBOnlySubQuery.AddToFilter(filter);

		var zDBOnlyQuery = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		zDBOnlyQuery.AddSubQuery(fieldName, zDBOnlySubQuery, JoinCondition.And);

		return zDBOnlyQuery;
	}
}
