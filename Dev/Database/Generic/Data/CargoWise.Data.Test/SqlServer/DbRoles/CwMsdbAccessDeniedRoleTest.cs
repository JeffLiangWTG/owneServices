using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class CwMsdbAccessDeniedRoleTest : TestCase
	{
		public void TestName()
		{
			AssertEquals("Name", DbRoleTypes.Constants.CwMsdbAccessDeniedRole, new CwMsdbAccessDeniedRole().Name);
		}

		public void TestEnsureExistsArguments()
		{
			var role = new CwMsdbAccessDeniedRole();

			using (var connection = Db.NewAdminConnection())
			{
				AssertExceptionThrown<ArgumentException>(() => role.EnsureExists(null, connection.CurrentDatabase));
				AssertExceptionThrown<ArgumentException>(() => role.EnsureExists(connection, null));
				AssertExceptionThrown<ArgumentException>(() => role.EnsureExists(connection, string.Empty));
			}
		}

		public void TestEnsureExists()
		{
			var role = new CwMsdbAccessDeniedRole();

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, role.Name);
					SqlSecurityUtils.AssertDbRoleExists(connection, Db.SqlMsdb, role.Name, expected: false);

					role.EnsureExists(connection, "fake database");

					SqlSecurityUtils.AssertDbRoleExists(connection, Db.SqlMsdb, role.Name, expected: false);

					role.EnsureExists(connection, connection.CurrentDatabase);

					SqlSecurityUtils.AssertDbRoleExists(connection, ((ICurrentDbControl)connection).InitialDatabase, role.Name, expected: false);
					SqlSecurityUtils.AssertDbRoleExists(connection, Db.SqlMsdb, role.Name, expected: true);

					SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, role.Name);

					SqlSecurityUtils.AssertDbRoleExists(connection, Db.SqlMsdb, role.Name, expected: false);

					role.EnsureExists(connection, Db.SqlMsdb);

					SqlSecurityUtils.AssertDbRoleExists(connection, Db.SqlMsdb, role.Name, expected: true);

					var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);
					SqlSecurityUtils.AssertDbPermissionExists(connection, Db.SqlMsdb, role.Name, permission, Db.SqlMsdb, expected: true);
				}
				finally
				{
					SqlSecurityUtils.DbRole.Drop(connection, Db.SqlMsdb, role.Name);
				}
			}
		}
	}
}
