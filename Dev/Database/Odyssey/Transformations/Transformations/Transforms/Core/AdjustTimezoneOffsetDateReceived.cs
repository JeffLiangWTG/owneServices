using System;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Core
{
	public class AdjustTimezoneOffsetDateReceived : DataTransformation
	{
		public override string UserDescription => "Adjust timezone offset in JobRequiredDocument";

		const string LastProcessedDateReceivedJobRequirementPK = "LastProcessedDateReceivedJobRequirementPK";
		const string TransformIsRunningName = "AdjustTimezoneOffsetDateReceived.IsRunning";
		const int ChunkSize = 2000;

		protected override void OnlinePreUpgradeTransform()
		{
			var registryHelper = new RegistryTransformationHelper();
			if (registryHelper.GetStmDataRowCount(TransformIsRunningName) == 0)
			{
				registryHelper.InsertStmDataRow(TransformIsRunningName, "BOL", Encoding.Unicode.GetBytes(bool.TrueString));
			}
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedDateReceivedJobRequirementPK);
			var lastProcessedPK = Guid.TryParse(lastProcessedPKString, out var parsedPK) ? parsedPK : (Guid?)null;

			var rowCount = DataUtils.GetApproximateRowCountForTable(Db.Connection, JobRequiredDocumentSchema.Constants.TableName);
			var chunks = GuidChunker.GenerateChunks(chunkSize: ChunkSize, rowCount, lastProcessedPK);

			var logFullnessProvider = new CargoWise.Data.SqlServer.LogFullnessProvider();
			var backlogWaiter = new CargoWise.Data.SqlServer.BacklogWaiter(new[] { logFullnessProvider });

			var stopWatch = Stopwatch.StartNew();
			foreach (var chunk in chunks)
			{
				ProcessBatch(chunk.LowerBound, chunk.UpperBound);

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedDateReceivedJobRequirementPK, chunk.LowerBound.ToString());
					manager.ShowInfoMessage($"Finished converting EQ_DateReceived offset up to: {chunk.LowerBound}.");

					token.ThrowIfCancellationRequested();

					backlogWaiter.WaitUntilBacklogIsAcceptable((elapsed, nextWaitTime, success, failureReason, bytesBacklog) =>
					{
						manager.ShowInfoMessage("\t      waiting for backlog to clear, " + (success ? bytesBacklog.BacklogDescription : failureReason) + ". Please run the LBK service task to reduce log fullness.");
					});

					stopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedDateReceivedJobRequirementPK);

			var registryHelper = new RegistryTransformationHelper();
			registryHelper.DeleteStmDataRow(TransformIsRunningName);
		}

		void ProcessBatch(Guid startGuid, Guid endGuid)
		{
			var sql = $@"
;WITH
	PROCESSING_DATA AS (
		SELECT *
		FROM
			dbo.JobRequiredDocument
		WHERE 1=1
			AND EQ_ParentID BETWEEN @startGuid AND @endGuid
			AND EQ_DateReceived IS NOT NULL
			AND DATEPART(tzoffset, EQ_DateReceived) = 0
		)
UPDATE PROCESSING_DATA SET
	EQ_DateReceived = COALESCE(EQ_DateReceived_WTG_OffsetFast, EQ_DateReceived_WTG_Offset, EQ_DateReceived),
	EQ_SystemLastEditTimeUtc = CASE WHEN ISNULL(EQ_DateReceived_WTG_OffsetFast, EQ_DateReceived_WTG_Offset) IS NULL THEN EQ_SystemLastEditTimeUtc ELSE GetUtcDate() END,
	EQ_SystemLastEditUser = CASE WHEN ISNULL(EQ_DateReceived_WTG_OffsetFast, EQ_DateReceived_WTG_Offset) IS NULL THEN EQ_SystemLastEditUser ELSE '~BP' END
FROM
	PROCESSING_DATA
	OUTER APPLY
	(
		SELECT GB_RL_NKHomePort as EQ_DateReceived_WTG_TimeZoneUnloco
	FROM
		dbo.StmALog
		JOIN dbo.GlbBranch WITH(FORCESEEK, INDEX(NR_UX__GB_Code)) ON GlbBranch.GB_Code = StmALog.SL_GB_NKBranch 
	WHERE
		SL_Parent = EQ_PK
		AND SL_SE_NKEvent = 'ADD'
	) as EQ_DateReceived_WTG_TimeZoneUnloco
	OUTER APPLY
	(
		SELECT
			TOP 1 EQ_DateReceived_WTG_OffsetFast
		FROM
			dbo.RefDatabase_RefUNLOCOUtcOffset
			CROSS APPLY (SELECT TODATETIMEOFFSET(EQ_DateReceived, ISNULL(RLO_OffsetMinutesFromUtc, 0)) AS EQ_DateReceived_WTG_OffsetFast) AS EQ_DateReceived_WTG_OffsetFast
		WHERE
			RLO_RL_NKCode = EQ_DateReceived_WTG_TimeZoneUnloco
			AND TODATETIMEOFFSET(RLO_StartTimeUtc, RLO_OffsetMinutesFromUtc) <= EQ_DateReceived
			AND TODATETIMEOFFSET(RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) > EQ_DateReceived
		ORDER BY
			EQ_DateReceived_WTG_OffsetFast DESC
	) AS EQ_DateReceived_WTG_OffsetFast
	OUTER APPLY
	(
		SELECT
			TODATETIMEOFFSET(EQ_DateReceived, Offset) AS EQ_DateReceived_WTG_Offset
		FROM
			dbo.CalculateTimeZoneOffsetInMinutesFromRefUNLOCO(EQ_DateReceived_WTG_TimeZoneUnloco, EQ_DateReceived, 0)
		WHERE
			EQ_DateReceived_WTG_OffsetFast IS NULL
	) AS EQ_DateReceived_WTG_Offset
	OPTION(FORCE ORDER, MAXDOP 1);
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@startGuid", SqlDbType.UniqueIdentifier, startGuid);
				command.AddParameter("@endGuid", SqlDbType.UniqueIdentifier, endGuid);
				command.ExecuteScalar();
			}
		}
	}
}
