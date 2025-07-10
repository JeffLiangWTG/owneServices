using System.Collections.Generic;

namespace CargoWise.Data
{
	public static class DbRoleTypes
	{
		public static class Constants
		{
			public static string CwMsdbAccessDeniedRole { get; } = "cwMsdbAccessDeniedRole";
		}

		public static DbRole CwMsdbAccessDeniedRole => new CwMsdbAccessDeniedRole();

		public const string DbOwnerRole = "db_owner";
		public const string DbDataReaderRole = "db_datareader";
		public const string CwReaderRole = "cwReaderRole";

		public const string DbBackupOperatorRole = "db_backupoperator";
		public const string DbDataWriterRole = "db_datawriter";

		public const string CwRestrictedReaderRole = "cwRestrictedReaderRole";
		public const string CwRestrictedWriterRole = "cwRestrictedWriterRole";
		public const string CwHRMStaffRole = "cwHRMStaffRole";
		public const string CwUnrestrictedWriterRole = "cwUnrestrictedWriterRole";

		public static IEnumerable<DbRole> AllDbRoles => new DbRole[]
			{
				new CwReaderRole(),
				new CwRestrictedReaderRole(),
				new CwRestrictedWriterRole(),
				new CwHRMStaffRole(),
				new CwUnrestrictedWriterRole()
			};
	}
}
