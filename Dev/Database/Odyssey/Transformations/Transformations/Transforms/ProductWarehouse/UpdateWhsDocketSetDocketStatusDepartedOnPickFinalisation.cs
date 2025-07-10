using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation : DataTransformation
	{
		public override string UserDescription => "Update dbo.WhsDocket to fix existing orders to become DEP if the pick is finalised.";

		const int BatchSize = 1000;
		const string OldTemporaryColumnName = "_ODS_WD_DocketStatus";
		string TemporaryColumnName => $"{ColumnSynchroniser.ColumnRenamePrefix}WD_DocketStatus";
		const string WhsDocketSyncTriggerName = "TG_WhsDocket_KeepWD_DocketStatusInSync";
		const string WhsPickSyncTriggerName = "TG_WhsPick_KeepWD_DocketStatusInSync";

		const string StoredFromPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.FromPK";
		const string StoredToPKName = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.ToPK";
		const string TransformationHasRunToCompletion = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.TransformationRunToCompletion";

		const string ExtPropertiesClearedForNewRun = "UpdateWhsDocketSetDocketStatusDepartedOnPickFinalisation.WD_DocketStatus.TransformationDefectFix_WI00748138";

		const string DepartedStatus = "DEP";

		protected override void OnlinePreUpgradeTransform()
		{
			ClearAllExtPropertiesAndOldColumnIfRequired();

			// If the original column does not exist we cannot transform anything.
			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName), WhsDocketSchema.WD_DocketStatus.Name))
			{
				var statusValue = ExtProperty.Database.Select(Db.Connection, TransformationHasRunToCompletion);
				if (statusValue != bool.TrueString)
				{
					// Use most common docket status as default value
					var defaultStatus = DataUtils.GetTheMostPopularValueForTheColumn(Db.Connection, Db.DatabaseName, WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName, WhsDocketSchema.WD_DocketStatus.Name);
					// When the column is renamed, constraints will restrict value, but until then the value may be empty
					DbObjectCreator.CreateColumnIfNotExists(Db.Connection, WhsDocketSchema.Constants.TableName, TemporaryColumnName, WhsDocketSchema.WD_DocketStatus.SqlDbTypeDeclaration, defaultValue: $"'{defaultStatus}'");

					CreateSyncTriggers();

					var (fromGuid, upperBoundGuid) = GetFromAndToPKs();
					// upperBoundGuid will be null if table is empty
					if (upperBoundGuid.HasValue)
					{
						DoTransform(fromGuid.Value, upperBoundGuid.Value);

						ClearExtProperty(StoredFromPKName, StoredToPKName);
					}

					ExtProperty.Database.Update(Db.Connection, TransformationHasRunToCompletion, bool.TrueString);
				}
			}
		}

		void ClearAllExtPropertiesAndOldColumnIfRequired()
		{
			// In case the old transformation created this column but was unable to complete the process.
			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName), OldTemporaryColumnName))
			{
				var defaultConstraintName = DbObjectCreator.GenerateDefaultColumnConstraintName(WhsDocketSchema.Constants.TableName, OldTemporaryColumnName);
				Db.Connection.ExecuteNonQuery($"ALTER TABLE dbo.WhsDocket DROP CONSTRAINT IF EXISTS {defaultConstraintName}");
				DbObjectCreator.DropTriggerIfExists(Db.Connection, WhsDocketSyncTriggerName);
				DbObjectCreator.DropTriggerIfExists(Db.Connection, WhsPickSyncTriggerName);
				new DbColumnDependencyRemover(WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName, OldTemporaryColumnName).DropRelatedIndexes(Db.Connection);
				Db.Connection.ExecuteNonQuery(Invariant($"ALTER TABLE [dbo].[WhsDocket] DROP COLUMN [{OldTemporaryColumnName}]"));

				ClearAndSetExtPropertyForNewRun();
			}
			else
			{
				// A defect was found and resolved in WI00748138 - DB Upgrade failure - Incorrect Upper Bound
				// Due to previous runs potentially being contaminated, we want a fresh run of the transformation
				// Basically, clear all existing External Properties.
				var extPropertiesClearedForNewRun = ExtProperty.Database.Select(Db.Connection, ExtPropertiesClearedForNewRun);
				if (extPropertiesClearedForNewRun != bool.TrueString)
				{
					ClearAndSetExtPropertyForNewRun();
				}
			}

			void ClearAndSetExtPropertyForNewRun()
			{
				ClearExtProperty(StoredFromPKName, StoredToPKName, TransformationHasRunToCompletion);
				ExtProperty.Database.Update(Db.Connection, ExtPropertiesClearedForNewRun, bool.TrueString);
			}
		}

		(Guid? FromPK, Guid? UpperBoundPK) GetFromAndToPKs()
		{
			Guid? fromPK, upperBoundPK = null;

			var fromPKString = ExtProperty.Database.Select(Db.Connection, StoredFromPKName);
			if (string.IsNullOrEmpty(fromPKString))
			{
				fromPK = Guid.Empty;
				upperBoundPK = GetFinalPKValue();

				if (upperBoundPK.HasValue)
				{
					ExtProperty.Database.Update(Db.Connection, StoredFromPKName, fromPK.Value.ToString());
					ExtProperty.Database.Update(Db.Connection, StoredToPKName, upperBoundPK.Value.ToString());
				}
			}
			else
			{
				var upperBoundPKString = ExtProperty.Database.Select(Db.Connection, StoredToPKName);
				fromPK = Guid.TryParse(fromPKString, out var parsedFromPK) ? parsedFromPK : null;
				upperBoundPK = Guid.TryParse(upperBoundPKString, out var parsedUpperBoundPK) ? parsedUpperBoundPK : null;
			}

			return (fromPK, upperBoundPK);
		}

		Guid? GetFinalPKValue()
		{
			var sqlQuery = Invariant($@"SELECT MAX(WD_PK) FROM WhsDocket");
			using (var command = Db.Connection.Command(sqlQuery))
			{
				return command.ExecuteScalar() is Guid finalPK ? finalPK : null;
			}
		}

		void DoTransform(Guid? fromPK, Guid upperBoundPK)
		{
			var logFullnessProvider = new CargoWise.Data.SqlServer.LogFullnessProvider();
			var backlogWaiter = new CargoWise.Data.SqlServer.BacklogWaiter(new[] { logFullnessProvider });
			var stopWatch = Stopwatch.StartNew();
			var startingPK = fromPK;

			var processedBatchCount = 0;
			while (fromPK != null && fromPK != upperBoundPK)
			{
				var nextPKToUpdate = UpdateChunk(fromPK.Value, upperBoundPK);
				fromPK = nextPKToUpdate;

				processedBatchCount += fromPK != upperBoundPK ? BatchSize : 0;
				if (stopWatch.Elapsed.TotalMinutes > 1)
				{
					if (fromPK.HasValue)
					{
						var fromPKString = fromPK.ToString();
						ExtProperty.Database.Update(Db.Connection, StoredFromPKName, fromPKString);
					}
					manager.ShowInfoMessage($"Finished converting WD_DocketStatus statuses for {processedBatchCount} entries.");

					backlogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) =>
					{
						manager.ShowInfoMessage("\t      waiting for backlog to clear, " + (success ? bytesBacklog.BacklogDescription : failureReason) + ". Please run the LBK service task to reduce log fullness.");
					});

					processedBatchCount = 0;
					stopWatch.Restart();
				}
			}
		}

		Guid? UpdateChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(GetUpdateTempDocketStatusColumnSQL()))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);

				return command.ExecuteScalar() is Guid nextPKToUpdate ? nextPKToUpdate : null;
			}
		}

		string GetUpdateTempDocketStatusColumnSQL()
		{
			return Invariant($@"
;WITH
	DATA AS (
		SELECT
			TOP({BatchSize}) WD_PK
		FROM 
			dbo.WhsDocket
		WHERE 1=1
			AND WD_PK > @FromPK
			AND WD_PK <= @ToPK
		ORDER BY
			WD_PK
		)

SELECT
	WD_PK,
	MaxUpdatedPK = MAX(WD_PK) OVER()
INTO #DATAWITHMAX
FROM
	DATA

UPDATE dbo.WhsDocket SET
	{TemporaryColumnName} = CASE WHEN NewStatus = '{DepartedStatus}' THEN NewStatus ELSE WD_DocketStatus END,
	WD_SystemLastEditTimeUtc = CASE WHEN NewStatus = '{DepartedStatus}' THEN GetUtcDate() ELSE SanitisedAuditData.LastEditTime END,
	WD_SystemLastEditUser = CASE WHEN NewStatus = '{DepartedStatus}' THEN '~BP' ELSE SanitisedAuditData.LastEditUser END
FROM
	dbo.WhsDocket
	OUTER APPLY
	(
		SELECT
			CASE WHEN WD_DocketType = 'ORD' THEN '{DepartedStatus}' ELSE WD_DocketStatus END NewStatus
		FROM
			dbo.WhsPick
		WHERE
			WD_WP = WP_PK
			AND WP_FinalizedDateUtc IS NOT NULL
	) NewStatus
	CROSS APPLY
	(
		SELECT
			ISNULL(WD_SystemLastEditTimeUtc, GetUtcDate()) AS LastEditTime,
			CASE WHEN WD_SystemLastEditUser = '' THEN '~BP' ELSE WD_SystemLastEditUser END AS LastEditUser
	) SanitisedAuditData
	WHERE
		WD_PK IN (SELECT WD_PK FROM #DATAWITHMAX)
		AND
		(
			NewStatus = '{DepartedStatus}'
			OR
			WD_DocketStatus <> {TemporaryColumnName}
		)

SELECT TOP 1 MaxUpdatedPK FROM #DATAWITHMAX
");
		}

		void ClearExtProperty(params string[] names)
		{
			foreach (var name in names)
			{
				ExtProperty.Database.Delete(Db.Connection, name);
			}
		}

		void CreateSyncTriggers()
		{
			CreateWhsDocketSyncTrigger();
			CreateWhsPickSyncTrigger();
		}

		void CreateWhsDocketSyncTrigger()
		{
			if (!DbObjectCreator.TriggerExists(Db.Connection, WhsDocketSchema.Constants.TableName, WhsDocketSyncTriggerName))
			{
				Db.Connection.ExecuteNonQuery(Invariant($@"
CREATE TRIGGER dbo.{WhsDocketSyncTriggerName}
	ON dbo.WhsDocket
	AFTER INSERT, UPDATE
AS
BEGIN
	SET NOCOUNT ON;

	IF UPDATE(WD_DocketStatus)
	BEGIN
		UPDATE tab
		SET
			tab.{TemporaryColumnName} = CASE WHEN NewStatus = '{DepartedStatus}' THEN NewStatus ELSE tab.WD_DocketStatus END,
			tab.WD_SystemLastEditTimeUtc = ISNULL(tab.WD_SystemLastEditTimeUtc, GetUtcDate()),
			tab.WD_SystemLastEditUser = CASE WHEN tab.WD_SystemLastEditUser = '' THEN '~BP' ELSE tab.WD_SystemLastEditUser END
		FROM
			dbo.WhsDocket AS tab
			OUTER APPLY
			(
				SELECT
					CASE WHEN tab.WD_DocketType = 'ORD' THEN '{DepartedStatus}' ELSE tab.WD_DocketStatus END NewStatus
				FROM
					dbo.WhsPick
				WHERE
					tab.WD_WP = WP_PK
					AND WP_FinalizedDateUtc IS NOT NULL
			) IsDeparted
			JOIN inserted on tab.WD_PK = inserted.WD_PK
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
		UPDATE docket
		SET
			docket.{TemporaryColumnName} = '{DepartedStatus}',
			docket.WD_SystemLastEditTimeUtc = ISNULL(docket.WD_SystemLastEditTimeUtc, GetUtcDate()),
			docket.WD_SystemLastEditUser = CASE WHEN docket.WD_SystemLastEditUser = '' THEN '~BP' ELSE docket.WD_SystemLastEditUser END
		FROM
			dbo.WhsDocket AS docket 
			JOIN inserted on docket.WD_WP = inserted.WP_PK
		WHERE
			inserted.WP_FinalizedDateUtc IS NOT NULL
			AND docket.WD_DocketType = 'ORD'
	END
END"));
			}
		}

		protected override void OfflinePreUpgradeTransform()
		{
			base.OfflinePreUpgradeTransform();

			// temp column won't exist if transform never ran
			if (DbObjectCreator.ColumnExists(Db.Connection, new DbObjectCreator.TableDescriptor(WhsDocketSchema.Constants.SqlSchemaName, WhsDocketSchema.Constants.TableName), TemporaryColumnName))
			{
				ExtProperty.Database.Delete(Db.Connection, TransformationHasRunToCompletion);
				ExtProperty.Database.Delete(Db.Connection, ExtPropertiesClearedForNewRun);
			}
		}
	}
}
