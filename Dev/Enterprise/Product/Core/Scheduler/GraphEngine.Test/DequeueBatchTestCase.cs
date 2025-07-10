using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.GraphEngine.Test;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	[UseSnapshotProtection]
	class DequeueBatchTestCase : TestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyEntityFactory = new Lazy<DummyEntityFactory>(() => new DummyEntityFactory());
			Factory1 = new BusinessObjectFactory();
			connection = Db.NewExtraConnectionToMainDb();
			Factory2 = new BusinessObjectFactory(connection);
		}

		DbConnection connection;

		protected override void TearDown()
		{
			connection?.Dispose();
			base.TearDown();
		}

		Lazy<DummyEntityFactory> LazyEntityFactory { get; set; }
		Lazy<DummyGrEngineServiceTask> LazyServiceTask1 { get; set; }
		Lazy<DummyGrEngineServiceTask> LazyServiceTask2 { get; set; }

		void UseBatchServiceTask(int batchSize)
		{
			LazyServiceTask1 = new Lazy<DummyGrEngineServiceTask>(() => GetServiceTask(batchSize, Factory1));
			LazyServiceTask2 = new Lazy<DummyGrEngineServiceTask>(() => GetServiceTask(batchSize, Factory2));
		}

		protected virtual DummyGrEngineServiceTask GetServiceTask(int batchSize, BusinessObjectFactory factory)
		{
			return new DummyGrEngineServiceTask(() => factory, batchSize);
		}

		#endregion

		#region Test vars

		BusinessObjectFactory Factory1 { get; set; }
		BusinessObjectFactory Factory2 { get; set; }

		DummyEntityFactory EntityFactory => LazyEntityFactory.Value;
		DummyGrEngineServiceTask ServiceTask1 => LazyServiceTask1.Value;
		DummyGrEngineServiceTask ServiceTask2 => LazyServiceTask2.Value;

		#endregion

		#region Test Dequeuer

		public void TestDequeuerBatch()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			{
				AssertEquals("Batch size is the batch size.", dequeueBatchSize, batch1.Count());
			}
		}

		public void TestDequeuerBatch_Multi()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			using (var batch2 = ServiceTask2.Dequeuer.LoadBatch(Factory2, null))
			{
				AssertEquals("Batch size is the batch size.", dequeueBatchSize, batch1.Count());
				AssertEquals("Batch size is the batch size.", dequeueBatchSize, batch2.Count());
			}
		}

		public void TestDequeuerBatch_Multi_All()
		{
			int dequeueBatchSize = 10;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			using (var batch2 = ServiceTask2.Dequeuer.LoadBatch(Factory2, null))
			{
				var commonKeys = new HashSet<Guid>(batch1.Concat(batch2).Select(s => s.ParentID));
				AssertContainsExactElementsInAnyOrder("Have all the items", bizos.Select(s => s.PK), commonKeys);
			}
		}

		public void TestLoadBatch_DisposeClearsLocks()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 4).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			var loaded = new List<StmQueueState>();
			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			{
				loaded.AddRange(batch1);
			}

			using (var batch1 = ServiceTask2.Dequeuer.LoadBatch(Factory2, null))
			{
				AssertContainsExactElementsInAnyOrder("As the locks were cleared by dispose, we reload the same items.", loaded.Select(s => s.Identifier), batch1.Select(s => s.Identifier));
			}
		}

		public void TestLoadBatch_DisposeClearsLocksAfterConnectionLost()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 4).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			using (var batch = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			{
				new List<StmQueueState>().AddRange(batch);

				AdoTestUtils.KillConnection(Db.Connection);

				AssertExceptionThrown("SQL app locks lost because connection gone", typeof(SqlLockLostException), () => { Db.Connection.EnsureIsOpen(); });
			}
		}

		public void TestLoadBatch_NotifyAfterLoad_Success()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			var loaded = new List<StmQueueState>();
			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			{
				loaded.AddRange(batch1);
				ServiceTask1.Dequeuer.Notify(batch1.Select(b => new QueueStateResult<StmQueueState>(b, QueueStateResultType.Processed)));
			}

			using (var batch1 = ServiceTask2.Dequeuer.LoadBatch(Factory2, null))
			{
				AssertCollectionNotContains("Nothing in the new batch should be in the notified batch.", batch1, t => loaded.Select(s => s.Identifier).Contains(t.Identifier));
			}
		}

		public void TestLoadBatch_NotifyAfterLoad_Default()
		{
			int dequeueBatchSize = 4;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);

			var loaded = new List<StmQueueState>();
			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
			{
				loaded.AddRange(batch1);
				ServiceTask1.Dequeuer.Notify(batch1.Select(b => new QueueStateResult<StmQueueState>(b, QueueStateResultType.Failed)));
			}

			using (var batch1 = ServiceTask2.Dequeuer.LoadBatch(Factory2, null))
			{
				AssertCollectionNotContains("Nothing in the new batch should be in the notified batch.", batch1, t => loaded.Select(s => s.Identifier).Contains(t.Identifier));
			}
		}

		public void TestLoadBatch_ExceptionAfterAcquiredLock()
		{
			int dequeueBatchSize = 20;
			UseBatchServiceTask(dequeueBatchSize);

			var bizos = Enumerable.Range(0, 20).Select(i => EntityFactory.New(i.ToString())).MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask1.Enqueuer.Enqueue(Factory1);
			var queueStateFactory = (StmQueueStateFactory)ServiceTask1.Setup.Factory;
			AssertExceptionThrown(typeof(InvalidOperationException), () =>
			{
				using (new DisposableAction(() => queueStateFactory.SetProviderThrowExceptionForTestInLoadBatch(true), () => queueStateFactory.SetProviderThrowExceptionForTestInLoadBatch(false)))
				using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory1, null))
				{
				}
			});

			using (var batch1 = ServiceTask1.Dequeuer.LoadBatch(Factory2, null))
			{
				AssertEquals(dequeueBatchSize, batch1.Count());
			}
		}

		#endregion
	}

	[UseSnapshotProtection]
	class ChainDequeueBatchTestCase : DequeueBatchTestCase
	{
		protected override DummyGrEngineServiceTask GetServiceTask(int batchSize, BusinessObjectFactory factory)
		{
			return new DummyGrEngineServiceTask(() => factory, batchSize, chainOption: GrEngineEnums.ChainOption.Yes);
		}
	}
}
