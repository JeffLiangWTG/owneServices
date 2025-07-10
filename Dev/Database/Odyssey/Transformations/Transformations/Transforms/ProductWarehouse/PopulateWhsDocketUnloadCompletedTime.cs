using System;
using System.Data;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common.HelperClasses;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsDocketUnloadCompletedTime : DataTransformation
	{
		public override string UserDescription => "Populate WhsDocket.WD_UnloadCompletedTime column.";
		const string TriggerName = "TG_WhsDocket_SetUnloadCompletedTime";
		const string LastProcessedChunkPKName = "PopulateWhsDocketUnloadCompletedTime.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsDocketSchema.Constants.TableName))
			{
				AddColumnAndTrigger();
				DoTransform();
			}
		}

		static void AddColumnAndTrigger()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_UnloadCompletedTime, "DATETIMEOFFSET(0)");

			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketSchema.Constants.TableName, TriggerName))
			{
				var createTriggerSQL = $@"
CREATE TRIGGER dbo.{TriggerName}
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WD_FinalisedDate)
	BEGIN
		UPDATE wd
		SET
			wd.WD_UnloadCompletedTime = i.WD_FinalisedDate,
			wd.WD_SystemLastEditUser = i.WD_SystemLastEditUser,
			wd.WD_SystemLastEditTimeUtc = i.WD_SystemLastEditTimeUtc
		FROM
			WhsDocket wd
			JOIN inserted i ON wd.WD_PK = i.WD_PK
		WHERE
			i.WD_DocketType = 'INW' AND
			i.WD_FinalisedDate IS NOT NULL AND
			wd.WD_UnloadCompletedTime IS NULL
	END
END";
				Db.Connection.ExecuteNonQuery(createTriggerSQL);
			}
		}

		void DoTransform()
		{
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
			var operation = new GuidChunkingOperation(manager, BatchSize, rowCount, ProcessChunk, LastProcessedChunkPKName);
			operation.DoChunking();
		}

		void ProcessChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(PopulateUnloadCompletedTimeColumnSQL()))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		string PopulateUnloadCompletedTimeColumnSQL()
		{
			return @"
UPDATE dbo.WhsDocket
SET
	WD_UnloadCompletedTime = WD_FinalisedDate,
	WD_SystemLastEditUser = '~BP',
	WD_SystemLastEditTimeUtc = GetUTCDate()
WHERE
	WD_PK >= @FromPK AND
	WD_PK <= @ToPK AND
	WD_DocketType = 'INW' AND
	WD_FinalisedDate IS NOT NULL AND
	WD_UnloadCompletedTime IS NULL";
		}
	}
}
