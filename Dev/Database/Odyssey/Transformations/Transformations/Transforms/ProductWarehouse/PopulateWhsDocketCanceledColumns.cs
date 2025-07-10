using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class PopulateWhsDocketCanceledColumns : DataTransformation
	{
		public override string UserDescription => "Populate WhsDocket WD_CanceledTimeUtc and WD_GS_NKCanceledBy columns.";
		const string TriggerName = "TG_WhsDocket_SetCanceledColumns";
		const string LastProcessedChunkPKName = "PopulateWhsDocketCanceledColumns.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			if (DbObjectCreator.TableExists(Db.Connection, WhsDocketSchema.Constants.TableName))
			{
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_GS_NKCanceledBy, "varchar(3)", "''");
				DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_CanceledTimeUtc, nameof(SqlDbType.SmallDateTime));

				if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketSchema.Constants.TableName, TriggerName))
				{
					Db.Connection.ExecuteNonQuery(CreateTriggerSQL);
				}
				var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
				var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
				var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
				var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

				var stopWatch = Stopwatch.StartNew();
				foreach (var chunk in chunks)
				{
					UpdateChunk(chunk.LowerBound, chunk.UpperBound);

					if (stopWatch.Elapsed.TotalMinutes > 1)
					{
						lastProcessedChunkPKString = chunk.UpperBound.ToString();
						ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPKString);

						stopWatch.Restart();
					}
				}

				ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
			}
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(PopulateCanceledColumnsSQL))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1119:Do Not Use SL_EventTime Table Column", Justification = "Using SL_EventTime for sorting to use its index")]
		const string PopulateCanceledColumnsSQL = @"
;WITH WhsDocketData AS
(
	SELECT
		CanceledValuesToSet.CanceledUserCode,
		CanceledValuesToSet.CanceledTime,
		WhsDocket.WD_GS_NKCanceledBy,
		WhsDocket.WD_CanceledTimeUtc,
		WhsDocket.WD_SystemLastEditUser,
		WhsDocket.WD_SystemLastEditTimeUtc
	FROM 
		WhsDocket
		OUTER APPLY
		(
			SELECT TOP 1
				SL_GS_NKUser,
				SL_EventTime
			FROM
				StmALog WITH (FORCESEEK INDEX([NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime])) 
			WHERE
				SL_Parent = WD_PK AND
				SL_SE_NKEvent = 'INA' AND
				SL_GS_NKUser != ''
			ORDER BY
				SL_EventTime DESC
		) CanceledLog
		CROSS APPLY
		(
			SELECT
				COALESCE(SL_GS_NKUser, WD_SystemLastEditUser, '~BP') as CanceledUserCode,
				COALESCE(SL_EventTime, WD_SystemLastEditTimeUtc, GetUtcDate()) as CanceledTime
		) CanceledValuesToSet
	WHERE
		WD_PK >= @FromPK AND
		WD_PK <= @ToPK AND
		WD_DocketStatus = 'CAN' AND
		(WD_CanceledTimeUtc IS NULL OR WD_GS_NKCanceledBy = '')
)

UPDATE
	WhsDocketData
SET
	WD_GS_NKCanceledBy = CanceledUserCode,
	WD_CanceledTimeUtc = CanceledTime,
	WD_SystemLastEditUser = '~BP',
	WD_SystemLastEditTimeUtc = GetUtcDate()";

		const string CreateTriggerSQL = @"
CREATE TRIGGER dbo.TG_WhsDocket_SetCanceledColumns
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WD_DocketStatus)
	BEGIN
		UPDATE wd
		SET
			wd.WD_GS_NKCanceledBy = CASE WHEN i.WD_DocketStatus = 'CAN' THEN i.WD_SystemLastEditUser ELSE '' END,
			wd.WD_CanceledTimeUtc = CASE WHEN i.WD_DocketStatus = 'CAN' THEN i.WD_SystemLastEditTimeUtc ELSE NULL END,
			wd.WD_SystemLastEditUser = i.WD_SystemLastEditUser,
			wd.WD_SystemLastEditTimeUtc = i.WD_SystemLastEditTimeUtc
		FROM
			WhsDocket wd
			JOIN inserted i ON wd.WD_PK = i.WD_PK
	END
END";
	}
}
