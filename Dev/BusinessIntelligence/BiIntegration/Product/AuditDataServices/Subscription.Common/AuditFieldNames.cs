namespace Enterprise.AuditDataServices.Subscription.Common
{
	public static class AuditFieldNames
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string StartLsnFieldName = "__$start_lsn";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string SeqValFieldName = "__$seqval";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string OperationFieldName = "__$operation";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string UpdateMaskFieldName = "__$update_mask";
		public const string TranEndTimeUtc = "TranEndTimeUtc"; // CDC reserved column (SQL Server).
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string LsnPeriodFieldName = "__$lsn_period";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CDC reserved column (SQL Server).")]
		public const string CommandIdFieldName = "__$command_id";
	}
}
