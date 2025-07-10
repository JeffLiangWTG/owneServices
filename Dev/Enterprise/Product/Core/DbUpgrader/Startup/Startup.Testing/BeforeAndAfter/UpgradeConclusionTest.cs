using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	[UseSnapshotProtection]
	sealed class UpgradeConclusionTest : TestCase
	{
		public void TestPropagateAlwaysOnDatabasesAOMInternalErrorsAreReportedWhenModernSecurityIsOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;
			var loginName = $"EnterpriseDbUser_{Db.DatabaseName}_{nameof(TestPropagateAlwaysOnDatabasesAOMInternalErrorsAreReportedWhenModernSecurityIsOff)}";
			var expectedErrorMessage = Invariant($"Failed to drop login \"{loginName}\". Server: [{Db.ServerName}]");
			var createScript = $@"
declare @sql nvarchar(max)

SET @sql = N'use [master]
CREATE LOGIN [{loginName}] WITH PASSWORD = '''', CHECK_POLICY = OFF;
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D];
CREATE DATABASE [30E86203-9D24-4BEB-B053-AE10AE29567D];'

EXECUTE sp_executesql @sql

SET @sql = N'
USE [30E86203-9D24-4BEB-B053-AE10AE29567D]
EXECUTE sp_changedbowner [{loginName}]'

EXECUTE sp_executesql @sql
";
			var dropScript = $@"
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D]

if (EXISTS (SELECT NULL FROM sys.server_principals WHERE name = '{loginName}'))
begin
	DROP LOGIN {loginName.QuoteName()};
end
";

			var errorReporterMock = new Mock<IErrorReporter>();
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var adminConnection = Db.NewAdminConnection())
			using (new DisposableAction(() => adminConnection.ExecuteNonQuery(dropScript)))
			using (BiServers.TemporarilySetAuditServerToNull())
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				adminConnection.ExecuteNonQuery(createScript);
				var upgradeConclusion = new UpgradeConclusion(adminConnection);

				// Act
				upgradeConclusion.RunAfterUpgradeSteps(Mock.Of<IUpgradeContext>(), loggerMock.Object);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(x => x
					.ShowInfoMessage(It.Is<string>(message => message.StartsWith(expectedErrorMessage))));
				errorReporterMock.Verify(reporter => reporter
				.Report(null, It.Is<string>(message => message.Contains(expectedErrorMessage)), It.IsAny<UpgradeManagerException>()), Times.Once);
			});
		}

		public void TestPropagateAlwaysOnDatabasesDSAInternalErrorsAreReportedWhenModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			var loginName = $"EnterpriseDbUser_{Db.DatabaseName}_{nameof(TestPropagateAlwaysOnDatabasesDSAInternalErrorsAreReportedWhenModernSecurityIsOn)}";
			var expectedErrorMessage = Invariant($"Login '{loginName}' owns one or more database(s). Change the owner of the database(s) before dropping the login.");

			var createScript = $@"
declare @sql nvarchar(max)

SET @sql = N'use [master]
CREATE LOGIN [{loginName}] WITH PASSWORD = '''', CHECK_POLICY = OFF;
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D];
CREATE DATABASE [30E86203-9D24-4BEB-B053-AE10AE29567D];'

EXECUTE sp_executesql @sql

SET @sql = N'
USE [30E86203-9D24-4BEB-B053-AE10AE29567D]
EXECUTE sp_changedbowner [{loginName}]'

EXECUTE sp_executesql @sql
";
			var dropScript = $@"
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D]

if (EXISTS (SELECT NULL FROM sys.server_principals WHERE name = '{loginName}'))
begin
	DROP LOGIN {loginName.QuoteName()};
end
";

			var errorReporterMock = new Mock<IErrorReporter>();
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var adminConnection = Db.NewAdminConnection())
			using (new DisposableAction(() => adminConnection.ExecuteNonQuery(dropScript)))
			using (BiServers.TemporarilySetAuditServerToNull())
			using (BiServers.TemporarilySetDataWarehouseServerToNull())
			{
				adminConnection.ExecuteNonQuery(createScript);
				var upgradeConclusion = new UpgradeConclusion(adminConnection);

				// Act
				upgradeConclusion.RunAfterUpgradeSteps(Mock.Of<IUpgradeContext>(), loggerMock.Object);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(x => x
					.ShowInfoMessage(It.Is<string>(message => message.Contains(expectedErrorMessage))));
				errorReporterMock.Verify(reporter => reporter
				.Report(
					null,
					It.Is<string>(message => message.Contains(expectedErrorMessage)),
					It.IsAny<UpgradeManagerException>()), Times.Once);
			});
		}

		public void TestPropagateAlwaysOnDatabasesSqlExceptionsAreReported()
		{
			// Arrange
			const string expectedErrorMessage = "Failed to propagate AlwaysOn databases";
			var sqlException = SqlExceptionBuilder.CreateSqlException(121, nameof(TestPropagateAlwaysOnDatabasesSqlExceptionsAreReported));
			var loggerMock = new Mock<IUpgradeTaskWorkflowLogger>();
			var errorReporterMock = new Mock<IErrorReporter>();

			loggerMock.Setup(x => x.StartTask("Propagating AlwaysOn databases")).Throws(sqlException);

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			using (var adminConnection = Db.NewAdminConnection())
			{
				var upgradeConclusion = new UpgradeConclusion(adminConnection);

				// Act
				upgradeConclusion.RunAfterUpgradeSteps(Mock.Of<IUpgradeContext>(), loggerMock.Object);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(x => x.ShowInfoMessage(It.Is<string>(message => message.StartsWith(expectedErrorMessage))));
				errorReporterMock.Verify(reporter => reporter
					.Report(null, expectedErrorMessage, It.IsAny<UpgradeManagerException>()), Times.Once);
			});
		}

		public void TestEnsureRegistryItemCacheIsPurged()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var upgradeContext = new Mock<IUpgradeContext>().Object;
				var testLogger = new DummyLoggerForTest();
				var upgConclusion = new UpgradeConclusion(testConnection);
				var setting = DataRegistry.Instance.EHubTesting;
				Assert("Pre-condition: some registry settings are cached", ((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Count > 0);

				upgConclusion.RunAfterUpgradeSteps(upgradeContext, testLogger);
				AssertEquals("Registry setting cache should be empty", 0, ((IRegistryItemDictionaryInternals)RegistryItemDictionary.Instance).Count);
			}
		}

		public void TestDuplicatedStatistics()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgradeContext = new Mock<IUpgradeContext>().Object;

				const string sqlText = @"
