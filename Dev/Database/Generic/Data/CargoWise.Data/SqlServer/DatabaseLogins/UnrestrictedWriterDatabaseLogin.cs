using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	class UnrestrictedWriterDatabaseLogin : DatabaseLogin<UnrestrictedWriterLoginCredentials>
	{
		public UnrestrictedWriterDatabaseLogin(AdminConnection connection)
			: base(connection)
		{
			Argument.NotNull(connection, nameof(connection));
		}

		public override string LoginSuffix => "UnrestrictedWriterLogin";

		public override string LoginName => UnrestrictedWriterLoginCredentials.UserNameFor(baseDbName);

		public override IEnumerable<DbRole> DbLevelRoles => new[] { new CwUnrestrictedWriterRole() };

		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };

		public override bool IsImpersonateEnterpriseDbUser => true;
	}
}
