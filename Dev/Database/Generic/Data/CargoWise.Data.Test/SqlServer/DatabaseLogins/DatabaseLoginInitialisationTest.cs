using CargoWise.Common;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DatabaseLoginInitialisationTest : TestCase
	{
		public void TestBasedDatabaseForLoginMaintenance()
		{
			using (var systemDbConnection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				AssertLoginName(systemDbConnection, Db.SqlMasterDb, "Initial DB = system, Current DB = system");
				AssertLoginName(systemDbConnection, testRefDb, "Initial DB = system, Current DB = dependent");
				AssertLoginName(systemDbConnection, Db.DatabaseName, "Initial DB = system, Current DB = main");
			}

			using (var refDbConnection = Db.NewAdminConnection(testRefDb))
			{
				AssertLoginName(refDbConnection, Db.SqlMasterDb, "Initial DB = dependent, Current DB = system");
				AssertLoginName(refDbConnection, testRefDb, "Initial DB = dependent, Current DB = dependent");
				AssertLoginName(refDbConnection, Db.DatabaseName, "Initial DB = dependent, Current DB = main");
			}

			using (var mainDbConnection = Db.NewAdminConnection())
			{
				AssertLoginName(mainDbConnection, Db.SqlMasterDb, "Initial DB = main, Current DB = system");
				AssertLoginName(mainDbConnection, testRefDb, "Initial DB = main, Current DB = dependent");
				AssertLoginName(mainDbConnection, Db.DatabaseName, "Initial DB = main, Current DB = main");
			}
		}

		void AssertLoginName(AdminConnection connection, string useDb, string assertionContext)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(useDb, nameof(useDb));
			Argument.NotNullOrEmpty(assertionContext, nameof(assertionContext));

			using (((ICurrentDbControl)connection).UseDatabase(useDb))
			{
				var dbLogin = new ReaderDatabaseLogin(connection);
				AssertEquals("Login Name (" + assertionContext + ")", expectedLoginName, dbLogin.LoginName);
			}
		}

		readonly string expectedLoginName = CargoWiseReaderLoginCredentials.UserNameFor(Db.DatabaseName);
		readonly string testRefDb = ((IPhysicalRefDbLocation)Db.Connection).GetReferenceDatabaseName(RefDbTypeEnum.Single, refCountryCode: null);
	}
}
