using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	class RestrictedReaderDatabaseLogin : DatabaseLogin<RestrictedReaderLoginCredentials>
	{
		public RestrictedReaderDatabaseLogin(AdminConnection connection)
			: base(connection)
		{
			Argument.NotNull(connection, nameof(connection));
		}

		public override string LoginSuffix => "RestrictedReaderLogin";

		public override string LoginName => RestrictedReaderLoginCredentials.UserNameFor(baseDbName);

		public override IEnumerable<DbRole> DbLevelRoles => new[] { new CwRestrictedReaderRole() };

		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };

		protected override IEnumerable<string> ImpersonatePermissionRoles => new string[] { DbRoleTypes.CwRestrictedWriterRole };
	}
}
