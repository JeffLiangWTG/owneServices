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
	public class PopulateWhsDocketStartedReceivingTimeUtcColumn : DataTransformation
	{
		public override string UserDescription => "Populate WhsDocket.WD_StartedReceivingTimeUtc column.";
		const string TriggerName = "TG_WhsDocket_StartedReceivingTimeUtc";
		const string LastProcessedChunkPKName = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "PopulateWhsDocketStartedReceivingTimeUtcColumn.WD_StartedReceivingTimeUtc.TransformationRunToCompletion";
		const int BatchSize = 1000;

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();
			ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
		}

		protected override void OnlinePreUpgradeTransform()
		{
			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);

			if (statusValue != bool.TrueString && DbObjectCreator.ColumnExists(Db.Connection, WhsDocketSchema.Constants.TableName, "WD_StartedReceiving"))
			{
				AddColumnAndTrigger();
				DoTransform();
			}
		}

		void AddColumnAndTrigger()
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSchema.Constants.WD_StartedReceivingTimeUtc, "SMALLDATETIME");

			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketSchema.Constants.TableName, TriggerName))
			{
				Db.Connection.ExecuteNonQuery(CreateTriggerSQL);
			}
		}

		void DoTransform()
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var totalDockets = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, totalDockets, lastProcessedPK);

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
			ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(PopulateColumnSQL))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		const string PopulateColumnSQL = @"
;WITH WhsDocketData AS
(
	SELECT
		COALESCE(AsnLines.AsnCreateTime, CAST(SWITCHOFFSET(WD_ArrivalDate, '+00:00') AS SMALLDATETIME), CAST(SWITCHOFFSET(WD_BookingDate, '+00:00') AS SMALLDATETIME)) AS StartedReceivingTimeUtc,
		WhsDocket.WD_SystemLastEditUser,
		WhsDocket.WD_SystemLastEditTimeUtc,
		WhsDocket.WD_StartedReceivingTimeUtc
	FROM
		WhsDocket
		OUTER APPLY
		(
			SELECT
				MIN(wal.WN_SystemCreateTimeUtc) AS AsnCreateTime
			FROM
				dbo.WhsAsnLine wal
			WHERE
				WD_PK = wal.WN_WD
		) AsnLines
	WHERE
		WD_PK >= @FromPK AND
		WD_PK <= @ToPK AND
		WD_StartedReceiving = 1 AND WD_DocketType = 'INW' AND WD_StartedReceivingTimeUtc IS NULL
)

UPDATE
	WhsDocketData
SET
	WD_StartedReceivingTimeUtc = StartedReceivingTimeUtc,
	WD_SystemLastEditUser = '~BP',
	WD_SystemLastEditTimeUtc = GetUtcDate()
OPTION (MAXDOP 1)";

		const string CreateTriggerSQL = @"
CREATE TRIGGER dbo.TG_WhsDocket_StartedReceivingTimeUtc
	ON dbo.WhsDocket
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WD_StartedReceiving)
	BEGIN
		UPDATE wd
		SET
			wd.WD_StartedReceivingTimeUtc = CASE WHEN i.WD_StartedReceiving = 1 THEN GetUtcDate() ELSE NULL END,
			wd.WD_SystemLastEditUser = wd.WD_SystemLastEditUser,
			wd.WD_SystemLastEditTimeUtc = wd.WD_SystemLastEditTimeUtc
		FROM
			WhsDocket wd
			JOIN inserted i ON wd.WD_PK = i.WD_PK
		WHERE
			i.WD_DocketType = 'INW'
	END
END";
	}
}
