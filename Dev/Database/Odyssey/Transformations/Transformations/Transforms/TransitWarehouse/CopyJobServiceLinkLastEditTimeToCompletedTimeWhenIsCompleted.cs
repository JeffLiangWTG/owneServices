using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse
{
	public class CopyJobServiceLinkLastEditTimeToCompletedTimeWhenIsCompleted : DataTransformation
	{
		public override string UserDescription => $"Copying dbo.JobServiceLink LastEditTimeUtc to CompletedTime when IsCompleted is true.";

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.ColumnExists(Db.Connection, JobServiceLinkSchema.Constants.TableName, "ESL_IsCompleted"))
			{
				if (!DbObjectCreator.ColumnExists(Db.Connection, JobServiceLinkSchema.Constants.TableName, JobServiceLinkSchema.Constants.ESL_CompletedTime))
				{
					Db.Connection.ExecuteNonQuery("ALTER TABLE dbo.JobServiceLink ADD ESL_CompletedTime DATETIMEOFFSET(0)");
				}

				Db.Connection.ExecuteNonQuery(@"
UPDATE dbo.JobServiceLink
SET
	ESL_CompletedTime = ESL_SystemLastEditTimeUtc,
	ESL_SystemLastEditTimeUtc = GetUtcDate(),
	ESL_SystemLastEditUser = '~BP'
WHERE
	ESL_IsCompleted = 1 AND ESL_CompletedTime IS NULL");
			}
		}
	}
}
