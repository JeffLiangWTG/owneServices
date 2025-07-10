using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	class UpdateESCServiceTaskIsActiveToFalse : DataTransformation
	{
		public override string UserDescription => "Update EscrowExportSourceCodeServiceTask IsActive to false";

		protected override void OfflinePostUpgradeTransform()
		{
			var sqlText = @"
UPDATE dbo.StmScheduleTask
SET
	S5_IsActive = 0,
	S5_SystemLastEditTimeUtc = GetUtcDate(),
	S5_SystemLastEditUser = '~BP'
WHERE S5_ScheduleType = 'ESC'
	AND S5_ParentTableCode = 'SH'
	AND S5_IsActive = 1

UPDATE dbo.StmServiceTask
SET
	SST_Active = 0,
	SST_SystemLastEditTimeUtc = GetUtcDate(),
	SST_SystemLastEditUser = '~BP'
WHERE SST_ServiceTaskCode = 'ESC' AND SST_Active = 1
";

			Db.Connection.ExecuteNonQuery(sqlText);
		}
	}
}
