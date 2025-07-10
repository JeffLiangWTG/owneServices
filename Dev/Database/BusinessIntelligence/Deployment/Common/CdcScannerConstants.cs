namespace Enterprise.ChangeDataCapture.Common
{
	public static class CdcScannerConstants
	{
		public const string SessionIDColumn = "session_id";
		public const string EmptyScanCountColumn = "empty_scan_count";
		public const string LastCommitLSNColumn = "last_commit_lsn";
		public const string EndTimeColumn = "end_time";
		public const string EndLSNColumn = "end_lsn";
		public const string DurationColumn = "duration";
		public const string SemiColon = ":";
	}
}
