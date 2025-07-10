using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.IO;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance.Testing
{
	class FileGrowthScriptRunnerTest : TestCase
	{
		public void TestFileGrowthOnFreshDb()
		{
			using (var tempDir = new TempDirectory())
			using (var mainDbConnection = Db.NewAdminConnection())
			using (CreateTemporaryTestDatabase(mainDbConnection, tempDir))
			{
				var dataBaseFileGrowthCollection = new DataBaseFileGrowthCollection(mainDbConnection, FileGrowthTestDb);
				Assert("Fresh database found without default FILEGROWTH for data files", dataBaseFileGrowthCollection.DataFiles.Any());
				Assert("Fresh database found without default FILEGROWTH for log files", dataBaseFileGrowthCollection.LogFiles.Any());

				var fileGrowthScriptRunner = new FileGrowthScriptRunner(mainDbConnection);
				fileGrowthScriptRunner.Run(FileGrowthTestDb);

				dataBaseFileGrowthCollection = new DataBaseFileGrowthCollection(mainDbConnection, FileGrowthTestDb);
				Assert("Database found with default FILEGROWTH for data files after FILEGROWTH update", !dataBaseFileGrowthCollection.DataFiles.Any());
				Assert("Database found with default FILEGROWTH for log files after FILEGROWTH update", !dataBaseFileGrowthCollection.LogFiles.Any());
			}
		}

		#region TemporaryTestDatabase

		IDisposable CreateTemporaryTestDatabase(AdminConnection connection, TempDirectory tempDir)
		{
			AdoTestUtils.DropDbIfExists(connection, FileGrowthTestDb);

			var createSqlText = string.Format(@"
				CREATE DATABASE [{0}]
					ON (name={0}_Data, filename = '{1}\{0}_Data.mdf', SIZE=5MB, FILEGROWTH=1%)
					LOG ON (name={0}_Log, filename = '{1}\{0}_Log.ldf', SIZE=1MB, FILEGROWTH=1%);
				",
				FileGrowthTestDb,
				tempDir.DirectoryName);

			connection.ExecuteNonQuery(createSqlText);

			return new DisposableAction(() => AdoTestUtils.DropDbIfExists(connection, FileGrowthTestDb));
		}

		const string FileGrowthTestDb = "FileGrowthScriptRunner_TestDb";

		#endregion
	}
}
