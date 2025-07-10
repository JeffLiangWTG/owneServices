using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsPickTrolleyJobLastEditTimeAndUser : DataTransformation
	{
		public override string UserDescription => "Populate WTJ_SystemLastEditTimeUtc and WTJ_SystemLastEditUser to not be empty.";
		const string TriggerName = "TG_WhsPickTrolleyJob_PopulateLastEditTimeAndUser";

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsPickTrolleyJobSchema.Constants.TableName))
			{
				if (!DbObjectCreator.TriggerExists(Db.Connection, WhsPickTrolleyJobSchema.Constants.TableName, TriggerName))
				{
					Db.Connection.ExecuteNonQuery(CreateSyncTriggerSQL);
				}

				Db.Connection.ExecuteNonQuery(
@"UPDATE dbo.WhsPickTrolleyJob
SET
	WTJ_SystemLastEditTimeUtc = ISNULL(WTJ_FinalisedDateUtc, WTJ_SystemCreateTimeUtc),
	WTJ_SystemLastEditUser = WTJ_SystemCreateUser
WHERE WTJ_SystemLastEditTimeUtc IS NULL");
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsPickTrolleyJobSchema.Constants.TableName))
			{
				DbObjectCreator.DropTriggerIfExists(Db.Connection, TriggerName);
			}
		}

		const string CreateSyncTriggerSQL = @"
CREATE TRIGGER dbo.TG_WhsPickTrolleyJob_PopulateLastEditTimeAndUser
	ON dbo.WhsPickTrolleyJob
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	UPDATE wtj
	SET
		wtj.WTJ_SystemLastEditTimeUtc = GetUtcDate(),
 		wtj.WTJ_SystemLastEditUser = IIF(i.WTJ_SystemLastEditUser = '', wtj.WTJ_SystemCreateUser, i.WTJ_SystemLastEditUser)
	FROM
		dbo.WhsPickTrolleyJob wtj
		JOIN inserted i ON wtj.WTJ_PK = i.WTJ_PK
	WHERE
		wtj.WTJ_SystemLastEditTimeUtc IS NULL
END";
	}
}
