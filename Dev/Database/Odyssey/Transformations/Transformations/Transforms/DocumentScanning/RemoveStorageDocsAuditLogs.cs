using System;
using System.Diagnostics;
using System.Threading;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning
{
	class RemoveStorageDocsAuditLogs : DataTransformation
	{
		public override string UserDescription => "Remove StorageDocs Audit Logs from StmALog table";

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var processed = ExtProperty.Database.Select(Db.Connection, RemoveStorageDocsAuditLogsProcessed);
			var processedDateTime = DateTime.MaxValue;
			if (string.IsNullOrEmpty(processed) || !DateTime.TryParse(processed, out processedDateTime))
			{
				// Retrieve min event post time
				var lowestWatermarkValue = Db.Connection.ExecuteScalar("SELECT MIN(SL_PostedTimeUtc) FROM dbo.StmALog");
				if (lowestWatermarkValue != DBNull.Value && lowestWatermarkValue != null)
				{
					processedDateTime = (DateTime)lowestWatermarkValue;
				}
			}

			var stopWatch = Stopwatch.StartNew();
			var sqlText = $@"
DECLARE @top int = 10000
WHILE 1 = 1
BEGIN
    DELETE TOP (@top) FROM dbo.StmALog WHERE SL_PostedTimeUtc >= {LowWatermarkName} AND SL_PostedTimeUtc < {HighWatermarkName} AND SL_Table = 'StorageDocs' OPTION (MAXDOP 1)
    IF @@ROWCOUNT < @top BREAK
END";

#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
			while (processedDateTime <= DateTime.UtcNow)
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
			{
				var processTo = processedDateTime.AddHours(1);
				_ = Db.Connection.ExecuteNonQuery(sqlText, cmd =>
				{
					_ = cmd.AddParameter(LowWatermarkName, System.Data.SqlDbType.DateTime, processedDateTime);
					_ = cmd.AddParameter(HighWatermarkName, System.Data.SqlDbType.DateTime, processTo);
				});

				if (stopWatch.Elapsed > TimeSpan.FromMinutes(1) || token.IsCancellationRequested)
				{
					var processToText = processTo.ToString("yyyy-MM-dd HH:mm:ss");
					ExtProperty.Database.Update(Db.Connection, RemoveStorageDocsAuditLogsProcessed, processToText);
					manager?.ShowInfoMessage($"Finished removing StorageDocs audit logs up to {processToText}.");
					stopWatch.Restart();
				}

				processedDateTime = processTo;
				token.ThrowIfCancellationRequested();
			}

			ExtProperty.Database.Delete(Db.Connection, RemoveStorageDocsAuditLogsProcessed);
			return;
		}

		public const string RemoveStorageDocsAuditLogsProcessed = "RemoveStorageDocsAuditLogs.Processed";
		const string HighWatermarkName = "@highWatermark";
		const string LowWatermarkName = "@lowWatermark";
	}
}
