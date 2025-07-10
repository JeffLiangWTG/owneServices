using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing.NamespaceToTestAuditSubscriber;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	class AuditSubscriberTaskQueueTest : TestCase
	{
		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestAuditSubscriberServiceTaskBacklog()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var result = connection.ExecuteScalar("select datediff(second, '2021-06-01 00:00:00.000', getutcdate())");

				PrepareTestData(connection);

				var provider = new AuditSubscriberTaskQueueForTest();
				CombineAssertions(() =>
				{
					AssertEquals("AuditSubscriberTaskForTest service task queue size.", 5, provider.QueueResult.QueueSize);
					var expectedAgeSeconds = (int)(DateTime.UtcNow - oldestHistoryDate).TotalSeconds;
					NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, Is.EqualTo(expectedAgeSeconds).Within(1), "AuditSubscriberTaskForTest service task queue age.");
				});
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestAuditSubscriberServiceTaskBacklogShouldNotRunIfNoChangesToCount()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				PrepareTestData(connection);
				var dummyOrgHeaderSubscriber = new DummyOrgHeaderSubscriber();
				var dummyRefCountrySubscriber = new DummyRefCountrySubscriber();
				var setupSql = $@"
delete from biadmin.TableState;
insert into biadmin.TableState (SourceSchemaName, SourceTableName, AetHWMHistorySummaryLsn) VALUES
	('{dummyOrgHeaderSubscriber.Table.SqlSchemaName}', '{dummyOrgHeaderSubscriber.Table.TableName}', 2),
	('{dummyRefCountrySubscriber.Table.SqlSchemaName}', '{dummyRefCountrySubscriber.Table.TableName}', 12);
