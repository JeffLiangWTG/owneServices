using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	class RestrictedWriterDatabaseLogin : DatabaseLogin<RestrictedWriterLoginCredentials>
	{
		public RestrictedWriterDatabaseLogin(AdminConnection connection)
			: base(connection)
		{
			Argument.NotNull(connection, nameof(connection));
		}

		public override string LoginSuffix => "RestrictedWriterLogin";

		public override string LoginName => RestrictedWriterLoginCredentials.UserNameFor(baseDbName);

		public override IEnumerable<DbRole> DbLevelRoles => new DbRole[] { new CwRestrictedWriterRole() };

		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };
	}
}
