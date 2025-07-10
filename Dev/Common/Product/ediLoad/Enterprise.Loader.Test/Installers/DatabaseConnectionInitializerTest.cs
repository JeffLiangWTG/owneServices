using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Loader.Common;
using NUnit.Framework;

#if NET
// Need to use System.Data.SqlClient while CargoWise*.Start is not multi-targeted
using SqlCommand = System.Data.SqlClient.SqlCommand;
using SqlConnection = System.Data.SqlClient.SqlConnection;
#endif

namespace Enterprise.Loader.Testing
{
	class DatabaseConnectionInitializerTest : TestCase
	{
		public void TestInstallExcludingDependenciesConnection()
		{
			Application.ConfigureApplicationServices();
			AssertInitialiseConnectionSucceeds(Db.DatabaseName, false);
		}

		public void TestInstallExcludingDependenciesConnectionFallback()
		{
			Application.ConfigureApplicationServices();
			const string testDb = "!TestInstallExcludingDependenciesConnectionFallback.DB!";

			var testHelper = new EnterpriseDatabaseHelperForTest(new EnterpriseConfiguration() { ServerName = DbServerName, DatabaseName = "master" });
			testHelper.OpenConnection(true);

			DropTestDb(testHelper.Connection, testDb);

			try
			{
				//
				// Fails with a non-existent database
				AssertInitialiseConnectionFails(testDb);

				//
				// Works with admin login (fallback) after creating test database
				RunDbCommand(testHelper.Connection, "CREATE DATABASE [{0}]", testDb);
				AssertInitialiseConnectionSucceeds(testDb, true);
			}
			finally
			{
				DropTestDb(testHelper.Connection, testDb);
			}
		}

		void RunDbCommand(SqlConnection connection, string cmdRaw, params string[] cmdParams)
		{
			var sqlText = String.Format(CultureInfo.InvariantCulture, cmdRaw, cmdParams);

			using (var cmd = new SqlCommand(sqlText, connection))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void DropTestDb(SqlConnection connection, string testDb)
		{
			RunDbCommand(connection, "IF EXISTS(SELECT null FROM sys.databases WHERE name = '{0}') DROP DATABASE [{0}]", testDb);
		}

		void AssertInitialiseConnectionSucceeds(string dbName, bool shouldBeAdmin)
		{
			var testResult = TryInitialiseConnection(dbName, out bool connectionWasAdmin);
			Assert("Installation error:\r\n" + testResult.Message, testResult.IsOK);
			AssertEquals("Should make a connection", true, testResult.IsOK);
			AssertEquals("Login Used", shouldBeAdmin, connectionWasAdmin);
		}

		void AssertInitialiseConnectionFails(string dbName)
		{
			var testResult = TryInitialiseConnection(dbName, out _);

			AssertEquals("Installation failed", true, testResult.IsError);
			AssertEquals("It should not display any other messages for the user.", "Could not connect to database.", testResult.Message, true);
		}

		InstallationResult TryInitialiseConnection(string dbName, out bool connectionWasAdmin)
		{
			var result = InstallationResult.Error("not yet tested");

			DatabaseConnectionInitializer.DatabaseHelperInstance = null;

			var connectionInitialiser = new DatabaseConnectionInitializerForTest(DbServerName, dbName);
			result = connectionInitialiser.InstallExcludingDependencies_Exposed();

			if (DatabaseConnectionInitializer.DatabaseHelperInstance is EnterpriseDatabaseHelperForTest testHelper)
			{
				connectionWasAdmin = testHelper.LastConnectionInstance is not null && testHelper.LastConnectionInstance == testHelper.LastAdminConnectionInstance;
			}
			else
			{
				connectionWasAdmin = false;
			}

			DatabaseConnectionInitializer.DatabaseHelperInstance?.Connection?.Close();
			DatabaseConnectionInitializer.DatabaseHelperInstance?.Connection?.Dispose();

			return result;
		}

		/// <summary>
		/// Db.ServerName 
		/// </summary>
		string DbServerName
		{
			get { return dbServerName ?? (dbServerName = MainArgs.Args.First()); }
		}
		string dbServerName;

		class DatabaseConnectionInitializerForTest : DatabaseConnectionInitializer
		{
			public DatabaseConnectionInitializerForTest(string serverName, string dbName)
				: base(new Installation(new EnterpriseConfiguration() { ServerName = serverName, DatabaseName = dbName }))
			{
			}

			protected override EnterpriseDatabaseHelper CreateNewEnterpriseDatabaseHelper()
			{
				return new EnterpriseDatabaseHelperForTest((EnterpriseConfiguration)Installation.Configuration);
			}

			public InstallationResult InstallExcludingDependencies_Exposed()
			{
				return base.InstallExcludingDependencies();
			}
		}

		class EnterpriseDatabaseHelperForTest : EnterpriseDatabaseHelper
		{
			public EnterpriseDatabaseHelperForTest(EnterpriseConfiguration configuration) : base(configuration)
			{
			}

			public SqlConnection LastAdminConnectionInstance;
			public SqlConnection LastConnectionInstance;
			public SqlConnection LastApplicationLoginConnectionInstance;

			protected internal override SqlConnection OpenNewApplicationLoginConnection()
			{
				LastApplicationLoginConnectionInstance = base.OpenNewApplicationLoginConnection();
				LastConnectionInstance = LastApplicationLoginConnectionInstance;
				return LastApplicationLoginConnectionInstance;
			}
			protected internal override SqlConnection OpenNewAdminConnection()
			{
				LastAdminConnectionInstance = base.OpenNewAdminConnection();
				LastConnectionInstance = LastAdminConnectionInstance;
				return LastAdminConnectionInstance;
			}
		}
	}
}
