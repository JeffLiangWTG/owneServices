namespace CargoWise.Data
{
	public static class DbRegistry
	{
		internal const string DatabaseMajorSchemaVersionName = "DATABASE_SCHEMA_VERSION";
		internal const string DatabaseMinorSchemaVersionName = "DATABASE_MINOR_SCHEMA_VERSION";
		public static IntDbRegistryItem DatabaseMajorSchemaVersion => new DatabaseVersionRegistryItem(DatabaseMajorSchemaVersionName);
		public static IntDbRegistryItem DatabaseMinorSchemaVersion => new DatabaseVersionRegistryItem(DatabaseMinorSchemaVersionName);

		internal const string DatabaseMajorScriptVersionName = "DatabaseMajorCoreScriptVersion";
		internal const string DatabaseMinorScriptVersionName = "DatabaseMinorCoreScriptVersion";
		public static IntDbRegistryItem DatabaseMajorScriptVersion => new DatabaseVersionRegistryItem(DatabaseMajorScriptVersionName);
		public static IntDbRegistryItem DatabaseMinorScriptVersion => new DatabaseVersionRegistryItem(DatabaseMinorScriptVersionName);

		internal const string DatabaseMajorTransformationVersionName = "DatabaseMajorTransformationVersion";
		internal const string DatabaseMinorTransformationVersionName = "DatabaseMinorTransformationVersion";
		public static IntDbRegistryItem DatabaseMajorTransformationVersion => new DatabaseVersionRegistryItem(DatabaseMajorTransformationVersionName);
		public static IntDbRegistryItem DatabaseMinorTransformationVersion => new DatabaseVersionRegistryItem(DatabaseMinorTransformationVersionName);

		public static IntDbRegistryItem DatabaseMajorClrAssembliesVersion => new DatabaseVersionRegistryItem("DatabaseMajorClrAssembliesVersion");
		public static IntDbRegistryItem DatabaseMinorClrAssembliesVersion => new DatabaseVersionRegistryItem("DatabaseMinorClrAssembliesVersion");

		public static IntDbRegistryItem DatabaseSystemDataVersionMajor => new DatabaseVersionRegistryItem("DatabaseSystemDataVersionMajor");
		public static IntDbRegistryItem DatabaseSystemDataVersionMinor => new DatabaseVersionRegistryItem("DatabaseSystemDataVersionMinor");

		internal static IntDbRegistryItem LockoutSpid => new IntDbRegistryItem("LockoutSpid", defaultValue: -1);
		internal static DateTimeRegistryItem LockoutLoginTime => new DateTimeRegistryItem("LockoutLoginTime");

		internal const string LockTimeoutName = "LockTimeout";
		public static IntDbRegistryItem LockTimeout => new IntDbRegistryItem(LockTimeoutName, DbConnection.LockTimeout.Default);

		public static StringDbRegistryItem BackupFilePath => new StringDbRegistryItem("BackupFilePath", preserveTestValue: true);

		#region Business Intelligence

		public static BoolDbRegistryItem BiResetChangeDataCapture => new BoolDbRegistryItem("BiResetChangeDataCapture", defaultValue: false, preserveTestValue: true);
		public static BoolDbRegistryItem BiDisableChangeDataCapture => new BoolDbRegistryItem("BiDisableChangeDataCapture", defaultValue: false, preserveTestValue: true);
		public static IntDbRegistryItem BiCdcMaxTransactions => new IntDbRegistryItem("CdcMaxTransactions", defaultValue: 100, preserveTestValue: true);
		public static IntDbRegistryItem BiCdcMaxScans => new IntDbRegistryItem("CdcMaxScans", defaultValue: 5, preserveTestValue: true);
		public static BoolDbRegistryItem BiAuditAPI => new BoolDbRegistryItem("BiAuditAPI", defaultValue: true, preserveTestValue: true);
		public static StringDbRegistryItem BiAuditServer => new StringDbRegistryItem("BiAuditServer", preserveTestValue: true);
		public static BoolDbRegistryItem BiReportAPI => new BoolDbRegistryItem("BiReportAPI", defaultValue: false, preserveTestValue: true);
		public static StringDbRegistryItem BiDataWarehouseServer => new StringDbRegistryItem("BiDataWarehouseServer", preserveTestValue: true);
		public static StringDbRegistryItem BiAnalysisServer => new StringDbRegistryItem("BiAnalysisServer", preserveTestValue: true);
		public static StringDbRegistryItem EnabledCdcTables => new StringDbRegistryItem("EnabledCdcTables", preserveTestValue: false);

		#endregion

		public static IntDbRegistryItem GetNewSystemUpgradeWarningPeriodItem() => new IntDbRegistryItem("SystemUpgradeWarningPeriod", defaultValue: 0);

		public static StringDbRegistryItem ClientDocumentName => new StringDbRegistryItem("ClientDocumentName");
		public static IntDbRegistryItem DocumentCustomisationVersionNumber => new IntDbRegistryItem("DocumentCustomisationVersionNumber", defaultValue: 0);
		public static IntDbRegistryItem ClientDocumentVersion => new IntDbRegistryItem("ClientDocumentVersion", defaultValue: 0);

		public static StringDbRegistryItem DatabaseRecoveryModel => new StringDbRegistryItem(
			"DatabaseRecoveryModel",
			defaultValue:
#if DEBUG
			"SIMPLE"
#else
			"FULL"
#endif
			);

		public static IntDbRegistryItem MaximumAllowedVLFsCount => new IntDbRegistryItem("MaximumAllowedVLFsCount", defaultValue: 100);
		public static IntDbRegistryItem MaximumLogToTwoWeekBackupPercentage => new IntDbRegistryItem("MaximumLogToTwoWeekBackupPercentage", defaultValue: 120);

		public static IntDbRegistryItem PasswordHistoryCount => new IntDbRegistryItem("PasswordHistoryCount", defaultValue: 0);
		public static StringDbRegistryItem SingleRefDatabaseName => new StringDbRegistryItem("SingleRefDatabaseName");

		public static BoolDbRegistryItem SuspendAuditTriggers => new BoolDbRegistryItem(
			SuspendAuditTriggersName,
			defaultValue: SuspendAuditTriggersDefaultValue);

		public const string SuspendAuditTriggersName = "SuspendAuditTriggers";
		public static bool SuspendAuditTriggersDefaultValue =>
#if DEBUG
			false
#else
			true
#endif
;
		public static BoolDbRegistryItem MissingFetchHintDetection => new BoolDbRegistryItem("Missing Fetch Hint Detection", false);
		public static IntDbRegistryItem MissingFetchHintThreshold => new IntDbRegistryItem("Missing Fetch Hint Threshold", 25);
		public static IntDbRegistryItem ReportSlowUpgradeTransformsSeconds => new IntDbRegistryItem("ReportSlowUpgradeTransformsSeconds", defaultValue: 30);
	}
}
