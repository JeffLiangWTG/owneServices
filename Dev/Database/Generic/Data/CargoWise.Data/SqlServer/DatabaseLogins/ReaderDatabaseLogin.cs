using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	public class ReaderDatabaseLogin : DatabaseLogin<CargoWiseReaderLoginCredentials>
	{
		public ReaderDatabaseLogin(AdminConnection connection)
			: base(connection)
		{
			Argument.NotNull(connection, nameof(connection));
		}

		public override string LoginSuffix => "CargoWiseReaderLogin";

		public override string LoginName => CargoWiseReaderLoginCredentials.UserNameFor(baseDbName);

		public override IEnumerable<DbRole> DbLevelRoles => new DbRole[] { new CwReaderRole(), new DbDataReaderRole() };

		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };

		protected override IEnumerable<string> ImpersonatePermissionRoles => new string[] { DbRoleTypes.CwRestrictedWriterRole };
	}
}
