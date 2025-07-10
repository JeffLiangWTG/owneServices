using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace CargoWise.Bi.Product.ServiceTask
{
	class CleanupTaskQueueTestCase : TestCase
	{
		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public void TestBidServiceTaskBacklog()
		{
			// Arrange
			using (var connection = Db.NewAdminConnection(Db.EdwDatabaseName))
			{
				IHostedServiceQueueProvider provider = new BiDeploymentTaskQueue();
				var createDate = new ZDateTime(2023, 1, 1);
				var sqlText = $@"
TRUNCATE TABLE biadmin.SsasPartitionUnprocessedDate
INSERT INTO biadmin.SsasPartitionUnprocessedDate(SchemaName, TableName, CreateDate)
SELECT ModelSchemaName, ModelTableName, '{createDate}' FROM biadmin.TransformTableConfiguration WHERE TransformId = 1";
				connection.Command(sqlText).ExecuteNonQuery();

				// Act
				var result = provider.QueueResult;

				// Assert
				var nonEdiStagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => !t.IsEdiClient && t.TableInEdw).Select(t => $"{t.SourceSchema}.{t.SourceTable}");
				var expectedCount = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Count(t => t.TransformId == 1 && nonEdiStagingTables.Contains($"{t.SourceSchema}.{t.StagingTable}"));
				AssertEquals("BID service task queue size", expectedCount, result.QueueSize);
				var expectedAge = ZDateTime.UtcNow - createDate;
				AssertEquals(expectedAge.TotalSeconds, result.MaximumItemAge.TotalSeconds, 60);
			}
		}
	}
}
