using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Workflow
{
	internal class UpdateInvalidRQT_JobType : DataTransformation
	{
		public override string UserDescription => "Update Invalid RQT_JobType";

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, ExternalRequestTypeSchema.Constants.TableName) && DbObjectCreator.ColumnExists(Db.Connection, ExternalRequestTypeSchema.Constants.TableName, ExternalRequestTypeSchema.Constants.RQT_JobType))
			{
				Db.Connection.ExecuteNonQuery("UPDATE dbo.ExternalRequestType SET RQT_JobType = 'ALL', RQT_SystemLastEditTimeUtc = GETUTCDATE(), RQT_SystemLastEditUser = '~BP' WHERE RQT_JobType IS NULL OR RQT_JobType NOT IN ('ALL', 'ORD', 'ORL', 'SBK', 'SBL', 'CLH', 'CLI', 'CLP', 'CPL', 'SHP', 'SPL', 'CON')");
			}
		}
	}
}
