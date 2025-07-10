[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(CargoWise.Bi.Common.BiConstants))]

namespace Enterprise.ChangeDataCapture.Common
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.Globalization;
	using CargoWise.Data;

	#region SuppressResourceStringsCheckRegion

	public abstract class CdcScanner
	{
		public virtual CdcScannerLogger CdcScannerLogger { get; set; }

		protected CdcScanProvider cdcScanProvider;
		public virtual CdcScanProvider CdcScanProvider
		{
			get
			{
				if (cdcScanProvider == null)
				{
					cdcScanProvider = new CdcScanProvider(DedicatedNonPooledCdcConnection, CdcScannerLogger);
				}
				return cdcScanProvider;
			}
		}

		public virtual long ScanUntilNoTransactionsToProcess()
		{
			return ScanUntilNoTransactionsToProcess(shouldAbort: () => false);
		}

		public long ScanUntilNoTransactionsToProcess(Func<bool> shouldAbort)
		{
			long accumulatedProcessedTransactionCount;
			using (DedicatedNonPooledCdcConnection)
			{
				var lastLogScanResult = GetLastLogScanResult();
				var initialSessionId = lastLogScanResult.SessionId;
				var endLsnMapping = new HashSet<string>
				{
					lastLogScanResult.EndLsn
				};

				bool stop = false;
				var loopCounter = 0;
				var maxTransSetCounter = 0;
				var timer = new Stopwatch();
				timer.Start();

				while (!stop && !shouldAbort())
				{
					CallCdcScanProcedure();
					var newLogScanResult = GetLastLogScanResult();

					if (endLsnMapping.Contains(newLogScanResult.EndLsn))
					{
						CdcScanProvider.MaxTransactions += 1000;
						maxTransSetCounter++;
						if (maxTransSetCounter > 5)
						{
							CdcScannerLogger.Error($"CDC Scan failed due to repeating scan sessions for the same LSN range being detected. @maxtransactions was automatically increased a maximum of 5 times to {CdcScanProvider.MaxTransactions} but failed to progress the log reader.");
							break;
						}

						accumulatedProcessedTransactionCount = GetAccumulatedProcessedTransactionCount(DedicatedNonPooledCdcConnection, initialSessionId);
						if (accumulatedProcessedTransactionCount > 0)
						{
							CdcScannerLogger.Debug($"Repeating scan sessions for the same LSN range have been detected. The system has temporarily increased @maxtransactions to {CdcScanProvider.MaxTransactions} in order to progress the log reader. This message requires no action.");
						}
					}

					if (newLogScanResult.HasNoNewEmptyScansSinceLastLog(lastLogScanResult))
					{
						if (loopCounter % 10 == 0)
						{
							endLsnMapping.Clear();
						}
						endLsnMapping.Add(newLogScanResult.EndLsn);
						lastLogScanResult = newLogScanResult;
						CdcScannerLogger.LogStatus(timer, newLogScanResult.CommitLsn, newLogScanResult.EndTimeUtc);
					}
					else if (newLogScanResult.HasCompletedScans(lastLogScanResult))
					{
						if (!lastLogScanResult.HasScannedNoTransactions())
						{
							CdcScannerLogger.Log($"Completed capturing changes for transactions up to LSN: 0x{lastLogScanResult.CommitLsn}, Transaction Date (UTC): {lastLogScanResult.EndTimeUtc}");
						}
						stop = true;
					}
					loopCounter++;
				}
				accumulatedProcessedTransactionCount = GetAccumulatedProcessedTransactionCount(DedicatedNonPooledCdcConnection, initialSessionId);
			}
			return accumulatedProcessedTransactionCount;
		}

		public static (int Count, TimeSpan Age) GetQueueSizeAndAge()
		{
			return CdcScannerLogger.GetQueueSizeAndAge();
		}

		internal virtual LogScanResult GetLastLogScanResult()
		{
			var result = new LogScanResult();

			var logScanSessionSQL = @"
				IF EXISTS(SELECT null FROM sys.dm_cdc_log_scan_sessions)
					SELECT TOP 1 session_id, empty_scan_count, last_commit_lsn, end_time, end_lsn, duration FROM sys.dm_cdc_log_scan_sessions ORDER BY session_id DESC";
			using (var reader = DedicatedNonPooledCdcConnection.Command(logScanSessionSQL).ExecuteReader())
			{
				return HydrateLogScanResult(result, reader);
			}
		}

		LogScanResult HydrateLogScanResult(LogScanResult result, IDataReader reader)
		{
			if (reader.Read())
			{
				result.SessionId = Convert.ToInt32(reader[CdcScannerConstants.SessionIDColumn], CultureInfo.InvariantCulture);
				result.EmptyScanCount = Convert.ToInt32(reader[CdcScannerConstants.EmptyScanCountColumn], CultureInfo.InvariantCulture);
				result.CommitLsn = Convert.ToString(reader[CdcScannerConstants.LastCommitLSNColumn], CultureInfo.InvariantCulture).Replace(CdcScannerConstants.SemiColon, string.Empty);
				result.EndTimeUtc = Convert.ToDateTime(reader[CdcScannerConstants.EndTimeColumn], CultureInfo.InvariantCulture).ToUniversalTime();
				result.EndLsn = Convert.ToString(reader[CdcScannerConstants.EndLSNColumn], CultureInfo.InvariantCulture).Replace(CdcScannerConstants.SemiColon, string.Empty);
				result.DurationInSeconds = Convert.ToInt32(reader[CdcScannerConstants.DurationColumn], CultureInfo.InvariantCulture);
			}
			return result;
		}

		long GetAccumulatedProcessedTransactionCount(DbConnection conn, int previousSessionId)
		{
			string sqlText = "SELECT sum(tran_count) FROM sys.dm_cdc_log_scan_sessions WHERE session_id > @previousSessionId";
			using (var cmd = conn.Command(sqlText))
			{
				cmd.AddParameter("@previousSessionId", SqlDbType.Int, previousSessionId);
				var objResult = cmd.ExecuteScalar();
				return (objResult == DBNull.Value) ? 0 : Convert.ToInt64(objResult, CultureInfo.InvariantCulture);
			}
		}

		internal virtual void CallCdcScanProcedure(int scanAttempts = 0)
		{
			CdcScanProvider.ExecuteProcedure(scanAttempts);
		}

		public void SetScanTimeout(int cmdTimeoutInSeconds)
		{
			CdcScanProvider.CommandTimeout = TimeSpan.FromSeconds(cmdTimeoutInSeconds);
		}

		protected virtual DbConnection DedicatedNonPooledCdcConnection
		{
			get
			{
				return (dedicatedNonPooledCDCConnection = dedicatedNonPooledCDCConnection ?? DedicatedConnectionForCDCScan.New());
			}
		}
		DedicatedConnectionForCDCScan dedicatedNonPooledCDCConnection;
	}

	#endregion
}
