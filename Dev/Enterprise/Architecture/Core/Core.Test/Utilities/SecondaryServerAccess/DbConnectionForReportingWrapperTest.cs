using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DbConnectionForReportingWrapperTest : TestCase
	{
		public void TestDontDisposeConnection()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				connection.EnsureIsOpen();
				AssertEquals("PRE", ConnectionState.Open, connection.State);
				new DbConnectionForReportingWrapper(connection, shouldDisposeConnection: false)
					.Dispose();
				AssertEquals("connection was not closed/disposed", ConnectionState.Open, connection.State);
			}
		}

		[UseSnapshotProtection]
		public void TestImpersonateDbUser_WithDbUserName()
		{
			var connection = Db.NewAdminConnection();
			using (var connectionWrapper = new DbConnectionForReportingWrapper(connection))
			{
				connectionWrapper.Connection.ExecuteNonQuery("CREATE USER User_TestImpersonateDbUser_WithDbUserName WITHOUT LOGIN;");

				_ = connectionWrapper.ImpersonateDbUser("User_TestImpersonateDbUser_WithDbUserName");

				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("User_TestImpersonateDbUser_WithDbUserName", connectionWrapper.Connection.ImpersonatedLogin);
				AssertEquals("User_TestImpersonateDbUser_WithDbUserName", currentDbUser);
			}
			AssertEquals("connection was closed/disposed", ConnectionState.Closed, connection.State);
		}

		[UseSnapshotProtection]
		public void TestImpersonateDbUser_WithoutDbUserName()
		{
			using (var connectionWrapper = new DbConnectionForReportingWrapper(Db.NewAdminConnection()))
			{
				_ = connectionWrapper.ImpersonateDbUser(null);

				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals(null, connectionWrapper.Connection.ImpersonatedLogin);
				AssertEquals("dbo", currentDbUser);
			}
		}

		[UseSnapshotProtection]
		public void TestImpersonateDbUser_WithDbUserName_NoException()
		{
			using (var testAdminConnection = Db.NewAdminConnection())
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestImpersonateDbUser_WithDbUserName_NoException WITHOUT LOGIN;");
			}

			using (var connectionWrapper = new DbConnectionForReportingWrapper(Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName)))
			{
				AssertNoExceptionThrown(() => _ = connectionWrapper.ImpersonateDbUser("User_TestImpersonateDbUser_WithDbUserName_NoException"));

				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;
				AssertEquals("User_TestImpersonateDbUser_WithDbUserName_NoException", currentDbUser);
			}
		}

		[UseSnapshotProtection]
		public void TestImpersonateDbUser_WithDbUserName_ApplicationLoginCannotImpersonateStaffUser_HasException()
		{
			using (var testAdminConnection = Db.NewAdminConnection())
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestImpersonateDbUser_WithDbUserName_ApplicationCannotImpersonateStaffUser_HaveException WITHOUT LOGIN;");
			}

			using (var connectionWrapper = new DbConnectionForReportingWrapper(Db.NewExtraRestrictedWriterConnection(Db.ServerName, Db.DatabaseName)))
			{
				AssertExceptionThrown(typeof(SqlException), $"Cannot execute as the database principal because the principal \"User_TestImpersonateDbUser_WithDbUserName_ApplicationCannotImpersonateStaffUser_HaveException\" does not exist, this type of principal cannot be impersonated, or you do not have permission.",
					() => _ = connectionWrapper.ImpersonateDbUser("User_TestImpersonateDbUser_WithDbUserName_ApplicationCannotImpersonateStaffUser_HaveException"));

				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;
				AssertEquals(RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), currentDbUser);
			}
		}
	}
}
