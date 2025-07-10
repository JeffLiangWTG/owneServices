using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	sealed class NewsTransmitterTestCase : TestCaseWithFactory
	{
		[TestDate(2012, 12, 4, 12, 0, 0)]
		public void TestLogQueueOrderedByPostedTimeNotEventTime()
		{
			// Group 2
			StmJobQueue log4 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log4.SJ_PostedTimeUtc = ZDateTime.Now.AddMinutes(-20);
			log4.SJ_EventTimeUtc = ZDateTime.Now.AddMinutes(-22);
			StmJobQueue log3 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log3.SJ_PostedTimeUtc = ZDateTime.Now.AddMinutes(-22);
			log3.SJ_EventTimeUtc = ZDateTime.Now.AddMinutes(-24);

			// Group 1
			StmJobQueue log2 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log2.SJ_PostedTimeUtc = ZDateTime.Now.AddMinutes(-38);
			log2.SJ_EventTimeUtc = ZDateTime.Now.AddMinutes(-40);
			StmJobQueue log1 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_PostedTimeUtc = ZDateTime.Now.AddMinutes(-40);
			log1.SJ_EventTimeUtc = ZDateTime.Now.AddMinutes(-42);

			Factory.Save();
			Factory.ReloadAllSafe<StmJobQueue>();

			NewsTransmitterWithSingleLoop transmitter = new NewsTransmitterWithSingleLoop(new MockSubscriber());

			// Group 1
			transmitter.ProcessLogQueueBatch(1);

			RefreshLogs(log1, log2, log3, log4);

			AssertEquals(JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertEquals(JobQueueStatus.StatusQueued, log2.SJ_Status);

			// Group 2
			transmitter.ProcessLogQueueBatch(2);

			RefreshLogs(log1, log2, log3, log4);

			AssertEquals(JobQueueStatus.StatusProcessed, log2.SJ_Status);
			AssertEquals(JobQueueStatus.StatusProcessed, log3.SJ_Status);
			AssertEquals(JobQueueStatus.StatusQueued, log4.SJ_Status);
		}

		public void TestPollutedFactoryDoesNotBlockSavingOfError()
		{
			var log1 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			var subscriber = new MockSubscriberThatCrashes(new Exception(), log1.PK);
			subscriber.ThingToDo += () => { throw new Exception(); };
			var logger = subscriber.TestLogger;

			var transmitter = new NewsTransmitterWithOrderedQueue(subscriber, logger);
			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);
			ErrorReporter.Clear();

			AssertMultilineASCIIEquals("", @"
[MockEventSubscriber] failed to process logs. Affected records will be processed again one-by-one.
Exception of type 'System.Exception' was thrown.
[MockEventSubscriber] Logs could not be processed due to an error:
Exception of type 'System.Exception' was thrown.
[MockEventSubscriber] The following logs have failed with unhandled errors:
[MockEventSubscriber] Log (Parent = SE " + log1.SJ_ParentID.ToString() + @", Evnt = QUC, Ref = )", logger.ToString());
		}

		public void TestProcessOneByOne_NoReally()
		{
			// Test that when we say, "Process logs one-by-one" we actually mean that, and not some other thing.
			var log1 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			var log2 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			var log3 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			var subscriber = new MockSubscriberThatCrashes(new Exception(), log3.PK);
			var logger = subscriber.TestLogger;
			var transmitter = new NewsTransmitterWithOrderedQueue(subscriber, logger);
			transmitter.QueueOverride = new[] { log3 };
			transmitter.ProcessLogQueueBatch(0);

			subscriber.LogsToCrashOn.Clear();
			transmitter.QueueOverride = new[] { log1, log2, log3 };
			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);

			AssertMultilineASCIIEquals("", @"
[MockEventSubscriber] failed to process logs. Affected records will be processed again one-by-one.
Exception of type 'System.Exception' was thrown.
[MockEventSubscriber] Logs could not be processed due to an error:
Exception of type 'System.Exception' was thrown.
[MockEventSubscriber] The following logs have failed with unhandled errors:
[MockEventSubscriber] Log (Parent = SE guid, Evnt = QUC, Ref = )", logger.ToString().StripGUIDs("guid"));
			ErrorReporter.Clear();
		}

		public void TestAdditionalRetryOnIndexViolation()
		{
			var log2 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);

			Factory.Save();
			var message = "NR_UX_test";
			var subscriber = new MockSubscriberThatCrashes(new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(2601, message), null, null), Factory), log2.PK);
			var logger = subscriber.TestLogger;
			var transmitter = new NewsTransmitterWithOrderedQueue(subscriber, logger);
			transmitter.QueueOverride = new[] { log2 };

			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);
			ErrorReporter.Clear();

			var retryMessage = @"[MockEventSubscriber] A conflict during save will cause logs to be processed one at a time.";

			logger.AllowDebug = true;
			AssertMultilineASCIIEquals("", $@"
{string.Concat(Enumerable.Repeat($"{retryMessage}{System.Environment.NewLine}", 10))}
[MockEventSubscriber] Failed with this concurrency exception:

** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = {message}


[MockEventSubscriber] The following logs have failed with unhandled errors:
[MockEventSubscriber] Log (Parent = SE {{guidguid-guid-guid-guid-guidguidguid}}, Evnt = QUC, Ref = )", logger.ToString().StripGUIDs());
		}

		public void TestAdditionalRetryOnConcurrencyException()
		{
			var log2 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);

			Factory.Save();

			var subscriber = new MockSubscriberThatCrashes(new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), Factory), log2.PK);
			var logger = subscriber.TestLogger;
			var transmitter = new NewsTransmitterWithOrderedQueue(subscriber, logger);
			transmitter.QueueOverride = new[] { log2 };

			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);
			transmitter.ProcessLogQueueBatch(0);
			ErrorReporter.Clear();

			var retryMessage = @"[MockEventSubscriber] A conflict during save will cause logs to be processed one at a time.";

			logger.AllowDebug = true;
			AssertMultilineASCIIEquals("", $@"
{string.Concat(Enumerable.Repeat($"{retryMessage}{System.Environment.NewLine}", 10))}
[MockEventSubscriber] Failed with this concurrency exception:
**CONCURRENCY Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = Exception of type 'System.Exception' was thrown.

[MockEventSubscriber] The following logs have failed with unhandled errors:
[MockEventSubscriber] Log (Parent = SE {{guidguid-guid-guid-guid-guidguidguid}}, Evnt = QUC, Ref = )", logger.ToString().StripGUIDs());
		}

		public void TestHandlesCriticalExceptionsDifferently()
		{
			var log1 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			var subscriber = new MockSubscriberThatCrashes(new OutOfMemoryException("Lol im an exception"), log1.PK);
			var logger = subscriber.TestLogger;
			var transmitter = new NewsTransmitterWithOrderedQueue(subscriber, logger);
			transmitter.QueueOverride = new[] { log1 };
			try
			{ transmitter.ProcessLogQueueBatch(0); }
			catch (OutOfMemoryException) { }
			try
			{ transmitter.ProcessLogQueueBatch(0); }
			catch (OutOfMemoryException) { }
			try
			{ transmitter.ProcessLogQueueBatch(0); }
			catch (OutOfMemoryException) { }
			try
			{ transmitter.ProcessLogQueueBatch(0); }
			catch (OutOfMemoryException) { }

			AssertMultilineASCIIEquals("", @"
[MockEventSubscriber] Exception happened during processing logs queue.
Lol im an exception
[MockEventSubscriber] Exception happened during processing logs queue.
Lol im an exception
[MockEventSubscriber] Exception happened during processing logs queue.
Lol im an exception
[MockEventSubscriber] Loaded logs that have aborted processing for unknown reasons.
[MockEventSubscriber] The following logs have failed with unhandled errors:
[MockEventSubscriber] Log (Parent = SE {guidguid-guid-guid-guid-guidguidguid}, Evnt = QUC, Ref = )", logger.ToString().StripGUIDs());
		}

		[Serializable]
		class MockSubscriberThatCrashes : MockSubscriber
		{
			public MockSubscriberThatCrashes(Exception exType, params ZGuid[] logsToCrashOn)
			{
				Ex = exType;
				LogsToCrashOn = new HashSet<ZGuid>(logsToCrashOn);
			}

			public Action ThingToDo { get; set; }

			public HashSet<ZGuid> LogsToCrashOn { get; }
			Exception Ex { get; }

			protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
			{
				queuedLogs[0].Factory.Saving += (_) => { ThingToDo?.Invoke(); };

				if (queuedLogs.Any(l => LogsToCrashOn.Contains(l.PK)))
				{
					throw Ex;
				}
				else
				{
					base.ProcessLogQueueItems(queuedLogs);
				}
			}
		}

		[Serializable]
		class NewsTransmitterWithOrderedQueue : NewsTransmitterForTesting
		{
			public NewsTransmitterWithOrderedQueue(MockSubscriber subscriber, ILogger logger = null)
				: base(subscriber, new[] { subscriber }, logger)
			{
			}

			public IList<IQueuedLog> QueueOverride { get; set; }

			StmJobQueueBatch GetQueueOverride()
			{
				if (QueueOverride != null)
				{
					var logs = new StmJobQueueBatch(QueueOverride.Select(s => new AppLockedItem<IQueuedLog>(s, null)).ToArray());
					foreach (var log in logs.Values)
					{
						if (log.Item is BusinessObject bizo)
						{
							bizo.Reload();
						}
					}
					return logs;
				}
				else
				{
					return null;
				}
			}

			protected override StmJobQueueBatch ReadQueue(int batchSize)
			{
				return GetQueueOverride() ?? base.ReadQueue(batchSize);
			}
		}

		public void TestProcessReadQueueQueryIsForcedToSeekIndexWhenRegistryItemIsTrue()
		{
			using (SystemDataRegistry.Instance.EnableLogWalkerForceSeekOnReadQueueQuery.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Db.Connection.TrackExecutedCommands())
			{
				MockSubscriber testSubscriber = new MockSubscriber();
				NewsTransmitterForTesting transmitter = new NewsTransmitterForTesting(testSubscriber);
				transmitter.ProcessLogQueueBatch(1);

				var jobQueueQuery = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("StmJobQueue WITH"));
				AssertNotNull(jobQueueQuery);

				AssertContains("WITH (FORCESEEK", jobQueueQuery);
				AssertContains("INDEX(NR_RC__SJ_FilterName_SJ_Status_SJ_ProcessOnOrAfterUtc)", jobQueueQuery);
			}
		}

		public void TestProcessReadQueueQueryIsNotForcedToSeekIndexWhenRegistryItemIsFalse()
		{ 
			using (Db.Connection.TrackExecutedCommands())
			{
				MockSubscriber testSubscriber = new MockSubscriber();
				NewsTransmitterForTesting transmitter = new NewsTransmitterForTesting(testSubscriber);
				transmitter.ProcessLogQueueBatch(1);

				var jobQueueQuery = Db.Connection.ExecutedCommands.FirstOrDefault(c => c.Contains("StmJobQueue WITH"));
				AssertNotNull(jobQueueQuery);

				AssertNotContains("WITH (FORCESEEK", jobQueueQuery);
				AssertNotContains("INDEX(NR_RC__SJ_FilterName_SJ_Status_SJ_ProcessOnOrAfterUtc)", jobQueueQuery);
			}
		}

		public void TestProcessReadQueueQueryIsNotUsingEventTime()
		{
			using (Db.Connection.TrackExecutedCommands())
			{
				MockSubscriber testSubscriber = new MockSubscriber();
				NewsTransmitterForTesting transmitter = new NewsTransmitterForTesting(testSubscriber);
				transmitter.ProcessLogQueueBatch(1);

				var queryMatch = Db.Connection.ExecutedCommands.Select(c => Regex.Match(c, @$"SELECT\s+.*\s+FROM\s+dbo\.StmJobQueue\s+.*WHERE(?<filter>.*)ORDER BY\s+(?<order>.*)\/\* Parameter Stats", RegexOptions.Multiline | RegexOptions.Singleline)).FirstOrDefault(r => r.Success);
				AssertNotNull(queryMatch);
				var filter = queryMatch.Groups["filter"].Value;
				Assert("WHERE clause should not contain SJ_EventTime", !Regex.IsMatch(filter, @$"\b{StmJobQueueSchema.SJ_EventTime.Name}\b"));
				Assert("WHERE clause should contain SJ_ProcessOnOrAfterUtc", Regex.IsMatch(filter, @$"\b{StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name}\b"));
				AssertEquals(StmJobQueueSchema.SJ_ProcessOnOrAfterUtc.Name, queryMatch.Groups["order"].Value.Trim());
			}
		}

		public void TestProcessWhileHasATime()
		{
			StmJobQueue log1 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			StmJobQueue log2 = MockSubscriber.QueueNewLogForTestSubscriber(Factory);
			Factory.Save();

			MockSubscriber testSubscriber = new MockSubscriber();
			NewsTransmitterForTesting transmitter = new NewsTransmitterForTesting(testSubscriber);
			transmitter.ProcessLogQueueBatch(1);

			RefreshLogs(log1, log2);

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log2.SJ_Status);
		}

		public void TestRecurOnNewLogs()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);

			Factory.Save();

			MockSubscriber testSubscriber = new BetterMockSubscriber(processLog: l =>
			{
				if (l.PK == log1.PK)
				{
					BetterMockSubscriber.CreateNewLog(l.Factory, MockSubscriber.TestEventType, l.Factory.Load<StmEvent>(l.SJ_ParentID));
				}
			}, tableNamesOverride: new[] { StmEventSchema.Constants.TableName }, eventTypesOverride: new[] { MockSubscriber.TestEventType });

			var transmitter = new NewsTransmitterForTesting(testSubscriber);
			transmitter.ProcessLogQueueBatch(1);

			log1.Reload();

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertEquals("Notified event count", 1, testSubscriber.TestLogger.NotifiedEventList.Count);
			AssertEquals("1st notified event description", $@"[MockEventSubscriber] attempted to add the following logs which are duplicates:
Table: StmEvent, Event: QUC, Parent PK: {log1.SJ_ParentID}, Reference: ", testSubscriber.TestLogger.NotifiedEventList[0]);
		}

		public void TestRecurOnNewLogs_MultipleSubscribers_Unused()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_FilterName = "SubScriber1";

			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();
			var additionalEventCode = Events.CustomisableEvent68.Code;

			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l =>
				{
					if (l.PK == log1.PK)
					{
						BetterMockSubscriber.CreateNewLog(l.Factory, MockSubscriber.TestEventType, l.Factory.Load<StmEvent>(evt.PK));
					}
				},
					tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "SubScriber1");

			var testSubscriber2 = new BetterMockSubscriber(
					tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
					eventTypesOverride: new[] { additionalEventCode },
					nameOverride: "Subscriber2");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1, testSubscriber2 });
			transmitter.ProcessLogQueueBatch(1);
			log1.Reload();

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertLogs(testSubscriber1, "[SubScriber1] recurring with subscriber [SubScriber1] and depth 1 for 1 logged event(s).");
		}

		public void TestLogSubscriberGroupsLogs()
		{
			var log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_SE_NKEvent = Events.CustomisableEvent00Code;
			log1.SJ_FilterName = "SubScriber1";

			var log2 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log2.SJ_SE_NKEvent = Events.CustomisableEvent00Code;
			log2.SJ_FilterName = "SubScriber1";

			Factory.Save();
			int groupLogsCalled = 0;
			var subscriber = new BetterMockSubscriber(groupLogs: l => l.PK,
				setContext: s => { groupLogsCalled++; return null; },
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { Events.CustomisableEvent00Code },
				nameOverride: "SubScriber1"
			);

			var transmitter = new NewsTransmitterForTesting(subscriber, new[] { subscriber });
			transmitter.ProcessLogQueueBatch(2);

			AssertEquals(2, groupLogsCalled);
		}

		public void TestRecur_LogSubscriberGroupsLogs()
		{
			var log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_SE_NKEvent = Events.CustomisableEvent00Code;
			log1.SJ_FilterName = "SubScriber1";
			var log2 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log2.SJ_SE_NKEvent = Events.CustomisableEvent00Code;
			log2.SJ_FilterName = "SubScriber1";
			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();
			int groupLogs1Called = 0;
			var subscriber = new BetterMockSubscriber(groupLogs: l => l.PK,
				setContext: s => { groupLogs1Called++; return null; },
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, Events.CustomisableEvent01Code, l.Factory.Load<StmEvent>(evt.PK)),
				tableNamesOverride: new[] { DummyBizoSchema.Constants.TableName },
				eventTypesOverride: new[] { Events.CustomisableEvent00Code },
				nameOverride: "SubScriber1"
			);
			var groupLogs2Called = 0;
			var subscriber2 = new BetterMockSubscriber(groupLogs: l => l.PK,
				setContext: s => { groupLogs2Called++; return null; },
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { Events.CustomisableEvent01Code },
				nameOverride: "SubScriber2"
			);

			var transmitter = new NewsTransmitterForTesting(subscriber, new[] { subscriber, subscriber2 });
			transmitter.ProcessLogQueueBatch(1);

			AssertEquals(2, groupLogs1Called);
			AssertEquals(2, groupLogs2Called);
		}

		public void TestRecurOnNewLogs_MultipleSubscribers_Diamond()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var evt = Factory.NewWithValidTestData<StmEvent>();
			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForGivenSubscriber(Factory, MockSubscriber.TestSubscriberName, evt);
			log1.SJ_FilterName = "SubScriber1";

			Factory.Save();
			var additionalEventCode = Events.CustomisableEvent68.Code;
			var additionalEventCode2 = Events.CustomisableEvent70.Code;

			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode, l.Factory.Load<StmEvent>(evt.PK)),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "SubScriber1");

			var testSubscriber2 = new BetterMockSubscriber(
					processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode2, l.Factory.Load<StmEvent>(evt.PK)),
					tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
					eventTypesOverride: new[] { additionalEventCode },
					nameOverride: "Subscriber2");

			var testSubscriber3 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode2, l.Factory.Load<StmEvent>(evt.PK)),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { additionalEventCode },
				nameOverride: "Subscriber3");

			var testSubscriber4 = new BetterMockSubscriber(
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { additionalEventCode2 },
				nameOverride: "Subscriber4");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1, testSubscriber2, testSubscriber3, testSubscriber4 });
			transmitter.ProcessLogQueueBatch(1);
			log1.Reload();

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertLogs(testSubscriber1,
			"[SubScriber1] recurring with subscriber [Subscriber3] and depth 1 for 1 logged event(s).",
			"[SubScriber1] recurring with subscriber [Subscriber4] and depth 2 for 1 logged event(s).",
			"[SubScriber1] recurring with subscriber [Subscriber2] and depth 1 for 1 logged event(s).",
			"[SubScriber1] recurring with subscriber [Subscriber4] and depth 2 for 1 logged event(s).");
		}

		public void TestRecurOnNewLogs_MultipleSubscribers_Explode()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			log1.SJ_FilterName = "SubScriber1";

			Factory.Save();
			var additionalEventCode = Events.CustomisableEvent68.Code;
			var additionalEventCode2 = Events.CustomisableEvent70.Code;

			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode, l.Factory.Load<StmEvent>(stmEvent.PK)),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "SubScriber1");

			var testSubscriber2 = new BetterMockSubscriber(
					processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode2, l.Factory.Load<StmEvent>(stmEvent.PK)),
					tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
					eventTypesOverride: new[] { additionalEventCode },
					nameOverride: "Subscriber2");

			var testSubscriber3 = new BetterMockSubscriber(
				processLog: l => throw new SqlLockLostException(),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { additionalEventCode },
				nameOverride: "Subscriber3");

			var testSubscriber4 = new BetterMockSubscriber(
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { additionalEventCode2 },
				nameOverride: "Subscriber4");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1, testSubscriber2, testSubscriber3, testSubscriber4 });
			transmitter.ProcessLogQueueBatch(1);
			log1.Reload();

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertLogs(testSubscriber1, "[SubScriber1] recurring with subscriber [Subscriber3] and depth 1 for 1 logged event(s).",
			"[SubScriber1] Sql lock lost.");

			// And then we test that the error recovery is ok
			var transmitter2 = new NewsTransmitterForTesting(testSubscriber2, new[] { testSubscriber1, testSubscriber2, testSubscriber3, testSubscriber4 });
			transmitter2.ProcessLogQueueBatch(1);

			AssertLogs(testSubscriber2, "[Subscriber2] recurring with subscriber [Subscriber4] and depth 1 for 1 logged event(s).");

			var transmitter3 = new NewsTransmitterForTesting(testSubscriber3, new[] { testSubscriber1, testSubscriber2, testSubscriber3, testSubscriber4 });
			transmitter3.ProcessLogQueueBatch(1);

			AssertLogs(testSubscriber3, "[Subscriber3] Sql lock lost.");
		}

		void AssertLogs(BetterMockSubscriber subscriber, params string[] logs)
		{
			CombineAssertions(() =>
			{
				for (int i = 0; i < logs.Length; i++)
				{
					AssertEquals($"log {i}", logs[i], subscriber.TestLogger.NotifiedEventList[i]);
				}
			});
		}

		public void TestRecurOnNewLogs_MultipleSubscribers()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_FilterName = "SubScriber1";
			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();
			var additionalEventCode = Events.CustomisableEvent68.Code;

			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, additionalEventCode, l.Factory.Load<StmEvent>(evt.PK)),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "SubScriber1");

			var testSubscriber2 = new BetterMockSubscriber(
					tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
					eventTypesOverride: new[] { additionalEventCode },
					nameOverride: "Subscriber2");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1, testSubscriber2 });
			transmitter.ProcessLogQueueBatch(1);
			log1.Reload();

			AssertEquals("Log status", JobQueueStatus.StatusProcessed, log1.SJ_Status);
			AssertEquals("[SubScriber1] recurring with subscriber [Subscriber2] and depth 1 for 1 logged event(s).", testSubscriber1.TestLogger.NotifiedEventList[0]);
		}

		class Guidizer
		{
			public StmEvent Next(BusinessObjectFactory factory)
			{
				var result = factory.NewWithValidTestData<StmEvent>();
				result.SE_Code = (++count).ToString("D3", CultureInfo.InvariantCulture);
				Last = result.PK;
				return result;
			}
			int count;

			public ZGuid Last { get; private set; }
		}

		public void TestRecurOnNewLogs_InifiniteRecursion()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.LogWalkerMaxEventRecursion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);
			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			var guidizer = new Guidizer();
			log1.SJ_FilterName = "TheDreamRuiner";

			Factory.Save();
			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, MockSubscriber.TestEventType, guidizer.Next(l.Factory)),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "TheDreamRuiner");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1 });
			transmitter.ProcessLogQueueBatch(1);

			AssertLogs(testSubscriber1.TestLogger, "[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 1 for 1 logged event(s).",
				"[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 2 for 1 logged event(s).",
				"[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 3 for 1 logged event(s).",
				"[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 4 for 1 logged event(s).",
				"[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 5 for 1 logged event(s).",
				"[TheDreamRuiner] attempted to add the following logs which could cause infinite feedback:",
				$"Table: StmEvent, Event: QUC, Parent PK: {guidizer.Last}");
		}

		public void TestRecurOnNewLogs_DuplicateRecursion()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.LogWalkerMaxEventRecursion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 5);

			StmJobQueue log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_FilterName = "TheDreamRuiner";

			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();
			var testSubscriber1 = new BetterMockSubscriber(
				processLog: l => BetterMockSubscriber.CreateNewLog(l.Factory, MockSubscriber.TestEventType, evt),
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "TheDreamRuiner");

			var transmitter = new NewsTransmitterForTesting(testSubscriber1, new[] { testSubscriber1 });
			transmitter.ProcessLogQueueBatch(1);

			AssertLogs(testSubscriber1.TestLogger,
				$@"[TheDreamRuiner] recurring with subscriber [TheDreamRuiner] and depth 1 for 1 logged event(s).
[TheDreamRuiner] attempted to add the following logs which are duplicates:
Table: StmEvent, Event: QUC, Parent PK: {evt.PK}, Reference: "
				);
		}

		void AssertLogs(LoggerForTesting testLogger, params string[] expectedLogs)
		{
			AssertMultilineASCIIEquals("Logs didn't match up", string.Join(System.Environment.NewLine, expectedLogs), string.Join(System.Environment.NewLine, testLogger.NotifiedEventList));
		}

		public void TestEnsureDisposableContextPersistedDuringSave()
		{
			var log1 = BetterMockSubscriber.QueueNewLogForTestSubscriber(Factory);
			log1.SJ_FilterName = "TerribleMistakesHaveBeenMade";

			Factory.Save();

			int contextFlag = 0;
			IDisposable setupContext(IEnumerable<IQueuedLog> x)
			{
				contextFlag++;
				return new DisposableAction(() => contextFlag--);
			}

			void explodeIfContextIsDisposed(BusinessObjectFactory x)
			{
				if (contextFlag != 1)
				{
					throw new InvalidOperationException("Context was disposed of before save!");
				}
			}

			var subscriber = new BetterMockSubscriber(
				processLog: l =>
				{
					explodeIfContextIsDisposed(l.Factory);
					l.Factory.Saving += explodeIfContextIsDisposed;
				},
				setContext: setupContext,
				tableNamesOverride: new[] { StmEventSchema.Constants.TableName },
				eventTypesOverride: new[] { MockSubscriber.TestEventType },
				nameOverride: "TerribleMistakesHaveBeenMade");

			var transmitter = new NewsTransmitterForTesting(subscriber, new[] { subscriber });
			transmitter.ProcessLogQueueBatch(1);

			AssertLogs(subscriber.TestLogger, String.Empty
				);
		}

		#region Implementation

		void RefreshLogs(params StmJobQueue[] logs)
		{
			foreach (StmJobQueue log in logs)
			{
				log.Reload();
			}
		}

		#endregion
	}
}
