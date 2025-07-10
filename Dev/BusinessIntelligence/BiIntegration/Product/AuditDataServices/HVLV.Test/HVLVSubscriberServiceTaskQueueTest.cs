using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.AuditDataServices.HVLV.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.HVLV.Test
{
	[TestedType(typeof(HVLVSubscriberServiceTaskQueue))]
	public class HVLVSubscriberServiceTaskQueueTest : TestCase
	{
		public void TestQueueResult_HIG_OnlyStmALog()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (SnapshotCreator.CreateSnapshot(connection, Db.Connection.CloseConnection, Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			{
				var provider = new HVLVSubscriberServiceTaskQueueForTest("HIG");

				PrepareSubscriberHWM(connection, "HIG", new Lsn(2), 2208);
				PrepareCdcHistorySummaryData(connection, StmALogSchema.Constants.TableName, numberOfRowsInsert: 1, numberOfRowsUpdate: 1, numberOfRowsDelete: 1);

				AssertEquals("Should return 0 as there is no effective table change", 0, provider.QueueResult.QueueSize);
			}
		}

		public void TestQueueResult_HIG_WithNoEffectiveChange()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (SnapshotCreator.CreateSnapshot(connection, Db.Connection.CloseConnection, Db.AuditDatabaseName, Db.AuditDatabaseName + "-SS"))
			{
				var provider = new HVLVSubscriberServiceTaskQueueForTest("HIG");

				PrepareCdcHistorySummaryData(connection, StmALogSchema.Constants.TableName);
				PrepareCdcHistorySummaryData(connection, HVLVItemSchema.Constants.TableName, numberOfRowsInsert: 1, numberOfRowsDelete: 1);

				AssertEquals("Should return 0 as there is no effective type of table change", 0, provider.QueueResult.QueueSize);
			}
		}

		[UseSnapshotProtection([DatabaseType.Audit])]
		public void TestQueueResult_HIG_WithHVLVItemCreationChange()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var provider = new HVLVSubscriberServiceTaskQueueForTest("HIG");

				PrepareSubscriberHWM(connection, "HIG", new Lsn(2), 2208);
				PrepareCdcHistorySummaryData(connection, StmALogSchema.Constants.TableName);
				PrepareCdcHistorySummaryData(connection, HVLVItemSchema.Constants.TableName, numberOfRowsUpdate: 1);

				AssertEquals("Should return 2 as there are effective type of table changes", 2, provider.QueueResult.QueueSize);
			}
		}

		void PrepareSubscriberHWM(DbConnection connection, string subscriberCode, Lsn lsnHighWaterMark, short periodHighWaterMark)
		{
			var insertSql = $@"
insert into [biadmin].[SubscriberControl]
(SubscriberCode, LsnHighWaterMark, PeriodHighWaterMark)
values('{subscriberCode}', {lsnHighWaterMark}, {periodHighWaterMark})";
		connection.ExecuteNonQuery(insertSql);
		}

		void PrepareCdcHistorySummaryData(DbConnection connection, string effectiveTableName, int numberOfRowsInsert = 0, int numberOfRowsUpdate = 0, int numberOfRowsDelete = 0)
		{
			var insertSql = $@"
insert into [biadmin].[CdcHistorySummary] (Lsn, SchemaName, ChangedTableName, LsnPeriod, TranEndTimeUTC, NumberOfRows, NumberOfRowsInsert, NumberOfRowsUpdate, NumberOfRowsDelete) values
(1, 'dbo', '{effectiveTableName}', 2207, '2022-07-08 05:40:54.837', 3, {numberOfRowsInsert}, {numberOfRowsUpdate}, {numberOfRowsDelete}),
(2, 'dbo', '{effectiveTableName}', 2207, '2022-07-08 05:40:54.837', 3, {numberOfRowsInsert}, {numberOfRowsUpdate}, {numberOfRowsDelete})

-- include below
insert into [biadmin].[CdcHistorySummary] (Lsn, SchemaName, ChangedTableName, LsnPeriod, TranEndTimeUTC, NumberOfRows, NumberOfRowsInsert, NumberOfRowsUpdate, NumberOfRowsDelete) values
(3, 'dbo', '{effectiveTableName}', 2208, '2022-08-08 05:40:54.837', 3, {numberOfRowsInsert}, {numberOfRowsUpdate}, {numberOfRowsDelete}),
(4, 'dbo', '{effectiveTableName}', 2209, '2022-09-08 05:40:54.837', 3, {numberOfRowsInsert}, {numberOfRowsUpdate}, {numberOfRowsDelete})

--If AetHWMHistorySummaryLsn = NULL then the table has no changes(not loaded), should be ignored
update biadmin.TableState set AetHWMHistorySummaryLsn = 4 where SourceTableName = '{effectiveTableName}';
";
			_ = connection.ExecuteNonQuery(insertSql);
		}

		class HVLVSubscriberServiceTaskQueueForTest : HVLVSubscriberServiceTaskQueue
		{
			public HVLVSubscriberServiceTaskQueueForTest(string namespaceForTesting)
				: base()
			{
				this.namespaceForTesting = namespaceForTesting;
			}

			readonly string namespaceForTesting;

			protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
			{
				return new HVLVSubscriberServiceTaskForTest(namespaceForTesting);
			}
		}

		class HVLVSubscriberServiceTaskForTest : HVLVSubscriberServiceTask
		{
			public HVLVSubscriberServiceTaskForTest(string namespaceForTesting)
				: base()
			{
				this.namespaceForTesting = namespaceForTesting;
			}

			readonly string namespaceForTesting;

			public override string AssemblyName => "Enterprise.AuditDataServices.HVLV.Test";

			public override string SubscriberNamespace => AssemblyName + "." +  namespaceForTesting + ".Subscribers";
		}
	}
}

namespace Enterprise.AuditDataServices.HVLV.Test.HAI.Subscribers
{
	class HVLVItemCreationSubscriberForTest : HVLVItemCreationSubscriber
	{
	}
}

namespace Enterprise.AuditDataServices.HVLV.Test.HIG.Subscribers
{
	class HVLVItemGlowUsageSubscriberForTest : HVLVItemGlowUsageSubscriber
	{
	}
}
