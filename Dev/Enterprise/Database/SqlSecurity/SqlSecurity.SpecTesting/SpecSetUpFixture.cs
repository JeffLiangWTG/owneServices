using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.SqlSecurity.Test.NoTestCase;
using TestHelper = Enterprise.SqlSecurity.Test.Helper;

namespace Enterprise.SqlSecurity.SpecTesting
{
	public static class SpecSetupFixture
	{
		public static IDisposable SetupSpecEnvironment()
		{
			var testSetup = new SetUpFixture();

			testSetup.OneTimeSetUp();

			using (var adminConnection = Db.NewAdminConnection(System.Environment.MachineName, Db.SqlMasterDb))
			{
				TestHelper.EnsureExtraDatabasesWithSchemas(adminConnection);
			}

			return new DisposableAction(() =>
			{
				testSetup.OneTimeTearDown();
			});
		}

		public static IDisposable SetupSpecTest(out AdminConnection connection)
		{
			var adminConnection = Db.NewAdminConnection(System.Environment.MachineName, Db.SqlMasterDb);
			connection = adminConnection;
			DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
			TestHelper.DropServerTestEntities(adminConnection, Db.DatabaseName);

			return new DisposableAction(() =>
			{
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = false;
				TestHelper.DropServerTestEntities(adminConnection, Db.DatabaseName);
				adminConnection.Dispose();
			});
		}
	}
}
