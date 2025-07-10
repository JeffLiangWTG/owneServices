using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	// this is currently only for TestCollationOfTime
	// feel free to refactor for performance if all other tests pass
	[UseSnapshotProtection]
	sealed class PerformanceStatisticsCollectorTest : TestCase
	{
		public void TestStartMonitoring_ForEmptyKey_ShouldReportError()
		{
			SetStatisticsCollectionEnabled(EnabledState.Simple);
			PerformanceStatisticsCollector.ResetInstance();

			using (PerformanceStatisticsCollector.StartMonitoring(string.Empty))
			{ }

			AssertEquals("Empty name parameter passed to StartMonitoring. This will cause performance statistics to fail when saving. Please pass a non-whitespace string instead.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[TestDateIncremental(seconds: -1)]
		[ExpectNoExceptions]
		public void TestClockDriftingBackwards()
		{
			SetStatisticsCollectionEnabled(EnabledState.Disabled);
			PerformanceStatisticsCollector.ResetInstance();
			using (PerformanceStatisticsCollector.StartMonitoring("Test"))
			{ Thread.Sleep(10); }
			PerformanceStatisticsCollector.AttemptFlush(true);

			try
			{
				SetStatisticsCollectionEnabled(EnabledState.Simple);
				PerformanceStatisticsCollector.ResetInstance();
				using (PerformanceStatisticsCollector.StartMonitoring("Test"))
				{ Thread.Sleep(10); }
				PerformanceStatisticsCollector.AttemptFlush(true);

				SetStatisticsCollectionEnabled(EnabledState.Detailed);
				PerformanceStatisticsCollector.ResetInstance();
				using (PerformanceStatisticsCollector.StartMonitoring("Test"))
				{ Thread.Sleep(10); }
				PerformanceStatisticsCollector.AttemptFlush(true);
			}
			finally
			{
				WaitForFlushToComplete();
			}
		}

		public void TestStartMonitoring_DoNothingIfNotEnabled()
		{
			var mock = new Mock<ISystemDataRegistry>();
			mock.Setup(m => m.StatisticsCollectionEnabled)
				.Returns("Disabled");

			using (ObjectFactory.Substitute(mock.Object))
			{
				var instance = new SpecificPerformanceStatisticsCollector();

				using (instance.StartMonitoring("Statistic1", "Context"))
				{ }
				AssertEquals("Logs not created if usage analysis disabled", false, instance.HasLogs);
			}

			mock.Verify(m => m.StatisticsCollectionEnabled);
		}

		[TestDate(2011, 10, 11)]
		[TestDateIncremental(0, 0, 0, 1)]
		[ExpectNoExceptions]
		public void TestRecordIsCalledAndClearsLogWhenActionIsComplete()
		{
			var persistenceMock = new Mock<IPerformanceStatisticsPersister>();
			persistenceMock.Setup(x => x
				.Record(It.IsAny<IEnumerable<IPerformanceStatisticCollectorToken>>(), It.IsAny<bool>())).Verifiable();

			var dataRegistryMock = new Mock<ISystemDataRegistry>();
			dataRegistryMock.Setup(x => x.StatisticsCollectionEnabled).Returns("Detailed").Verifiable();

			using (ObjectFactory.Substitute(dataRegistryMock.Object))
			{
				using (ObjectFactory.Substitute(persistenceMock.Object))
				{
					var collector = new SpecificPerformanceStatisticsCollector();

					using (collector.StartMonitoring("Ranch"))
					{
						Thread.Sleep(100);
					}

					dataRegistryMock.Verify();
					persistenceMock.Verify();
					Assert(!collector.HasLogs);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestRecordIsCalledAndDoesNotClearLogWhenActionIsComplete()
		{
			var mockl = new Mock<IPerformanceStatisticsPersister>(MockBehavior.Strict);
			mockl.Setup(m => m.Record(It.IsAny<IEnumerable<IPerformanceStatisticCollectorToken>>(), It.IsAny<bool>()));

			var mock = new Mock<ISystemDataRegistry>();
			mock.Setup(m => m.StatisticsCollectionEnabled)
				.Returns("Detailed");

			using (ObjectFactory.Substitute(mock.Object))
			{
				using (ObjectFactory.Substitute(mockl.Object))
				{
					var collector = new SpecificPerformanceStatisticsCollector();
					using (collector.StartMonitoring("Outer"))
					{
						Assert(!collector.HasLogs);
						using (collector.StartMonitoring("Inner"))
						{
							Assert(!collector.HasLogs);
							Thread.Sleep(100);
						}
						Assert(!collector.HasLogs);
					}
					// logs cleared by Record == true
					Assert(!collector.HasLogs);
				}
				mockl.VerifyAll();
				mock.VerifyAll();
			}
		}

		[TestDate(2017, 11, 10)]
		public void TestNoTimeFallsThroughTheCracksWhenSleepIsAfterInnerCollect()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			TestTimeCore(new Action<Action>(startMonitoring1 =>
			{
				using (PerformanceStatisticsCollector.StartMonitoring("s0"))
				{
					startMonitoring1();
					TestDateAttribute.AddMilliseconds(250);
				}
			}));
		}

		[TestDate(2017, 11, 10)]
		public void TestCollationOfTime()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			TestTimeCore(new Action<Action>(startMonitoring_s1 =>
			{
				using (PerformanceStatisticsCollector.StartMonitoring("s0"))
				{
					TestDateAttribute.AddMilliseconds(250);
					startMonitoring_s1();
				}
			}));
		}

		public void TestCalculateStatisticModeWhenDatabaseUpgradedExceptionHasBeenThrown()
		{
			var mock = new Mock<ISystemDataRegistry>();
			mock.Setup(m => m.StatisticsCollectionEnabled).
				Throws(new DatabaseUpgradedException());
			CleanupUsagesAndSetSavesCount();
			Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;
			using (ObjectFactory.Substitute(mock.Object))
			{
				try
				{
					var instance = new SpecificPerformanceStatisticsCollector();
					AssertEquals("Since an upgrade has happened and we can't touch the DB, we should disable statistics", EnabledState.Disabled, instance.StatisticMode);
				}
				finally
				{
					Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				}
			}

			mock.Verify();
		}

		void TestTimeCore(Action<Action> startMonitoring_s0)
		{
			// statistic0 should accrue 250 milliseconds
			// statistic1 should accrue 2500 milliseconds

			CleanupUsagesAndSetSavesCount();
			SetStatisticsCollectionEnabled(EnabledState.Detailed);

			startMonitoring_s0(() =>
			{
				using (PerformanceStatisticsCollector.StartMonitoring("s1"))
				{
					TestDateAttribute.AddMilliseconds(500);

					using (PerformanceStatisticsCollector.Exclude())
					{
						TestDateAttribute.AddMilliseconds(500);
					}

					TestDateAttribute.AddSeconds(2);
				}
			});

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("s0",
					new[] { new ActionTreeNode("s1") })
			});
			var delta = 0.100m;
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "s0").Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "s0", actionCount: 1, excluded: 0.250m, included: 2.750m, timeDiff: 3.250m, delta: delta);
				AssertActionSingle(usage, "s1", actionCount: 1, excluded: 2.500m, included: 2.500m, timeDiff: 3.000m, delta: delta);
			});
		}

		[TestDateIncremental(0, 0, 0, 1)]
		[ExpectNoExceptions]
		public void TestLargeSave()
		{
			SetStatisticsCollectionEnabled(EnabledState.Detailed);
			PerformanceStatisticsCollector.ResetInstance();

			using (PerformanceStatisticsCollector.StartMonitoring("Statistic0"))
			{
				for (int i = 0; i < 100000; i++)
				{
					using (PerformanceStatisticsCollector.StartMonitoring("Statistic1"))
					{
						using (PerformanceStatisticsCollector.StartMonitoring("Statistic2"))
						{
							using (PerformanceStatisticsCollector.StartMonitoring("Statistic3"))
							{
								using (PerformanceStatisticsCollector.StartMonitoring("Statistic1"))
								{
								}
							}
						}
					}
				}
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();
		}

		[TestDate(2017, 11, 10)]
		public void TestSimpleStatisticsModeReturnsOneEvent()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			CleanupUsagesAndSetSavesCount();
			SetStatisticsCollectionEnabled(EnabledState.Simple);

			using (PerformanceStatisticsCollector.StartMonitoring("Statistic1"))
			{
				TestDateAttribute.AddMilliseconds(150);

				using (PerformanceStatisticsCollector.StartMonitoring("Statistic2"))
				{
					TestDateAttribute.AddMilliseconds(100);
				}

				TestDateAttribute.AddMilliseconds(50);
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("Statistic1")
			});
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "Statistic1").Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "Statistic1", actionCount: 1, excluded: 0.300m, included: 0.300m, timeDiff: 0.300m, delta: 0.100m);
			});
		}

		[TestDate(2017, 11, 10)]
		public void TestDetailed_Tree_Simple()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			CleanupUsagesAndSetSavesCount();

			SetStatisticsCollectionEnabled(EnabledState.Detailed);

			using (PerformanceStatisticsCollector.StartMonitoring("action_1__"))
			{
				TestDateAttribute.AddMilliseconds(50);

				using (PerformanceStatisticsCollector.StartMonitoring("action_11_"))
				{
					TestDateAttribute.AddMilliseconds(50);

					for (int i = 1; i <= 3; i++)
					{
						using (PerformanceStatisticsCollector.StartMonitoring(String.Format("action_11{0}", i)))
						{
							TestDateAttribute.AddMilliseconds(100);
						}
					}

					TestDateAttribute.AddMilliseconds(50);
				}

				TestDateAttribute.AddMilliseconds(50);
			}

			using (PerformanceStatisticsCollector.StartMonitoring("action_2__"))
			{
				TestDateAttribute.AddMilliseconds(50);

				using (PerformanceStatisticsCollector.StartMonitoring("action_21_"))
				{
					TestDateAttribute.AddMilliseconds(100);
				}

				TestDateAttribute.AddMilliseconds(50);
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_1__",
					new[] { new ActionTreeNode("action_11_",
						new[] { new ActionTreeNode("action_111"),
								new ActionTreeNode("action_112"),
								new ActionTreeNode("action_113") }) }),
				new ActionTreeNode("action_2__",
					new[] { new ActionTreeNode("action_21_") })
			});
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_1__").Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_1__", actionCount: 1, excluded: 0.100m, included: 0.500m, timeDiff: 0.500m, delta: 0.100m);
				AssertActionSingle(usage, "action_11_", actionCount: 1, excluded: 0.100m, included: 0.400m, timeDiff: 0.400m, delta: 0.100m);
				AssertActionSingle(usage, "action_111", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: 0.100m);
				AssertActionSingle(usage, "action_112", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: 0.100m);
				AssertActionSingle(usage, "action_113", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: 0.100m);
				AssertActionSingle(usage, "action_2__", actionCount: 1, excluded: 0.100m, included: 0.200m, timeDiff: 0.200m, delta: 0.100m);
				AssertActionSingle(usage, "action_21_", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: 0.100m);
			});
		}

		[TestDate(2018, 5, 29, 17, 50, 0)]
		public void TestDetailed_Tree_Threads()
		{
			var mainThread = Thread.CurrentThread;
			ObjectFactory.Substitute<IStopwatch>(() =>
			{
				if (Thread.CurrentThread == mainThread)
				{
					return new TestDateStopwatch();
				}
				else
				{
					return new FixedDurationStopwatchTest(TimeSpan.FromMilliseconds(10));
				}
			});

			CleanupUsagesAndSetSavesCount();

			SetStatisticsCollectionEnabled(EnabledState.Detailed);

			using (PerformanceStatisticsCollector.StartMonitoring("action_1__"))
			{
				TestDateAttribute.AddMilliseconds(50);

				using (PerformanceStatisticsCollector.StartMonitoring("action_11_"))
				{
					TestDateAttribute.AddMilliseconds(50);

					var thread1 = new Thread(() =>
					{
						SetStatisticsCollectionEnabled(EnabledState.Detailed);

						using (Db.DisposableActionForDbConnection())
						{
							using (PerformanceStatisticsCollector.StartMonitoring("action_thread"))
							{
							}
						}
					});
					thread1.Start();

					var thread2 = new Thread(() =>
					{
						SetStatisticsCollectionEnabled(EnabledState.Detailed);

						using (Db.DisposableActionForDbConnection())
						{
							using (PerformanceStatisticsCollector.StartMonitoring("action_thread"))
							{
							}
						}
					});
					thread2.Start();

					var thread3 = new Thread(() =>
					{
						SetStatisticsCollectionEnabled(EnabledState.Detailed);

						using (Db.DisposableActionForDbConnection())
						{
							using (PerformanceStatisticsCollector.StartMonitoring("action_thread"))
							{
							}
						}
					});
					thread3.Start();

					thread1.Join();
					thread2.Join();
					thread3.Join();

					TestDateAttribute.AddMilliseconds(50);
				}

				using (PerformanceStatisticsCollector.StartMonitoring("action_12_"))
				{
					TestDateAttribute.AddMilliseconds(50);

					var thread1 = new Thread(() =>
					{
						SetStatisticsCollectionEnabled(EnabledState.Detailed);

						using (Db.DisposableActionForDbConnection())
						{
							using (PerformanceStatisticsCollector.StartMonitoring("action_thread"))
							{
							}
						}
					});
					thread1.Start();

					var thread2 = new Thread(() =>
					{
						SetStatisticsCollectionEnabled(EnabledState.Detailed);

						using (Db.DisposableActionForDbConnection())
						{
							using (PerformanceStatisticsCollector.StartMonitoring("action_thread"))
							{
							}
						}
					});
					thread2.Start();

					thread1.Join();
					thread2.Join();
					TestDateAttribute.AddMilliseconds(50);
				}

				TestDateAttribute.AddMilliseconds(50);
			}

			using (PerformanceStatisticsCollector.StartMonitoring("action_2__"))
			{
				TestDateAttribute.AddMilliseconds(50);

				using (PerformanceStatisticsCollector.StartMonitoring("action_21_"))
				{
					TestDateAttribute.AddMilliseconds(100);
				}

				TestDateAttribute.AddMilliseconds(50);
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_thread"),
				new ActionTreeNode("action_thread"),
				new ActionTreeNode("action_thread"),
				new ActionTreeNode("action_thread"),
				new ActionTreeNode("action_thread"),
				new ActionTreeNode("action_1__", new [] {
					new ActionTreeNode("action_11_"),
					new ActionTreeNode("action_12_") }),
				new ActionTreeNode("action_2__", new [] {
					new ActionTreeNode("action_21_") })
			});
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_1__").Single();
			var delta = 0.100m;

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_1__", actionCount: 1, excluded: 0.100m, included: 0.400m, timeDiff: 0.400m, delta: delta);
				AssertActionSingle(usage, "action_11_", actionCount: 1, excluded: 0.200m, included: 0.200m, timeDiff: 0.200m, delta: delta);
				AssertActionSingle(usage, "action_12_", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: delta);
				AssertActionSingle(usage, "action_2__", actionCount: 1, excluded: 0.100m, included: 0.200m, timeDiff: 0.200m, delta: delta);
				AssertActionSingle(usage, "action_21_", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: delta);
				AssertActionFirst(usage, "action_thread", actionCount: 1, excluded: 0.100m, included: 0.100m, timeDiff: 0.100m, delta: delta);
			});
		}

		public void TestResetStatisticMode()
		{
			CleanupUsagesAndSetSavesCount();

			SetStatisticsCollectionEnabled(EnabledState.Detailed);

			using (StartMonitoring("action_1_", expectNoAction: false))
			{
				using (StartMonitoring("action_11", expectNoAction: false))
				{ Thread.Sleep(50); }
			}

			SetStatisticsCollectionEnabled(EnabledState.Simple);
			PerformanceStatisticsCollector.AttemptFlush(true);
			AssertFlushSucceeded();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_1_", new[] {
					new ActionTreeNode("action_11") })
			});
			var delta = 0.050m;
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_1_").Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_1_", actionCount: 1, excluded: 0.000m, included: 0.050m, timeDiff: 0.050m, delta: delta);
				AssertActionSingle(usage, "action_11", actionCount: 1, excluded: 0.050m, included: 0.050m, timeDiff: 0.050m, delta: delta);
			});

			using (StartMonitoring("action_2_", expectNoAction: false))
			{
				using (StartMonitoring("action_21", expectNoAction: true))
				{ Thread.Sleep(50); }
			}

			SetStatisticsCollectionEnabled(EnabledState.Disabled);
			PerformanceStatisticsCollector.AttemptFlush(true);
			AssertFlushSucceeded();

			usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_2_").Single();

			expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_2_")
			});

			CombineAssertions(() =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_2_", actionCount: 1, excluded: 0.050m, included: 0.050m, timeDiff: 0.050m, delta: delta);
			});

			using (StartMonitoring("action_3_", expectNoAction: true))
			{
				using (StartMonitoring("action_31", expectNoAction: true))
				{ Thread.Sleep(50); }
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			AssertNull("No changes since collecting has been disabled", StatisticsTestHelper.GetUsageActions(new BusinessObjectFactory(), "action_3_").SingleOrDefault());
		}

		[TestDate(2017, 11, 10)]
		public void TestSuspendStatisticsCollector()
		{
			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			CleanupUsagesAndSetSavesCount();

			//1. Statistics collector should be collecting by default
			SetStatisticsCollectionEnabled(EnabledState.Detailed);
			using (PerformanceStatisticsCollector.StartMonitoring("action_1_"))
			{
				using (PerformanceStatisticsCollector.StartMonitoring("action_11"))
				{
					TestDateAttribute.AddMilliseconds(50);
				}
			}
			SetStatisticsCollectionEnabled(EnabledState.Disabled);
			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_1_", new[] {
					new ActionTreeNode("action_11") })
			});
			var delta = 0.050m;
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_1_").Single();

			CombineAssertions("Statistics should be collected.", () =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_1_", actionCount: 1, excluded: 0.000m, included: 0.050m, timeDiff: 0.050m, delta: delta);
				AssertActionSingle(usage, "action_11", actionCount: 1, excluded: 0.050m, included: 0.050m, timeDiff: 0.050m, delta: delta);
			});

			CleanupUsagesAndSetSavesCount();

			//2. Statistics collector should NOT be collecting if suspended
			using (PerformanceStatisticsCollector.SuspendStatisticsCollector())
			{
				Action performMonitoringAndAssertNoStatsHaveBeenCollected = () =>
				{
					SetStatisticsCollectionEnabled(EnabledState.Detailed);
					using (PerformanceStatisticsCollector.StartMonitoring($"action_2_"))
					{
						using (PerformanceStatisticsCollector.StartMonitoring($"action_22"))
						{
							TestDateAttribute.AddMilliseconds(50);
						}
					}
					SetStatisticsCollectionEnabled(EnabledState.Disabled);

					PerformanceStatisticsCollector.AttemptFlush(true);
					WaitForFlushToComplete();

					AssertNull("StatisticsCollector has been suspended. No statistics should be collected.", StatisticsTestHelper.GetUsageActions(new BusinessObjectFactory(), "action_2_").SingleOrDefault());

					CleanupUsagesAndSetSavesCount();
				};

				performMonitoringAndAssertNoStatsHaveBeenCollected();

				//Nesting statistics collection suspension
				using (PerformanceStatisticsCollector.SuspendStatisticsCollector())
				{
					performMonitoringAndAssertNoStatsHaveBeenCollected();
				}

				//Statistics collection should still be suspended
				performMonitoringAndAssertNoStatsHaveBeenCollected();
			}

			//3. Statistics collection should resume
			SetStatisticsCollectionEnabled(EnabledState.Detailed);
			using (PerformanceStatisticsCollector.StartMonitoring("action_3_"))
			{
				using (PerformanceStatisticsCollector.StartMonitoring("action_33"))
				{
					TestDateAttribute.AddMilliseconds(50);
				}
			}
			SetStatisticsCollectionEnabled(EnabledState.Disabled);

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("action_3_", new[] {
					new ActionTreeNode("action_33") })
			});
			usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "action_3_").Single();

			CombineAssertions("StatisticsCollector has been resumed. Statistics should be collected.", () =>
			{
				AssertEquals("Tree", expectedTree, new ActionTreeNodeCollection(usage));
				AssertActionSingle(usage, "action_3_", actionCount: 1, excluded: 0.000m, included: 0.050m, timeDiff: 0.050m, delta: delta);
				AssertActionSingle(usage, "action_33", actionCount: 1, excluded: 0.050m, included: 0.050m, timeDiff: 0.050m, delta: delta);
			});
		}

		public void TestCrossThreadErrorReporting()
		{
			var collector = new SpecificPerformanceStatisticsCollector();

			using (collector.StartMonitoring("foreground"))
			{
				var thread = new Thread(() =>
				{
					using (collector.StartMonitoring("background"))
					{
						Thread.Sleep(10);
					}
				});

				thread.Start();
				thread.Join();
			}

			Assert("Error reported with owner and current thread stack trace", ErrorReporter.LastMessageReported.Contains("Attempted to access an object owned by another thread."));
			ErrorReporter.Clear();
		}

		public void TestIsCallerMonitoringPerformanceStatistics()
		{
			CleanupUsagesAndSetSavesCount();
			SetStatisticsCollectionEnabled(EnabledState.Detailed);

			PerformanceStatisticsCollector.ResetInstance();

			AssertEquals("GIVEN IsMonitoring = FALSE", false, PerformanceStatisticsCollector.CalculateIsMonitoring());

			using (PerformanceStatisticsCollector.StartMonitoring("MonitorPerformanceStatistics 1"))
			{
				Thread.Sleep(10);
				AssertEquals("WHEN monitoring, THEN IsMonitoring should be true", true, PerformanceStatisticsCollector.CalculateIsMonitoring());
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();

			AssertEquals("WHEN monitoring finished, THEN IsMonitoring should be false", false, PerformanceStatisticsCollector.CalculateIsMonitoring());
		}

		[UseSnapshotProtection]
		public void TestLoggingNestedPerformanceStatistics_SingleThread_DetailMode()
		{
			TestLoggingNestedPerformanceStatistics("Detail mode", EnabledState.Detailed, shouldLogNestedPerformanceStats: true);

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("LoggingNestedPerformanceStatistics 1 - Indirect Child", new[] { // indirect child is ignored
					new ActionTreeNode("LoggingNestedPerformanceStatistics 2", new[] {
						new ActionTreeNode("LoggingNestedPerformanceStatistics 4 - Direct Child", new[] { // direct child is ignored
							new ActionTreeNode("LoggingNestedPerformanceStatistics 5 - Direct Sibling", 2), // direct sibling is tallied
							new ActionTreeNode("LoggingNestedPerformanceStatistics 6 - Indirect Sibling"), // indirect sibling is treated as different stat
							new ActionTreeNode("LoggingNestedPerformanceStatistics 7"),
							new ActionTreeNode("LoggingNestedPerformanceStatistics 6 - Indirect Sibling") // indirect sibling is treated as different stat
			}) }) }) });
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "LoggingNestedPerformanceStatistics 1 - Indirect Child").Single();

			AssertEquals("GIVEN detailed-mode, WHEN logging THEN should log all the nested stats", expectedTree, new ActionTreeNodeCollection(usage));
		}

		[UseSnapshotProtection]
		public void TestLoggingNestedPerformanceStatistics_SingleThread_SimpleMode()
		{
			TestLoggingNestedPerformanceStatistics("Simple mode", EnabledState.Simple, shouldLogNestedPerformanceStats: false);

			var expectedTree = new ActionTreeNodeCollection(new[] {
				new ActionTreeNode("LoggingNestedPerformanceStatistics 1 - Indirect Child")
			});
			var usage = StatisticsTestHelper.GetUsagesWithMatchingActions(new BusinessObjectFactory(), "LoggingNestedPerformanceStatistics 1 - Indirect Child").Single();

			AssertEquals("GIVEN simple-mode, WHEN logging THEN should log just the root stats", expectedTree, new ActionTreeNodeCollection(usage));
		}

		void TestLoggingNestedPerformanceStatistics(string message, EnabledState mode, bool shouldLogNestedPerformanceStats)
		{
			CleanupUsagesAndSetSavesCount();
			SetStatisticsCollectionEnabled(mode);

			PerformanceStatisticsCollector.ResetInstance();

			using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 1 - Indirect Child"))
			{
				Thread.Sleep(10);
				using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 2"))
				{
					Thread.Sleep(10);
					using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 1 - Indirect Child"))
					{
						Thread.Sleep(10);
						using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 4 - Direct Child"))
						{
							Thread.Sleep(10);
							using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 4 - Direct Child"))
							{
								Thread.Sleep(10);
								using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 5 - Direct Sibling"))
								{
									Thread.Sleep(10);
								}

								using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 5 - Direct Sibling"))
								{
									Thread.Sleep(10);
								}

								using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 6 - Indirect Sibling"))
								{
									Thread.Sleep(10);
								}

								using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 7"))
								{
									Thread.Sleep(10);
								}

								using (PerformanceStatisticsCollector.StartMonitoring("LoggingNestedPerformanceStatistics 6 - Indirect Sibling"))
								{
									Thread.Sleep(10);
								}
							}
						}
					}
				}
			}

			PerformanceStatisticsCollector.AttemptFlush(true);
			WaitForFlushToComplete();
		}

		[TestDate(2023, 10, 18)]
		public void TestFolding_SequentialIdenticalZeroTimeTokensAreCombined()
		{
			var persisterMock = new Mock<IPerformanceStatisticsPersister>();
			var recorded = new List<List<IPerformanceStatisticCollectorToken>>();
			persisterMock.Setup(x => x.Record(It.IsAny<IEnumerable<IPerformanceStatisticCollectorToken>>(), It.IsAny<bool>()))
				.Callback<IEnumerable<IPerformanceStatisticCollectorToken>, bool>((elements, forceWrite) =>
				{
					recorded.Add(elements.ToList());
				});

			var dataRegistryMock = new Mock<ISystemDataRegistry>();
			dataRegistryMock.Setup(x => x.StatisticsCollectionEnabled).Returns("Detailed").Verifiable();

			ObjectFactory.Substitute<IStopwatch>(() => new TestDateStopwatch());

			using (ObjectFactory.Substitute(dataRegistryMock.Object))
			using (ObjectFactory.Substitute(persisterMock.Object))
			{
				var collector = new SpecificPerformanceStatisticsCollector();

				// Simulate monitoring a call to Load, which takes some time.
				// Inside Load, each record has its call to OnLoaded monitored which takes zero time (ignoring fractional milliseconds).
				using (collector.StartMonitoring("Load"))
				{
					TestDateAttribute.Date = TestDateAttribute.Date.AddMilliseconds(1);
					for (var i = 0; i < 3; ++i)
					{
						using (collector.StartMonitoring("OnLoaded"))
						{
							TestDateAttribute.Date = TestDateAttribute.Date.AddMilliseconds(0.01);
						}
					}
				}

				dataRegistryMock.Verify();

				CombineAssertions(() =>
				{
					AssertEquals("calls to Persister", 1, recorded.Count);
					AssertEquals("parent records", 1, recorded[0].Count);
					var parent = recorded[0][0];
					AssertEquals("child count", 1, parent.Children.Count());
					var child = parent.Children.First();
					AssertEquals("number of tokens folded together", 3, child.ActionCount);
					AssertEquals("ElapsedExcludingChildren", TimeSpan.FromMilliseconds(0.03), child.ElapsedExcludingChildren);
				});
			}
		}

		#region Implementation

		public static void SetStatisticsCollectionEnabled(EnabledState enabledState)
		{
			var newValue = (enabledState == EnabledState.Simple || enabledState == EnabledState.Detailed)
				? enabledState.ToString()
				: nameof(EnabledState.Disabled);

			ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = newValue;

			TestPerformanceStatisticsCollector.ResetStatisticMode();
		}

		void AssertActionSingle(StmUsage usage, string name, int actionCount, decimal excluded, decimal included, decimal timeDiff, decimal delta)
		{
			var action = usage.UsageActionsSettings.Single(c => c.Name.Equals(name));
			AssertAction(action, name, actionCount, excluded, included, timeDiff, delta);
		}

		void AssertActionFirst(StmUsage usage, string name, int actionCount, decimal excluded, decimal included, decimal timeDiff, decimal delta)
		{
			var action = usage.UsageActionsSettings.First(c => c.Name.Equals(name));
			AssertAction(action, name, actionCount, excluded, included, timeDiff, delta);
		}

		void AssertAction(Statistics.Xml.IUsage action, string name, int actionCount, decimal excluded, decimal included, decimal timeDiff, decimal delta)
		{
			var dbExcluded = Convert.ToDecimal(action.ElapsedWithoutChildrenSeconds);
			var dbIncluded = Convert.ToDecimal(action.ElapsedWithChildrenSeconds);
			var dbTimeDiff = Convert.ToDecimal(action.EndTimeUTC.Subtract(action.StartTimeUTC).TotalMilliseconds / 1000);
			var actual = "Data not found";
			var expected = FormatAction(name, null, actionCount, excluded, included, timeDiff);

			if (actionCount != action.ActionCount
				|| Math.Abs(excluded - dbExcluded) > delta
				|| Math.Abs(included - dbIncluded) > delta
				|| Math.Abs(timeDiff - dbTimeDiff) > delta
				)
			{
				actual = FormatAction(name, null, action.ActionCount, dbExcluded, dbIncluded, dbTimeDiff);
			}
			else
			{
				actual = expected;
			}

			AssertEquals(String.Format("Delta: {0:0.000}", delta), expected, actual);
		}

		static void CleanupUsages()
		{
			using (PerformanceStatisticsCollector.StartMonitoring("initialize_collector_classes"))
			{
				Thread.Sleep(10);
			}
			PerformanceStatisticsCollector.AttemptFlush(true); // Flush everything that may not be flushed previously
			WaitForFlushToComplete();
			Db.Connection.ExecuteNonQuery("DELETE dbo.StmUsageQueue; DELETE dbo.StmUsage;");
			AssertNotNull(ZDateTime.Now); // Just to pre-cache RefTimeZoneCollection
		}

		void CleanupUsagesAndSetSavesCount()
		{
			var existingStatisticsCollectionMode = ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled;
			PerformanceStatisticsCollector.ResetInstance();
			SetStatisticsCollectionEnabled(EnabledState.Simple);

			CleanupUsages();

			PerformanceStatisticsCollector.ResetInstance();
			ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = existingStatisticsCollectionMode;
			TestPerformanceStatisticsCollector.ResetStatisticMode();

			lastSuccessfullSavesCount = TestPerformanceStatisticsCollector.SuccessfullSaves;
			lastFailedSavesCount = TestPerformanceStatisticsCollector.FailedSaves;
		}

		string FormatAction(string name, string subName, int actionCount, decimal excluded, decimal included, decimal timeDiff)
		{
			return String.Format(@"Name: '{0}'{1}{2}, Excluded: {3:0.000}, Included: {4:0.000} ({5:0.000});"
				/* 0 */, name
				/* 1 */, (String.IsNullOrWhiteSpace(subName)) ? String.Empty : String.Format(" ('{0}')", subName)
				/* 2 */, (actionCount == 1) ? "" : String.Format(" x {0}", actionCount)
				/* 3 */, excluded
				/* 4 */, included
				/* 5 */, timeDiff
				);
		}

		static void WaitForFlushToComplete()
		{
			// Wait for the pending saves to complete, with a maximum wait time
			SpinWait.SpinUntil(() => TestPerformanceStatisticsCollector.PendingSaves == 0, TimeSpan.FromSeconds(10));

			// Ensure that all pending saves have completed
			AssertEquals(0, TestPerformanceStatisticsCollector.PendingSaves);
		}

		void AssertFlushSucceeded()
		{
			WaitForFlushToComplete();
			var newSaveCount = TestPerformanceStatisticsCollector.SuccessfullSaves;
			var newFailedCount = TestPerformanceStatisticsCollector.FailedSaves;
			AssertEquals(lastFailedSavesCount, newFailedCount);
			Assert(newSaveCount > lastSuccessfullSavesCount);
			lastSuccessfullSavesCount = newSaveCount;
			lastFailedSavesCount = newFailedCount;
		}

		IDisposable StartMonitoring(string name, bool expectNoAction)
		{
			var monitorDisposable = expectNoAction ? ObjectFactory.Get<SpecificPerformanceStatisticsCollector>().StartMonitoring(name)
				: PerformanceStatisticsCollector.StartMonitoring(name);
			var isNoAction = (monitorDisposable == DisposableAction.NoAction);
			AssertEquals(expectNoAction, isNoAction);
			return monitorDisposable;
		}

		int lastSuccessfullSavesCount;
		int lastFailedSavesCount;

		#endregion // Implementation
	}
}
