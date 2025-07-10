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
	public class PopulateWhsPickFinalizedByColumn : DataTransformation
	{
		public override string UserDescription => "Populate WhsPick.WP_GS_NKFinalizedBy column.";
		const string TriggerName = "TG_WhsPick_SetFinalizedBy";
		const string LastProcessedChunkPKName = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "PopulateWhsPickFinalizedByColumn.WP_GS_NKFinalizedBy.TransformationRunToCompletion";
		const int BatchSize = 1000;

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();
			ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
		}

		protected override void OnlinePreUpgradeTransform()
		{
			var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);

			if (statusValue != bool.TrueString && DbObjectCreator.TableExists(Db.Connection, WhsPickSchema.Constants.TableName))
			{
				AddColumnAndTrigger();
				DoTransform();
			}
		}

		static void AddColumnAndTrigger()
		{
			var defaultFinalizedBy = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, WhsPickSchema.Constants.SqlSchemaName, WhsPickSchema.Constants.TableName, WhsPickSchema.WP_SystemLastEditUser.Name);
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsPickSchema.Constants.TableName, WhsPickSchema.Constants.WP_GS_NKFinalizedBy, "varchar(3)", $"'{defaultFinalizedBy}'");

			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsPickSchema.Constants.TableName, TriggerName))
			{
				Db.Connection.ExecuteNonQuery(CreateTriggerSQL);
			}
		}

		void DoTransform()
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsPickSchema.Constants.TableName);
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
			ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(PopulateFinalizedByColumnSQL()))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1119:Do Not Use SL_EventTime Table Column", Justification = "Using SL_EventTime for sorting to use its index")]
		string PopulateFinalizedByColumnSQL()
		{
			return $@"
;WITH WhsPickData AS
(
	SELECT
		FinalizedByUserToSet.UserCode,
		WhsPick.*
	FROM 
		WhsPick
		OUTER APPLY
		(
			SELECT TOP 1
				SL_GS_NKUser
			FROM
				StmALog WITH (FORCESEEK INDEX([NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime])) 
			WHERE
				SL_Parent = WP_PK AND
				SL_SE_NKEvent = 'FIN' AND
				SL_GS_NKUser != ''
			ORDER BY
				SL_EventTime DESC
		) FinalizedLogUser
		CROSS APPLY
		(
			SELECT IIF(WP_FinalizedDateUtc IS NOT NULL, ISNULL(SL_GS_NKUser, WP_SystemLastEditUser), '') as UserCode
		) FinalizedByUserToSet
	WHERE
		WP_PK >= @FromPK AND
		WP_PK <= @ToPK AND
		WP_GS_NKFinalizedBy != FinalizedByUserToSet.UserCode
)

UPDATE
	WhsPickData
SET
	WP_GS_NKFinalizedBy = UserCode,
	WP_SystemLastEditUser = WP_SystemLastEditUser,
	WP_SystemLastEditTimeUtc = WP_SystemLastEditTimeUtc";
		}

		const string CreateTriggerSQL = @"
CREATE TRIGGER dbo.TG_WhsPick_SetFinalizedBy
	ON dbo.WhsPick
	FOR INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;
	IF UPDATE (WP_FinalizedDateUtc)
	BEGIN
		UPDATE wp
		SET
			wp.WP_GS_NKFinalizedBy = IIF(i.WP_FinalizedDateUtc IS NOT NULL, i.WP_SystemLastEditUser, ''),
			wp.WP_SystemLastEditUser = i.WP_SystemLastEditUser,
			wp.WP_SystemLastEditTimeUtc = i.WP_SystemLastEditTimeUtc
		FROM
			WhsPick wp
			JOIN inserted i ON wp.WP_PK = i.WP_PK
	END
END";
	}
}
