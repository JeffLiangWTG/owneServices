namespace Enterprise.DbHealth.Check
{
	public class DatabaseWarning : DbHealthWarning
	{
		public DatabaseWarning(string source, string warningType, string description, string action)
			: base(source, warningType, description, action)
		{
		}

		public override string SourceType
		{
			get { return DatabaseSourceType; }
		}

		public const string DatabaseSourceType = "Database";

		public const string ExpressDbSizeWarning = "Express Engine DB Size Limit";
		public const string FileLocationWarning = "Data / Log File Storage";
		public const string BackupWarning = "Backup";
		public const string RestoreWarning = "Restore";
		public const string DbConsistencyWarning = "Database Consistency";
		public const string IncorrectUtcTimeWarning = "Incorrect Utc Time";
		public const string SnapshotIsolationWarning = "Snapshot Isolation Disabled";
		public const string GhostRecordsWarning = "Ghost Records Identified";
		public const string AlwaysOnWarning = "AlwaysOn Availability Groups";
		public const string CdcWarning = "Change Data Capture";
		public const string DbNameWarning = "Inconsistent Main Database Name Case";
	}
}
