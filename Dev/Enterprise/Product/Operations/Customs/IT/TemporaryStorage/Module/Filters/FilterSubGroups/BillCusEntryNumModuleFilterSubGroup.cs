using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.TemporaryStorage.Module;

sealed class BillCusEntryNumModuleFilterSubGroup(string entryType) : EU.TemporaryStorage.Module.FilterSubGroup
{
	readonly string _entryType = entryType;

	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var entryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID, AsycudaBillSchema.PK);
		_ = entryNumQuery
			.AddToFilter(CusEntryNumSchema.CE_ParentTable, AsycudaBillSchema.Constants.TableName)
			.AddToFilter(CusEntryNumSchema.CE_EntryType, _entryType)
			.AddToFilter(filter);

		var billQuery = new ZDBOnlySubQuery(typeof(AsycudaBill), AsycudaBillSchema.ABL_AMA, AsycudaManifestHeaderSchema.PK);
		billQuery.AddSubQuery(entryNumQuery, JoinCondition.And);

		var result = new ZDBOnlyQuery(typeof(TemporaryStorageHeader));
		result.AddSubQuery(billQuery, JoinCondition.And);
		return result;
	}
}
