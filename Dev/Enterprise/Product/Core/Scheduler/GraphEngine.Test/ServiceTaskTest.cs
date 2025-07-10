using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GraphEngine.Test;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	[UseSnapshotProtection]
	class ServiceTaskTest : DummyGrEngineServiceTaskTestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			CleanServiceTask();
			LazyNotifications = new Lazy<NotificationBuffer>(() => new NotificationBuffer());
			LazyEntityFactory = new Lazy<DummyEntityFactory>(() => new DummyEntityFactory());
			Db.ConnectionOverrideForTest = Db.NewExtraConnectionToMainDb();
			factory = new BusinessObjectFactory();
		}

		protected override void TearDown()
		{
			base.TearDown();
			Db.ConnectionOverrideForTest.Dispose();
			Db.ConnectionOverrideForTest = null;
		}

		Lazy<DummyEntityFactory> LazyEntityFactory { get; set; }
		Lazy<DummyGrEngineServiceTask> LazyServiceTask { get; set; }
		Lazy<NotificationBuffer> LazyNotifications { get; set; }

		void CleanServiceTask()
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => factory, int.MaxValue));
		}

		void UseBatchServiceTask(int batchSize)
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => factory, batchSize));
		}

		void UseCapacityServiceTask(int capacity, GrEngineEnums.PreKeyOption keyOption = GrEngineEnums.PreKeyOption.None)
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => factory, int.MaxValue, keyOption: keyOption, capacity: capacity));
		}

		void UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption keyOption, int maxNum = -1)
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => factory, int.MaxValue, keyOption: keyOption, maxBacklogSize: maxNum));
		}

		void UseChain(GrEngineEnums.ChainOption chainOption, int batchSize = int.MaxValue)
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => factory, batchSize, chainOption: chainOption)
			{
				MaxEnqueues = 1
			});
		}

		#endregion

		#region Test vars

		DummyEntityFactory EntityFactory => LazyEntityFactory.Value;
		DummyGrEngineServiceTask ServiceTask => LazyServiceTask.Value;
		NotificationBuffer Notifications => LazyNotifications.Value;
		int SomeLargeNumber => 8;
		int SqrtLarge => (int)Math.Sqrt(SomeLargeNumber);

		#endregion

		#region Test Pre Enqueuer

		public void TestPreEnqueuer_GeneratesKeys()
		{
			// Assert that we can calculate keys before the enqueuer decides processing order.
			// This is because sometimes calculating keys is a bottle-neck.
			// The downside of this using the GrEngine in this case is we may double the amount of load required to process messages.
			// The upside is that we can multiply throughput. 
			var dummies = EntityFactory.New("1", "2").New("1").MakeBizos(factory);
			factory.Save();
			dummies.ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));
			AssertStatusInQueue("The new items have prekey status", QueueStatusCodes.Codes.PreKey, dummies);
		}

		public void TestPreEnqueuer_NoKeys_ShortCircuit()
		{
			// Assert that the pre-enqueuer can short-circuit during key generation (And immediately short circuit/do some parallelisable operation).
			// This is obvious, because there are states like "Message Rejected" that can be determined during key generation of the GrEngine.
			var dummies = EntityFactory.New().New().MakeBizos(factory);
			factory.Save();
			dummies.ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));
			AssertStatusInQueue("As the item has no keys, it is now queued", QueueStatusCodes.Codes.Queued, dummies);
		}

		public void TestPreEnqueuer_FlipToEnqueuer()
		{
			// Assert that when there are items pre-enqueued, the enqueuer makes them either Waiting or Queued.
			var dummies = EntityFactory.New("1").New("2").MakeBizos(factory);
			factory.Save();
			dummies.ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));
			AssertStatusInQueue("The items are preKey", QueueStatusCodes.Codes.PreKey, dummies);

			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory);
			ServiceTask.Enqueuer.Enqueue(factory);
			AssertStatusInQueue("The items have been flipped", QueueStatusCodes.Codes.Queued, dummies);
		}

		[TestDate(2018, 8, 28, 12, 13, 0)]
		public void TestPreEnqueuer_Limit()
		{
			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory, 10);
			// Assert that we can limit whether or not to enqueue
			var dummies = Enumerable.Range(0, 30).Select(s => EntityFactory.New("" + s)).MakeBizos(factory);
			factory.Save();
			dummies.Take(10).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));
			ServiceTask.Enqueuer.Enqueue(factory);
			dummies.Skip(10).Take(10).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));

			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "BBQ", new ZQuery(), new ZQuery(), 20, null))
			{
				AssertEquals("Don't load anything because the queue is full.", 0, batch.Values.Count());
			}

			TestDateAttribute.AddSeconds(19);
			ServiceTask.Enqueuer.Enqueue(factory);

			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "BBQ", new ZQuery(), new ZQuery(), 20, null))
			{
				AssertEquals("Not enough time has passed.", 0, batch.Values.Count());
			}

			TestDateAttribute.AddSeconds(19);
			ServiceTask.Enqueuer.Enqueue(factory);

			UseBatchServiceTask(20);
			using (var batch = ServiceTask.Dequeuer.LoadBatch(factory, null))
			{
				ServiceTask.Dequeuer.Notify(batch.Select(e => new QueueStateResult<StmQueueState>(e, QueueStateResultType.Processed)));
			}

			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory, 10);

			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "BBQ", new ZQuery(), new ZQuery(), 20, null))
			{
				AssertEquals("Load em all, since the queue is cleared", 10, batch.Values.Count());
			}
		}

		[TestDate(2018, 8, 28, 12, 13, 0)]
		public void TestPreEnqueuer_Limit_AvoidStuckness()
		{
			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory, 10);
			// Assert that we can limit whether or not to enqueue
			var dummies = Enumerable.Range(0, 30).Select(s => EntityFactory.New("" + s)).MakeBizos(factory);
			factory.Save();
			dummies.Take(10).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));

			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "BBQ", new ZQuery(), new ZQuery(), 20, null))
			{
				AssertEquals("Find items, because there is a small chance that none of the enqueued items is allowed to be processed", 20, batch.Values.Count());
				ServiceTask.Enqueuer.Enqueue(factory);
				batch.Values.Take(10).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));
			}

			TestDateAttribute.AddSeconds(25);

			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "BBQ", new ZQuery(), new ZQuery(), 20, null))
			{
				AssertEquals("Since there are queued items, block.", 0, batch.Values.Count());
			}
		}

		#endregion

		#region Test Enqueuer

		public void TestNotInQueue()
		{
			var bizo = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			AssertNotInQueue(bizo);
		}

		[ExpectNoExceptions]
		public void TestEnqueue_Empty()
		{
			ServiceTask.Enqueuer.Enqueue(factory);
		}

		public void TestEnqueue_Single()
		{
			var bizo = EntityFactory.New("A").MakeBizo(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);

			AssertReadyToProcess(bizo);
		}

		public void TestEnqueue_Wide()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New(i.ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);

			AssertReadyToProcess(bizos);
			var bizo = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();
			AssertNotInQueue(bizo);
		}

		public void TestEnqueue_Tall()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New("A")).MakeBizos(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);

			AssertReadyToProcess(bizos[0]);
			AssertNotReadyToProcess(bizos.Skip(1).ToArray());
		}

		public void TestEnqueue_WideAndTall()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);

			var toProcess = bizos.Take(SqrtLarge).ToArray();
			AssertReadyToProcess(toProcess);
			AssertNotReadyToProcess(bizos.Except(toProcess).ToArray());
		}

		public void TestEnqueue_ManyTimes()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);
			ServiceTask.Enqueuer.Enqueue(factory);
			ServiceTask.Enqueuer.Enqueue(factory);
			ServiceTask.Enqueuer.Enqueue(factory);

			AssertEquals(bizos.Length, CountInQueue(bizos));
		}

		public void TestEnqueue_ServiceTaskCleared()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals(SomeLargeNumber, CountInQueue(bizos));
			AssertEquals(SomeLargeNumber, ServiceTask.Enqueuer.ItemsLoaded);
			CleanServiceTask();
			AssertEquals("By proving reset works, itemsLoaded can prove load works", 0, ServiceTask.Enqueuer.ItemsLoaded);
		}

		public void TestEnqueue_LoadsFromDatabase()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);
			CleanServiceTask();
			ServiceTask.Enqueuer.Enqueue(factory);

			AssertEquals(SomeLargeNumber, CountInQueue(bizos));
			AssertEquals(SomeLargeNumber, ServiceTask.Enqueuer.ItemsLoaded);
		}

		public void TestEnqueuer_ToggleToOnlyUsePrecalculatedKeys()
		{
			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Nothing pre-keyed so do nothing", 0, CountInQueue(bizos));

			bizos.ForEach(bizo => ServiceTask.PreEnqueuer.Enqueue(bizo, bizo.GetStringKeys()));

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals(SomeLargeNumber, CountInQueue(bizos));

			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory); // Flush the service task.
			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Check again to make sure everything is ok.", SomeLargeNumber, CountInQueue(bizos));
		}

		public void TestCapacity()
		{
			const int CAPACITY = 2;
			UseCapacityServiceTask(CAPACITY);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Do not load more than capacity.", CAPACITY, CountInQueue(bizos));

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Especially the second time", CAPACITY, CountInQueue(bizos));

			using (var batch = ServiceTask.Dequeuer.LoadBatch(factory, null))
			{
				ServiceTask.Dequeuer.Notify(batch.Select(p => new QueueStateResult<StmQueueState>(p, QueueStateResultType.Processed)));
			}

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("There ought to be processed rows", CAPACITY, CountProcessed(bizos));
			AssertEquals("And the queue is repopulated.", CAPACITY * 2, CountInQueue(bizos));
			AssertEquals(CAPACITY, ServiceTask.Enqueuer.ItemsLoaded);
		}

		public void TestCapacity_WithPreKeys()
		{
			const int CAPACITY = 2;
			UseCapacityServiceTask(CAPACITY, GrEngineEnums.PreKeyOption.Mandatory);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			bizos.ForEach(bizo => ServiceTask.PreEnqueuer.Enqueue(bizo, bizo.GetStringKeys()));
			factory.Save();

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Do not load more than capacity.", CAPACITY, CountInQueue(bizos) - GraphEngineTestExtensions.CountInQueue(bizos, QueueStatusCodes.Codes.PreKey));

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("Especially the second time", CAPACITY, CountInQueue(bizos) - GraphEngineTestExtensions.CountInQueue(bizos, QueueStatusCodes.Codes.PreKey));

			using (var batch = ServiceTask.Dequeuer.LoadBatch(factory, null))
			{
				ServiceTask.Dequeuer.Notify(batch.Select(p => new QueueStateResult<StmQueueState>(p, QueueStateResultType.Processed)));
			}

			ServiceTask.Enqueuer.Enqueue(factory);
			AssertEquals("There ought to be processed rows", CAPACITY, CountProcessed(bizos));
			AssertEquals("And the queue is repopulated.", CAPACITY * 2, CountInQueue(bizos) - GraphEngineTestExtensions.CountInQueue(bizos, QueueStatusCodes.Codes.PreKey));
			AssertEquals(CAPACITY, ServiceTask.Enqueuer.ItemsLoaded);
		}

		#endregion

		#region Test Process

		[ExpectNoExceptions]
		public void TestProcess()
		{
			ServiceTask.Process(Notifications);

			AssertMultilineASCIIEquals("Empty service task logs are ok?",
@"Starting
Initializing Grengine
PRS (0)
Enqueuing
QUE (0). BLK (0)
Chains loaded (0)
Updated chains (0)
Union done (0)
Save method updateList (0)
Save method insertList (0)
Queues saved (0)
Ending
",
				Notifications.AsString);
		}

		public void TestProcess_MutateAllOfTheDummies()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Process(Notifications);

			CombineAssertions(() =>
			{
				foreach (var bizo in bizos)
				{
					AssertEquals(bizo.Z0_Number, bizo.Z0_AnotherNumber);
				}
			});
		}

		[TestDate(2016, 07, 03)]
		public void TestLogsArePeriodicallyDeleted()
		{
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New(i.ToString())).MakeBizos(factory);
			factory.Save();

			ServiceTask.Process(Notifications);
			AssertEquals(SomeLargeNumber, CountInQueue(bizos));
			ServiceTask.Process(Notifications);
			AssertEquals(SomeLargeNumber, CountInQueue(bizos));

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(3);

			ServiceTask.Process(Notifications);
			AssertEquals(0, CountInQueue(bizos));
		}

		public void TestProcess_PreEnqueue()
		{
			UsePreKeyedEnqueuer(GrEngineEnums.PreKeyOption.Mandatory);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New((i % SqrtLarge).ToString())).MakeBizos(factory);
			factory.Save();

			bizos.Take(SomeLargeNumber / 2).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));

			ServiceTask.Process(Notifications);
			AssertEquals("First batch of items are keyed", SomeLargeNumber / 2, CountInQueue(bizos));

			factory.Save();
			bizos.Skip(SomeLargeNumber / 2).ForEach(dummy => ServiceTask.PreEnqueuer.Enqueue(dummy, dummy.GetStringKeys()));

			ServiceTask.Process(Notifications);
			AssertEquals("Second batch of items has been keyed.", SomeLargeNumber, CountInQueue(bizos));
		}

		public void TestStmQueueStateCanNotBeChangedWhileQueued()
		{
			var queueState = new StmQueueState(EntityFactory.New("abc").MakeBizo(factory), null, "1");
			queueState.UpdateStatusForTesting(QueueStatusCodes.Codes.Blocked);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			queueState.UpdateStatusForTesting(QueueStatusCodes.Codes.PreKey);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			queueState.UpdateStatusForTesting(QueueStatusCodes.Codes.Queued);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			queueState.UpdateStatusForTesting(QueueStatusCodes.Codes.PreKey);
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals($"StmQueueState.Status should never change after a row is queued: ParentID:{queueState.ParentID}, ChainID:00000000-0000-0000-0000-000000000000", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region Test Chain

		public void TestChain_Empty()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			AssertNoExceptionThrown("Do not blow up on empty service", () => ServiceTask.Process(Notifications));
		}

		public void TestChain_RunningMultipleIsSafe()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(_ => EntityFactory.New("Mung")).MakeBizos(factory);
			factory.Save();
			AssertNoExceptionThrown("Do not blow up on repeat runs.", () =>
			 {
				 ServiceTask.Process(Notifications);
				 ServiceTask.Process(Notifications);
				 ServiceTask.Process(Notifications);
			 });
		}

		public void TestChain_Sequence()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(_ => EntityFactory.New("Mung")).MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Everything is in the same chain so it is all done.", QueueStatusCodes.Codes.Notify, bizos);
		}

		public void TestChain_MultipleChains()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New("Mung" + i % SqrtLarge)).MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Multiple chains all process at the same time.", QueueStatusCodes.Codes.Notify, bizos);
		}

		public void TestChain_NoKeys()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = Enumerable.Range(0, SomeLargeNumber).Select(i => EntityFactory.New()).MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Just like with no chain.", QueueStatusCodes.Codes.Notify, bizos);
		}

		public void TestChain_Merge()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = EntityFactory.New("1").New("1").New("1")
				.New("2").New("2").New("2")
				.New("1", "2").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("The last bizo is not in a chain", QueueStatusCodes.Codes.Notify, bizos.Take(bizos.Length - 1).ToArray());
			AssertStatusInQueue("The last bizo is not in a chain", QueueStatusCodes.Codes.Blocked, bizos[bizos.Length - 1]);
		}

		public void TestChain_Split()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizos = EntityFactory.New("1", "2").New("1").New("1")
				.New("2").New("2").MakeBizos(factory);

			var chain = bizos.Where(b => b.Z0_VarCharMax.Contains("1")).ToArray();

			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("The last bizo is not in a chain", QueueStatusCodes.Codes.Notify, chain);
			AssertStatusInQueue("The last bizo is not in a chain", QueueStatusCodes.Codes.Blocked, bizos.Except(chain).ToArray());
		}

		public void TestChain_WeirdKeys()
		{
			UseChain(GrEngineEnums.ChainOption.Yes);
			var bizo = EntityFactory.New(
			@"
▒▒▒░░░░░░░░░░▄▐░░░░
▒░░░░░░▄▄▄░░▄██▄░░░
░░░░░░▐▀█▀▌░░░░▀█▄░ U HAVE BEEN SPOOKED BY THE
░░░░░░▐█▄█▌░░░░░░▀█▄
░░░░░░░▀▄▀░░░▄▄▄▄▄▀▀
░░░░░▄▄▄██▀▀▀▀░░░░░
░░░░█▀▄▄▄█░▀▀░░░░░░ SPOOKY SKILENTON
░░░░▌░▄▄▄▐▌▀▀▀░░░░░
░▄░▐░░░▄▄░█░▀▀░░░░░
░▀█▌░░░▄░▀█▀░▀░░░░░
░░░░░░░░▄▄▐▌▄▄░░░░░
░░░░░░░░▀███▀█░▄░░░
░░░░░░░▐▌▀▄▀▄▀▐▄░░░
░░░░░░░▐▀░░░░░░▐▌░░
░░░░░░░█░░░░░░░░█░░ ADD THIS TO 3 UNIT TEST OR SKELINTONS WILL EAT YOU
░░░░░░▐▌░░░░░░░░░█░").MakeBizo(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Having a weird key is ok.", QueueStatusCodes.Codes.Notify, bizo);
		}

		public void TestChain_BatchLimit_NoChain()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("2").New("3").New("4").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("The last bizo is not in a chain", QueueStatusCodes.Codes.Notify, bizos);
		}

		public void TestChain_OverBatchLimit_NoChain()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("2").New("3").New("4").New("5").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process in order", QueueStatusCodes.Codes.Notify, bizos.Take(4).ToArray());
			AssertStatusInQueue("Still be queued", QueueStatusCodes.Codes.Queued, bizos.Skip(4).ToArray());

			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process final", QueueStatusCodes.Codes.Notify, bizos.Skip(4).ToArray());
		}

		public void TestChain_OverBatchLimit()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("2").New("3").New("4").New("1").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process in order", QueueStatusCodes.Codes.Notify, bizos.Except(bizos[3]).ToArray());
			AssertStatusInQueue("Chain is not loaded because enough non-chained entities were loaded.", QueueStatusCodes.Codes.Queued, new[] { bizos[3] });

			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process final", QueueStatusCodes.Codes.Notify, new[] { bizos[3] });
		}

		public void TestChain_ProcessDepthFirst()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("1").New("1").New("2").New("2").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process in order", QueueStatusCodes.Codes.Notify, bizos.Take(4).ToArray());
			AssertStatusInQueue("Chain is not loaded because enough non-chained entities were loaded.", QueueStatusCodes.Codes.Blocked, bizos.Skip(4).ToArray());

			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process final", QueueStatusCodes.Codes.Notify, bizos.Skip(4).ToArray());
		}

		public void TestChain_Batches()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("1").New("1").New("1").New("2").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process in order", QueueStatusCodes.Codes.Notify, bizos.Take(4).ToArray());
			AssertStatusInQueue("This the last item was not processed because it was dropped.", QueueStatusCodes.Codes.Queued, bizos.Skip(4).ToArray());

			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Process final", QueueStatusCodes.Codes.Notify, bizos.Skip(4).ToArray());
		}

		public void TestChain_ThreeLongBatches()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("1").New("1").New("1")
				.New("2").New("2").New("2").New("2")
				.New("3").New("3").New("3").New("3").MakeBizos(factory);
			factory.Save();
			ServiceTask.Process(Notifications);
			ServiceTask.Process(Notifications);
			ServiceTask.Process(Notifications);
			ServiceTask.Enqueuer.Enqueue(factory); // Removing the Notify

			AssertStatusInQueue("If we weren't using chains, they would not all be done yet.", QueueStatusCodes.Codes.Processed, bizos);
		}

		public void TestChain_LookInTheDatabase()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = EntityFactory.New("1").New("1").New("1").New("1").MakeBizos(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);
			ServiceTask.Enqueuer.Enqueue(factory);
			AssertSharesChainId(bizos);
		}

		public void TestChain_DoNotAddIdToQueuedItems()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var queuedBizo = EntityFactory.New("1").MakeBizo(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);
			var bizos = EntityFactory.New("1").New("1").New("1").New("1").MakeBizos(factory);
			factory.Save();
			ServiceTask.Enqueuer.Enqueue(factory);
			AssertSharesChainId(bizos);
		}

		[TestDate(2019, 7, 3)]
		public void TestChain_IsProcessedInTheRightOrder()
		{
			UseChain(GrEngineEnums.ChainOption.Yes, batchSize: 4);
			var bizos = new List<DummyBusinessObject>();
			for (int i = 0; i < 4; i++)
			{
				bizos.Add(EntityFactory.New("1").MakeBizo(factory));
				bizos[i].Z0_Date = ZDateTime.Now.AddMinutes(i);
			}
			factory.Save();
			ServiceTask.Process(Notifications);
			bizos.ForEach(b => b.Reload());
			AssertArrayEqualsByElements(bizos.ToArray(), bizos.OrderBy(d => d.Z0_Decimal).ToArray());
		}

		#endregion

		#region Test Update Column Race Condition

		public void TestUpdateColumn_InOrderToNotReopenClosedStmQueueStateRows_IgnoreCompleted()
		{
			var dummies = EntityFactory.New("1").New("2").MakeBizos(factory);
			factory.Save();
			ServiceTask.MaxEnqueues = 1;
			ServiceTask.Process(Notifications);
			AssertStatusInQueue("Don't requeue processed stmqueuestate rows", QueueStatusCodes.Codes.Notify, dummies);
			var allQueues = ServiceTask.Dequeuer.LoadAll();
			((StmQueueStateFactory)ServiceTask.Setup.Factory).UpdateColumn(allQueues, (StmQueueStateSchema.SQS_Status, QueueStatusCodes.Codes.Queued));
			AssertStatusInQueue("Don't requeue processed stmqueuestate rows", QueueStatusCodes.Codes.Notify, dummies);
		}

		#endregion
	}
}
