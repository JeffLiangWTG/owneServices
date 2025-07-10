using System.Collections.Generic;

namespace CargoWise.Data
{
	//
	// This is part of the old Db class, containing various static constants.
	//
	// In the future, many of these constants need to be re-considered and grouped into their relevant locations.
	//
	// There are no dependencies to other parts of Db.
	//

	public partial class Db
	{
		public const string AttemptToUseConnectionWithoutDisposableAction = "Attempt to use Db.Connection without using Db.DisposableActionForDbConnection()\r\nPlease add this to the highest position in the call stack that this would make sense.\r\nie. the call after a Form.WndProc()"; // Development constant, should not use Res.GetString
		public const string SharedTestServer = "syduatalpha.db.corporate.cargowise.com"; // Development constant, should not use Res.GetString
		public const string SqlMasterDb = "master"; // Development constant, should not use Res.GetString
		public const string SqlMsdb = "msdb"; // Development constant, should not use Res.GetString
		public const string SqlDbOwnerSchema = "dbo"; // Development constant, should not use Res.GetString
		public static readonly string[] SqlReservedSchemas = { "sys", "cdc" }; // Development constant, should not use Res.GetString
		public static readonly string[] CW1AdditionalSchemas = { DbSecurity.SqlHrmSchema, "OrderTracking" }; // Development constant, should not use Res.GetString
		public const string SysAdminUserLogin = "sa"; // Development constant, should not use Res.GetString
		public const string DatabaseCollation = "SQL_Latin1_General_CP1_CI_AS"; // Development constant, should not use Res.GetString
		public const string DatabaseCaseSensitiveCollation = "SQL_Latin1_General_CP1_CS_AS"; // Development constant, should not use Res.GetString
		public const string StorageDocDbSuffixSqlPattern = "[_]SD[0-9][0-9][0-9]"; // Development constant, should not use Res.GetString
		public static readonly IEnumerable<string> SystemDatabasesForBackup = new List<string> { "master", "msdb", "model" }; // Development constant, should not use Res.GetString
		public const string AuditDatabaseSuffix = "_Audit";
		public const string EdwDatabaseSuffix = "_EDW";
		public const string BackupDiffFileSuffix = "_Diff";
		public const string SDDatabaseAffix = "_SD";
		public const string BackupDiffFileExtension = ".dbk";
	}
}
