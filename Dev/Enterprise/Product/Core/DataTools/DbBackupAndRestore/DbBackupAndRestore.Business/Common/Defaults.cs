using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class Defaults
	{
		public static void ResetDefaultInstance()
		{
			instance = new Lazy<Defaults>();
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Read-only and thread-safe")]
		static Lazy<Defaults> instance = new Lazy<Defaults>();

		public static Defaults Instance => instance.Value;

		public bool RunDbRestore { get; set; }

		public bool RunSalesDbRestore { get; set; }

		public string SalesDbRestore { get; set; }

		public string ServerName { get; set; }

		public string DatabaseName { get; set; }

		public string BackupFileName { get; set; }

		public DbRestoreOption RestoreOption { get; set; }

		public bool RestoreOperationalDatabases { get; set; }

		public bool RestoreDifferentialBackups { get; set; }

		public bool RestoreTransactionLogBackups { get; set; }

		public bool IsAuditDBExcludedFromRestore { get; set; }

		public bool AddDbToAvailabilityGroup { get; set; }

		public string AvailabilityGroup { get; set; }

		public string DataFilePath { get; set; }

		public string LogFilePath { get; set; }

		public string AuditServerName { get; set; }

		public string AuditBackupFileName { get; set; }

		public string AuditDataFilePath { get; set; }

		public string AuditLogFilePath { get; set; }

		public string DataWarehouseServerName { get; set; }

		public string EdwBackupFileName { get; set; }

		public string EdwDataFilePath { get; set; }

		public string EdwLogFilePath { get; set; }

		public bool DisplayHelpText { get; set; }

		public string BackupFolder { get; set; }

		public string AuditBackupFolder { get; set; }

		public string EdwBackupFolder { get; set; }

		public bool IncludeOperationalDbs { get; set; }

		public bool IncludeReferenceDbs { get; set; }

		public bool IncludeBiDbs { get; set; }
	}
}
