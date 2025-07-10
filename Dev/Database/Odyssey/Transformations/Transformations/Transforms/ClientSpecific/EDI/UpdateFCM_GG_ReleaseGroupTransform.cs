using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	class UpdateFCM_GG_ReleaseGroupTransform : DataTransformation
	{
		public override string UserDescription => "Update FCM_GG_ReleaseGroup for FeatureControlHeader Table";

		const string TableName = "FeatureControlHeader";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, TableName))
			{
				var updateColumnSql = @"
	UPDATE dbo.FeatureControlHeader
		SET [FCM_GG_ReleaseGroup] = (SELECT GG_PK FROM dbo.GlbGroup WHERE GG_Code = 'ALL'), FCM_SystemLastEditTimeUtc = GETUTCDATE(), FCM_SystemLastEditUser = 'E'
		WHERE [FCM_GG_ReleaseGroup] IS NULL;
";
				Db.Connection.ExecuteNonQuery(updateColumnSql);
			}
		}
	}
}
