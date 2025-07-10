using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class PrimaryServerConnectionProviderTest : TestCase
	{
		public void TestGetNewConnectionWrapper()
		{
			var provider = new PrimaryServerConnectionProvider();

			using (var connectionWrapper = provider.GetNewConnectionWrapper())
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("DatabaseName", Db.DatabaseName, connectionWrapper.Connection.CurrentDatabase);
				AssertEquals("SPID same as main connection?", true, connectionWrapper.Connection.SPID == Db.Connection.SPID);
				AssertNotEquals("Connection doesn't use RestrictedReaderLogin as it's MainConnection", RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection is not impersonated", RestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), currentDbUser);
				AssertEquals("ServerName", Db.Connection.ServerNameReportedByDatabase, connectionWrapper.Connection.ServerNameReportedByDatabase);
			}
		}

		[UseSnapshotProtection]
		public void TestGetNewConnectionWrapper_Impersonate()
		{
			var provider = new PrimaryServerConnectionProvider();

			using (var testAdminConnection = Db.NewAdminConnection(Db.ServerName, Db.DatabaseName))
			{
				testAdminConnection.ExecuteNonQuery("CREATE USER User_TestGetNewConnectionWrapper_Impersonate WITHOUT LOGIN;");
				testAdminConnection.ExecuteNonQuery($"GRANT IMPERSONATE ON USER:: User_TestGetNewConnectionWrapper_Impersonate TO {UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName)};");
			}

			using (var connectionWrapper = provider.GetNewConnectionWrapper("User_TestGetNewConnectionWrapper_Impersonate"))
			{
				var currentDbUser = connectionWrapper.Connection.ExecuteScalar("SELECT USER_NAME()") as string;

				AssertEquals("DatabaseName", Db.DatabaseName, connectionWrapper.Connection.CurrentDatabase);
				AssertEquals("SPID same as main connection?", false, connectionWrapper.Connection.SPID == Db.Connection.SPID);
				AssertEquals("Connection use UnrestrictedWriterLogin", UnrestrictedWriterLoginCredentials.UserNameFor(Db.DatabaseName), connectionWrapper.Connection.UserLogin);
				AssertEquals("Connection must be impersonated", "User_TestGetNewConnectionWrapper_Impersonate", currentDbUser);
				AssertEquals("ServerName", Db.Connection.ServerNameReportedByDatabase, connectionWrapper.Connection.ServerNameReportedByDatabase);
			}
		}
	}
}
