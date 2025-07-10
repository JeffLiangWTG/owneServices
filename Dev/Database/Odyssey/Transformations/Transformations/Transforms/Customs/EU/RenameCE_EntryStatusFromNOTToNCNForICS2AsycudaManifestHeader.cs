using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;

sealed class RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeader : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Rename CE_EntryStatus from NOT to NCN for ICS2 AsycudaManifestHeader";

	public TransformationIndexProvider IndexProvider => BuildIndexProvider();

	TransformationIndexProvider BuildIndexProvider()
	{
		var indexInfo = IndexInfo.Builder
			.New(Db.SqlDbOwnerSchema, "CusEntryNum", "IX_RenameCE_EntryStatusFromNOTToNCNForICS2AsycudaManifestHeader")
			.Key(CusEntryNumSchema.CE_ParentTable.Name, CusEntryNumSchema.CE_EntryType.Name, CusEntryNumSchema.CE_EntryStatus.Name)
			.Include(CusEntryNumSchema.CE_SystemLastEditTimeUtc.Name, CusEntryNumSchema.CE_SystemLastEditUser.Name)
			.Where("[CE_ParentTable]='AsycudaManifestHeader' AND [CE_EntryType]='ASY' AND [CE_EntryStatus]='NOT'")
			.GetInfo();

		return new TransformationIndexProvider(this) { indexInfo.Yield() };
	}

	protected override void OfflinePostUpgradeTransform()
	{
		const string sql = @"
	UPDATE dbo.CusEntryNum
	SET
		CE_EntryStatus = 'NCN',
		CE_SystemLastEditTimeUtc = GETUTCDATE(),
		CE_SystemLastEditUser = 'E'
	FROM dbo.CusEntryNum
	INNER JOIN dbo.AsycudaManifestHeader ON CE_ParentID = AMA_PK
	WHERE AMA_ManifestType = 'ENS' AND CE_ParentTable = 'AsycudaManifestHeader' AND CE_EntryType = 'ASY' AND CE_EntryStatus = 'NOT';
";

		Db.Connection.ExecuteNonQuery(sql);
	}
}
