using CargoWise.Data;
using CargoWise.DataProtection;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class FixLoginIssueWhenDoingReportTest : TransactionedTestCase
	{
		public void TestFixReaderLoginAtReportServer()
		{
			var reportDb = new ReportDbForSelfHealingTesting(Db.ServerName, Db.DatabaseName);
			var provider = SecondaryServerConnectionProviderProvider.GetProvider(reportDb);
			reportDb.ThrowSqlException1 = true;
			AssertEquals("There shouldn't be a delay if both databases are the same", 0, provider.SecondaryServerConnectionDetails.GetDelayBetweenPrimaryAndReportDatabaseInMinutes());

			reportDb = new ReportDbForSelfHealingTesting(Db.ServerName, Db.DatabaseName);
			reportDb.ThrowSqlException2 = true;
			AssertEquals("There shouldn't be a delay if both databases are the same", 0, provider.SecondaryServerConnectionDetails.GetDelayBetweenPrimaryAndReportDatabaseInMinutes());
		}

		public void TestFixReaderLoginAtPrimaryServer()
		{
			var loginName = RestrictedReaderLoginCredentials.UserNameFor(Db.DatabaseName);

			using (var adminConn = Db.NewAdminConnection(Db.DatabaseName))
			{
				var sqlTextDropLogin = string.Format(@"
					IF  EXISTS (SELECT * FROM master.sys.server_principals WHERE name = N'{0}')
						DROP LOGIN {0}", loginName);
				adminConn.ExecuteNonQuery(sqlTextDropLogin);
			}

			var reportDb = new ReportDbForSelfHealingTesting(Db.ServerName, Db.DatabaseName);
			reportDb.ThrowSqlException1 = true;
			AssertEquals("There shouldn't be a delay if both databases are the same", 0, reportDb.GetDelayBetweenPrimaryAndReportDatabaseInMinutes());
		}

		public class ReportDbForSelfHealingTesting : SecondaryServerConnectionDetailsProvider
		{
			public bool ThrowSqlException1 { get; set; }
			public bool ThrowSqlException2 { get; set; }

			public ReportDbForSelfHealingTesting(string serverName, string dbName)
			{
				this.serverName = serverName;
				this.dbName = dbName;
			}

			protected override string DatabaseName
			{
				get { return dbName; }
			}

			readonly string dbName;

			internal protected override long SuitableDelay
			{
				get { return 5; }
			}

			protected override bool IsPartOfAlwaysOn
			{
				get { return true; }
			}

			protected override string[] GetAllReportServerNames()
			{
				if (!string.IsNullOrEmpty(serverName))
				{
					return new string[1] { serverName };
				}
				else
				{
					return System.Array.Empty<string>();
				}
			}

			readonly string serverName;

			protected override DbConnectionForReportingWrapper GetNewConnectionCore(string reportServerName, string dbUserName, string applicationNameSuffix = null)
			{
				if (ThrowSqlException1)
				{
					ThrowSqlException1 = false;
					ThrowSqlException1ForTest();
				}

				if (ThrowSqlException2)
				{
					ThrowSqlException2 = false;
					ThrowSqlException2ForTest();
				}

				var result = base.GetNewConnectionCore(reportServerName, dbUserName, applicationNameSuffix);
				return result;
			}

			void ThrowSqlException1ForTest()
			{
				var sqlError = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlError(18456, 1, 14, serverName, "Login failed for user ...", "", 65536);
				var errors = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
				var exception = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(errors);
				throw exception;
			}

			void ThrowSqlException2ForTest()
			{
				var sqlError = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlError(4060, 1, 11, serverName, "Cannot open database ...", "", 65536);
				var errors = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlErrorCollection(sqlError);
				var exception = CargoWise.Data.Testing.SqlExceptionBuilder.CreateSqlException(errors);
				throw exception;
			}
		}
	}
}
