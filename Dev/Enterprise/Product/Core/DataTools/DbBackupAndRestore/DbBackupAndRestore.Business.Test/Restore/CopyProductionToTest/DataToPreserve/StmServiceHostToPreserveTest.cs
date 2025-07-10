using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	sealed class StmServiceHostToPreserveTest : TestCase
	{
		public void TestSchema()
		{
			StmServiceHostToPreserveForTesting stmServiceHostToPreserve = new StmServiceHostToPreserveForTesting();
			Assert("StmServiceHost exists", SchemaTestHelper.TableExists(stmServiceHostToPreserve.MainTableName_Exposed));
		}

		public void TestCopyProductionToTest_ComputedColumn()
		{
			// Arrange
			const string targetDbName = "TestDb-" + nameof(StmServiceHostToPreserve);
			const string dataVaultDbName = targetDbName + "-DataVault";

			var stmServiceHostToPreserve = new StmServiceHostToPreserve();
			var initialRecords = new[] { Guid.Parse("2D83C58F-5ADF-4276-93DA-7965BF161033"), Guid.Parse("95F4C785-BBEA-460E-9E48-695330E8BA9C") };

			using (AdoTestUtils.CreateDbDropExistingDisposable(targetDbName))
			using (AdoTestUtils.CreateDbDropExistingDisposable(dataVaultDbName))
			using (var connection = Db.NewAdminConnection(Db.SqlMasterDb))
			{
				PrepareTargetDatabase(connection);
				PerformCopyProductionToTestPreRestore(connection, dataVaultDbName, targetDbName, stmServiceHostToPreserve);
				SimulateRestoreRandomRecords(connection);

				// Act
				PerformCopyProductionToTestPostRestore(connection, dataVaultDbName, targetDbName, stmServiceHostToPreserve);

				// Assert
				AssertContainsExactElementsInAnyOrder("Records should be preserved", initialRecords, FetchCurrentRecords(connection));
			}

			void PrepareTargetDatabase(AdminConnection connection)
			{
				using (((ICurrentDbControl)connection).UseDatabase(targetDbName))
				{
					connection.ExecuteNonQuery(@"
					CREATE TABLE dbo.[StmServiceHost]
					(
						[SH_PK] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
						[SH_DeleteTimeStampUtc] SMALLDATETIME NULL,
						[SH_Status] AS CASE WHEN [SH_DeleteTimeStampUtc] IS NULL THEN 'INS' ELSE 'OBS' end,
					)");

					connection.ExecuteNonQuery($"INSERT INTO dbo.StmServiceHost (SH_PK) values {string.Join(",", initialRecords.Select(pk => $"('{pk}')"))}");
				}
			}

			void SimulateRestoreRandomRecords(DbConnection connection)
			{
				using (((ICurrentDbControl)connection).UseDatabase(targetDbName))
				{
					connection.ExecuteNonQuery("DELETE dbo.StmServiceHost");
					connection.ExecuteNonQuery("INSERT INTO dbo.StmServiceHost (SH_PK) values (NewId()),(NewId()),(NewId())");
				}
			}

			IEnumerable<Guid> FetchCurrentRecords(DbConnection connection)
			{
				var pks = new List<Guid>();
				connection.ExecuteReader($"SELECT SH_PK from {targetDbName.QuoteName()}.dbo.StmServiceHost", reader => pks.Add((Guid)reader[0]));

				return pks;
			}
		}

		static void PerformCopyProductionToTestPreRestore(DbConnection connection, string dataVaultDbName, string targetDbName, IPreserveTestValueScripts preserveTestValueScripts)
		{
			var populateTemporaryDataScript = preserveTestValueScripts.GetPopulateTemporaryDataScript(dataVaultDbName, targetDbName);

			connection.ExecuteNonQuery(populateTemporaryDataScript);
		}

		static void PerformCopyProductionToTestPostRestore(DbConnection connection, string dataVaultDbName, string targetDbName, IPreserveTestValueScripts preserveTestValueScripts)
		{
			var copyRecordsScript = preserveTestValueScripts.GetClearDataToBeOverwrittenByTestDataScript(dataVaultDbName, targetDbName);
			copyRecordsScript += preserveTestValueScripts.GetCopyTempDbDataToTestDbScript(dataVaultDbName, targetDbName);

			connection.ExecuteNonQuery(copyRecordsScript);
		}
	}
}
