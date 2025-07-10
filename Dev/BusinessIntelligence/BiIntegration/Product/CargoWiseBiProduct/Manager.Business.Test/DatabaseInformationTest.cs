using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business
{
	class DatabaseInformationTest : TestCase
	{
		public void TestMainDbInfo()
		{
			var mainDbInfo = new DatabaseInformation(Db.ServerName, Db.DatabaseName, DbType.MainDb);

			AssertEquals("Server Name", Db.ServerName, mainDbInfo.ServerName);
			Assert("Server Version", mainDbInfo.ServerVersion.Contains(Db.Connection.ServerVersionNumber.ToString()));
			AssertEquals("Database Name", Db.DatabaseName, mainDbInfo.DatabaseName);
			AssertEquals("Database Version", SchemaVersion.Application.ToString(), mainDbInfo.DatabaseVersion);
		}

		public void TestAuditDbInfo()
		{
			var auditDbInfo = new DatabaseInformation(Db.ServerName, Db.AuditDatabaseName, DbType.BiDb);

			AssertEquals("Server Name", Db.ServerName, auditDbInfo.ServerName);
			Assert("Server Version", auditDbInfo.ServerVersion.Contains(Db.Connection.ServerVersionNumber.ToString()));
			AssertEquals("Database Name", Db.AuditDatabaseName, auditDbInfo.DatabaseName);
			AssertEquals("Database Version", SchemaVersion.Application.ToString(), auditDbInfo.DatabaseVersion);
		}

		public void TestEdwDbInfo()
		{
			var edwDbInfo = new DatabaseInformation(Db.ServerName, Db.EdwDatabaseName, DbType.BiDb);

			AssertEquals("Server Name", Db.ServerName, edwDbInfo.ServerName);
			Assert("Server Version", edwDbInfo.ServerVersion.Contains(Db.Connection.ServerVersionNumber.ToString()));
			AssertEquals("Database Name", Db.EdwDatabaseName, edwDbInfo.DatabaseName);
			AssertEquals("Database Version", SchemaVersion.Application.ToString(), edwDbInfo.DatabaseVersion);
		}

		[UseSnapshotProtection]
		public void TestMainDdSizeAndUsedSize()
		{
			var initialMainDbInfo = new DatabaseInformation(Db.ServerName, Db.DatabaseName, DbType.MainDb);

			string[] parts = initialMainDbInfo.DatabaseSize.Split(' ');
			decimal initialDbTotalSize = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
			decimal initialDbSizeUsed = decimal.Parse(parts[2].Substring(1), CultureInfo.InvariantCulture);

			string[] logParts = initialMainDbInfo.DatabaseLogSize.Split(' ');
			decimal initialLogSize = decimal.Parse(logParts[0], CultureInfo.InvariantCulture);
			decimal initialLogSizeUsed = decimal.Parse(logParts[2].Substring(1), CultureInfo.InvariantCulture);

			InsertTemporaryData(Db.ServerName, Db.DatabaseName);

			var newMainDbInfo = new DatabaseInformation(Db.ServerName, Db.DatabaseName, DbType.MainDb);

			string[] newParts = newMainDbInfo.DatabaseSize.Split(' ');
			decimal newDbSizeUsed = decimal.Parse(newParts[2].Substring(1), CultureInfo.InvariantCulture);

			string[] newLogParts = newMainDbInfo.DatabaseLogSize.Split(' ');
			decimal newLogSizeUsed = decimal.Parse(newLogParts[2].Substring(1), CultureInfo.InvariantCulture);

			Assert("Database Size", initialDbTotalSize > 0);
			Assert("Database Used Size", newDbSizeUsed - initialDbSizeUsed > 0);
			Assert("Database Log Size", initialLogSize > 0);
			Assert("Database Log Used Size", newLogSizeUsed - initialLogSizeUsed > 0);
		}

		public void TestAuditDbAndEdwDbSizeAndUsedSize() 
		{
			void TestCase(string databaseName)
			{
				var initialDbInfo = new DatabaseInformation(Db.ServerName, databaseName, DbType.BiDb);

				string[] parts = initialDbInfo.DatabaseSize.Split(' ');
				decimal initialDbTotalSize = decimal.Parse(parts[0], CultureInfo.InvariantCulture);
				decimal initialDbSizeUsed = decimal.Parse(parts[2].Substring(1), CultureInfo.InvariantCulture);

				string[] logParts = initialDbInfo.DatabaseLogSize.Split(' ');
				decimal initialLogSize = decimal.Parse(logParts[0], CultureInfo.InvariantCulture);
				decimal initialLogSizeUsed = decimal.Parse(logParts[2].Substring(1), CultureInfo.InvariantCulture);

				var newDbInfo = new DatabaseInformation(Db.ServerName, databaseName, DbType.BiDb);

				string[] newParts = newDbInfo.DatabaseSize.Split(' ');
				decimal newDbSizeUsed = decimal.Parse(newParts[2].Substring(1), CultureInfo.InvariantCulture);

				string[] newLogParts = newDbInfo.DatabaseLogSize.Split(' ');
				decimal newLogSizeUsed = decimal.Parse(newLogParts[2].Substring(1), CultureInfo.InvariantCulture);

				Assert("Database Size", initialDbTotalSize > 0);
				Assert("Database Used Size", newDbSizeUsed - initialDbSizeUsed >= 0);
				Assert("Database Log Size", initialLogSize > 0);
				Assert("Database Log Used Size", newLogSizeUsed - initialLogSizeUsed >= 0);
			}

			TestCase(Db.AuditDatabaseName);
			TestCase(Db.EdwDatabaseName);
		}

		public void TestErrorMessage()
		{
			var dbInfo = new DatabaseInformation("TestServer", "TestDatabase", DbType.BiDb);

			AssertEquals("Server Name", null, dbInfo.ServerName);
			AssertEquals("Server Version", null, dbInfo.ServerVersion);
			AssertEquals("Database Name", null, dbInfo.DatabaseName);
			AssertEquals("Database Version", null, dbInfo.DatabaseVersion);
			AssertEquals("Database Size", string.Empty, dbInfo.DatabaseSize);
			AssertEquals("Database Log Size", string.Empty, dbInfo.DatabaseLogSize);
			Assert("Error Message\r\nActual: " + dbInfo.ErrorMessage, dbInfo.ErrorMessage.Contains("The server was not found or was not accessible."));
		}

		void InsertTemporaryData(string serverName, string databaseName)
		{
			if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(databaseName))
			{
				using (var adminConnection = Db.NewAdminConnection(serverName, databaseName))
				{
					string createTableQuery = @"
					CREATE TABLE TempLargeDataTable (
						ID INT PRIMARY KEY IDENTITY(1,1),
						Data VARBINARY(MAX)
					);";

					using (var command = adminConnection.Command(createTableQuery))
					{
						command.ExecuteNonQuery();
					}

					string insertDataQuery = "INSERT INTO TempLargeDataTable (Data) VALUES (@Data)";
					using (var command = adminConnection.Command(insertDataQuery))
					{
						for (int i = 0; i < 1000; i++)
						{
							byte[] data = BitConverter.GetBytes(i);
							command.RemoveParameterIfExists("@Data");
							command.AddParameter("@Data", SqlDbType.Binary, 32, 0, 0, data);
							command.ExecuteNonQuery();
						}
					}
				}
			}
		}
	}
}
