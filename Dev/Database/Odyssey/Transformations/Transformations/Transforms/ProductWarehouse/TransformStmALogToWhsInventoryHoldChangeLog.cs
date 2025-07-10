using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse
{
	public class TransformStmALogToWhsInventoryHoldChangeLog : DataTransformation, ITransformationIndexProvider
	{
		public override string UserDescription => "Transform StmALog to WhsInventoryHoldChangeLog.";

		const string LastDocketPKName = "TransformStmALogToWhsInventoryHoldChangeLog.LastDocketPK";
		const string MaxDocketPKName = "TransformStmALogToWhsInventoryHoldChangeLog.MaxDocketPK";
		const string TotalDocketsName = "TransformStmALogToWhsInventoryHoldChangeLog.TotalDockets";
		const string CurrentCountName = "TransformStmALogToWhsInventoryHoldChangeLog.CurrentCount";
		const string FromTimeName = "TransformStmALogToWhsInventoryHoldChangeLog.FromTime";
		const string ToTimeName = "TransformStmALogToWhsInventoryHoldChangeLog.ToTime";
		const string HighWaterMarkName = "TransformStmALogToWhsInventoryHoldChangeLog.HighWaterMark";
		const string IsNewTransform = "TransformStmALogToWhsInventoryHoldChangeLog.IsNewTransform";

		const int BatchSize = 1000;

		protected override void OnlinePreUpgradeTransform()
		{
			base.OnlinePreUpgradeTransform();

			if (DbObjectCreator.TableExists(Db.Connection, WhsDocketLineSchema.Constants.TableName))
			{
				OnlinePreUpgradeTransformCore();
			}
		}

		void OnlinePreUpgradeTransformCore()
		{
			if (ExtProperty.Database.Select(Db.Connection, IsNewTransform) != bool.TrueString)
			{
				ExtProperty.Database.Delete(Db.Connection, LastDocketPKName);
				ExtProperty.Database.Delete(Db.Connection, MaxDocketPKName);
				ExtProperty.Database.Delete(Db.Connection, TotalDocketsName);
				ExtProperty.Database.Delete(Db.Connection, CurrentCountName);
				ExtProperty.Database.Delete(Db.Connection, FromTimeName);
				ExtProperty.Database.Delete(Db.Connection, ToTimeName);
				ExtProperty.Database.Delete(Db.Connection, HighWaterMarkName);

				ExtProperty.Database.Update(Db.Connection, IsNewTransform, bool.TrueString);
			}

			if (string.IsNullOrEmpty(ExtProperty.Database.Select(Db.Connection, HighWaterMarkName)))
			{
				ExtProperty.Database.Update(Db.Connection, HighWaterMarkName, SqlFormatInfo.ToSqlDateTimeString(DateTime.UtcNow.AddHours(-24)));
			}

			CreateInventoryHoldChangeLog();

			var indexProvider = ((ITransformationIndexProvider)this).IndexProvider;
			var arrivalDateCoveringIndex = indexProvider.SingleOrDefault(i => i.Filter.EndsWith("[WE_AdjustmentArrivalDate] IS NOT NULL", StringComparison.InvariantCulture));
			DbObjectCreator.CreateIndexIfNotExists(Db.Connection, WhsDocketLineSchema.Constants.TableName, arrivalDateCoveringIndex.IndexName, arrivalDateCoveringIndex.SQL_Create);

			var (fromTime, upperBoundToTime) = GetFromAndToTime();
			if (upperBoundToTime.HasValue)
			{
				var stopWatch = Stopwatch.StartNew();

				while (fromTime.GetValueOrDefault() < upperBoundToTime)
				{
					var maxUpdatedTime = UpdateChunk(fromTime, upperBoundToTime.Value);
					fromTime = maxUpdatedTime ?? upperBoundToTime;

					if (stopWatch.Elapsed.TotalMinutes > 1)
					{
						var fromTimeString = SqlFormatInfo.ToSqlDateTimeString(fromTime.Value);
						ExtProperty.Database.Update(Db.Connection, FromTimeName, fromTimeString);
						manager.ShowInfoMessage($"Finished processing Hold Code Logs for Current Stock up to: {fromTimeString}.");

						stopWatch.Restart();
					}
				}

				ExtProperty.Database.Delete(Db.Connection, FromTimeName);
				ExtProperty.Database.Delete(Db.Connection, ToTimeName);
			}

			void CreateInventoryHoldChangeLog()
			{
				var createTableSql = @"
CREATE TABLE [dbo].[WhsInventoryHoldChangeLog]
(
	[WHL_PK] [uniqueidentifier] NOT NULL,
	[WHL_WHC_NKCode] [varchar](8) NOT NULL CONSTRAINT [DF_WhsInventoryHoldChangeLog_WHL_WHC_NKCode] DEFAULT (''),
	[WHL_Reason] [varchar](50) NOT NULL CONSTRAINT [DF_WhsInventoryHoldChangeLog_WHL_Reason] DEFAULT (''),
	[WHL_WE_ParentDocketLine] [uniqueidentifier] NOT NULL,
	[WHL_LogVersion] [smallint] NOT NULL CONSTRAINT [DF_WhsInventoryHoldChangeLog_WHL_LogVersion] DEFAULT ((0)),
	[WHL_SystemCreateTimeUtc] [datetime] NOT NULL,
	[WHL_SystemCreateUser] [varchar](3) NOT NULL CONSTRAINT [DF_WhsInventoryHoldChangeLog_WHL_SystemCreateUser] DEFAULT (''),
	[WHL_SystemLastEditTimeUtc] [datetime] NOT NULL,
	[WHL_SystemLastEditUser] [varchar](3) NOT NULL CONSTRAINT [DF_WhsInventoryHoldChangeLog_WHL_SystemLastEditUser] DEFAULT (''),
	[WHL_EventTime] [datetimeoffset](0) NOT NULL,
	CONSTRAINT [PK_UX__WHL_PK] PRIMARY KEY NONCLUSTERED 
	(
		[WHL_PK] ASC
	),
	CONSTRAINT [WhsInventoryHoldChangeLog_WHL_WE_ParentDocketLine_FK2_WhsDocketLine_RRR_120N] FOREIGN KEY([WHL_WE_ParentDocketLine])
	REFERENCES [dbo].[WhsDocketLine] ([WE_PK]),
	CONSTRAINT [Constraint_WHL_LogVersion] CHECK ([WHL_LogVersion]>(0)),
	CONSTRAINT [Constraint_WHL_SystemCreateUser] CHECK ([WHL_SystemCreateUser]<>''),
	CONSTRAINT [Constraint_WHL_SystemLastEditUser] CHECK ([WHL_SystemLastEditUser]<>''),
	CONSTRAINT [NR_UC__WHL_WE_ParentDocketLine_WHL_LogVersion] UNIQUE CLUSTERED
	(
		[WHL_WE_ParentDocketLine] ASC,
		[WHL_LogVersion] ASC
	)
)";

				DbObjectCreator.CreateTableIfNotExists(Db.Connection, WhsInventoryHoldChangeLogSchema.Constants.TableName, createTableSql);

				if (!DbObjectCreator.ColumnExists(Db.Connection, WhsInventoryHoldChangeLogSchema.Constants.TableName, WhsInventoryHoldChangeLogSchema.Constants.WHL_EventTime))
				{
					// It's possible to have a system that has the table but not the column WHL_EventTime as it was added after the Table
					// There should be no existing rows as this table was not exposed in the application till after this column was added.
					Db.Connection.ExecuteNonQuery("ALTER TABLE WhsInventoryHoldChangeLog ADD WHL_EventTime datetimeoffset(0) NOT NULL");
				}

				// Since we are in the online PRE-upgrade - we might have the old version of this trigger which is NOT deferrable and would not
				// allow this transform to succeed, so we simply drop it here (as there will be no usage of this table in the pre-upgrade phase).
				DbObjectCreator.DropTriggerIfExists(Db.Connection, "TG_WhsInventoryHoldChangeLog_LogVersionCheck");
			}

			(DateTime? FromTime, DateTime? UpperBoundToTime) GetFromAndToTime()
			{
				DateTime? fromTime, upperBoundTime;

				var toTimeString = ExtProperty.Database.Select(Db.Connection, ToTimeName);
				if (string.IsNullOrEmpty(toTimeString))
				{
					upperBoundTime = GetUpperBoundTimeFromDatabase();
					if (upperBoundTime.HasValue)
					{
						ExtProperty.Database.Update(Db.Connection, ToTimeName, SqlFormatInfo.ToSqlDateTimeString(upperBoundTime.Value));
					}

					fromTime = null;
				}
				else
				{
					var fromTimeString = ExtProperty.Database.Select(Db.Connection, FromTimeName);
					fromTime = SqlFormatInfo.TryParseFromSqlDateTime(fromTimeString, out var parsedFromTime) ? parsedFromTime : null;
					upperBoundTime = SqlFormatInfo.TryParseFromSqlDateTime(toTimeString, out var parsedToTime) ? parsedToTime : null;
				}

				return (fromTime, upperBoundTime);
			}

			static DateTime? GetUpperBoundTimeFromDatabase()
			{
				var getTimeSql = @"
SELECT
	MAX(WE_AdjustmentArrivalDate) as ToTime
FROM
	dbo.WhsDocketLine
WHERE
	WE_DocketLineType IN ('INW', 'ADJ', 'TFR') AND
	WE_StockOnHand > 0 AND
	WE_AdjustmentArrivalDate IS NOT NULL";

				return Db.Connection.ExecuteScalar(getTimeSql) is DateTime toTime ? toTime : null;
			}

			DateTime? UpdateChunk(DateTime? fromTime, DateTime toTime)
			{
				var sql = TransformStmALogToWhsInventoryHoldChangeLogSQL(TransformationSection.OnlinePreUpgrade, hasHighWaterMark: false, fromTime.HasValue);

				using (var manager = Db.Connection.BeginTransactionWithManager())
				using (var command = Db.Connection.Command(sql))
				{
					// Arrival Date will be smalldatetime when this preupgrade transform runs.
					if (fromTime.HasValue)
					{
						command.AddParameter("@from", SqlDbType.SmallDateTime, fromTime);
					}

					command.AddParameter("@to", SqlDbType.SmallDateTime, toTime);

					var result = command.ExecuteScalar() is DateTime rawUpdatedTime ? rawUpdatedTime : (DateTime?)null;
					manager.CommitTransaction();
					return result;
				}
			}
		}

		protected override void OfflinePostUpgradeTransform()
		{
			ExtProperty.Database.Delete(Db.Connection, IsNewTransform);

			var highWaterMarkString = ExtProperty.Database.Select(Db.Connection, HighWaterMarkName);
			var hasHighWaterMark = !string.IsNullOrEmpty(highWaterMarkString);
			var sql = TransformStmALogToWhsInventoryHoldChangeLogSQL(TransformationSection.OfflinePostUpgrade, hasHighWaterMark, hasLowerBoundTime: false);

			if (hasHighWaterMark)
			{
				var highWaterMark = SqlFormatInfo.FromSqlDateTime(highWaterMarkString);
				using (var command = Db.Connection.Command(sql))
				{
					command.AddParameter("@HighWaterMark", SqlDbType.SmallDateTime, highWaterMark);
					command.ExecuteNonQuery();
				}
			}
			else
			{
				Db.Connection.ExecuteNonQuery(sql);
			}

			ExtProperty.Database.Delete(Db.Connection, HighWaterMarkName);
		}

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			var lastDocketPKString = ExtProperty.Database.Select(Db.Connection, LastDocketPKName);
			var (lastDocketPK, maxDocketPK, totalDockets, currentCount) = string.IsNullOrEmpty(lastDocketPKString)
				? GetInitialDocketInfo()
				: GetCurrentDocketInfo(lastDocketPKString);

			var stopWatch = Stopwatch.StartNew();

			while (lastDocketPK != maxDocketPK)
			{
				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				{
					lastDocketPK = UpdateChunk(lastDocketPK, maxDocketPK) ?? maxDocketPK;
					transactionManager.CommitTransaction();
				}

				currentCount = currentCount + BatchSize > totalDockets ? totalDockets : currentCount + BatchSize;

				if (token.IsCancellationRequested || stopWatch.Elapsed.TotalMinutes > 1)
				{
					ExtProperty.Database.Update(Db.Connection, CurrentCountName, currentCount.ToString());
					ExtProperty.Database.Update(Db.Connection, LastDocketPKName, lastDocketPK.ToString());

					manager.ShowInfoMessage($@"Processed {currentCount} of {totalDockets} Jobs.");
					token.ThrowIfCancellationRequested();
					stopWatch.Restart();
				}
			}

			ClearExtPropertyTable();
		}

		TransformationIndexProvider ITransformationIndexProvider.IndexProvider
		{
			get
			{
				var indexProvider = new TransformationIndexProvider(this);
				indexProvider.New(WhsDocketLineSchema.Instance)
					.Key(WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc, WhsDocketLineSchema.Constants.PK)
					.Where("([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_TransactionQuantity]>=(0) AND [WE_StockOnHand]>(0) AND [WE_SystemLastEditTimeUtc] IS NOT NULL")
					.GetInfo();

				indexProvider.New(WhsDocketLineSchema.Instance)
					.Key(WhsDocketLineSchema.Constants.WE_IsOriginalInventory, WhsDocketLineSchema.Constants.PK, WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode)
					.Include(WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus, WhsDocketLineSchema.Constants.WE_StockOnHand, WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc, WhsDocketLineSchema.Constants.WE_SystemLastEditUser, "WE_AutoVersion")
					.Where("[WE_WE_ParentDocketLine] IS NOT NULL AND [WE_IsOriginalInventory]=(0)")
					.GetInfo();

				indexProvider.New(WhsDocketLineSchema.Instance)
					.Key(WhsDocketLineSchema.Constants.WE_AdjustmentArrivalDate)
					.Include(WhsDocketLineSchema.Constants.WE_StockOnHand, WhsDocketLineSchema.Constants.WE_DocketLineType, WhsDocketLineSchema.Constants.PK)
					.Where("[WE_StockOnHand]>(0) AND ([WE_DocketLineType] IN ('INW', 'ADJ', 'TFR')) AND [WE_AdjustmentArrivalDate] IS NOT NULL")
					.GetInfo();
				return indexProvider;
			}
		}

		static (Guid LastDocketPK, Guid MaxDocketPK, int TotalDockets, int CurrentCount) GetInitialDocketInfo()
		{
			var (minDocketPK, maxDocketPK, totalDockets) = GetDocketInfoFromDatabase();

			Guid lastDocketPK;

			if (minDocketPK == Guid.Empty)
			{
				lastDocketPK = Guid.Empty;
			}
			else
			{
				var previousDocketPK = Db.Connection.ExecuteScalar(
					"SELECT MAX(WE_WD) FROM WhsDocketLine WHERE WE_WD < @MinDocketPK",
					command => command.AddParameter("@MinDocketPK", SqlDbType.UniqueIdentifier, minDocketPK));

				lastDocketPK = previousDocketPK is Guid value ? value : Guid.Empty;
			}

			ExtProperty.Database.Update(Db.Connection, LastDocketPKName, lastDocketPK.ToString());
			ExtProperty.Database.Update(Db.Connection, MaxDocketPKName, maxDocketPK.ToString());
			ExtProperty.Database.Update(Db.Connection, TotalDocketsName, totalDockets.ToString());
			ExtProperty.Database.Update(Db.Connection, CurrentCountName, "0");

			return (lastDocketPK, maxDocketPK, totalDockets, 0);
		}

		static (Guid MinDocketPK, Guid MaxDocketPK, int TotalDockets) GetDocketInfoFromDatabase()
		{
			var getDocketInfoSql = @"
SELECT
	MIN(WD_PK) as MinDocketPK,
	MAX(WD_PK) as MaxDocketPK,
	COUNT(WD_PK) as TotalDockets
FROM
	dbo.WhsDocket
WHERE
	WD_DocketType NOT IN ('ORD', 'WOR', 'DWO') AND
	EXISTS
	(
		SELECT NULL
		FROM
			dbo.WhsDocketLine
		WHERE
			WE_WD = WD_PK AND
			WE_TransactionQuantity >= 0 AND
			WE_StockOnHand = 0
	)
";
			using (var command = Db.Connection.Command(getDocketInfoSql))
			using (var reader = command.ExecuteReader())
			{
				reader.Read();
				var totalDockets = (int)reader["TotalDockets"];
				var minDocketPK = totalDockets > 0 ? (Guid)reader["MinDocketPK"] : Guid.Empty;
				var maxDocketPK = totalDockets > 0 ? (Guid)reader["MaxDocketPK"] : Guid.Empty;

				return (minDocketPK, maxDocketPK, totalDockets);
			}
		}

		static (Guid LastDocketPK, Guid MaxDocketPK, int TotalDockets, int CurrentCount) GetCurrentDocketInfo(string lastUpdateDocketString)
		{
			var lastDocketPK = Guid.Parse(lastUpdateDocketString);
			var maxDocketPK = Guid.Parse(ExtProperty.Database.Select(Db.Connection, MaxDocketPKName));
			var currentCount = int.Parse(ExtProperty.Database.Select(Db.Connection, CurrentCountName));
			var totalDockets = int.Parse(ExtProperty.Database.Select(Db.Connection, TotalDocketsName));
			return (lastDocketPK, maxDocketPK, totalDockets, currentCount);
		}

		static Guid? UpdateChunk(Guid lastDocketPK, Guid maxDocketPK)
		{
			var sql = TransformStmALogToWhsInventoryHoldChangeLogSQL(TransformationSection.OnlinePostUpgrade, hasHighWaterMark: false, hasLowerBoundTime: false);
			var result = Db.Connection.ExecuteScalar(
			   sql,
			   cmd =>
			   {
				   cmd.AddParameter("@LastDocketPK", SqlDbType.UniqueIdentifier, lastDocketPK);
				   cmd.AddParameter("@MaxDocketPK", SqlDbType.UniqueIdentifier, maxDocketPK);
			   });

			return result is Guid value ? value : (Guid?)null;
		}

		static string GetDocketsForBatch()
		{
			var sql = $@"
DECLARE @MaxUpdatedDocketPK uniqueidentifier;

;WITH Dockets AS
(
	SELECT
		TOP({BatchSize}) WD_PK
	FROM
		dbo.WhsDocket
	WHERE
		EXISTS (SELECT NULL FROM WhsDocketLine WHERE WE_WD = WD_PK AND WE_DocketLineType NOT IN ('ORD', 'WOR', 'DWO') AND WE_TransactionQuantity >= 0 AND WE_StockOnHand = 0) AND
		WD_PK > @LastDocketPK AND
		WD_PK <= @MaxDocketPK
	ORDER BY
		WD_PK
)
";
			return sql;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1119:DoNotUseSLEventTimeTableColumn", Justification = "We need Event Time to Order Rows")]
		static string TransformStmALogToWhsInventoryHoldChangeLogSQL(TransformationSection section, bool hasHighWaterMark, bool hasLowerBoundTime)
		{
			var getBatch = GetBatch(section);
			var selectClause = GetSelectClause(section);
			var fromClause = GetFromClause(section);
			var whereClause = GetWhereClause(section);

			var autoVersionClause = section == TransformationSection.OfflinePostUpgrade
				? ", WE_AutoVersion = (WE_AutoVersion + 1) % 32768"
				: "";

			var populateVariable = GetPopulateVariableStatement(section);
			var selectVariableStatement = GetSelectVariableStatement(section);

			var preExec = section == TransformationSection.OfflinePostUpgrade
	? @"
EXEC dbo.SuspendTrigger 'TG_CP_INS_StmALogQueue';
EXEC dbo.SuspendTrigger 'TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised';
EXEC dbo.SuspendTrigger 'TG_WhsDocketLine_StockOnHandIsBalanced';
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsDocketLine_UpdateAutoVersion')
BEGIN
	DISABLE TRIGGER TG_WhsDocketLine_UpdateAutoVersion ON dbo.WhsDocketLine
END
"
	: "";
			var postExec = section == TransformationSection.OfflinePostUpgrade
	? @"
EXEC dbo.ResumeTrigger 'TG_CP_INS_StmALogQueue';
EXEC dbo.ResumeTrigger 'TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised';
EXEC dbo.ResumeTrigger 'TG_WhsDocketLine_StockOnHandIsBalanced';

IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_WhsDocketLine_UpdateAutoVersion')
BEGIN
	ENABLE TRIGGER TG_WhsDocketLine_UpdateAutoVersion ON dbo.WhsDocketLine;
END
"
	: "";

			var sql = $@"
BEGIN TRY
{preExec}

EXEC dbo.SuspendTrigger 'TG_WhsInventoryHoldChangeLog_LogVersionCheck'
EXEC dbo.SuspendTrigger 'TG_WhsInventoryHoldChangeLog_EnsureHoldCodeConsistency'

{getBatch}

SELECT
	WE_PK AS DocketLinePK,
	CASE WHEN PositionOfNew IS NULL THEN '' ELSE REPLACE(SUBSTRING(EventReference, PositionOfNew, CHARINDEX('|', EventReference, PositionOfNew + 1) - PositionOfNew), '''', '') END AS NewCodeValue, 
	CASE WHEN PositionOfOld IS NULL THEN '' ELSE REPLACE(SUBSTRING(EventReference, PositionOfOld, CHARINDEX('|', EventReference, PositionOfOld + 1) - PositionOfOld), '''', '') END AS OldCodeValue, 
	CASE WHEN PositionOfRes IS NULL THEN '' ELSE REPLACE(SUBSTRING(EventReference, PositionOfRes, CHARINDEX('|', EventReference, PositionOfRes + 1) - PositionOfRes), '''', '') END AS ReasonValue,
	ROW_NUMBER() OVER (PARTITION BY WE_PK ORDER BY SL_EventTime) AS LogVersion,
	ISNULL(SL_EventTimeOffset, SL_EventTime) AS EventTimeOffSet,
	SL_PostedTimeUtc AS CreateTime, 
	SL_GS_NKUser AS CreateUser,
	SL_PostedTimeUtc AS LastEditTime, 
	SL_GS_NKUser AS LastEditUser{selectClause}
INTO #InfoToLog
FROM
	{fromClause}
	{(section == TransformationSection.OfflinePostUpgrade ? "" : "LEFT ")}JOIN dbo.StmALog WITH (FORCESEEK INDEX([NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime])) ON
			SL_Parent = WE_PK AND
			SL_SE_NKEvent = 'CID' AND
			SL_Reference + '|' LIKE '%|TYP=Hold Code|%'
	JOIN dbo.WhsDocket Docket ON WE_WD = Docket.WD_PK
	JOIN dbo.WhsWarehouse ON Docket.WD_WW_Whs = WW_PK
	JOIN dbo.GlbBranch ON WW_GB_RelatedCompanyBranch = GB_PK
	CROSS APPLY
	(
		SELECT
			SL_Reference + '|' AS EventReference,
			PATINDEX('%|NEW=[^|]%|%', SL_Reference + '|' ) AS ParameterPositionOfNew,
			PATINDEX('%|RES=[^|]%|%', SL_Reference + '|' ) AS ParameterPositionOfRes,
			PATINDEX('%|OLD=[^|]%|%', SL_Reference + '|' ) AS ParameterPositionOfOld
	) ParameterInfo
	CROSS APPLY
	(
		SELECT
			CASE WHEN ParameterPositionOfNew > 0 THEN ParameterPositionOfNew + 5 END AS PositionOfNew,
			CASE WHEN ParameterPositionOfRes > 0 THEN ParameterPositionOfRes + 5 END AS PositionOfRes,
			CASE WHEN ParameterPositionOfOld > 0 THEN ParameterPositionOfOld + 5 END AS PositionOfOld
	) ParameterPositionInfo
	OUTER APPLY
	(
	  SELECT TOP 1 SL_EventTimeOffset
	  FROM
	    RefUNLOCOUtcOffset
	    CROSS APPLY (SELECT TODATETIMEOFFSET(SL_EventTime, ISNULL(RLO_OffsetMinutesFromUtc, 0)) AS SL_EventTimeOffset) AS SL_EventTimeOffset
	  WHERE
	    RLO_RL_NKCode = GB_RL_NKHomePort
	    AND TODATETIMEOFFSET(RLO_StartTimeUtc, RLO_OffsetMinutesFromUtc) <= SL_EventTime
	    AND TODATETIMEOFFSET(RLO_EndTimeUtc, RLO_OffsetMinutesFromUtc) > SL_EventTime
	  ORDER BY
	    SL_EventTimeOffset DESC
	) AS SL_EventTimeOffset
{(string.IsNullOrEmpty(whereClause) ? "" : "WHERE")}{whereClause}

INSERT INTO dbo.WhsInventoryHoldChangeLog (WHL_PK, WHL_WE_ParentDocketLine, WHL_WHC_NKCode, WHL_Reason, WHL_LogVersion, WHL_EventTime, WHL_SystemCreateTimeUtc, WHL_SystemCreateUser, WHL_SystemLastEditTimeUtc, WHL_SystemLastEditUser)
SELECT
	NEWID(),
	DocketLinePK,
	NewCodeValue, 
	ReasonValue,
	LogVersion,
	EventTimeOffSet,
	CreateTime, 
	CreateUser,
	LastEditTime, 
	LastEditUser
FROM
	#InfoToLog
WHERE
	CreateTime IS NOT NULL
	AND NOT EXISTS
	(
		SELECT NULL
		FROM
			dbo.WhsInventoryHoldChangeLog
		WHERE
			WHL_WE_ParentDocketLine = DocketLinePK
			AND WHL_LogVersion = LogVersion
	)

DECLARE @updatedDocketLines TABLE (
	WE_PK UNIQUEIDENTIFIER,
	LogReference VARCHAR(255)
)

UPDATE
	dbo.WhsDocketLine
SET
	WE_OriginalInventoryStatus = IIF(OldCodeValue = '', 'AVL', 'HEL'),
	WE_WHC_NKOriginalInventoryHeldCode = OldCodeValue,
	WE_SystemLastEditUser = '~BP',
	WE_SystemLastEditTimeUtc = GetUtcDate()
	{autoVersionClause}
OUTPUT
	INSERTED.WE_PK,
	CONCAT('TransformStmALogToWhsInventoryHoldChangeLog: Updated WE_OriginalInventoryStatus from ''', DELETED.WE_OriginalInventoryStatus, ''' to ''', INSERTED.WE_OriginalInventoryStatus, ''' and WE_WHC_NKOriginalInventoryHeldCode from ''', DELETED.WE_WHC_NKOriginalInventoryHeldCode, ''' to ''', INSERTED.WE_WHC_NKOriginalInventoryHeldCode, '''.')
INTO
	@updatedDocketLines
FROM
	#InfoToLog
WHERE
	DocketLinePK = WE_PK
	AND WE_WE_ParentDocketLine IS NOT NULL
	AND WE_IsOriginalInventory = 0 
	AND LogVersion = 1
	AND WE_WHC_NKOriginalInventoryHeldCode != OldCodeValue

INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_EventTime, SL_GS_NKUser)
SELECT
	NEWID(),
	'WhsDocketLine',
	WE_PK,
	'MIS',
	LogReference,
	GETDATE(),
	'~BP'
FROM
	@updatedDocketLines

{populateVariable}

DROP TABLE #InfoToLog

EXEC dbo.ResumeTrigger 'TG_WhsInventoryHoldChangeLog_LogVersionCheck'
EXEC dbo.ResumeTrigger 'TG_WhsInventoryHoldChangeLog_EnsureHoldCodeConsistency'

{postExec}

{selectVariableStatement}
END TRY
BEGIN CATCH
	THROW
END CATCH";

			return sql;

			string GetSelectClause(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return @",
	MaxAdjustmentArrivalDate = MAX(WE_AdjustmentArrivalDate) OVER()";
					case TransformationSection.OfflinePostUpgrade: return "";
					case TransformationSection.OnlinePostUpgrade: return @",
	MaxDocketPK = MAX(WE_WD) OVER()";
					default: throw new NotSupportedException();
				}
			}

			string GetFromClause(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return "DocketLines";
					case TransformationSection.OfflinePostUpgrade: return "dbo.WhsDocketLine";
					case TransformationSection.OnlinePostUpgrade: return @"Dockets
	JOIN dbo.WhsDocketLine on WE_WD = WD_PK";
					default: throw new NotSupportedException();
				}
			}

			string GetWhereClause(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return "";
					case TransformationSection.OfflinePostUpgrade: return hasHighWaterMark
						? @"
WE_DocketLineType IN ('INW', 'ADJ', 'TFR') AND WE_TransactionQuantity >= 0
AND WE_StockOnHand > 0
AND WE_SystemLastEditTimeUtc > @HighWaterMark
AND WE_SystemLastEditTimeUtc IS NOT NULL"
						: @"
WE_DocketLineType IN ('INW', 'ADJ', 'TFR') AND WE_TransactionQuantity >= 0
AND WE_StockOnHand > 0";
					case TransformationSection.OnlinePostUpgrade: return @"
WE_DocketLineType NOT IN ('ORD', 'WOR', 'DWO') AND WE_TransactionQuantity >= 0
AND WE_StockOnHand = 0";
					default: throw new NotSupportedException();
				}
			}

			string GetBatch(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return GetDocketLinesForBatch(hasLowerBoundTime);
					case TransformationSection.OfflinePostUpgrade: return "";
					case TransformationSection.OnlinePostUpgrade: return GetDocketsForBatch();
					default: throw new NotSupportedException();
				}
			}

			string GetDocketLinesForBatch(bool hasLowerBoundTime)
			{
				var fromTimeWhereClause = hasLowerBoundTime
					? @"
		WE_AdjustmentArrivalDate > @from AND"
					: "";
				return $@"
DECLARE @MaxArrivalDate smalldatetime;

;WITH DocketLines AS
(
	SELECT TOP({BatchSize}) WITH TIES
		WE_PK,
		WE_WD,
		WE_AdjustmentArrivalDate
	FROM
		dbo.WhsDocketLine
	WHERE
		WE_DocketLineType IN ('INW', 'ADJ', 'TFR') AND
		WE_StockOnHand > 0 AND{fromTimeWhereClause}
		WE_AdjustmentArrivalDate <= @to AND
		WE_AdjustmentArrivalDate IS NOT NULL
	ORDER BY
		WE_AdjustmentArrivalDate
)";
			}

			string GetPopulateVariableStatement(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return "SET @MaxArrivalDate = (SELECT TOP 1 MaxAdjustmentArrivalDate FROM #InfoToLog)";
					case TransformationSection.OfflinePostUpgrade: return "";
					case TransformationSection.OnlinePostUpgrade: return "SET @MaxUpdatedDocketPK = (SELECT TOP 1 MaxDocketPK FROM #InfoToLog)";
					default: throw new NotSupportedException();
				}
			}

			string GetSelectVariableStatement(TransformationSection section)
			{
				switch (section)
				{
					case TransformationSection.OnlinePreUpgrade: return "SELECT @MaxArrivalDate";
					case TransformationSection.OfflinePostUpgrade: return "";
					case TransformationSection.OnlinePostUpgrade: return "SELECT @MaxUpdatedDocketPK";
					default: throw new NotSupportedException();
				}
			}
		}

		void ClearExtPropertyTable()
		{
			ExtProperty.Database.Delete(Db.Connection, LastDocketPKName);
			ExtProperty.Database.Delete(Db.Connection, MaxDocketPKName);
			ExtProperty.Database.Delete(Db.Connection, TotalDocketsName);
			ExtProperty.Database.Delete(Db.Connection, CurrentCountName);
		}
	}
}
