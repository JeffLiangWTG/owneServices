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

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	public class ComplianceRiskStatusCalculateJobEndDate : DataTransformation
	{
		public override string UserDescription => "Populate all COR_JobEndDate columns in dbo.ComplianceRiskStatus.";
		const string LastProcessedChunkPKName = "PopulateAllCOR_JobEndDateColumnsInComplianceRiskStatus.LastProcessedChunkPKName";
		const int BatchSize = 5000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastProcessedChunkPKString = ExtProperty.Database.Select(Db.Connection, LastProcessedChunkPKName);
			var lastProcessedPK = Guid.TryParse(lastProcessedChunkPKString, out var parsedPK) ? parsedPK : (Guid?)null;
			var helper = new RegistryTransformationHelper();

			var jobUpdatePeriod = 3;

			var jobUpdatePeriodRegistrySettings = helper.GetStmDataValue("JobUpdatePeriod");
			if (jobUpdatePeriodRegistrySettings is not null)
			{
				var jobUpdatePeriodString = Encoding.Unicode.GetString(jobUpdatePeriodRegistrySettings);
				int.TryParse(jobUpdatePeriodString, out jobUpdatePeriod);
			}

			var chunks = GuidChunker.GenerateChunks(BatchSize, DataUtils.GetApproximateRowCountForTable(Db.Connection, ComplianceRiskStatusSchema.Constants.TableName), lastProcessedPK);

			var loggingStopWatch = Stopwatch.StartNew();

			foreach (var chunk in chunks)
			{
				ProcessChunk(chunk.LowerBound, chunk.UpperBound, jobUpdatePeriod);

				if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, LastProcessedChunkPKName, chunk.UpperBound.ToString());

					token.ThrowIfCancellationRequested();
					loggingStopWatch.Restart();
				}
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedChunkPKName);
		}
		void ProcessChunk(Guid fromPK, Guid toPK, int jobUpdatePeriod)
		{
			using (var command = Db.Connection.Command(CalculateJobEndDateChunkSQL))
			{
				command.AddParameter("@jobUpdatePeriod", SqlDbType.TinyInt, jobUpdatePeriod);
				command.AddParameter("@fromInstructionPK", SqlDbType.UniqueIdentifier, fromPK);
				command.AddParameter("@toInstructionPK", SqlDbType.UniqueIdentifier, toPK);
				command.ExecuteNonQuery();
			}
		}

		const string CalculateJobEndDateChunkSQL = @"
UPDATE dbo.ComplianceRiskStatus
SET COR_JobEndDate =
	CASE COR_ParentTableCode
		WHEN 'JK' THEN (
			SELECT MAX(ISNULL(dt, '2000-01-01'))
			FROM (
				SELECT (SELECT MAX(ISNULL(v, '2000-01-01')) FROM (VALUES (JW_ETD), (JW_ETA), (JW_ATD), (JW_ATA)) AS value(v)) AS dt
				FROM dbo.JobConsolTransport
				WHERE JW_ParentGUID = COR_ParentID
				UNION
				SELECT DATEADD(MONTH, @jobUpdatePeriod, JK_SystemCreateTimeUtc)
				FROM dbo.JobConsol
				WHERE JK_PK = COR_ParentID
					AND NOT EXISTS (
						SELECT 1 FROM dbo.JobConsolTransport
						WHERE JW_ParentGUID = COR_ParentID
						AND (JW_ETD IS NOT NULL OR JW_ETA IS NOT NULL OR JW_ATD IS NOT NULL OR JW_ATA IS NOT NULL)
					)
			) t
		)
		WHEN 'JS' THEN (
			SELECT MAX(ISNULL(dt, '2000-01-01'))
			FROM (
				SELECT (SELECT MAX(ISNULL(v, '2000-01-01')) FROM (VALUES (JW_ETD), (JW_ETA), (JW_ATD), (JW_ATA)) AS value(v)) AS dt
				FROM dbo.JobConsolTransport
				WHERE JW_ParentGUID = COR_ParentID
				UNION
				SELECT (SELECT MAX(ISNULL(v, '2000-01-01')) FROM (VALUES (JS_E_DEP), (JS_E_ARV)) AS value(v)) AS dt
				FROM dbo.JobShipment WHERE JS_PK = COR_ParentID
				UNION
				SELECT DATEADD(MONTH, 3, JS_SystemCreateTimeUtc)
				FROM dbo.JobShipment
				WHERE JS_PK = COR_ParentID
					AND NOT EXISTS (
						SELECT 1 FROM dbo.JobConsolTransport
						WHERE JW_ParentGUID = COR_ParentID
						AND (JW_ETD IS NOT NULL OR JW_ETA IS NOT NULL OR JW_ATD IS NOT NULL OR JW_ATA IS NOT NULL)
					)
					AND NOT EXISTS (
						SELECT 1 FROM dbo.JobShipment
						WHERE JS_PK = COR_ParentID AND (JS_E_DEP IS NOT NULL OR JS_E_ARV IS NOT NULL)
					)
			) t
		)
		WHEN 'TH' THEN (
			SELECT MAX(ISNULL(dt, '2000-01-01'))
			FROM (
				SELECT (SELECT MAX(ISNULL(v, '2000-01-01')) FROM (VALUES (JS_E_DEP), (JS_E_ARV), (JW_ETD), (JW_ETA), (JW_ATD), (JW_ATA)) AS value(v)) AS dt
				FROM dbo.JobShipment WITH(FORCESEEK INDEX(FK_UX__JS_TH_OneTimeQuote))
				LEFT JOIN dbo.JobConsolTransport ON JS_PK = JW_ParentGUID
				WHERE JS_TH_OneTimeQuote = COR_ParentID
				UNION
				SELECT DATEADD(MONTH, @jobUpdatePeriod, MAX(JS_SystemCreateTimeUtc))
				FROM dbo.JobShipment WITH(FORCESEEK INDEX(FK_UX__JS_TH_OneTimeQuote))
				WHERE JS_TH_OneTimeQuote = COR_ParentID
					AND NOT EXISTS (
						SELECT 1 FROM dbo.JobConsolTransport
						JOIN dbo.JobShipment WITH(FORCESEEK INDEX(FK_UX__JS_TH_OneTimeQuote)) ON JW_ParentGUID = JS_PK
						WHERE JS_TH_OneTimeQuote = COR_ParentID
						AND (JW_ETD IS NOT NULL OR JW_ETA IS NOT NULL OR JW_ATD IS NOT NULL OR JW_ATA IS NOT NULL)
					)
					AND NOT EXISTS (
						SELECT 1 FROM dbo.JobShipment WITH(FORCESEEK INDEX(FK_UX__JS_TH_OneTimeQuote))
						WHERE JS_TH_OneTimeQuote = COR_ParentID AND (JS_E_DEP IS NOT NULL OR JS_E_ARV IS NOT NULL)
					)
			) t
		)
		ELSE '2000-01-01'
	END, COR_SystemLastEditTimeUtc = GetUtcDate(), COR_SystemLastEditUser = '~BP'
WHERE COR_JobEndDate IS NULL AND COR_PK BETWEEN @fromInstructionPK AND @toInstructionPK
OPTION (MAXDOP 1)
";
	}
}
