using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class CustomsStatusDateSubGroup : ModuleFilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
		_ = subQuery
			.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration)
			.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaManifestHeaderSchema.Constants.TableName)
			.AddToFilter(filter);

		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);

		return query;
	}
}

