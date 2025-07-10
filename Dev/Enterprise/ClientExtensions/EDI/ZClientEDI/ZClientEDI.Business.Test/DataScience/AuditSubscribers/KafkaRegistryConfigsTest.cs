using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Schema;
using Confluent.Kafka;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests
{
	class KafkaRegistryConfigsTest : TestCase
	{
		public void TestKafkaTopicLockIsPerTopic()
		{
			var coreConfigs = new[] { new KafkaRegistryConfigs.Core(), new KafkaRegistryConfigs.Core(), new KafkaRegistryConfigs.Core() };
			var billingConfigs = new[] { new KafkaRegistryConfigs.Billing(), new KafkaRegistryConfigs.Billing(), new KafkaRegistryConfigs.Billing() };
			var incidentRelatedConfigs = new[] { new KafkaRegistryConfigs.IncidentRelated(), new KafkaRegistryConfigs.IncidentRelated(), new KafkaRegistryConfigs.IncidentRelated() };
			var productivityConfigs = new[] { new KafkaRegistryConfigs.Productivity(), new KafkaRegistryConfigs.Productivity(), new KafkaRegistryConfigs.Productivity() };
			var issuesConfigs = new[] { new KafkaRegistryConfigs.Issues(), new KafkaRegistryConfigs.Issues(), new KafkaRegistryConfigs.Issues() };

			var coreTopicLock = coreConfigs[0].KafkaTopicLock;
			Assert(ReferenceEquals(coreTopicLock, coreConfigs[1].KafkaTopicLock));
			Assert(ReferenceEquals(coreTopicLock, coreConfigs[2].KafkaTopicLock));

			var billingTopicLock = billingConfigs[0].KafkaTopicLock;
			Assert(ReferenceEquals(billingTopicLock, billingConfigs[1].KafkaTopicLock));
			Assert(ReferenceEquals(billingTopicLock, billingConfigs[2].KafkaTopicLock));

			var incidentRelatedTopicLock = incidentRelatedConfigs[0].KafkaTopicLock;
			Assert(ReferenceEquals(incidentRelatedTopicLock, incidentRelatedConfigs[1].KafkaTopicLock));
			Assert(ReferenceEquals(incidentRelatedTopicLock, incidentRelatedConfigs[2].KafkaTopicLock));

			var productivityTopicLock = productivityConfigs[0].KafkaTopicLock;
			Assert(ReferenceEquals(productivityTopicLock, productivityConfigs[1].KafkaTopicLock));
			Assert(ReferenceEquals(productivityTopicLock, productivityConfigs[2].KafkaTopicLock));

			var issuesTopicLock = issuesConfigs[0].KafkaTopicLock;
			Assert(ReferenceEquals(issuesTopicLock, issuesConfigs[1].KafkaTopicLock));
			Assert(ReferenceEquals(issuesTopicLock, issuesConfigs[2].KafkaTopicLock));

			Assert(!ReferenceEquals(coreTopicLock, billingTopicLock));
			Assert(!ReferenceEquals(coreTopicLock, incidentRelatedTopicLock));
			Assert(!ReferenceEquals(coreTopicLock, productivityTopicLock));
			Assert(!ReferenceEquals(coreTopicLock, issuesTopicLock));
			Assert(!ReferenceEquals(billingTopicLock, incidentRelatedTopicLock));
			Assert(!ReferenceEquals(billingTopicLock, productivityTopicLock));
			Assert(!ReferenceEquals(billingTopicLock, issuesTopicLock));
			Assert(!ReferenceEquals(incidentRelatedTopicLock, productivityTopicLock));
			Assert(!ReferenceEquals(incidentRelatedTopicLock, issuesTopicLock));
		}

		sealed class TestRegistryMock
		{
			public static readonly TestRegistryMock Instance = new();
			public bool EnableSubscriber { get; set; }
			public bool UseTransaction { get; set; }
			public string KafkaTopic { get; set; }
			public string EdiKafkaBootstrapServers { get; set; }
			public string KafkaSaslUsername { get; set; }
			public string KafkaSaslPassword { get; set; }

			public void Reset()
			{
				EnableSubscriber = false;
				UseTransaction = false;
				KafkaTopic = string.Empty;
				EdiKafkaBootstrapServers = string.Empty;
				KafkaSaslUsername = string.Empty;
				KafkaSaslPassword = string.Empty;
			}
		}

		class TestKafkaRegistryConfigs : KafkaRegistryConfigsBase<TestKafkaRegistryConfigs>
		{
			public override bool EnableSubscriber => TestRegistryMock.Instance.EnableSubscriber;
			public override bool UseTransaction => TestRegistryMock.Instance.UseTransaction;
			public override string KafkaTopic => TestRegistryMock.Instance.KafkaTopic;
			protected override string EdiKafkaBootstrapServers => TestRegistryMock.Instance.EdiKafkaBootstrapServers;
			protected override string KafkaSaslUsername => TestRegistryMock.Instance.KafkaSaslUsername;
			protected override string KafkaSaslPassword => TestRegistryMock.Instance.KafkaSaslPassword;

			protected override IProducer<string, string> CreateProducer(ILogger logger)
			{
				return new Mock<IProducer<string, string>>().Object;
			}
		}

		public void TestReuseProducerInstanceAsLongAsRegistryIsNotUpdated()
		{
			TestRegistryMock.Instance.Reset();
			var logger = new TestServiceLogger();
			var producer1 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);
			var producer2 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);
			var producer3 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);

			Assert(ReferenceEquals(producer1, producer2));
			Assert(ReferenceEquals(producer1, producer3));
			Mock.Get(producer1).Verify(p => p.Dispose(), Times.Never);
		}

		public void TestRenewedProducerInstanceAsSoonAsRegistryIsUpdated()
		{
			TestRegistryMock.Instance.Reset();
			var logger = new TestServiceLogger();
			var producer1 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);
			var producer2 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);

			TestRegistryMock.Instance.KafkaSaslUsername = "Batman";
			var producer3 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);
			var producer4 = new TestKafkaRegistryConfigs().GetOrCreateProducer(logger);

			Assert(ReferenceEquals(producer1, producer2));
			Assert(!ReferenceEquals(producer2, producer3));
			Assert(ReferenceEquals(producer3, producer4));

			Mock.Get(producer1).Verify(p => p.Dispose(), Times.Once);
			Mock.Get(producer3).Verify(p => p.Dispose(), Times.Never);
		}
	}

	class KafkaProducerRaceConditionsTest : TestCase
	{
		[Immutable]
		class TestTableFooSchema : ITableSchema
		{
			public string SqlSchemaName => "dbo";
			public string TableName => "TestTableFoo";

			public static readonly TestTableFooSchema Instance = new ();
			public static readonly SchemaPKColumn PK = new (Instance, "TF_PK", true);
			public static readonly SchemaStringColumn TF_Foo = new (Instance, "TF_Foo", 1, SqlDbType.Int, null, true, 0);

			SchemaPKColumn ITableSchema.PK => PK;
			string ITableSchema.PkIndexName => string.Empty;
			SchemaColumnCollection ITableSchema.All { get; } = new SchemaColumnCollection(PK, [TF_Foo]);
			SchemaColumn ITableSchema.GetSchemaColumn(string columnName) => columnName switch
			{
				"TF_PK" => PK,
				"TF_Foo" => TF_Foo,
				_ => null
			};

			public static DataTable CreateDummyChanges(Random random, int numChanges)
			{
				var changes = new DataTable();
				var startLsnColumn = changes.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
				var seqValColumn = changes.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
				var commandIdColumn = changes.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
				var operationColumn = changes.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
				var transactionTimeColumn = changes.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
				var pkColumn = changes.Columns.Add("TF_PK", PK.DotNetType);
				var fooColumn = changes.Columns.Add("TF_Foo", TF_Foo.DotNetType);
				var lsn = new byte[10];
				var seqval = new byte[10];

				for (var i = 0; i < numChanges; ++i)
				{
					var pk = Guid.NewGuid();
					var before = random.Next();
					var after = random.Next();
					var row = changes.Rows.Add();
					row[startLsnColumn] = lsn;
					row[seqValColumn] = seqval;
					row[transactionTimeColumn] = DateTime.UtcNow;
					row[commandIdColumn] = 1;
					row[operationColumn] = 3;
					row[pkColumn] = pk;
					row[fooColumn] = before;
					row.AcceptChanges();
					row.SetModified();
					row[fooColumn] = after;
					row[operationColumn] = 4;
				}

				return changes;
			}
		}

		[Immutable]
		class TestTableBarSchema : ITableSchema
		{
			public string SqlSchemaName => "dbo";
			public string TableName => "TestTableBar";

			public static readonly TestTableFooSchema Instance = new ();
			public static readonly SchemaPKColumn PK = new (Instance, "TB_PK", true);
			public static readonly SchemaStringColumn TB_Bar = new (Instance, "TB_Bar", 1, SqlDbType.Int, null, true, 0);

			SchemaPKColumn ITableSchema.PK => PK;
			string ITableSchema.PkIndexName => string.Empty;
			SchemaColumnCollection ITableSchema.All { get; } = new SchemaColumnCollection(PK, [TB_Bar]);
			SchemaColumn ITableSchema.GetSchemaColumn(string columnName) => columnName switch
			{
				"TB_PK" => PK,
				"TB_Bar" => TB_Bar,
				_ => null
			};

			public static DataTable CreateDummyChanges(Random random, int numChanges)
			{
				var changes = new DataTable();
				var startLsnColumn = changes.Columns.Add(AuditFieldNames.StartLsnFieldName, typeof(byte[]));
				var seqValColumn = changes.Columns.Add(AuditFieldNames.SeqValFieldName, typeof(byte[]));
				var commandIdColumn = changes.Columns.Add(AuditFieldNames.CommandIdFieldName, typeof(int));
				var operationColumn = changes.Columns.Add(AuditFieldNames.OperationFieldName, typeof(int));
				var transactionTimeColumn = changes.Columns.Add(AuditFieldNames.TranEndTimeUtc, typeof(DateTime));
				var pkColumn = changes.Columns.Add("TB_PK", PK.DotNetType);
				var barColumn = changes.Columns.Add("TB_Bar", TB_Bar.DotNetType);
				var lsn = new byte[10];
				var seqval = new byte[10];

				for (var i = 0; i < numChanges; ++i)
				{
					var pk = Guid.NewGuid();
					var before = random.Next();
					var after = random.Next();
					var row = changes.Rows.Add();
					row[startLsnColumn] = lsn;
					row[seqValColumn] = seqval;
					row[commandIdColumn] = 1;
					row[operationColumn] = 3;
					row[pkColumn] = pk;
					row[barColumn] = before;
					row[transactionTimeColumn] = DateTime.UtcNow;
					row.AcceptChanges();
					row.SetModified();
					row[barColumn] = after;
					row[operationColumn] = 4;
				}

				return changes;
			}
		}

		sealed class MockRegistry
		{
			public static readonly MockRegistry Instance = new ();

			public MockRegistry() => ResetForTest();
			public void ResetForTest()
			{
				EnableSubscriber = true;
				UseTransaction = false;
				KafkaTopic = "test-topic-foobar";
				EdiKafkaBootstrapServers = "foo;bar;";
				KafkaSaslUsername = "admin";
				KafkaSaslPassword = "admin";
			}

			public bool EnableSubscriber { get; set; }
			public bool UseTransaction { get; set; }
			public string KafkaTopic { get; set; }
			public string EdiKafkaBootstrapServers { get; set; }
			public string KafkaSaslUsername { get; set; }
			public string KafkaSaslPassword { get; set; }
		}

		sealed class TestConfigs : KafkaRegistryConfigsBase<TestConfigs>
		{
			public override bool EnableSubscriber { get; } = MockRegistry.Instance.EnableSubscriber;
			public override bool UseTransaction { get; } = MockRegistry.Instance.UseTransaction;
			public override string KafkaTopic { get; } = MockRegistry.Instance.KafkaTopic;
			protected override string EdiKafkaBootstrapServers { get; } = MockRegistry.Instance.EdiKafkaBootstrapServers;
			protected override string KafkaSaslUsername { get; } = MockRegistry.Instance.KafkaSaslUsername;
			protected override string KafkaSaslPassword { get; } = MockRegistry.Instance.KafkaSaslPassword;

			protected override IProducer<string, string> CreateProducer(ILogger logger) => new ReentrancyTestProducer() { Name = "Test", ExpectedTopic = KafkaTopic };
			public override IProducer<string, string> GetOrCreateProducer(ILogger logger)
			{
				try
				{
					return base.GetOrCreateProducer(logger);
				}
				finally
				{
					// this is just to increase chance or race-condition
					Thread.Sleep(50);
				}
			}

			public static void ResetForTest() => TestConfigs.ResetKafkaProducerCache();
		}

		sealed class ReentrancyTestProducer : IProducer<string, string>
		{
			public static readonly ConcurrentHashSet<ReentrancyTestProducer> AllInstances = new ();
			public readonly ConcurrentHashSet<Thread> ReentrantThreads = new ();
			public readonly ConcurrentHashSet<Thread> UseAfterDisposeThreads = new ();
			public int reentrancy;
			public bool IsDisposed { get; private set; }
			public string Name { get; set; }
			public string ExpectedTopic { get; set; } = string.Empty;

			public static void ResetForTest()
			{
				AllInstances.Clear();
			}

			public ReentrancyTestProducer()
			{
				_ = AllInstances.TryAdd(this);
			}

			public void Dispose()
			{
				IsDisposed = true;
			}

			public void Produce(string topic, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler) => Produce(topic);
			public void Produce(TopicPartition topicPartition, Message<string, string> message, Action<DeliveryReport<string, string>> deliveryHandler) => Produce(topicPartition.Topic);
			public async Task<DeliveryResult<string, string>> ProduceAsync(string topic, Message<string, string> message, CancellationToken cancellationToken)
			{
				await Task.Yield();
				Produce(topic);
				return new ();
			}

			public async Task<DeliveryResult<string, string>> ProduceAsync(TopicPartition topicPartition, Message<string, string> message, CancellationToken cancellationToken)
			{
				await Task.Yield();
				Produce(topicPartition.Topic);
				return new ();
			}

			void Produce(string topic)
			{
				var currentThread = Thread.CurrentThread;
				if (IsDisposed)
				{
					_ = UseAfterDisposeThreads.TryAdd(currentThread);
				}

				if (topic != ExpectedTopic)
				{
					throw new ArgumentException($"Unexpected topic {topic}");
				}

				var previousReentrancy = Interlocked.Increment(ref reentrancy);
				try
				{
					if (previousReentrancy > 1)
					{
						_ = ReentrantThreads.TryAdd(currentThread);
					}

					Thread.Sleep(10); // Artificially increase the chance of race condition
				}
				finally
				{
					_ = Interlocked.Decrement(ref reentrancy);
				}
			}

			int IClient.AddBrokers(string brokers) => throw new NotImplementedException();

#if NET
			public void SetSaslCredentials(string username, string password) => throw new NotImplementedException();
#endif

			Handle IClient.Handle => throw new NotImplementedException();
			void IProducer<string, string>.Flush(CancellationToken cancellationToken) { }
			void IProducer<string, string>.AbortTransaction(TimeSpan timeout) { }
			void IProducer<string, string>.AbortTransaction() { }
			void IProducer<string, string>.BeginTransaction() { }
			void IProducer<string, string>.CommitTransaction(TimeSpan timeout) { }
			void IProducer<string, string>.CommitTransaction() { }
			int IProducer<string, string>.Flush(TimeSpan timeout) => 0;
			void IProducer<string, string>.InitTransactions(TimeSpan timeout) { }
			int IProducer<string, string>.Poll(TimeSpan timeout) => 0;
			void IProducer<string, string>.SendOffsetsToTransaction(IEnumerable<TopicPartitionOffset> offsets, IConsumerGroupMetadata groupMetadata, TimeSpan timeout) => throw new NotImplementedException();
		}

		sealed class TestFooSubscriber : DataScienceSubscriberToKafkaBase<TestConfigs>
		{
			public override int DataSchemaVersion => 42;
			public override string Code => "DT1";
			public override ITableSchema Table => TestTableFooSchema.Instance;
			public override IEnumerable<SchemaColumn> SpecificColumns { get; } =
			[
				TestTableFooSchema.PK,
				TestTableFooSchema.TF_Foo,
			];
		}

		sealed class TestBarSubscriber : DataScienceSubscriberToKafkaBase<TestConfigs>
		{
			public override int DataSchemaVersion => 69;
			public override string Code => "DT2";
			public override ITableSchema Table => TestTableBarSchema.Instance;
			public override IEnumerable<SchemaColumn> SpecificColumns { get; } =
			[
				TestTableBarSchema.PK,
				TestTableBarSchema.TB_Bar,
			];
		}

		class ServiceTaskParams
		{
			public TimeSpan MinDelay { get; set; } = TimeSpan.Zero;
			public TimeSpan DelayRandomisation { get; set; } = TimeSpan.FromSeconds(1.0);
			public int NumberOfChanges { get; set; } = 100;
		}

		static void FooServiceTask(object threadStartParameters)
		{
			var parameters = (ServiceTaskParams)threadStartParameters;
			var random = new Random();
			var delay = parameters.MinDelay.Add(TimeSpan.FromSeconds(parameters.DelayRandomisation.TotalSeconds * random.NextDouble()));
			Thread.Sleep(delay);

			var changes = TestTableFooSchema.CreateDummyChanges(random, parameters.NumberOfChanges);
			var subscriber = new TestFooSubscriber();
			var logger = new TestServiceLogger();
			subscriber.ProcessChanges(logger, changes);
		}

		static void BarServiceTask(object threadStartParameters)
		{
			var parameters = (ServiceTaskParams)threadStartParameters;
			var random = new Random();
			var delay = parameters.MinDelay.Add(TimeSpan.FromSeconds(parameters.DelayRandomisation.TotalSeconds * random.NextDouble()));
			Thread.Sleep((int)(random.NextDouble() * 1000));

			var changes = TestTableFooSchema.CreateDummyChanges(random, 100);
			var subscriber = new TestFooSubscriber();
			var logger = new TestServiceLogger();
			subscriber.ProcessChanges(logger, changes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockRegistry.Instance.ResetForTest();
			TestConfigs.ResetForTest();
			ReentrancyTestProducer.ResetForTest();
		}

		public void TestStrictKafkaProducerNoReentrant()
		{
			var threads = Enumerable.Range(0, 100).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();

			// ACT
			foreach (var thread in threads)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			// ASSERTS
			AssertEquals("Exactly 1 Kafka producer should have been created", 1, ReentrancyTestProducer.AllInstances.Count);
			var producer = ReentrancyTestProducer.AllInstances.Single();
			AssertEquals("Should not be any reentrant calls to Produce()", 0, producer.ReentrantThreads.Count);
		}

		public void TestStrictKafkaProducerNoReentrantWithRegoChange()
		{
			var threadGroup1 = Enumerable.Range(0, 50).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();
			var threadGroup2 = Enumerable.Range(0, 50).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();

			// ACT
			foreach (var thread in threadGroup1)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threadGroup1)
			{
				thread.Join();
			}

			MockRegistry.Instance.KafkaSaslUsername = "YSL";
			MockRegistry.Instance.KafkaSaslPassword = "Ha";

			foreach (var thread in threadGroup2)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threadGroup2)
			{
				thread.Join();
			}

			// ASSERTS
			var producers = ReentrancyTestProducer.AllInstances.ToList();
			AssertEquals("Exactly 2 Kafka producers should have been created", 2, producers.Count);
			var producer1 = producers[0];
			AssertEquals("Should not be any reentrant calls to Produce() on producer 2", 0, producer1.ReentrantThreads.Count);
			var producer2 = producers[1];
			AssertEquals("Should not be any reentrant calls to Produce() on producer 2", 0, producer2.ReentrantThreads.Count);
		}

		public void TestStrictKafkaProducerNoUseAfterDispose()
		{
			var threads = Enumerable.Range(0, 100).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();

			// ACT
			foreach (var thread in threads)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			// ASSERTS
			AssertEquals("Exactly 1 Kafka producer should have been created", 1, ReentrancyTestProducer.AllInstances.Count);
			var producer = ReentrancyTestProducer.AllInstances.Single();
			AssertEquals("Should not be any use-after-dispose on the producer", 0, producer.UseAfterDisposeThreads.Count);
		}

		public void TestStrictKafkaProducerNoUseAfterDisposeWithRegoChange()
		{
			var threadGroup1 = Enumerable.Range(0, 50).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();
			var threadGroup2 = Enumerable.Range(0, 50).Select(i => (i % 2) == 0 ? new Thread(FooServiceTask) : new Thread(BarServiceTask)).ToList();

			// ACT
			foreach (var thread in threadGroup1)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threadGroup1)
			{
				thread.Join();
			}

			MockRegistry.Instance.KafkaSaslUsername = "YSL";
			MockRegistry.Instance.KafkaSaslPassword = "Ha";

			foreach (var thread in threadGroup2)
			{
				thread.Start(new ServiceTaskParams());
			}

			foreach (var thread in threadGroup2)
			{
				thread.Join();
			}

			// ASSERTS
			var producers = ReentrancyTestProducer.AllInstances.ToList();
			AssertEquals("Exactly 2 Kafka producers should have been created", 2, producers.Count);
			var producer1 = producers[0];
			AssertEquals("Should not be any use-after-dispose on producer 1", 0, producer1.UseAfterDisposeThreads.Count);
			var producer2 = producers[1];
			AssertEquals("Should not be any use-after-dispose on producer 2", 0, producer2.UseAfterDisposeThreads.Count);
		}
	}
}
