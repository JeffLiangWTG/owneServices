using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Admin.ServerConfiguration;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Admin.ServerConfiguaration
{
	[TestedType(typeof(ConfigureServer))]

	public class ConfigureServerTest : TestCase, IDisposable
	{
		readonly string testLinkedServerName = "LOOPBACK_TEST_" + Guid.NewGuid().ToString().Replace("-", "");
		readonly AdminConnection connection = Db.NewAdminConnection();

		protected override void SetUp()
		{
			base.SetUp();
			DeleteLinkedServerIfExists();
		}

		void DeleteLinkedServerIfExists()
		{
			connection.ExecuteNonQuery(@"
				IF EXISTS (SELECT * FROM sys.servers WHERE [name] = @LINKEDSERVERNAME) 
				BEGIN 
					EXEC sys.sp_dropserver @LINKEDSERVERNAME, @droplogins='droplogins'
				END",
			cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, testLinkedServerName));
		}

		protected override void TearDown()
		{
			DeleteLinkedServerIfExists();
			base.TearDown();
		}

		public void TestEnsureLoopbackLinkedServerIsConfigured()
		{
			var exists = QueryLinkedExists(testLinkedServerName);
			AssertEquals(false, exists);

			ExecuteConfigureServer(connection, testLinkedServerName);

			exists = QueryLinkedExists(testLinkedServerName);
			AssertEquals(true, exists);
		}

		void ExecuteConfigureServer(AdminConnection connection, string linkedServerName)
		{
			connection.ExecuteNonQuery(
				@"ConfigureServer",
				cmd =>
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, linkedServerName);
				}
				);
		}

		bool QueryLinkedExists(string name)
		{
			return connection.Exists("FROM sys.servers WHERE [name] = @LINKEDSERVERNAME",
				cmd => cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, name));
		}

		public void Dispose()
		{
			connection?.Dispose();
		}
	}
}

