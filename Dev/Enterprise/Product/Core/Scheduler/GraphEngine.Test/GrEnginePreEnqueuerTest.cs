using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.GraphEngine.Test;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	/// <summary>
	/// Definining terms:
	/// PreEnqueuer:
	///			The part of GrEngine that is identifying which messages can be processed in parallel.
	/// Performance regression monitor:
	///			Making sure that the sql in PreKeyEnqueuer doesn't progressively get worse and worse.
	///			(Basically by timing how long the query takes... :p)
	/// Performance regression level: 
	///			Some increasingly drastic action in order to avoid unnecessary load on SQL Server
	///			especially when SQL server is using (unnecessarily) bad execution plans.
	/// </summary>
	[UseSnapshotProtection]
	class GrEnginePreEnqueuerTest : TestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyEntityFactory = new Lazy<DummyEntityFactory>(() => new DummyEntityFactory());
			Factory1 = new BusinessObjectFactory();
			connection = Db.NewExtraConnectionToMainDb();
			UseBatchServiceTask(100);
		}

		DbConnection connection;

		protected override void TearDown()
		{
			connection?.Dispose();
			base.TearDown();
		}

		Lazy<DummyEntityFactory> LazyEntityFactory { get; set; }
		Lazy<DummyGrEngineServiceTask> LazyServiceTask { get; set; }

		void UseBatchServiceTask(int batchSize)
		{
			LazyServiceTask = new Lazy<DummyGrEngineServiceTask>(() => new DummyGrEngineServiceTask(() => Factory1, batchSize));
		}

		#endregion

		#region Test vars

		BusinessObjectFactory Factory1 { get; set; }

		DummyEntityFactory EntityFactory => LazyEntityFactory.Value;
		DummyGrEngineServiceTask ServiceTask => LazyServiceTask.Value;

		#endregion

		#region Regression testing

		public void TestDummyPreEnqueuer()
		{
			var dummy = EntityFactory.New("1").New("1").New("1").New("1").MakeBizos(Factory1);
			Factory1.Save();
			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(Factory1, "JAM", new ZQuery(), new ZQuery(), 4, null))
			{
				AssertEquals("Precondition test: It can load things.", 4, batch.Values.Count());
			}
		}

		public void TestConcurrentAccess()
		{
			var entityBuilder = EntityFactory.New("1");
			var builderChain = entityBuilder.New("2");
			for (int i = 3; i <= 1000; ++i)
			{
				builderChain = builderChain.New(i.ToString());
			}

			var bizos = new ConcurrentDictionary<ZGuid, bool>(builderChain.MakeBizos(Factory1).Select(x => { x.Z0_Bool = false; return new KeyValuePair<ZGuid, bool>(x.PK, false); }));
			Factory1.Save();

			List<Thread> threads = new List<Thread>();

			int remaining = 1000;
			var errors = new ConcurrentBag<string>();

			for (int i = 0; i < 10; ++i)
			{
				threads.Add(new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							var factory = new BusinessObjectFactory();
							int retrieved = 0;
							var serviceTask = new DummyGrEngineServiceTask(() => factory, 5);
							var query = new ZQuery();
							query.AddToFilter(DummyBizoSchema.Z0_Bool, false);

							do
							{
								using (var batch = serviceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "JAM", query, new ZQuery(), 5, null))
								{
									var retrievedObjs = batch.Values.ToArray();
									retrieved = retrievedObjs.Length;

									for (int k = 0; k < retrievedObjs.Length; ++k)
									{
										Interlocked.Decrement(ref remaining);
										Assert("Shouldn't be pulling random bizos", bizos.TryGetValue(retrievedObjs[k].PK, out var value));
										AssertEquals("Bizo has been processed twice", false, value);
										Assert("Should be able to update the state", bizos.TryUpdate(retrievedObjs[k].PK, true, false));
										retrievedObjs[k].Z0_Bool = true;
									}

									factory.Save();

									if (remaining > 50)
									{
										AssertEquals("Unexpected early exit.", 5, batch.Values.Count());
									}
								}
							}
							while (retrieved > 0);
						}
					}
					catch (AssertionFailedError ex)
					{
						errors.Add(ex.ToString());
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			CombineAssertions(() =>
			{
				foreach (var error in errors)
				{
					HtmlFail(error);
				}
			});

			AssertEquals("Precondition test: All items processed", 0, remaining);
		}

		ZQuery GetCheckFilter()
		{
			var prequeuedQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			prequeuedQuery.AddFilterAndZSQLParameterCollection(
				FormattableString.Invariant($"{DummyBizoSchema.Constants.PK} not in (select {StmQueueStateSchema.Constants.SQS_ParentID} from dbo.StmQueueState where {StmQueueStateSchema.Constants.SQS_Status} in ('{QueueStatusCodes.Codes.PreKey}', '{QueueStatusCodes.Codes.Queued}', '{QueueStatusCodes.Codes.Blocked}'))"),
				new ZSqlParameterCollection()
			);

			return prequeuedQuery;
		}

		public void TestConcurrentAccessCrossTable()
		{
			var entityBuilder = EntityFactory.New("1");
			var builderChain = entityBuilder.New("2");
			for (int i = 3; i <= 1000; ++i)
			{
				builderChain = builderChain.New(i.ToString());
			}

			var bizos = new ConcurrentDictionary<ZGuid, bool>(builderChain.MakeBizos(Factory1).Select(x => { x.Z0_Bool = false; return new KeyValuePair<ZGuid, bool>(x.PK, false); }));
			Factory1.Save();

			List<Thread> threads = new List<Thread>();

			int remaining = 1000;

			for (int i = 0; i < 10; ++i)
			{
				threads.Add(new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var factory = new BusinessObjectFactory();
						int retrieved = 0;
						var serviceTask = new DummyGrEngineServiceTask(() => factory, 5);
						var query = new ZQuery();

						do
						{
							using (var batch = serviceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(factory, "JAM", query, GetCheckFilter(), 5, null))
							{
								var retrievedObjs = batch.Values.ToArray();
								retrieved = retrievedObjs.Length;

								for (int k = 0; k < retrievedObjs.Length; ++k)
								{
									Interlocked.Decrement(ref remaining);
									serviceTask.PreEnqueuer.Enqueue(retrievedObjs[k], new[] { "dummyKey" });
								}
							}
						}
						while (retrieved > 0 || remaining > 50);
					}
				}));
			}

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			var dequeueServiceTask = new DummyGrEngineServiceTask(() => Factory1, 1000);
			var allTheKeys = dequeueServiceTask.Dequeuer.LoadPreKeys(2000, new ZQuery());
			var dupeCount = allTheKeys.GroupBy(x => x.ParentID).Sum(x => x.Count() - 1);
			AssertEquals("Duplicate Message has occured", 0, dupeCount);
			AssertEquals("Precondition test: All items processed", 0, remaining);
		}

		#endregion

		#region Performance monitoring

		public void TestQueryPerformanceMonitor()
		{
			// Test that if the query is slow, we go up a performance level.
			var dummy = EntityFactory.New("1").New("1").New("1").New("1").MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask.PreEnqueuer.DummyPerformanceMonitor.ShouldChangeLevel = (a, b) => true;
			ServiceTask.PreEnqueuer.DummyPerformanceMonitor.StopWatch = new DummyStopwatch { ElapsedMilliseconds = (long)TimeSpan.FromMinutes(10).TotalMilliseconds };
			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(Factory1, "JAM", new ZQuery(), new ZQuery(), 4, null))
			{
				AssertEquals(4, batch.Values.Count());
				AssertEquals(1, ServiceTask.PreEnqueuer.DummyPerformanceMonitor.PerformanceLevel.Item2);
			}
		}

		public void TestQueryPerformanceMonitor_Reset()
		{
			// Test that if performance is good, we go back to the first level.
			var dummy = EntityFactory.New("1").New("1").New("1").New("1").MakeBizos(Factory1);
			Factory1.Save();
			ServiceTask.PreEnqueuer.DummyPerformanceMonitor.ShouldChangeLevel = (a, b) => true;
			ServiceTask.PreEnqueuer.DummyPerformanceMonitor.StopWatch = new DummyStopwatch { ElapsedMilliseconds = (long)TimeSpan.FromMinutes(10).TotalMilliseconds };
			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(Factory1, "JAM", new ZQuery(), new ZQuery(), 4, null))
			{
				AssertEquals(1, ServiceTask.PreEnqueuer.DummyPerformanceMonitor.PerformanceLevel.Item2);
			}

			ServiceTask.PreEnqueuer.DummyPerformanceMonitor.StopWatch = new DummyStopwatch { ElapsedMilliseconds = 0 };
			using (var batch = ServiceTask.PreEnqueuer.LoadPreKeyBatch<DummyBusinessObject>(Factory1, "JAM", new ZQuery(), new ZQuery(), 4, null))
			{
				AssertEquals(0, ServiceTask.PreEnqueuer.DummyPerformanceMonitor.PerformanceLevel.Item2);
			}
		}

		#endregion
	}
}
