using System;

namespace Enterprise.ChangeDataCapture.Common
{
	internal class LogScanResult
	{
		public LogScanResult()
		{
			this.SessionId = -1;
			this.EmptyScanCount = 0;
			this.CommitLsn = string.Empty;
			this.EndTimeUtc = new DateTime(); // assigns default value 01/01/0001 00:00:00
			this.DurationInSeconds = 0;
		}

		public LogScanResult(int sessionId, int emptyScanCount, string commitLsn, DateTime endTimeUtc, string endLsn, int durationInSeconds)
		{
			this.SessionId = sessionId;
			this.EmptyScanCount = emptyScanCount;
			this.CommitLsn = commitLsn;
			this.EndTimeUtc = endTimeUtc;
			this.EndLsn = endLsn;
			this.DurationInSeconds = durationInSeconds;
		}

		public int SessionId;
		public int EmptyScanCount;
		public string CommitLsn;
		public DateTime EndTimeUtc;
		public string EndLsn;
		public int DurationInSeconds;

		internal bool HasNoNewEmptyScansSinceLastLog(LogScanResult lastLogScanResult)
		{
			return this.SessionId > lastLogScanResult.SessionId && this.EmptyScanCount == 0;
		}
		internal bool HasCompletedScans(LogScanResult lastLogScanResult)
		{
			return this.SessionId != lastLogScanResult.SessionId || this.EmptyScanCount != lastLogScanResult.EmptyScanCount;
		}

		internal bool HasScannedNoTransactions()
		{
			return string.IsNullOrEmpty(this.CommitLsn) || this.CommitLsn.Contains("00000000000000000000");
		}
	}
}
