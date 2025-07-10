using System.Collections.Generic;
using CargoWise.DataProtection;

namespace CargoWise.Data
{
	class CargoWiseWriterLogin : DatabaseLogin<CargoWiseWriterLoginCredentials>
	{
		public CargoWiseWriterLogin(AdminConnection connection) : base(connection)
		{
		}

		public override string LoginName => CargoWiseWriterLoginCredentials.UserNameFor(baseDbName);
		public override string LoginSuffix => "CargoWiseWriterLogin";

		public override IEnumerable<DbRole> DbLevelRoles => new DbRole[] { new CwRestrictedWriterRole() };
		protected override IEnumerable<string> DbLevelPermissions => new[] { "EXECUTE" };
	}
}
