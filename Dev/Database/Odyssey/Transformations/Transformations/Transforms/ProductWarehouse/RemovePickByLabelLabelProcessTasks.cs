using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class RemovePickByLabelLabelProcessTasks : DataTransformation
	{
		public override string UserDescription => "Remove pick by label workflow data.";
		public const string LastProcessedChunkPKName = "RemovePickByLabelLabelProcessTasks.LastProcessedChunkPK";
		const int BatchSize = 1000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, WhsPickByLabelLabelSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(BatchSize, rowCount, lastProcessedPK);

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				DeleteChunk(chunk.LowerBound, chunk.UpperBound);
				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					var lastProcessedChunkPK = chunk.UpperBound.ToString();
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, lastProcessedChunkPK);
					manager?.ShowInfoMessage($"Deleted process tasks for pick by label up to {lastProcessedChunkPK}.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}

		void DeleteChunk(Guid fromPK, Guid toPK)
		{
			using (var command = Db.Connection.Command(RemovePickByLabelLabelProcessTasksQuery))
			{
				command.AddParameter("@FromPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@ToPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		const string RemovePickByLabelLabelProcessTasksQuery = @"
BEGIN TRY
	DROP TABLE IF EXISTS #PickByLabelParentPks
	DROP TABLE IF EXISTS #ProcessTasksToDeletePks
	DROP TABLE IF EXISTS #ProcessHeadersToDeletePks

	CREATE TABLE #PickByLabelParentPks
	(
		PickByLabelParentPK UNIQUEIDENTIFIER NOT NULL
	)

	CREATE TABLE #ProcessTasksToDeletePks
	(
		ProcessTaskToDeletePK UNIQUEIDENTIFIER NOT NULL
	)

	CREATE TABLE #ProcessHeadersToDeletePks
	(
		ProcessHeaderToDeletePK UNIQUEIDENTIFIER NOT NULL
	)

	INSERT INTO #PickByLabelParentPks
	SELECT
		WTL_PK
	FROM
		dbo.WhsPickByLabelLabel
	WHERE
		WTL_PK >= @FromPK
		AND WTL_PK <= @ToPK

	INSERT INTO #ProcessTasksToDeletePks
	SELECT
		P9_PK
	FROM
		dbo.ProcessTasks
	WHERE
		P9_ParentID IN (SELECT PickByLabelParentPK FROM #PickByLabelParentPks)

	INSERT INTO #ProcessHeadersToDeletePks
	SELECT
		FH_PK
	FROM
		dbo.ProcessHeader
	WHERE
		FH_ParentId IN (SELECT PickByLabelParentPK FROM #PickByLabelParentPks)

	UPDATE dbo.WhsCycleCountLocation
	SET
		WCL_P9_Task = NULL,
		WCL_SystemLastEditTimeUtc = GETUTCDATE(),
		WCL_SystemLastEditUser = '~BP'
	WHERE
		WCL_P9_Task IS NOT NULL
		AND WCL_P9_Task IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WhsDocket
	SET
		WD_P9_PackingTask = NULL,
		WD_SystemLastEditTimeUtc = GETUTCDATE(),
		WD_SystemLastEditUser = '~BP'
	WHERE
		WD_P9_PackingTask IS NOT NULL
		AND WD_P9_PackingTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WhsDocketLine
	SET
		WE_P9_Task = NULL,
		WE_SystemLastEditTimeUtc = GETUTCDATE(),
		WE_SystemLastEditUser = '~BP'
	WHERE
		WE_P9_Task IS NOT NULL
		AND WE_P9_Task IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WhsPickByLabelJob
	SET
		WTK_P9_Task = NULL,
		WTK_SystemLastEditTimeUtc = GETUTCDATE(),
		WTK_SystemLastEditUser = '~BP'
	WHERE
		WTK_P9_Task IS NOT NULL
		AND WTK_P9_Task IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WhsPickLine
	SET
		WZ_P9_Task = NULL,
		WZ_SystemLastEditTimeUtc = GETUTCDATE(),
		WZ_SystemLastEditUser = '~BP'
	WHERE
		WZ_P9_Task IS NOT NULL
		AND WZ_P9_Task IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WorkItem
	SET
		WKI_P9_DefectCausedByTask = NULL,
		WKI_SystemLastEditTimeUtc = GETUTCDATE(),
		WKI_SystemLastEditUser = '~BP'
	WHERE
		WKI_P9_DefectCausedByTask IS NOT NULL
		AND WKI_P9_DefectCausedByTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.WorkItem
	SET
		WKI_P9_DefectFirstMissedInTask = NULL,
		WKI_SystemLastEditTimeUtc = GETUTCDATE(),
		WKI_SystemLastEditUser = '~BP'
	WHERE
		WKI_P9_DefectFirstMissedInTask IS NOT NULL
		AND WKI_P9_DefectFirstMissedInTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE ChildProcessTaskIterationLink
	SET
		ChildProcessTaskIterationLink.P9I_P9I_ParentIteration = NULL,
		ChildProcessTaskIterationLink.P9I_SystemLastEditTimeUtc = GETUTCDATE(),
		ChildProcessTaskIterationLink.P9I_SystemLastEditUser = '~BP'
	FROM
		dbo.ProcessTaskIterationLink AS ChildProcessTaskIterationLink
		JOIN dbo.ProcessTaskIterationLink AS ParentProcessTaskIterationLink ON ChildProcessTaskIterationLink.P9I_P9I_ParentIteration = ParentProcessTaskIterationLink.P9I_PK
	WHERE
		ChildProcessTaskIterationLink.P9I_P9I_ParentIteration IS NOT NULL
		AND ParentProcessTaskIterationLink.P9I_P9_ContainmentBarrierTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.ProcessTaskIterationLink
	SET
		P9I_P9_IterationTask = NULL,
		P9I_SystemLastEditTimeUtc = GETUTCDATE(),
		P9I_SystemLastEditUser = '~BP'
	WHERE
		P9I_P9_IterationTask IS NOT NULL
		AND P9I_P9_IterationTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	UPDATE dbo.ProcessTasks
	SET
		P9_FH_ProcessHeader = NULL,
		P9_SystemLastEditTimeUtc = GETUTCDATE(),
		P9_SystemLastEditUser = '~BP'
	WHERE
		P9_FH_ProcessHeader IS NOT NULL
		AND P9_FH_ProcessHeader IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	UPDATE dbo.ProcessTaskIterationLink
	SET
		P9I_FH_IterationWorkflow = NULL,
		P9I_SystemLastEditTimeUtc = GETUTCDATE(),
		P9I_SystemLastEditUser = '~BP'
	WHERE
		P9I_FH_IterationWorkflow IS NOT NULL
		AND P9I_FH_IterationWorkflow IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	UPDATE dbo.ProcessHeader
	SET
		FH_FH_ParentHeader = NULL,
		FH_SystemLastEditTimeUtc = GETUTCDATE(),
		FH_SystemLastEditUser = '~BP'
	WHERE
		FH_FH_ParentHeader IS NOT NULL
		AND FH_FH_ParentHeader IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	UPDATE dbo.BMNCNAttachment
	SET
		BNA_FP_ProcessHeaderLink = NULL,
		BNA_SystemLastEditTimeUtc = GETUTCDATE(),
		BNA_SystemLastEditUser = '~BP'
	FROM
		dbo.BMNCNAttachment
		JOIN dbo.ProcessHeaderLink ON BNA_FP_ProcessHeaderLink = FP_PK
	WHERE
		BNA_FP_ProcessHeaderLink IS NOT NULL
		AND FP_FH_HeaderTo IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	UPDATE dbo.BMNCNAttachment
	SET
		BNA_FP_ProcessHeaderLink = NULL,
		BNA_SystemLastEditTimeUtc = GETUTCDATE(),
		BNA_SystemLastEditUser = '~BP'
	FROM
		dbo.BMNCNAttachment
		JOIN dbo.ProcessHeaderLink ON BNA_FP_ProcessHeaderLink = FP_PK
	WHERE
		BNA_FP_ProcessHeaderLink IS NOT NULL
		AND FP_FH_HeaderFrom IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	DELETE dbo.ProcessTaskIterationLinkPivot
	FROM
		#ProcessTasksToDeletePks
	WHERE
		P9P_P9_Task = ProcessTaskToDeletePK

	DELETE dbo.ProcessTaskIterationLinkPivot
	FROM
		dbo.ProcessTaskIterationLinkPivot
		JOIN dbo.ProcessTaskIterationLink ON P9P_P9I_Iteration = P9I_PK
	WHERE
		P9I_P9_ContainmentBarrierTask IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	DELETE dbo.ProcessTaskIterationLink
	FROM
		#ProcessTasksToDeletePks
	WHERE
		P9I_P9_ContainmentBarrierTask = ProcessTaskToDeletePK

	DELETE dbo.ProcessTasksSecure
	FROM
		#ProcessTasksToDeletePks
	WHERE
		P9H_P9_Parent = ProcessTaskToDeletePK

	DELETE dbo.ProcessWorkflowException
	FROM
		#ProcessTasksToDeletePks
	WHERE
		WEX_P9_ProcessTask = ProcessTaskToDeletePK

	DELETE dbo.BMReleaseSequenceItem
	FROM
		#ProcessHeadersToDeletePks
	WHERE
		BMI_FH_ProcessHeader = ProcessHeaderToDeletePK

	DELETE dbo.ProcessHeaderLink
	FROM
		#ProcessHeadersToDeletePks
	WHERE
		FP_FH_HeaderFrom = ProcessHeaderToDeletePK

	DELETE dbo.ProcessHeaderLink
	FROM
		#ProcessHeadersToDeletePks
	WHERE
		FP_FH_HeaderTo = ProcessHeaderToDeletePK

	DELETE dbo.ProcessEstimateLog
	FROM
		dbo.ProcessEstimateLog
	WHERE
		P9E_ParentId IN (SELECT ProcessTaskToDeletePK FROM #ProcessTasksToDeletePks)

	DELETE dbo.ProcessEstimateLog
	FROM
		dbo.ProcessEstimateLog
	WHERE
		P9E_ParentId IN (SELECT ProcessHeaderToDeletePK FROM #ProcessHeadersToDeletePks)

	DELETE dbo.ProcessTasks
	FROM
		#PickByLabelParentPks
	WHERE
		P9_ParentID IS NOT NULL
		AND P9_ParentID = PickByLabelParentPK

	DELETE dbo.ProcessHeader
	FROM
		#PickByLabelParentPks
	WHERE
		FH_ParentId IS NOT NULL
		AND FH_ParentId = PickByLabelParentPK

	DROP TABLE #PickByLabelParentPks
	DROP TABLE #ProcessTasksToDeletePks
	DROP TABLE #ProcessHeadersToDeletePks
END TRY
BEGIN CATCH
	THROW;
END CATCH
";
	}
}
