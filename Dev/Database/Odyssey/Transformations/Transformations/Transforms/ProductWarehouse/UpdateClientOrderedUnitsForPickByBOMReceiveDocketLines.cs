using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.Common;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ProductWarehouse;

public class UpdateClientOrderedUnitsForPickByBOMReceiveDocketLines : DataTransformation, ITransformationIndexProvider
{
	public override string UserDescription => "Update ClientOrderedUnits To WE_TransactionQuantity on Pick by BOM Receive Lines";

	const string FromTimeName = "UpdateClientOrderedUnitsForPickByBOMReceiveDocketLines.From";
	const int BatchSize = 1000;

	public TransformationIndexProvider IndexProvider
	{
		get
		{
			var indexProvider = new TransformationIndexProvider(this);

			indexProvider.New("dbo", "WhsDocketLine")
				.Key("WE_DocketLineStatus")
				.Key("WE_ClientOrderedUnits")
				.Key("WE_TransactionQuantity")
				.Key("WE_DocketLineType")
				.Where("[WE_DocketLineStatus]<>'FIN'")
				.Where("[WE_ClientOrderedUnits]=0")
				.Where("[WE_TransactionQuantity]>0")
				.Where("[WE_DocketLineType]='INW'")
				.GetInfo();

			return indexProvider;
		}
	}

	protected override void OfflinePostUpgradeTransform()
	{
		var sql = @"
UPDATE
	dbo.WhsDocketLine
SET
	WE_ClientOrderedUnits = WE_TransactionQuantity,
	WE_SystemLastEditTimeUtc = GetUtcDate(),
	WE_SystemLastEditUser = '~BP'
FROM
	dbo.WhsDocketLine
	INNER JOIN dbo.WhsDocket on WE_WD = WD_PK
WHERE
	WE_DocketLineStatus <> 'FIN'
	AND WE_ClientOrderedUnits = 0
	AND WE_TransactionQuantity > 0
	AND WE_DocketLineType = 'INW'
	AND WhsDocket.WD_WP_ParentPickForReceive IS NOT NULL
 ";
		Db.Connection.ExecuteNonQuery(sql);
	}

	protected override void OnlinePostUpgradeTransform(CancellationToken token)
	{
		var fromTimeString = ExtProperty.Database.Select(Db.Connection, FromTimeName);
		DateTimeOffset? fromTime;
		if (string.IsNullOrEmpty(fromTimeString))
		{
			fromTime = GetInitialFromTime();
		}
		else
		{
			SqlFormatInfo.TryParseFromSqlDateTimeOffset(fromTimeString, out var parsedTime);
			fromTime = parsedTime;
		}

		var loggingStopWatch = Stopwatch.StartNew();
		while (fromTime != null)
		{
			var (maxProcessedFinalizedDate, processedCount) = UpdateChunk(fromTime.Value);
			fromTime = maxProcessedFinalizedDate;
			if (token.IsCancellationRequested || loggingStopWatch.Elapsed.TotalMinutes > 1)
			{
				if (maxProcessedFinalizedDate != null)
				{
					ExtProperty.Database.Update(Db.Connection, FromTimeName, SqlFormatInfo.ToSqlDateTimeOffsetString(maxProcessedFinalizedDate.Value));
					manager?.ShowInfoMessage($@"Processed {processedCount} Receive Docket Lines with WE_FinalisedDate up to {maxProcessedFinalizedDate}.");
				}
				token.ThrowIfCancellationRequested();
				loggingStopWatch.Restart();
			}
		}

		ExtProperty.Database.Delete(Db.Connection, FromTimeName);
	}

	DateTimeOffset? GetInitialFromTime()
	{
		DateTimeOffset? fromTime;
		var getTimeSql = @"
SELECT
	DATEADD(MINUTE, -1, MIN(WE_FinalisedDate)) AS MinFinalisedTime
FROM
	dbo.WhsDocketLine
	INNER JOIN dbo.WhsDocket on WE_WD = WD_PK
WHERE
	WE_DocketLineStatus = 'FIN'
	AND WE_ClientOrderedUnits = 0
	AND WE_TransactionQuantity > 0
	AND WE_DocketLineType = 'INW'
	AND WhsDocket.WD_WP_ParentPickForReceive IS NOT NULL
";
		using (var command = Db.Connection.Command(getTimeSql))
		{
			using (var reader = command.ExecuteReader())
			{
				fromTime = reader.Read() && reader["MinFinalisedTime"] is not DBNull ? (DateTimeOffset)reader["MinFinalisedTime"] : null;
			}
		}

		if (fromTime != null)
		{
			ExtProperty.Database.Update(Db.Connection, FromTimeName, SqlFormatInfo.ToSqlDateTimeOffsetString(fromTime.Value));
		}

		return fromTime;
	}

	(DateTimeOffset?, int) UpdateChunk(DateTimeOffset minFinalisedTime)
	{
		var sql = @$"
SELECT
	TOP {BatchSize} WITH TIES
	WE_PK AS FinalisedPK,
	WE_FinalisedDate AS FinalisedMaxTime
INTO
	#WhsFinalisedDocketLine
FROM
	dbo.WhsDocketLine
	INNER JOIN dbo.WhsDocket on WE_WD = WD_PK
WHERE
	WE_FinalisedDate > @MinFinalisedTime
	AND WE_DocketLineStatus = 'FIN'
	AND WE_ClientOrderedUnits = 0
	AND WE_TransactionQuantity > 0
	AND WE_DocketLineType = 'INW'
	AND WhsDocket.WD_WP_ParentPickForReceive IS NOT NULL
ORDER BY
	WE_FinalisedDate ASC
OPTION (FAST {BatchSize})

UPDATE
	dbo.WhsDocketLine
SET
	WE_ClientOrderedUnits = WE_TransactionQuantity,
	WE_SystemLastEditTimeUtc = GetUtcDate(),
	WE_SystemLastEditUser = '~BP'
WHERE 
	WE_PK IN (SELECT FinalisedPK FROM #WhsFinalisedDocketLine)
OPTION (MAXDOP 1)

DECLARE @ProcessedCount INT = @@rowcount;
DECLARE @MaxProcessedTime DATETIMEOFFSET = (SELECT MAX(FinalisedMaxTime) FROM #WhsFinalisedDocketLine);

DROP TABLE #WhsFinalisedDocketLine;

SELECT @MaxProcessedTime AS MinFinalisedTime, @ProcessedCount AS ProcessedCount
";

		DateTimeOffset? newMinFinalisedTime = null;
		var processedCount = 0;
		using (var manager = Db.Connection.BeginTransactionWithManager())
		using (var command = Db.Connection.Command(sql))
		{
			command.AddParameter("@MinFinalisedTime", SqlDbType.DateTimeOffset, minFinalisedTime);
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					var readerMinFinalisedTime = reader["MinFinalisedTime"];
					newMinFinalisedTime = readerMinFinalisedTime == DBNull.Value ? null : (DateTimeOffset)readerMinFinalisedTime;
					processedCount = (int)reader["ProcessedCount"];
				}
			}
			manager.CommitTransaction();
		}

		return (newMinFinalisedTime, processedCount);
	}
}
