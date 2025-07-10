using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class ReferenceNumberSubGroup : FilterSubGroup
{
	public ReferenceNumberSubGroup(ZString type)
	{
		this.type = type;
	}

	readonly ZString type;

	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var query = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		var subQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);

		subQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, type);
		subQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, SQLComparisonOperator.Equal, AsycudaManifestHeaderSchema.Constants.TableName);
		subQuery.AddToFilter(filter);

		query.AddSubQuery(AsycudaManifestHeaderSchema.PK, subQuery, JoinCondition.And);
		return query;
	}
}
