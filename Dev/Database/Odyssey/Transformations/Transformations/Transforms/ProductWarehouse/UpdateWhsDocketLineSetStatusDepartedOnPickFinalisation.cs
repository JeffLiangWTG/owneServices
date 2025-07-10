using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation : DataTransformation
	{
		public override string UserDescription => "Update dbo.WhsDocketLine to update existing order lines to be departed if the pick is finalised.";

		const int BatchSize = 1000;
		string TemporaryColumnName => $"{ColumnSynchroniser.ColumnRenamePrefix}WE_DocketLineStatus";
		const string WhsDocketLineSyncTriggerName = "TG_WhsDocketLine_KeepDocketLineStatusInSync";
		const string WhsPickSyncTriggerName = "TG_WhsPick_KeepDocketLineStatusInSync";
		const string LastProcessedChunkPKName = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.LastProcessedChunkPK";
		const string TransformationHasRunToCompletion = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.TransformationRunToCompletion";
		const string ExtPropertiesAndTempTriggersClearedForNewRun = "UpdateWhsDocketLineSetStatusDepartedOnPickFinalisation.TransformationDefectFix_WI00843593";

		protected override void OnlinePreUpgradeTransform()
		{
			ClearAllExtPropertiesAndOldTriggersIfRequired();

			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(WhsDocketLineSchema.Constants.SqlSchemaName, WhsDocketLineSchema.Constants.TableName), WhsDocketLineSchema.WE_DocketLineStatus.Name))
			{
				var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);
				if (statusValue != bool.TrueString)
				{
					var defaultStatus = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, WhsDocketLineSchema.Constants.SqlSchemaName, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSchema.WE_DocketLineStatus.Name);
					DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketLineSchema.Constants.TableName, TemporaryColumnName, WhsDocketLineSchema.WE_DocketLineStatus.SqlDbTypeDeclaration, defaultValue: $"'{defaultStatus}'");

					CreateSyncTriggers();

					var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
					var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
					var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsDocketSchema.Constants.TableName);
					var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

					var stopWatch = Stopwatch.StartNew();
					using (DataTransformationHelper.SuspendTriggerIfExists("TG_CheckDocketLineStatusAndDateForDocketLine", WhsDocketLineSchema.Constants.TableName))
					{
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
					}

					ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
					ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
				}
			}
		}

		void ClearAllExtPropertiesAndOldTriggersIfRequired()
		{
			var clearedFlag = ExtProperty.Database.Select(Db.Connection, ExtPropertiesAndTempTriggersClearedForNewRun);
			if (clearedFlag != bool.TrueString)
			{
				DbObjectCreator.DropTriggerIfExists(Db.Connection, WhsDocketLineSyncTriggerName);
				DbObjectCreator.DropTriggerIfExists(Db.Connection, WhsPickSyncTriggerName);
				ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
				ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);

				ExtProperty.Database.Update(Db.Connection, ExtPropertiesAndTempTriggersClearedForNewRun, bool.TrueString);
			}
		}

		void CreateSyncTriggers()
		{
			CreateWhsDocketLineSyncTrigger();
			CreateWhsPickSyncTrigger();
		}

		void CreateWhsDocketLineSyncTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketLineSchema.Constants.TableName, WhsDocketLineSyncTriggerName))
			{
				Db.Connection.ExecuteNonQuery(Invariant($@"
CREATE TRIGGER dbo.{WhsDocketLineSyncTriggerName}
	ON dbo.WhsDocketLine
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WE_DocketLineStatus)
	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE tab.WE_DocketLineStatus END,
			tab.WE_SystemLastEditTimeUtc = ISNULL(tab.WE_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WE_SystemLastEditUser = CASE WHEN tab.WE_SystemLastEditUser = '' THEN '~BP' ELSE tab.WE_SystemLastEditUser END
		FROM
			dbo.WhsDocketLine AS tab
			JOIN dbo.WhsDocket ON tab.WE_WD = WD_PK
			JOIN inserted on tab.WE_PK = inserted.WE_PK
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WE_DocketLineType = 'ORD' THEN 'DEP' ELSE tab.WE_DocketLineStatus END NewStatus
				FROM
					dbo.WhsPick
				WHERE
					WP_PK = WD_WP
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
	END
END"));
			}
		}

		void CreateWhsPickSyncTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsPickSchema.Constants.TableName, WhsPickSyncTriggerName))
			{
				Db.Connection.ExecuteNonQuery(Invariant($@"
CREATE TRIGGER dbo.{WhsPickSyncTriggerName}
	ON dbo.WhsPick
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WP_FinalizedDateUtc)
	BEGIN
		UPDATE docketLine
		SET
			docketLine.{TemporaryColumnName} = 'DEP',
			docketLine.WE_SystemLastEditTimeUtc = ISNULL(docketLine.WE_SystemLastEditTimeUtc, GetUtcDate()),
			docketLine.WE_SystemLastEditUser = CASE WHEN docketLine.WE_SystemLastEditUser = '' THEN '~BP' ELSE docketLine.WE_SystemLastEditUser END
		FROM
			dbo.WhsDocketLine AS docketLine
			JOIN dbo.WhsDocket on docketLine.WE_WD = WD_PK
			JOIN inserted on WD_WP = inserted.WP_PK
		WHERE
			inserted.WP_FinalizedDateUtc IS NOT NULL
			AND docketLine.WE_DocketLineType = 'ORD'
	END
END"));
			}
		}

		void UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(GetUpdateTempDocketLineStatusColumnSQL()))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		string GetUpdateTempDocketLineStatusColumnSQL()
		{
			return Invariant($@"
BEGIN TRY

	BEGIN TRANSACTION

	;WITH
		DATA AS (
			SELECT
				WD_PK, WD_WP
			FROM 
				dbo.WhsDocket
			WHERE 1=1
				AND WD_PK > @FromPK
				AND WD_PK <= @ToPK
			)

	UPDATE dbo.WhsDocketLine SET
		{TemporaryColumnName} = CASE WHEN NewStatus = 'DEP' THEN NewStatus ELSE WE_DocketLineStatus END,
		WE_SystemLastEditTimeUtc = CASE WHEN NewStatus = 'DEP' THEN GetUtcDate() ELSE ISNULL(WE_SystemLastEditTimeUtc, GetUtcDate()) END,
		WE_SystemLastEditUser = CASE WHEN NewStatus = 'DEP' OR WE_SystemLastEditUser = '' THEN '~BP' ELSE WE_SystemLastEditUser END
	FROM
		dbo.WhsDocketLine
		JOIN DATA ON WE_WD = WD_PK
		OUTER APPLY
		(
			SELECT
				CASE WHEN WE_DocketLineType = 'ORD' THEN 'DEP' ELSE WE_DocketLineStatus END NewStatus
			FROM
				dbo.WhsPick
			WHERE
				WD_WP = WP_PK
				AND WP_FinalizedDateUtc IS NOT NULL
		) NewStatus
		WHERE
			NewStatus = 'DEP'
			OR
			WE_DocketLineStatus <> {TemporaryColumnName}

	COMMIT TRANSACTION

END TRY
BEGIN CATCH
	IF (@@TRANCOUNT > 0)
	BEGIN
		ROLLBACK TRANSACTION;
		THROW;
	END
END CATCH
");
		}

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			// temp column won't exist if transform never ran
			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(WhsDocketLineSchema.Constants.SqlSchemaName, WhsDocketLineSchema.Constants.TableName), TemporaryColumnName))
			{
				ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
				ExtProperty.Database.Delete(Db.Connection, ExtPropertiesAndTempTriggersClearedForNewRun);
			}
		}
	}
}
