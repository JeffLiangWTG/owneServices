using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Accounting
{
	public sealed class UpdateAccChargeTaxOverrideTransportModeEmptyValue : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Update AccChargeTaxOverride AO_TransportMode empty value with 'ALL'. Cleanup data for new constraint on column AO_TransportMode";

		protected override void OfflinePostUpgradeTransform()
		{
			var sql = @"
BEGIN TRY
	UPDATE dbo.AccChargeTaxOverride
SET AO_TransportMode = 'ALL',
	AO_SystemLastEditTimeUtc = GetUtcDate(),
	AO_SystemLastEditUser = '~BP'
WHERE AO_TransportMode = ''

DECLARE @DeletedRows table(
	AO_PK uniqueidentifier,
	AO_ParentID uniqueidentifier,
	AO_ParentTableCode VARCHAR(3),
	AO_TransportMode VARCHAR(3),
	AO_AT uniqueidentifier
);

DELETE dbo.AccChargeTaxOverride
OUTPUT deleted.AO_PK, deleted.AO_ParentID, deleted.AO_ParentTableCode, deleted.AO_TransportMode, deleted.AO_AT INTO @DeletedRows
WHERE AO_TransportMode NOT IN ('ALL', 'AIR', 'COU', 'FAS', 'FIX', 'FSA', 'IWT', 'MAI', 'OWN', 'RAI', 'ROA', 'SEA')

INSERT INTO dbo.StmALog (SL_Table, SL_Parent, SL_SE_NKEvent, SL_EventTime, SL_GS_NKUser, SL_Reference)
SELECT 'AccChargeTaxOverride', deletedRows.AO_PK, 'DEL', getDate(), '~BP',
'UpdateAccChargeTaxOverrideTransportModeEmptyValue|AO_ParentID:' + ISNULL(CONVERT(varchar(36), AO_ParentID), 'NULL') + '|AO_ParentTableCode:' + AO_ParentTableCode + '|AO_TransportMode:' + AO_TransportMode + '|AO_AT:' + ISNULL(CONVERT(varchar(36), AO_AT), 'NULL') + ''
FROM @DeletedRows deletedRows
END TRY
BEGIN CATCH
	THROW
END CATCH";

			Db.Connection.ExecuteNonQuery(sql);
		}

		public TransformationIndexProvider IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);

				indexProvider.New(AccChargeTaxOverrideSchema.Instance)
					.Key(AccChargeTaxOverrideSchema.Constants.AO_TransportMode)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_SystemLastEditTimeUtc)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_SystemLastEditUser)
					.Where("[AO_TransportMode]=''")
					.GetInfo();

				indexProvider.New(AccChargeTaxOverrideSchema.Instance)
					.Key(AccChargeTaxOverrideSchema.Constants.AO_TransportMode)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_SystemCreateTimeUtc)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_SystemLastEditTimeUtc)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_A9_DefaultVATClass)
					.Include(AccChargeTaxOverrideSchema.Constants.AO_GB)
					.Where("[AO_TransportMode]<>'ALL' AND [AO_TransportMode]<>'AIR' AND [AO_TransportMode]<>'COU' AND [AO_TransportMode]<>'FAS' AND [AO_TransportMode]<>'FIX' AND [AO_TransportMode]<>'FSA' AND [AO_TransportMode]<>'IWT' AND [AO_TransportMode]<>'MAI' AND [AO_TransportMode]<>'OWN' AND [AO_TransportMode]<>'RAI' AND [AO_TransportMode]<>'ROA' AND [AO_TransportMode]<>'SEA'")
					.GetInfo();

				return indexProvider;
			}
		}
	}
}
