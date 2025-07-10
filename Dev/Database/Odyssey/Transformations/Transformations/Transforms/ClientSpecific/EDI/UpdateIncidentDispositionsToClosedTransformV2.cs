using System.Threading;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI
{
	public class UpdateIncidentDispositionsToClosedTransformV2 : DataTransformation
	{
		public override string UserDescription => "Update resolution details for Incidents in ediprod for IM_ClosureResolution refactor Version 2";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (!DbObjectCreator.TableExists(Db.Connection, Db.Connection.CurrentDatabase, "IncidentMain"))
			{
				return;
			}

			var createIndexQuery = @"
IF NOT EXISTS (SELECT Name FROM sys.indexes WHERE name='NR_RX__IM_SystemCreateTimeUtc_Closed' AND object_id = OBJECT_ID('dbo.IncidentMain'))
	CREATE INDEX NR_RX__IM_SystemCreateTimeUtc_Closed ON dbo.IncidentMain (IM_SystemCreateTimeUtc) INCLUDE (IM_PK, IM_ResolutionCode, IM_SystemLastEditTimeUtc)
	WHERE
		IM_Status = 'CLS'
		AND IM_ResolutionCode IN ('URP', 'UPD', 'TSP', 'TRN', 'TRM', 'SYS', 'SRS', 'REF', 'OTH', 'NSC', 'DUP', 'DTF', 'COM', 'CIG', 'QDC', 'NFR', 'CAN', 'NDF', 'DBR', 'CLI', 'SAL', 'NCS', 'NCR', 'CRA', 'NCN', 'NRC', 'PDM', 'PFR', 'PSL', 'PTI', 'PTT', 'REI', 'SFR')
	WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)";

			Db.Connection.ExecuteNonQuery(createIndexQuery);

			while (!token.IsCancellationRequested)
			{
				var query = $@"
DECLARE @IncidentResolutionInfo TABLE 
(
	IncidentPK UNIQUEIDENTIFIER NOT NULL,
	ResolutionCode CHAR(3) NOT NULL,
	ResolvedTimeUtc DATETIME NOT NULL,
	HasSTCEvent BIT NOT NULL,
	HasJCLEvent BIT NOT NULL
);

DECLARE @IncidentResolutionInfoWithEventTime TABLE 
(
	IncidentPK UNIQUEIDENTIFIER NOT NULL,
	ResolutionCode CHAR(3) NOT NULL,
	ResolvedTimeUtc DATETIME NOT NULL,
	ShouldAddIRSEvent BIT NOT NULL,
	BranchCode VARCHAR(3) NULL,
	EventTime DATETIME NULL
);

DECLARE @PostedTimeUtc DATETIME = GETUTCDATE();

BEGIN TRY
	BEGIN TRANSACTION
		INSERT INTO @IncidentResolutionInfo
		SELECT
			IncidentPK = IM_PK,
			ResolutionCode = IM_ResolutionCode,
			ResolvedTimeUtc = COALESCE(MAX(STCLog.SL_PostedTimeUtc), MAX(JCLLog.SL_PostedTimeUtc), IM_SystemLastEditTimeUtc),
			HasSTCEvent = CASE WHEN MAX(STCLog.SL_PostedTimeUtc) IS NOT NULL THEN 1 ELSE 0 END,
			HasJCLEvent = CASE WHEN MAX(JCLLog.SL_PostedTimeUtc) IS NOT NULL THEN 1 ELSE 0 END
		FROM
			(
				SELECT TOP {BatchSize} IM_PK, IM_ResolutionCode, IM_SystemLastEditTimeUtc, IM_SystemLastEditUser
				FROM
				dbo.IncidentMain
				WHERE
					IM_Status = 'CLS'
					AND IM_ResolutionCode IN ('URP', 'UPD', 'TSP', 'TRN', 'TRM', 'SYS', 'SRS', 'REF', 'OTH', 'NSC', 'DUP', 'DTF', 'COM', 'CIG', 'QDC', 'NFR', 'CAN', 'NDF', 'DBR', 'CLI', 'SAL', 'NCS', 'NCR', 'CRA', 'NCN', 'NRC', 'PDM', 'PFR', 'PSL', 'PTI', 'PTT', 'REI', 'SFR')
				ORDER BY IM_SystemCreateTimeUtc DESC
			) a
			LEFT JOIN dbo.StmALog STCLog ON STCLog.SL_Parent = IM_PK AND STCLog.SL_SE_NKEvent = 'STC' AND STCLog.SL_Reference LIKE '% to CLS'
			LEFT JOIN dbo.StmALog JCLLog ON JCLLog.SL_Parent = IM_PK AND JCLLog.SL_SE_NKEvent = 'JCL'
		GROUP BY IM_PK, IM_ResolutionCode, IM_SystemLastEditTimeUtc;

		INSERT INTO @IncidentResolutionInfoWithEventTime
		SELECT
			IncidentPK = info.IncidentPK,
			ResolutionCode = info.ResolutionCode,
			ResolvedTimeUtc = info.ResolvedTimeUtc,
			ShouldAddIRSEvent = CASE WHEN info.HasSTCEvent = 1 OR info.HasJCLEvent = 1 THEN 1 ELSE 0 END,
			BranchCode = COALESCE(STCLog.SL_GB_NKBranch, JCLLog.SL_GB_NKBranch),
			EventTime = DATEADD(minute, COALESCE(MAX(STCUNLOCOOffset.RLO_OffsetMinutesFromUtc), MAX(JCLUNLOCOOffset.RLO_OffsetMinutesFromUtc)), info.ResolvedTimeUtc)
		FROM
			@IncidentResolutionInfo info
			LEFT JOIN dbo.StmALog STCLog ON STCLog.SL_Parent = info.IncidentPK AND STCLog.SL_SE_NKEvent = 'STC' AND STCLog.SL_Reference LIKE '% to CLS' AND STCLog.SL_PostedTimeUtc = info.ResolvedTimeUtc AND info.HasSTCEvent = 1
			LEFT JOIN dbo.StmALog JCLLog ON JCLLog.SL_Parent = info.IncidentPK AND JCLLog.SL_SE_NKEvent = 'JCL' AND JCLLog.SL_PostedTimeUtc = info.ResolvedTimeUtc AND info.HasSTCEvent = 0 AND info.HasJCLEvent = 1
			LEFT JOIN dbo.GlbBranch STCBranch ON STCLog.SL_GB_NKBranch = STCBranch.GB_Code
			LEFT JOIN dbo.GlbBranch JCLBranch ON JCLLog.SL_GB_NKBranch = JCLBranch.GB_Code
			LEFT JOIN dbo.RefUNLOCOUtcOffset STCUNLOCOOffset ON STCBranch.GB_RL_NKHomePort = STCUNLOCOOffset.RLO_RL_NKCode AND STCUNLOCOOffset.RLO_StartTimeUtc <= info.ResolvedTimeUtc AND STCUNLOCOOffset.RLO_EndTimeUtc > info.ResolvedTimeUtc
			LEFT JOIN dbo.RefUNLOCOUtcOffset JCLUNLOCOOffset ON JCLBranch.GB_RL_NKHomePort = JCLUNLOCOOffset.RLO_RL_NKCode AND JCLUNLOCOOffset.RLO_StartTimeUtc <= info.ResolvedTimeUtc AND JCLUNLOCOOffset.RLO_EndTimeUtc > info.ResolvedTimeUtc
		GROUP BY info.IncidentPK, info.ResolutionCode, info.ResolvedTimeUtc, info.HasSTCEvent, info.HasJCLEvent, STCLog.SL_GB_NKBranch, JCLLog.SL_GB_NKBranch

		UPDATE dbo.IncidentMain SET
		IM_ResolutionCode = 'CLS',
		IM_ClosureResolution = ResolutionCode,
		IM_ResolveTimeUtc = ResolvedTimeUtc,
		IM_SystemLastEditTimeUtc = @PostedTimeUtc,
		IM_SystemLastEditUser = '~BP'
		FROM dbo.IncidentMain
		JOIN @IncidentResolutionInfo ON IncidentPK = IM_PK;

		INSERT INTO dbo.StmALog (SL_PK, SL_SE_NKEvent, SL_Reference, SL_EventTime, SL_EventTimeUtc, SL_Table, SL_Parent, SL_PostedTimeUtc, SL_GS_NKUser, SL_GB_NKBranch)
		SELECT
			SL_PK = NEWID(),
			SL_SE_NKEvent = 'IRS',
			SL_Reference = 'Data Transform',
			SL_EventTime = COALESCE(EventTime, ResolvedTimeUtc),
			SL_EventTimeUtc = ResolvedTimeUtc,
			SL_Table = 'IncidentMain',
			SL_Parent = IncidentPK,
			SL_PostedTimeUtc = @PostedTimeUtc,
			SL_GS_NKUser = '~BP',
			SL_GB_NKBranch = BranchCode
		FROM
			@IncidentResolutionInfoWithEventTime
		WHERE
			ShouldAddIRSEvent = 1
	COMMIT TRANSACTION
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION;
	THROW;
END CATCH;

SELECT CASE WHEN EXISTS (SELECT NULL FROM @IncidentResolutionInfo)
THEN CAST(1 AS BIT)
ELSE CAST(0 AS BIT) END
";

				var shouldContinue = (bool)Db.Connection.ExecuteScalar(query);

				if (!shouldContinue)
				{
					DropIndexIfExists();
					break;
				}
			}

			token.ThrowIfCancellationRequested();
		}

		protected virtual int BatchSize => 100;

		void DropIndexIfExists()
		{
			var dropIndexQuery = "DROP INDEX IF EXISTS NR_RX__IM_SystemCreateTimeUtc_Closed ON dbo.IncidentMain";
			Db.Connection.ExecuteNonQuery(dropIndexQuery);
		}
	}
}
