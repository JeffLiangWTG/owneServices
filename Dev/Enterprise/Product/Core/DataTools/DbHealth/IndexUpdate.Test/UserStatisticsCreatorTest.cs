using System;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	public class UserStatisticsCreatorTest : TransactionedTestCase
	{
		public void TestStatisticsCreatedForColumnsWithoutStatistics()
		{
			// Arrange
			var postfix = Guid.NewGuid().ToString().Substring(0, 8);
			var testDbName = "TestDb4ManualStatisticsCreation" + postfix;
			var testTableName = "TestTable" + postfix;
		
			Env.Registry.ISU_DisableAutoStatisticsDuringUpgrade = false;

			using (var connection = Db.NewAdminConnection())
			using (AdoTestUtils.CreateDbDropExistingDisposable(connection, testDbName))
			using (((ICurrentDbControl)connection).UseDatabase(testDbName))
			{
				string sqlText = $@"
					CREATE TABLE [dbo].[{testTableName}] (
						COL1 INT PRIMARY KEY NOT NULL,
						COL2 VARCHAR(32) DEFAULT '' NOT NULL,
						COL3 VARCHAR(32) DEFAULT '' NOT NULL,
						COL4 VARCHAR(32) DEFAULT '' NOT NULL,
						COL5 VARCHAR(32),
						COL6 VARCHAR(32) DEFAULT '' NOT NULL,
						COL7 AS CONCAT(COL2, COL3),
						INDEX INDEX_COL2 CLUSTERED (COL2),
						INDEX INDEX_COL3_COL4 (COL3, COL4),
						INDEX INDEX_WITH_FILTER_COL5 (COL5) WHERE COL5 IS NOT NULL
					);
			
					CREATE STATISTICS USER_CREATED_STATS_COL6 ON [dbo].[{testTableName}] (COL6);
				";
				connection.ExecuteNonQuery(sqlText);
		
				AssertNumberOfStatisticsForTable(connection, testDbName, testTableName, 5);
		
				var loggerMock = new Mock<ILogger>();
				loggerMock.Setup(x => x.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				var userStatisticsCreator = new UserStatisticsCreator(loggerMock.Object);
		
				// Act
				userStatisticsCreator.Run(connection, testDbName);
		
				// Assert
				AssertNumberOfStatisticsForTable(connection, testDbName, testTableName, 7);
				loggerMock.Verify(logger => logger.Log(LogType.Information, It.IsAny<string>()), Times.Exactly(3));
				loggerMock.Verify(logger => logger.Log(LogType.Information, "Creating statistics for each column that do not have a dedicated statistics"), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, $"(+) CREATE STATISTICS [WTG_COL4] ON [dbo].[{testTableName}] ([COL4]);"), Times.Once);
				loggerMock.Verify(logger => logger.Log(LogType.Information, $"(+) CREATE STATISTICS [WTG_COL5] ON [dbo].[{testTableName}] ([COL5]);"), Times.Once);
			}
		}

		void AssertNumberOfStatisticsForTable(DbConnection connection, string testDbName, string testTableName, int numberOfStatistics)
		{
			var sql = $@"
					SELECT COUNT(*)
					FROM {testDbName.QuoteName()}.sys.stats AS sts
					WHERE 1=1
						AND sts.object_id = OBJECT_ID('{testDbName.QuoteName()}.[dbo].{testTableName.QuoteName()}');
				";

			Assert(numberOfStatistics == Convert.ToInt32(connection.ExecuteScalar(sql)));
		}
	}
}