";
				connection.ExecuteNonQuery(setupSql);

				var provider = new AuditSubscriberTaskQueueForTest();
				AssertEquals("AuditSubscriberTaskForTest service task queue size should be zero when table's AetHWM is below the subscribers LsnHighWaterMark.", 0, provider.QueueResult.QueueSize);
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		[ExpectNoExceptions]
		public void TestClientSpecificAuditSubscriberTaskShouldRun()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				PrepareTestData(connection);
				var provider = new AuditSubscriberTaskQueueForClientSpecificTest();
				AssertEquals("Queue size should be zero when running in non edi environment.", 0, provider.QueueResult.QueueSize);
				AssertEquals("Queue age should be zero when running in non edi environment.", TimeSpan.Zero, provider.QueueResult.MaximumItemAge);
			}
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestReturnsZeroForDisabledSubscribers()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				PrepareTestData(connection);
				var provider = new AuditSubscriberTaskQueueForDisabledSubscriberTest();

				AssertEquals("AuditSubscriberTaskForTest service task queue size.", 0, provider.QueueResult.QueueSize);
			}
		}

		readonly DateTime oldestHistoryDate = new DateTime(2021, 6, 1);

		void PrepareTestData(DbConnection connection)
		{
			var dummyOrgHeaderSubscriber = new DummyOrgHeaderSubscriber();
			var dummyRefCountrySubscriber = new DummyRefCountrySubscriber();
			var insertSql = $@"
insert into [biadmin].[SubscriberControl]
(SubscriberCode, LsnHighWaterMark, PeriodHighWaterMark)
values('{dummyOrgHeaderSubscriber.Code}', {2}, 2209),
('{dummyRefCountrySubscriber.Code}', {12}, 2208);

DECLARE @OHTableName as varchar(50)
set @OHTableName = '{(dummyOrgHeaderSubscriber as IActualDataChangesAuditSubscriber).Table.TableName}'
insert into [biadmin].[CdcHistorySummary] (Lsn, SchemaName, ChangedTableName, LsnPeriod, TranEndTimeUTC, NumberOfRows) values
(1, 'dbo', @OHTableName, 2207, '2022-07-08 05:40:54.837', 1),
(2, 'dbo', @OHTableName, 2208, '2022-08-08 05:40:54.837', 1),

-- include below
(3, 'dbo', @OHTableName, 2209, '2022-09-08 05:40:54.837', 1),
(4, 'dbo', @OHTableName, 2209, '2022-09-08 05:40:54.837', 1);

DECLARE @RCTableName as varchar(50)
set @RCTableName = '{(dummyRefCountrySubscriber as IActualDataChangesAuditSubscriber).Table.TableName}'
insert into [biadmin].[CdcHistorySummary] (Lsn, SchemaName, ChangedTableName, LsnPeriod, TranEndTimeUTC, NumberOfRows) values
(11, 'dbo', @RCTableName, 2207, '2022-07-08 05:40:54.837', 1),
(12, 'dbo', @RCTableName, 2207, '2022-07-08 05:40:54.837', 1),

-- include below
(13, 'dbo', @RCTableName, 2208, '2022-08-08 05:40:54.837', 1),
(14, 'dbo', @RCTableName, 2209, '{oldestHistoryDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}', 1),
(15, 'dbo', @RCTableName, 2209, '{oldestHistoryDate.ToString("yyyy-MM-dd HH:mm:ss.fff")}', 1);

--If AetHWMHistorySummaryLsn = NULL then the table has no changes(not loaded), should be ignored
update biadmin.TableState set AetHWMHistorySummaryLsn = 4 where SourceTableName = @OHTableName;
update biadmin.TableState set AetHWMHistorySummaryLsn = 15 where SourceTableName = @RCTableName;
";
			connection.ExecuteNonQuery(insertSql);
		}

		class AuditSubscriberTaskForTest : AuditSubscriberTask
		{
			public override string AssemblyName => "Enterprise.AuditDataServices.Subscription";
			public override string SubscriberNamespace => "Enterprise.AuditDataServices.Subscription.Subscribers";
			public override string ServiceTaskCode => throw new NotImplementedException();
			public override string ServiceTaskDescription => throw new NotImplementedException();
		}

		class AuditSubscriberTaskQueueForTest : AuditSubscriberTaskQueue
		{
			protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
			{
				return new AuditSubscriberTaskForTest();
			}
		}

		class AuditSubscriberTaskForClientSpecificTest : ClientSpecificAuditSubscriberTask
		{
			public override string AssemblyName => "Enterprise.AuditDataServices.Subscription";
			public override string SubscriberNamespace => "Enterprise.AuditDataServices.Subscription.Subscribers";
			public override string ServiceTaskCode => throw new NotImplementedException();
			public override string ServiceTaskDescription => throw new NotImplementedException();
		}

		class AuditSubscriberTaskQueueForClientSpecificTest : AuditSubscriberTaskQueue
		{
			protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
			{
				return new AuditSubscriberTaskForClientSpecificTest();
			}
		}

		class AuditSubscriberTaskForDisabledSubscriberTest : AuditSubscriberTask
		{
			public override string AssemblyName => typeof(DisabledSubscriberTest).Assembly.FullName;
			public override string SubscriberNamespace => typeof(DisabledSubscriberTest).Namespace;
			public override string ServiceTaskCode => throw new NotImplementedException();
			public override string ServiceTaskDescription => throw new NotImplementedException();
		}

		class AuditSubscriberTaskQueueForDisabledSubscriberTest : AuditSubscriberTaskQueue
		{
			protected override AuditSubscriberTask GetAuditSubscriberServiceTask()
			{
				return new AuditSubscriberTaskForDisabledSubscriberTest();
			}
		}
	}

	namespace NamespaceToTestAuditSubscriber
	{
		class DisabledSubscriberTest : ActualDataChangesAuditSubscriber
		{
			public override bool IsRequired()
			{
				return false;
			}

			public override bool NotifyInsert => throw new NotImplementedException();

			public override bool NotifyUpdate => throw new NotImplementedException();

			public override bool NotifyDelete => throw new NotImplementedException();

			public override string Code => new DummyOrgHeaderSubscriber().Code;

			public override string Description => throw new NotImplementedException();

			public override ITableSchema Table => throw new NotImplementedException();

			public override IEnumerable<SchemaColumn> SpecificColumns => throw new NotImplementedException();

			public override Action<DataRow> CustomFilter => throw new NotImplementedException();

			public override void ProcessChanges(ILogger logger, DataTable changeTable)
			{
				throw new NotImplementedException();
			}
		}
	}
}
