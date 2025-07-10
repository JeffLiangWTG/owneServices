using System;
using System.Data;
using System.Diagnostics;
using CargoWise.Data;

namespace Enterprise.ChangeDataCapture.Common
{
	public abstract class CdcScannerLogger
	{
		public virtual TimeSpan LogInterval => TimeSpan.FromSeconds(60);

		public abstract void Log(string logMessage);
		public abstract void Debug(string errorMesage);
		public abstract void Error(string errorMesage);
		public abstract void Warning(string warningMessage);

		public void LogStatus(Stopwatch timer, string transactionCommitLsn, DateTime transactionEndTimeUtc)
		{
			if (timer.Elapsed.TotalSeconds >= LogInterval.TotalSeconds)
			{
				Log($"Captured changes for transactions up to LSN: 0x{transactionCommitLsn}, Transaction Date (UTC): {transactionEndTimeUtc}");
				LogRemainingTransactions(timer);
				timer.Restart();
			}
		}

		void LogRemainingTransactions(Stopwatch timer)
		{
			var lastQueueSize = QueueSize;
			QueueSize = GetQueueSizeAndAge().Count;
			var clearRate = (lastQueueSize - QueueSize) / timer.Elapsed.TotalSeconds;
			Log($"Number of transaction in the log awaiting CDC Scan: {QueueSize} @ {(int)clearRate} transactions per second");
		}

		int? queueSize;
		public int QueueSize
		{
			get
			{
				if (queueSize == null)
				{
					queueSize = GetQueueSizeAndAge().Count;
				}
				return queueSize.Value;
			}
			private set
			{
				queueSize = value;
			}
		}

		public static (int Count, TimeSpan Age) GetQueueSizeAndAge()
		{
			var sqlText = @"
				DECLARE @BacklogCount int = 0
				DECLARE @ReplicationLatency int = 0
				DECLARE @LatencyResult TABLE
					(
					[database] sysname,
					[replicated transactions] int,
					[replication rate trans/sec] float,
					[replication latency (sec)] float,
					replbeginlsn binary(10),
					replnextlsn binary(10)
					)
				INSERT INTO @LatencyResult ([database], [replicated transactions], [replication rate trans/sec], [replication latency (sec)], replbeginlsn, replnextlsn) EXEC sp_replcounters;
				SELECT TOP 1
					@BacklogCount = ISNULL([replicated transactions], 0),
					@ReplicationLatency = ISNULL([replication latency (sec)] ,0)
				FROM @LatencyResult
				WHERE
					[database] = @CurrentDatabase

				SELECT @BacklogCount, @ReplicationLatency
			";

			try
			{
				using (var connection = Db.NewAdminConnection())
				using (var cmd = connection.Command(sqlText))
				{
					cmd.AddParameter("@CurrentDatabase", SqlDbType.NVarChar, Db.DatabaseName);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return (reader.GetInt32(0), TimeSpan.FromSeconds(reader.GetInt32(1)));
						}
						return (-1, TimeSpan.Zero);
					}
				}
			}
			catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.InvalidObjectName)
			{
				return (-1, TimeSpan.Zero);
			}
		}
	}
}