CREATE TABLE Duplicated_TestTable
(
	duplicated_stats int
);

INSERT Duplicated_TestTable DEFAULT VALUES;

-- create auto stats for duplicated_stats
SELECT TOP(1) * FROM Duplicated_TestTable WHERE duplicated_stats is NULL;

-- create user stats for duplicated_stats
CREATE STATISTICS _user_duplicated_stats on Duplicated_TestTable(duplicated_stats);

-- create index stats for duplicated_stats
CREATE INDEX _index_duplicated_stats on Duplicated_TestTable(duplicated_stats);

-- create filtered index stats for duplicated_stats
CREATE INDEX _filtered_index_duplicated_stats on Duplicated_TestTable(duplicated_stats) WHERE duplicated_stats is NULL;

INSERT Duplicated_TestTable DEFAULT VALUES;
";
				connection.ExecuteNonQuery(sqlText);

				var testLogger = new DummyLoggerForTest();
				var upgConclusion = new UpgradeConclusion(connection);

				AssertStatisticsCount("Duplicated stats exist for the field duplicated_stats before statistics synchronisation", 4, fieldName: "duplicated_stats");

				upgConclusion.RunAfterUpgradeSteps(upgradeContext, testLogger);

				AssertStatisticsCount("No duplicated stats exist for the field duplicated_stats after statistics synchronisation", 2, fieldName: "duplicated_stats");
				AssertStatisticsName("Are the stats for the field duplicated_stats correct?", "_filtered_index_duplicated_stats; _index_duplicated_stats", fieldName: "duplicated_stats");
			}
		}

		public void TestAlwaysOn()
		{
			using (var connection = Db.NewAdminConnection())
			{
				var upgradeContext = new Mock<IUpgradeContext>().Object;
				var logger = new DummyLoggerForTest();
				var upgConclusion = new UpgradeConclusion(connection);

				upgConclusion.RunAfterUpgradeSteps(upgradeContext, logger);

				AssertContains("Propagating AlwaysOn databases", string.Join(System.Environment.NewLine, logger.Logs));
			}
		}

		void AssertStatisticsCount(string message, int expected, string fieldName)
		{
			var sql = @"
SELECT
	COUNT(*)
FROM
	sys.stats              AS s
	JOIN sys.stats_columns AS sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
	JOIN sys.columns       AS c  ON c.object_id = sc.object_id AND c.column_id = sc.column_id
WHERE 1=1
	AND s.object_id = OBJECT_ID(N'dbo.Duplicated_TestTable', N'U')
	AND c.name = @col_name

";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@col_name", SqlDbType.NVarChar, 128, fieldName);

				var value = cmd.ExecuteScalar();
				AssertEquals(message, expected, Convert.ToInt32(value));
			}
		}

		void AssertStatisticsName(string message, string expected, string fieldName)
		{
			var sql = @"
SELECT
	s.name
FROM
	sys.stats              AS s
	JOIN sys.stats_columns AS sc ON sc.object_id = s.object_id AND sc.stats_id = s.stats_id
	JOIN sys.columns       AS c  ON c.object_id = sc.object_id AND c.column_id = sc.column_id
WHERE 1=1
	AND s.object_id = OBJECT_ID(N'dbo.Duplicated_TestTable', N'U')
	AND c.name = @col_name
ORDER BY
	s.name

";

			var actual = new List<string>();

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameter("@col_name", SqlDbType.NVarChar, 128, fieldName);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						actual.Add((string)reader["name"]);
					}
				}
			}

			AssertEquals(message, expected, string.Join("; ", actual));
		}

		protected override void SetUp()
		{
			base.SetUp();

			// UpgradeConclusion.RunAfterUpgradeSteps invokes many functions internally, which might lead to writes to BI databases.
			// However, we do not expect BI databases to get involved in these tests since it is not the focus of the tests.
			// Therefore, we set the BI servers to empty to avoid any writes to BI databases.
			// Note, if we remove the registry item instead, we will end up using main db server (due to one tricky setup for testing e.g. DbSecurity.LoadAuditServer) which will also result in the same issue.
			DbRegistry.BiAuditServer.SaveValue(string.Empty, Db.Connection);
			DbRegistry.BiDataWarehouseServer.SaveValue(string.Empty, Db.Connection);
		}
	}
}
