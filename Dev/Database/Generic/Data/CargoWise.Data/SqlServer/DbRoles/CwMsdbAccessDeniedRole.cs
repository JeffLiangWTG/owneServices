using System;
using CargoWise.Common;

namespace CargoWise.Data
{
	public sealed class CwMsdbAccessDeniedRole : DbRole
	{
		public override string Name { get; } = DbRoleTypes.Constants.CwMsdbAccessDeniedRole;

		public override void EnsureExists(AdminConnection connection, string dbName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (dbName.Equals(Db.SqlMsdb, StringComparison.OrdinalIgnoreCase) || dbName.Equals(((ICurrentDbControl)connection).InitialDatabase, StringComparison.OrdinalIgnoreCase))
			{
				base.EnsureExists(connection, Db.SqlMsdb);

				new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database)
					.EnsureExists(connection, dbName: Db.SqlMsdb, dbPrincipalName: Name, permissionClassValue: Db.SqlMsdb);
			}
		}
	}
}
