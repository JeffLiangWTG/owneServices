using System;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class SqlSecurityUtilsTest : TestCase
	{
		public void TestLoginCreate()
		{
			var loginName = "fake login";

			SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: false);

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName, "CvNqr'S3A#9FN");

				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: true);

				AssertNoExceptionThrown(() => { SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName, "CvNqrS3A#9FN"); });
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {loginName.QuoteName()}");
			}
		}

		public void TestLoginExists()
		{
			var loginName = "fake login";

			AssertEquals("Login exists?", false, SqlSecurityUtils.Login.Exists(TestConnection, loginName));
			SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: false);

			try
			{
				TestConnection.ExecuteNonQuery($"CREATE LOGIN {loginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");

				AssertEquals("Login exists?", true, SqlSecurityUtils.Login.Exists(TestConnection, loginName));
				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {loginName.QuoteName()}");
			}
		}

		public void TestTryToGetAssociatedDbUserWhileNoDbUser()
		{
			const string loginName = "TestLogin";

			try
			{
				// Arrange
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName, "CvNqr'S3A#9FN");

				// Act
				var isExist = SqlSecurityUtils.Login.TryToGetAssociatedDbUser(TestConnection, TestConnection.CurrentDatabase, loginName, out _);

				// Assert
				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: true);
				Assert("User shouldn't be found", !isExist);
			}
			finally
			{
				SqlSecurityUtils.Login.Drop(TestConnection, loginName);
			}
		}

		public void TestTryToGetAssociatedDbUser()
		{
			const string loginName = "TestLogin";
			const string dbUserName = loginName + "_AssociatedDbUser";
			try
			{
				// Arrange
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName, "CvNqr'S3A#9FN");
				TestConnection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()} FOR LOGIN {loginName.QuoteName()};");

				// Act
				var isExist = SqlSecurityUtils.Login.TryToGetAssociatedDbUser(TestConnection, TestConnection.CurrentDatabase, loginName, out var realAssociatedDbUser);

				// Assert
				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: true);
				Assert("User should be found", isExist);
				AssertEquals($"Real DbUser: {realAssociatedDbUser} is different from expected User: {dbUserName}", dbUserName, realAssociatedDbUser);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);
				SqlSecurityUtils.Login.Drop(TestConnection, loginName);
			}
		}

		public void TestGetAllStaffDbLogins()
		{
			var staffloginName1 = $"EnterpriseDbUser_{Db.DatabaseName}_StaffName1";
			var staffloginName2 = $"EnterpriseDbUser_{Db.DatabaseName}_StaffName2";
			var staffloginName3 = $"EnterpriseDbUser_{Db.DatabaseName}_StaffName3";
			var staffloginName4 = $"UserMatchDefaultDatabase_StaffName4";

			SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName1, expected: false);
			SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName2, expected: false);
			SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName3, expected: false);
			SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName4, expected: false);

			try
			{
				var staffDbLogins_before = SqlSecurityUtils.Login.GetAllStaffDbLogins(TestConnection, Db.DatabaseName);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {staffloginName1.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				TestConnection.ExecuteNonQuery($"CREATE LOGIN {staffloginName2.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				TestConnection.ExecuteNonQuery($"CREATE LOGIN {staffloginName3.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				TestConnection.ExecuteNonQuery($"CREATE LOGIN {staffloginName4.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>', DEFAULT_DATABASE = {Db.DatabaseName}");

				SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName1, expected: true);
				SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName2, expected: true);
				SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName3, expected: true);
				SqlSecurityUtils.AssertLoginExists(TestConnection, staffloginName4, expected: true);

				var staffDbLogins_after = SqlSecurityUtils.Login.GetAllStaffDbLogins(TestConnection, Db.DatabaseName);

				AssertCollectionNotContains(staffloginName1, staffDbLogins_before);
				AssertCollectionNotContains(staffloginName2, staffDbLogins_before);
				AssertCollectionNotContains(staffloginName3, staffDbLogins_before);
				AssertCollectionNotContains(staffloginName4, staffDbLogins_before);

				AssertCollectionContains(staffloginName1, staffDbLogins_after);
				AssertCollectionContains(staffloginName2, staffDbLogins_after);
				AssertCollectionContains(staffloginName3, staffDbLogins_after);
				AssertCollectionContains(staffloginName4, staffDbLogins_after);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {staffloginName1.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {staffloginName2.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {staffloginName3.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {staffloginName4.QuoteName()}");
			}
		}

		public void TestLoginDrop()
		{
			var loginName = "fake login";

			SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: false);

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName, "CvNqrS3A#9FN");
				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: true);

				SqlSecurityUtils.Login.Drop(TestConnection, loginName);
				SqlSecurityUtils.AssertLoginExists(TestConnection, loginName, expected: false);

				AssertNoExceptionThrown(() => { SqlSecurityUtils.Login.Drop(TestConnection, loginName); });
			}
			catch (Exception)
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {loginName.QuoteName()}");

				throw;
			}
		}

		public void TestDbRoleExists()
		{
			var dbRoleName = "fake role";

			AssertEquals("DbRole exists?", false, SqlSecurityUtils.DbRole.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName));
			AssertEquals("DbRole exists?", false, SqlSecurityUtils.DbRole.Exists(TestConnection, dbRoleName));
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: false);
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: false);

			try
			{
				TestConnection.ExecuteNonQuery($"CREATE ROLE {dbRoleName.QuoteName()}");

				AssertEquals("DbRole exists?", true, SqlSecurityUtils.DbRole.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName));
				AssertEquals("DbRole exists?", true, SqlSecurityUtils.DbRole.Exists(TestConnection, dbRoleName));
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: true);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP ROLE {dbRoleName.QuoteName()}");
			}
		}

		public void TestCreateDbRole()
		{
			var dbRoleName = "fake role";

			SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: false);

			try
			{
				SqlSecurityUtils.DbRole.Create(TestConnection, TestConnection.CurrentDatabase, dbRoleName);

				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: true);

				AssertNoExceptionThrown(() =>
				{
					SqlSecurityUtils.DbRole.Create(TestConnection, TestConnection.CurrentDatabase, dbRoleName);
				});

				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: true);
			}
			finally
			{
				if (SqlSecurityUtils.DbRole.Exists(TestConnection, dbRoleName))
				{
					TestConnection.ExecuteNonQuery($"DROP ROLE {dbRoleName.QuoteName()}");
				}
			}
		}

		public void TestDropDbRole()
		{
			var dbRoleName = "fake role";
			var dbUserName = "fake user";

			SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: false);

			try
			{
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, TestConnection.CurrentDatabase, dbUserName);
				SqlSecurityUtils.DbRole.Create(TestConnection, TestConnection.CurrentDatabase, dbRoleName);
				SqlSecurityUtils.DbRole.AddMember(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName);

				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: true);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: true);

				SqlSecurityUtils.DbRole.Drop(TestConnection, TestConnection.CurrentDatabase, dbRoleName);

				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: false);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: false);

				AssertNoExceptionThrown(() =>
				{
					SqlSecurityUtils.DbRole.Drop(TestConnection, TestConnection.CurrentDatabase, dbRoleName);
				});

				SqlSecurityUtils.AssertDbRoleExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, expected: false);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: false);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);

				if (SqlSecurityUtils.DbRole.Exists(TestConnection, dbRoleName))
				{
					TestConnection.ExecuteNonQuery($"DROP ROLE {dbRoleName.QuoteName()}");
				}
			}
		}

		public void TestGetDbRoleMembers()
		{
			var dbName = TestConnection.CurrentDatabase;
			var dbUserName1 = "fake user 1";
			var dbUserName2 = "fake user 2";
			var dbRoleName = "fake role";

			try
			{
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, dbName, dbUserName1);
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, dbName, dbUserName2);

				var members = SqlSecurityUtils.DbRole.GetMembers(TestConnection, dbName, dbRoleName);
				AssertEquals("No role", string.Empty, string.Join(", ", members));

				SqlSecurityUtils.DbRole.Create(TestConnection, dbName, dbRoleName);

				members = SqlSecurityUtils.DbRole.GetMembers(TestConnection, dbName, dbRoleName);
				AssertEquals("Empty role", string.Empty, string.Join(", ", members));

				SqlSecurityUtils.DbRole.AddMember(TestConnection, dbName, dbRoleName, dbUserName1);

				members = SqlSecurityUtils.DbRole.GetMembers(TestConnection, dbName, dbRoleName);
				AssertEquals("One user", $"{dbUserName1}", string.Join(", ", members));

				SqlSecurityUtils.DbRole.AddMember(TestConnection, dbName, dbRoleName, dbUserName2);

				members = SqlSecurityUtils.DbRole.GetMembers(TestConnection, dbName, dbRoleName);
				AssertEquals("Two users", $"{dbUserName1}, {dbUserName2}", string.Join(", ", members));
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbName, dbUserName1);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbName, dbUserName2);
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbName, dbRoleName);
			}
		}

		public void TestDbUserExists()
		{
			var dbUserName = "fake user";

			AssertEquals("DbUser exists?", false, SqlSecurityUtils.DbUser.Exists(TestConnection, TestConnection.CurrentDatabase, dbUserName));
			AssertEquals("DbUser exists?", false, SqlSecurityUtils.DbUser.Exists(TestConnection, dbUserName));
			SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

			try
			{
				TestConnection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()} WITHOUT LOGIN");

				AssertEquals("DbUser exists?", true, SqlSecurityUtils.DbUser.Exists(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("DbUser exists?", true, SqlSecurityUtils.DbUser.Exists(TestConnection, dbUserName));
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP USER {dbUserName.QuoteName()}");
			}
		}

		public void TestCreateDbUser()
		{
			var dbUserName = "fake user";

			SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: false);
			TestConnection.ExecuteNonQuery($"CREATE LOGIN {dbUserName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
			SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: true);

			try
			{
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

				SqlSecurityUtils.DbUser.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName);

				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);

				AssertNoExceptionThrown(() =>
				{
					SqlSecurityUtils.DbUser.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName);
				});

				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);
				TestConnection.ExecuteNonQuery($"DROP LOGIN {dbUserName.QuoteName()}");
			}
		}

		public void TestDropDbUser()
		{
			var dbUserName = "fake user";

			SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

			try
			{
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, TestConnection.CurrentDatabase, dbUserName);

				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);

				SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);

				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

				AssertNoExceptionThrown(() =>
				{
					SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);
				});

				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, TestConnection.CurrentDatabase, dbUserName);
			}
		}

		public void TestDbPermissionExists()
		{
			var dbRoleName = "fake role";
			var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);
			var permissionClassValue = TestConnection.CurrentDatabase;

			try
			{
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: false);

				AssertEquals("DbPermission exists?", false, SqlSecurityUtils.DbPermission.Exists(TestConnection, dbRoleName, permission, permissionClassValue));
				AssertEquals("DbPermission exists?", false, SqlSecurityUtils.DbPermission.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue));
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: false);
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue, expected: false);

				SqlSecurityUtils.DbRole.Create(TestConnection, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: true);

				AssertEquals("DbPermission exists?", false, SqlSecurityUtils.DbPermission.Exists(TestConnection, dbRoleName, permission, permissionClassValue));
				AssertEquals("DbPermission exists?", false, SqlSecurityUtils.DbPermission.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue));
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: false);
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue, expected: false);

				TestConnection.ExecuteNonQuery($"DENY SELECT ON DATABASE::{TestConnection.CurrentDatabase.QuoteName()} TO {dbRoleName.QuoteName()}");

				AssertEquals("DbPermission exists?", true, SqlSecurityUtils.DbPermission.Exists(TestConnection, dbRoleName, permission, permissionClassValue));
				AssertEquals("DbPermission exists?", true, SqlSecurityUtils.DbPermission.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue));
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: true);
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, permissionClassValue, expected: true);
			}
			finally
			{
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbRoleName);
			}
		}

		public void TestDbPermissionCreate()
		{
			var dbRoleName = "fake role";
			var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);
			var permissionClassValue = TestConnection.CurrentDatabase;

			try
			{
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: false);
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: false);

				SqlSecurityUtils.DbRole.Create(TestConnection, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: true);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: false);

				SqlSecurityUtils.DbPermission.Create(TestConnection, dbRoleName, permission, permissionClassValue);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, permissionClassValue, expected: true);
			}
			finally
			{
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbRoleName);
			}
		}

		public void TestDbPermissionCreateWithDbName()
		{
			var testDb = "TestDbPermissionCreateWithDbNameTestDb";

			var dbRoleName = "fake role";
			var permission = new DbPermission(DbPermissionState.Deny, DbPermissionType.Select, DbPermissionClass.Database);
			var permissionClassValue = testDb;

			using (AdoTestUtils.CreateDbDropExistingDisposable(TestConnection, testDb))
			{
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, testDb, dbRoleName, expected: false);
				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, testDb, dbRoleName, permission, permissionClassValue, expected: false);

				SqlSecurityUtils.DbRole.Create(TestConnection, testDb, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, testDb, dbRoleName, expected: true);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, testDb, dbRoleName, permission, permissionClassValue, expected: false);

				SqlSecurityUtils.DbPermission.Create(TestConnection, testDb, dbRoleName, permission, permissionClassValue);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, testDb, dbRoleName, permission, permissionClassValue, expected: true);
			}
		}

		public void TestDbPermissionRevoke()
		{
			var dbRoleName = "fake role";
			var dbUserName = "fake user";
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.User);

			try
			{
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: false);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: false);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbUserName, permission, dbUserName, expected: false);

				SqlSecurityUtils.DbRole.Create(TestConnection, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbRoleName, expected: true);
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName, ">J6]Ld.ff=[&");
				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: true);
				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, dbUserName, expected: false);

				SqlSecurityUtils.DbPermission.Create(TestConnection, dbRoleName, permission, dbUserName);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, dbUserName, expected: true);

				SqlSecurityUtils.DbPermission.Revoke(TestConnection, TestConnection.CurrentDatabase, dbRoleName, permission, dbUserName);

				SqlSecurityUtils.AssertDbPermissionExists(TestConnection, dbRoleName, permission, dbUserName, expected: false);
			}
			finally
			{
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbRoleName);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName);
			}
		}

		public void TestDbPermissionGetImpersonatePermissionGranteePrincipalNames()
		{
			var dbUserName1 = "fake user 1";
			var dbUserName2 = "fake user 2";
			var dbUserName3 = "fake user 3";

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName1, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName2, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName3, ">J6]Ld.ff=[&");

				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName1);
				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName2);
				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName3);

				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName1.QuoteName()} TO {dbUserName2.QuoteName()}");
				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName1.QuoteName()} TO {dbUserName3.QuoteName()}");
				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName2.QuoteName()} TO {dbUserName3.QuoteName()}");

				var granteePrincipalNames1 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName1);
				var granteePrincipalNames2 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName2);
				var granteePrincipalNames3 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName3);

				AssertEquals(2, granteePrincipalNames1.Count);
				AssertEquals(1, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);

				AssertContainsExactElementsInAnyOrder(new[] { dbUserName2, dbUserName3 }, granteePrincipalNames1);
				AssertContainsExactElementsInAnyOrder(new[] { dbUserName3 }, granteePrincipalNames2);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName3);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName2);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName1);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName3);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName2);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName1);
			}
		}

		public void TestDbPermissionRevokeImpersonatePermissions()
		{
			var dbUserName1 = "fake user 1";
			var dbUserName2 = "fake user 2";
			var dbUserName3 = "fake user 3";

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName1, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName2, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, dbUserName3, ">J6]Ld.ff=[&");

				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName1);
				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName2);
				SqlSecurityUtils.DbUser.Create(TestConnection, dbUserName3);

				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName1.QuoteName()} TO {dbUserName2.QuoteName()}");
				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName1.QuoteName()} TO {dbUserName3.QuoteName()}");
				TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER::{dbUserName2.QuoteName()} TO {dbUserName3.QuoteName()}");

				var granteePrincipalNames1 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName1);
				var granteePrincipalNames2 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName2);
				var granteePrincipalNames3 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName3);

				AssertEquals(2, granteePrincipalNames1.Count);
				AssertEquals(1, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);

				AssertContainsExactElementsInAnyOrder(new[] { dbUserName2, dbUserName3 }, granteePrincipalNames1);
				AssertContainsExactElementsInAnyOrder(new[] { dbUserName3 }, granteePrincipalNames2);

				SqlSecurityUtils.DbPermission.RevokeImpersonatePermissions(TestConnection, Db.DatabaseName, dbUserName1);
				SqlSecurityUtils.DbPermission.RevokeImpersonatePermissions(TestConnection, Db.DatabaseName, dbUserName2);
				SqlSecurityUtils.DbPermission.RevokeImpersonatePermissions(TestConnection, Db.DatabaseName, dbUserName3);

				granteePrincipalNames1 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName1);
				granteePrincipalNames2 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName2);
				granteePrincipalNames3 = SqlSecurityUtils.DbPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, Db.DatabaseName, dbUserName3);

				AssertEquals(0, granteePrincipalNames1.Count);
				AssertEquals(0, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName3);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName2);
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbUserName1);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName3);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName2);
				SqlSecurityUtils.Login.Drop(TestConnection, dbUserName1);
			}
		}

		public void TestDbRoleContains()
		{
			var dbRoleName = "fake role";
			var dbUserName = "fake user";

			AssertEquals("DbRole exists?", false, SqlSecurityUtils.DbRole.Exists(TestConnection, TestConnection.CurrentDatabase, dbRoleName));
			AssertEquals("DbUser exists?", false, SqlSecurityUtils.DbUser.Exists(TestConnection, TestConnection.CurrentDatabase, dbUserName));
			AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName));
			AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, dbRoleName, dbUserName));
			SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: false);

			try
			{
				TestConnection.ExecuteNonQuery($"CREATE ROLE {dbRoleName.QuoteName()}");
				AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName));
				AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, dbRoleName, dbUserName));
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()} WITHOUT LOGIN");
				AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName));
				AssertEquals("DbRole contains DbUser?", false, SqlSecurityUtils.DbRole.Contains(TestConnection, dbRoleName, dbUserName));
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: false);

				TestConnection.ExecuteNonQuery($"ALTER ROLE {dbRoleName.QuoteName()} ADD MEMBER {dbUserName.QuoteName()}");

				AssertEquals("DbRole contains DbUser?", true, SqlSecurityUtils.DbRole.Contains(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName));
				AssertEquals("DbRole contains DbUser?", true, SqlSecurityUtils.DbRole.Contains(TestConnection, dbRoleName, dbUserName));
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, TestConnection.CurrentDatabase, dbRoleName, dbUserName, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP USER {dbUserName.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP ROLE {dbRoleName.QuoteName()}");
			}
		}

		public void TestDbRoleAddMember()
		{
			var dbName = TestConnection.CurrentDatabase;
			var dbUserName = "fake user";
			var dbRoleName = "fake role";

			SqlSecurityUtils.AssertDbUserExists(TestConnection, dbName, dbUserName, expected: false);
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbName, dbRoleName, expected: false);

			try
			{
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, dbName, dbUserName);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, dbName, dbUserName, expected: true);

				SqlSecurityUtils.DbRole.Create(TestConnection, dbName, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbName, dbRoleName, expected: true);

				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, dbName, dbRoleName, dbUserName, expected: false);

				SqlSecurityUtils.DbRole.AddMember(TestConnection, dbName, dbRoleName, dbUserName);

				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, dbName, dbRoleName, dbUserName, expected: true);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbName, dbUserName);
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbName, dbRoleName);
			}
		}

		public void TestDbRoleDropMember()
		{
			var dbName = TestConnection.CurrentDatabase;
			var dbUserName = "fake user";
			var dbRoleName = "fake role";

			SqlSecurityUtils.AssertDbUserExists(TestConnection, dbName, dbUserName, expected: false);
			SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbName, dbRoleName, expected: false);

			try
			{
				SqlSecurityUtils.CreateDbUserWithoutLogin_ForTest(TestConnection, dbName, dbUserName);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, dbName, dbUserName, expected: true);

				SqlSecurityUtils.DbRole.Create(TestConnection, dbName, dbRoleName);
				SqlSecurityUtils.AssertDbRoleExists(TestConnection, dbName, dbRoleName, expected: true);

				SqlSecurityUtils.DbRole.AddMember(TestConnection, dbName, dbRoleName, dbUserName);
				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, dbName, dbRoleName, dbUserName, expected: true);

				SqlSecurityUtils.DbRole.DropMember(TestConnection, dbName, dbRoleName, dbUserName);

				SqlSecurityUtils.AssertDbRoleContainsDbUser(TestConnection, dbName, dbRoleName, dbUserName, expected: false);
			}
			finally
			{
				SqlSecurityUtils.DbUser.Drop(TestConnection, dbName, dbUserName);
				SqlSecurityUtils.DbRole.Drop(TestConnection, dbName, dbRoleName);
			}
		}

		public void TestServerPermissionExistsByGrantor()
		{
			var grantorLoginName = "fake grantor login";
			var granteeLoginName = "fake grantee login";
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.Login);

			try
			{
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: false);
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: false);

				AssertEquals("ServerPermission exists?", false, SqlSecurityUtils.ServerPermission.ExistsByGrantor(TestConnection, grantorLoginName, permission));
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {granteeLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: true);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {grantorLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: true);

				AssertEquals("ServerPermission exists?", false, SqlSecurityUtils.ServerPermission.ExistsByGrantor(TestConnection, grantorLoginName, permission));
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
				{
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{grantorLoginName.QuoteName()} TO {granteeLoginName.QuoteName()}");
				}

				AssertEquals("ServerPermission exists?", true, SqlSecurityUtils.ServerPermission.ExistsByGrantor(TestConnection, grantorLoginName, permission));
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {granteeLoginName.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {grantorLoginName.QuoteName()}");
			}
		}

		public void TestServerPermissionCreate()
		{
			var grantorLoginName = "fake grantor login";
			var granteeLoginName = "fake grantee login";
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.Login);

			try
			{
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: false);
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: false);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {granteeLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: true);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {grantorLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: true);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				SqlSecurityUtils.ServerPermission.Create(TestConnection, permission, grantorLoginName, granteeLoginName);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: true);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {granteeLoginName.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {grantorLoginName.QuoteName()}");
			}
		}

		public void TestServerPermissionRevoke()
		{
			var grantorLoginName = "fake grantor login";
			var granteeLoginName = "fake grantee login";
			var permission = new DbPermission(DbPermissionState.Grant, DbPermissionType.Impersonate, DbPermissionClass.Login);

			try
			{
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: false);
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: false);

				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {granteeLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, granteeLoginName, expected: true);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {grantorLoginName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");
				SqlSecurityUtils.AssertLoginExists(TestConnection, grantorLoginName, expected: true);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				SqlSecurityUtils.ServerPermission.Revoke(TestConnection, permission, grantorLoginName, granteeLoginName);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);

				SqlSecurityUtils.ServerPermission.Create(TestConnection, permission, grantorLoginName, granteeLoginName);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: true);

				SqlSecurityUtils.ServerPermission.Revoke(TestConnection, permission, grantorLoginName, granteeLoginName);
				SqlSecurityUtils.AssertServerPermissionExistsByGrantor(TestConnection, grantorLoginName, permission, expected: false);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP LOGIN {granteeLoginName.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {grantorLoginName.QuoteName()}");
			}
		}

		public void TestServerPermissionGetImpersonatePermissionGranteePrincipalNames()
		{
			var loginName1 = "fake login 1";
			var loginName2 = "fake login 2";
			var loginName3 = "fake login 3";

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName1, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName2, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName3, ">J6]Ld.ff=[&");

				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
				{
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName1.QuoteName()} TO {loginName2.QuoteName()}");
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName1.QuoteName()} TO {loginName3.QuoteName()}");
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName2.QuoteName()} TO {loginName3.QuoteName()}");
				}

				var granteePrincipalNames1 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName1);
				var granteePrincipalNames2 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName2);
				var granteePrincipalNames3 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName3);

				AssertEquals(2, granteePrincipalNames1.Count);
				AssertEquals(1, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);

				AssertContainsExactElementsInAnyOrder(new[] { loginName2, loginName3 }, granteePrincipalNames1);
				AssertContainsExactElementsInAnyOrder(new[] { loginName3 }, granteePrincipalNames2);
			}
			finally
			{
				SqlSecurityUtils.Login.Drop(TestConnection, loginName3);
				SqlSecurityUtils.Login.Drop(TestConnection, loginName2);
				SqlSecurityUtils.Login.Drop(TestConnection, loginName1);
			}
		}

		public void TestServerPermissionRevokeImpersonatePermissions()
		{
			var loginName1 = "fake login 1";
			var loginName2 = "fake login 2";
			var loginName3 = "fake login 3";

			try
			{
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName1, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName2, ">J6]Ld.ff=[&");
				SqlSecurityUtils.Login.Create(TestConnection, TestConnection.CurrentDatabase, loginName3, ">J6]Ld.ff=[&");

				using (((ICurrentDbControl)TestConnection).UseDatabase(Db.SqlMasterDb))
				{
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName1.QuoteName()} TO {loginName2.QuoteName()}");
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName1.QuoteName()} TO {loginName3.QuoteName()}");
					TestConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON LOGIN::{loginName2.QuoteName()} TO {loginName3.QuoteName()}");
				}

				var granteePrincipalNames1 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName1);
				var granteePrincipalNames2 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName2);
				var granteePrincipalNames3 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName3);

				AssertEquals(2, granteePrincipalNames1.Count);
				AssertEquals(1, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);

				AssertContainsExactElementsInAnyOrder(new[] { loginName2, loginName3 }, granteePrincipalNames1);
				AssertContainsExactElementsInAnyOrder(new[] { loginName3 }, granteePrincipalNames2);

				SqlSecurityUtils.ServerPermission.RevokeImpersonatePermissions(TestConnection, loginName1);
				SqlSecurityUtils.ServerPermission.RevokeImpersonatePermissions(TestConnection, loginName2);
				SqlSecurityUtils.ServerPermission.RevokeImpersonatePermissions(TestConnection, loginName3);

				granteePrincipalNames1 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName1);
				granteePrincipalNames2 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName2);
				granteePrincipalNames3 = SqlSecurityUtils.ServerPermission.GetImpersonatePermissionGranteePrincipalNames(TestConnection, loginName3);

				AssertEquals(0, granteePrincipalNames1.Count);
				AssertEquals(0, granteePrincipalNames2.Count);
				AssertEquals(0, granteePrincipalNames3.Count);
			}
			finally
			{
				SqlSecurityUtils.Login.Drop(TestConnection, loginName3);
				SqlSecurityUtils.Login.Drop(TestConnection, loginName2);
				SqlSecurityUtils.Login.Drop(TestConnection, loginName1);
			}
		}

		public void TestLoginSidMatchesDbUserSid()
		{
			var dbUserName = "fake user";

			try
			{
				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: false);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, dbUserName));
				SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {dbUserName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");

				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, dbUserName));
				SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE USER {dbUserName.QuoteName()}");

				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				AssertEquals("Login Sid Matches DbUser Sid?", true, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("Login Sid Matches DbUser Sid?", true, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, dbUserName));
				SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);

				TestConnection.ExecuteNonQuery($"DROP LOGIN {dbUserName.QuoteName()}");

				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: false);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, dbUserName));
				SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);

				TestConnection.ExecuteNonQuery($"CREATE LOGIN {dbUserName.QuoteName()} WITH PASSWORD = '<enterStrongPasswordHere>'");

				SqlSecurityUtils.AssertLoginExists(TestConnection, dbUserName, expected: true);
				SqlSecurityUtils.AssertDbUserExists(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: true);
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName));
				AssertEquals("Login Sid Matches DbUser Sid?", false, SqlSecurityUtils.LoginSidMatchesDbUserSid(TestConnection, dbUserName));
				SqlSecurityUtils.AssertLoginSidMatchesDbUserSid(TestConnection, TestConnection.CurrentDatabase, dbUserName, expected: false);
			}
			finally
			{
				TestConnection.ExecuteNonQuery($"DROP USER {dbUserName.QuoteName()}");
				TestConnection.ExecuteNonQuery($"DROP LOGIN {dbUserName.QuoteName()}");
			}
		}

		public void TestGetCurrentMainDbLogins()
		{
			var logins = SqlSecurityUtils.GetCurrentMainDbApplicationLogins(TestConnection, TestConnection.CurrentDatabase);

			AssertContains(new RestrictedWriterDatabaseLogin(TestConnection).LoginName, string.Join(System.Environment.NewLine, logins));
		}

		#region Implementation

		AdminConnection TestConnection { get; set; }

		protected override void SetUp()
		{
			base.SetUp();

			TestConnection = Db.NewAdminConnection();
		}

		protected override void TearDown()
		{
			TestConnection.Dispose();
			TestConnection = null;

			base.TearDown();
		}

		#endregion // Implementation
	}
}
