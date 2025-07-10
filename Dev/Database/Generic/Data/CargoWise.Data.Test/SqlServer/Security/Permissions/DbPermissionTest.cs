using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbPermissionTest : TestCase
	{
		public void TestPermissionState()
		{
			var permissionState = DbPermissionState.Grant;
			var permission = new DbPermission(permissionState, DbPermissionType.Select, DbPermissionClass.Database);

			AssertEquals("PermissionState", permissionState, permission.PermissionState);
		}

		public void TestPermissionType()
		{
			var permissionType = DbPermissionType.Select;
			var permission = new DbPermission(DbPermissionState.Grant, permissionType, DbPermissionClass.Database);

			AssertEquals("PermissionType", permissionType, permission.PermissionType);
		}

		public void TestPermissionClass()
		{
			var permissionClass = DbPermissionClass.Database;
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Select, permissionClass);

			AssertEquals("PermissionClass", permissionClass, permission.PermissionClass);
		}

		public void TestEnsureExists()
		{
			var dbRoleName = "fake role";
			var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);

			using (var connection = Db.NewAdminConnection())
			{
				try
				{
					SqlSecurityUtils.AssertDbRoleExists(connection, dbRoleName, expected: false);

					SqlSecurityUtils.DbRole.Create(connection, dbRoleName);

					SqlSecurityUtils.AssertDbRoleExists(connection, dbRoleName, expected: true);
					SqlSecurityUtils.AssertDbPermissionExists(connection, dbRoleName, permission, connection.CurrentDatabase, expected: false);

					permission.EnsureExists(connection, connection.CurrentDatabase, dbRoleName, connection.CurrentDatabase);

					SqlSecurityUtils.AssertDbPermissionExists(connection, dbRoleName, permission, connection.CurrentDatabase, expected: true);
				}
				finally
				{
					SqlSecurityUtils.DbRole.Drop(connection, dbRoleName);
				}
			}
		}

		public void TestToString()
		{
			var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);

			AssertEquals("DENY SELECT ON DATABASE", permission.ToString());
		}

		public void TestToRevoke()
		{
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Select, DbPermissionClass.Database);

			AssertEquals("REVOKE SELECT ON DATABASE", permission.ToRevoke());
		}
	}
}
