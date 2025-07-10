using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.LogWalker.LogSubscriber;

namespace Enterprise.LogWalker.Test
{
	public class NewsTransmitterTest : LogWalkerTestCase
	{
		#region Setup

		public ProcessTask AddTrigger(IWorkflowProvider provider, Event eventType)
		{
			var trigger = provider.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = eventType.Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_EmailAddr = "turkey@dresses.milnor";

			return trigger;
		}

		DummyWithWorkflow GetDummy(BusinessObjectFactory factory, DummyWithWorkflow dummyWithWorkflow) => factory.Load<DummyWithWorkflow>(dummyWithWorkflow.PK);

		public MockNewsTransmitter GetNewsTransmitter(LogSubscriber subscriber, params LogSubscriber[] otherSubscribers) => new MockNewsTransmitter(subscriber, otherSubscribers.Append(subscriber).ToArray(), new SubscriberParameters() { Logger = Logger });

		#endregion

		#region Recursion Tests

		public void TestAllowInfiniteLoops_WhenDisabled()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			bool ninetyNineAdded = false;
			var subscriber = new MockSubscriber(logs =>
			{
				GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent00);
				if (!ninetyNineAdded)
				{
					GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent99);
					ninetyNineAdded = true;
				}
			});
			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			var result = string.Join(System.Environment.NewLine, Logger.LogEntries);

			AssertEquals(1, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent99Code)).Length);
		}

		public void TestPreventInfiniteLoops()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			bool ninetyNineAdded = false;
			var subscriber = new MockSubscriber(logs =>
			{
				GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent00);
				if (!ninetyNineAdded)
				{
					GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent99);
					ninetyNineAdded = true;
				}
			});
			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			var result = string.Join(System.Environment.NewLine, Logger.LogEntries);
			AssertContains("attempted to add the following logs which are duplicates:", result);
			AssertContains("The duplicate log ought to be noted", "Event: Z00", result);
			AssertNotContains("The non-duplicate log should be hidden!", "Event: Z99", result);
			AssertNotNull(dummy.Logs.Find(l => l.SL_SE_NKEvent == Events.CustomisableEvent99Code));
		}

		public void TestNewsTransmitterAllowRecursion()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent01);
			var subscriber = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01));
			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("recurring with subscriber [MockSubscriber] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertContains("attempted to add the following logs which are duplicates:", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestNewsTransmitterAllowRecursion_DoNotCreateLogsForSubscribersWithoutMatchingTable()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent01);
			var subscriber1 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01));
			var nonMatchingSubscriber = new MockSubscriber(logs => { },
				name: "BadSubscriber",
				tableNames: new[] { "IAmNotARealTable" });
			var transmitter = GetNewsTransmitter(subscriber1, nonMatchingSubscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertNotContains("recurring with subscriber [BadSubscriber] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestNewsTransmitterAllowRecursion_SiblingsAreNotDuplicates()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent01);
			var deletedPk = ZGuid.Empty;
			var subscriber = new MockSubscriber(logs =>
			{
				deletedPk = GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01).SL_Parent;
				GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent02);
				GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent02);
			}, eventTypes: new[] { Events.CustomisableEvent00Code, Events.CustomisableEvent01Code });
			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code), new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertMultilineASCIIEquals("The sibling logs (i.e. the Z02 logs) are not deleted because they have nothing to do with any fired events.",
				$@"[MockSubscriber] processing 2 logged event(s).
[MockSubscriber] recurring with subscriber [MockSubscriber] and depth 1 for 1 logged event(s).
[MockSubscriber] processing 1 logged event(s).
[MockSubscriber] attempted to add the following logs which are duplicates:
Table: DummyBizo, Event: Z01, Parent PK: {deletedPk.ToString()}, Reference: 
[MockSubscriber] finished processing logs.
", string.Join(System.Environment.NewLine, Logger.LogEntries.Select(le => le.Message)));
		}

		public void TestNewsTransmitterAllowRecursion_MultiSubscriber()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber1 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01), name: "MingMing");
			var subscriber2 = new MockSubscriber(logs => GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01), name: "Boky");
			var transmitter = GetNewsTransmitter(subscriber1, subscriber2);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("recurring with subscriber [MingMing] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertContains("recurring with subscriber [Boky] and depth 1 for 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestLogsCanRetryEvenWithRecursion()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber = new MockSubscriber(logs =>
			{
				if (logs.Any(l => l.SJ_SE_NKEvent == Events.CustomisableEvent01Code))
				{
					throw new InvalidOperationException("Disallow recursion.");
				}
				else
				{
					GetDummy(logs[0].Factory, dummy).Logs.AddNew(Events.CustomisableEvent01);
				}
			});

			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogs(new[] { new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code) });
			AssertContains("[MockSubscriber] failed to process logs. Affected records will be processed again one-by-one.", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertContains(@"[MockSubscriber] processing 1 logged event(s).
[MockSubscriber] recurring with subscriber [MockSubscriber] and depth 1 for 1 logged event(s).
[MockSubscriber] processing 1 logged event(s).
[MockSubscriber] failed to process logs. Affected records will be processed again one-by-one.
Disallow recursion.
[MockSubscriber] Processing log (PK = 'I AM DA GUID') individually.
[MockSubscriber] recurring with subscriber [MockSubscriber] and depth 1 for 1 logged event(s).
[MockSubscriber] processing 1 logged event(s).
[MockSubscriber] Logs could not be processed due to an error:
Disallow recursion.
[MockSubscriber] The following logs have failed with unhandled errors:
[MockSubscriber] Log (Parent = Z0 I AM DA GUID, Evnt = Z01, Ref = )
[MockSubscriber] finished processing logs.", string.Join(System.Environment.NewLine, Logger.LogEntries.Select(le => le.Message)).StripGUIDs("I AM DA GUID"));
			ErrorReporter.Clear();
		}

		public void TestConcurrencyErrorOnSaveCausesAdditionalRetry()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var count = 0;
			var subscriber = new MockSubscriber(logs =>
			{
				count++;
				var factory = logs[0].Factory;
				factory.Saving += (e) =>
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Herpa derp"), ((INeedRow)dummy).Row, Db.Connection), logs[0].Factory);
				};
			});
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });

			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogsFromDb(10);
			AssertContains("Failed with this concurrency exception:", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertEquals("We get even more retries, because retries are cool.", retryAttempts + 1, count);
			ErrorReporter.Clear();
		}

		public void TestLogsCreatedViaSqlAreIdenticalToLogsCreatedViaCSharp()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var newsPublisher = new MockNewsPublisher();
			MockSubscriber subscriber1 = null;
			subscriber1 = new MockSubscriber(logs =>
			{
				AssertEquals(1, logs.Length);
				var faked = MockNewsTransmitter.CreateLogQueueItemForTest(log.Factory, log, subscriber1);
				var queued = logs.Single();
				AssertQueuedLogsMatch(queued, faked);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertLoggerHasNoErrors();
			AssertContains("processing 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
			ErrorReporter.Clear();
		}

		public void TestLogs_MultipleTransmitters()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(name: "AllCustomEvents");
			var subscriber2 = new MockSubscriber(name: "TheCustomEvents");

			Factory.Save();

			var transmitter1 = GetNewsTransmitter(subscriber1);
			var transmitter2 = GetNewsTransmitter(subscriber2);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1, subscriber2 }));
			transmitter1.ProcessLogsFromDb(1);
			transmitter2.ProcessLogsFromDb(1);
			AssertContains("processing 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));
		}

		public void TestLogs_DeletedRecurringThings()
		{
			SystemDataRegistry.Instance.LogWalkerRecursOnNewEvents.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(name: "AllCustomEvents", logsDelegate: (l) =>
			{
				dummy = l[0].Factory.Load<DummyWithWorkflow_DeleteEverything>(dummy.PK);
				var l1 = dummy.Logs.AddNew(Events.CustomisableEvent00);
				var l2 = dummy.Logs.AddNew(Events.CustomisableEvent00);
			});

			Factory.Save();

			var transmitter1 = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter1.ProcessLogsFromDb(1);
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestFailureHasErrorReport()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var subscriber = new MockSubscriber(logs => throw new InvalidOperationException("Bang."));
			var transmitter = GetNewsTransmitter(subscriber);
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });
			transmitter.ProcessLogsFromDb(10);
			transmitter.ProcessLogsFromDb(10);
			transmitter.ProcessLogsFromDb(10);
			AssertContains("Subscriber Name: MockSubscriber,\r\nParent:", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestFailureHasConcurrencyErrorReport()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var subscriber = new MockSubscriber(logs =>
			{
				throw new Exception("DN");
				throw new ZDataConcurrencyException(new Exception(), null, null);
			}, null);

			var transmitter = GetNewsTransmitter(subscriber);
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });
			transmitter.ProcessLogsFromDb(10);
			transmitter.ProcessLogsFromDb(10);
			transmitter.ProcessLogsFromDb(10);
			AssertContains("Subscriber Name: MockSubscriber,\r\nParent: ", ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestShouldRetry_SqlExceptionDDLPriority()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var sqlException = SqlExceptionBuilder.CreateSqlException(1219, "Your session has been disconnected because of a high priority DDL operation.");
			var count = 0;
			var subscriberFail = new MockSubscriber(logs =>
			{
				count++;
				var factory = logs[0].Factory;
				factory.Saving += (e) =>
				{
					throw new ZSaveException(new ZDataException(sqlException, null, null), Factory);
				};
			});
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriberFail });
			var transmitter = GetNewsTransmitter(subscriberFail);
			transmitter.ProcessLogsFromDb(10);
			ErrorReporter.Clear();

			var retryMessage = @"[MockSubscriber] A conflict during save will cause logs to be processed one at a time.
[MockSubscriber] Processing log (PK = '{guidguid-guid-guid-guid-guidguidguid}') individually.
[MockSubscriber] processing 1 logged event(s).";

			AssertEquals("", $@"[MockSubscriber] processing 1 logged event(s).
{string.Concat(Enumerable.Repeat($"{retryMessage}{System.Environment.NewLine}", retryAttempts))}[MockSubscriber] Failed with this concurrency exception:

** Error Saving Record **

ServerName: {Db.ServerName}
DatabaseName: {Db.DatabaseName}
Unknown record

Inner Message = Your session has been disconnected because of a high priority DDL operation.


[MockSubscriber] The following logs have failed with unhandled errors:
[MockSubscriber] Log (Parent = Z0 {{guidguid-guid-guid-guid-guidguidguid}}, Evnt = Z00, Ref = )
[MockSubscriber] finished processing logs.", string.Join("\r\n", Logger.LogEntries.Select(le => le.Message)).StripGUIDs());

			AssertEquals(retryAttempts + 1, count);
		}

		public void TestDbTriggerIsSuspended()
		{
			AssertEquals(0, Db.Connection.ExecuteScalar<int>("select (case when (SELECT Result FROM dbo.IsTriggerSuspended('TG_CP_INS_StmALogQueue')) = 0 then 0 else 1 end) d"));

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var newsPublisher = new MockNewsPublisher();
			MockSubscriber subscriber1 = null;
			subscriber1 = new MockSubscriber(logs =>
			{
				logs[0].Factory.Load<DummyWithWorkflow>(logs[0].SJ_ParentID).Logs.AddNew(Events.CustomisableEvent01);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertContains("processing 1 logged event(s).", string.Join(System.Environment.NewLine, Logger.LogEntries));

			AssertEquals(0, Db.Connection.ExecuteScalar<int>("select (case when (SELECT Result FROM dbo.IsTriggerSuspended('TG_CP_INS_StmALogQueue')) = 0 then 0 else 1 end) d"));
		}

		#endregion

		#region Exception Testing

		public void TestTryHanddleExceptionWithSaveAsSuccessfulInNewFactoryResult()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			Exception expectedException = null;
			Exception tryHandleExceptionException = null;
			Exception finalizeExceptionHadlingException = null;
			var subscriber = new MockSubscriber(
				logs =>
				{
					logs[0].Factory.Load<DummyWithWorkflow>(dummy.PK).Logs.AddNew(Events.CustomisableEvent99);

					counter++;
					throw (expectedException = new InvalidOperationException("My exception"));
				},
				name: "AllCustomEvents",
				tryHandleException: TryHandleException,
				finalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory: FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory
			);

			var transmitter = GetNewsTransmitter(subscriber);
			AssertEquals("Precondition: Number of queued logs created", 1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber }));

			transmitter.ProcessLogsFromDb(1);

			var newTestFactory = new BusinessObjectFactory();
			CombineAssertions(() =>
			{
				AssertEquals("Records saved in main factory", 0, newTestFactory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent99Code)).Length);
				AssertEquals("Records saved in new factory", 1, newTestFactory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent88Code)).Length);
				AssertEquals("LastMessageReported", string.Empty, ErrorReporter.LastMessageReported);
				var queueLogs = newTestFactory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, dummy.PK));
				AssertEquals("Number of queued logs loaded", 1, queueLogs.Length);
				AssertEquals("Queued log status", JobQueueStatus.StatusProcessed, queueLogs[0].SJ_Status);
				AssertEquals("Number of processing iterations", 1, counter);
				AssertNotNull("Postcondition: exception is thrown in subscriber", expectedException);
				AssertEquals("Exception in TryHandleException: it handles the same exception instance as was thrown.", expectedException, tryHandleExceptionException);
				AssertEquals("Exception in FinalizeExceptionHadling: it gets the same exception instance as was thrown.", expectedException, finalizeExceptionHadlingException);
			});

			ExceptionHandlingResult TryHandleException(Exception ex, IEnumerable<IQueuedLog> logs, int retryCount)
			{
				tryHandleExceptionException = ex;
				return LogSubscriber.ExceptionHandlingResult.SaveAsSuccessfulInNewFactory;
			}

			void FinalizeExceptionHadlingAsSaveAsSuccessfulInNewFactory(BusinessObjectFactory newFactory, Exception ex)
			{
				finalizeExceptionHadlingException = ex;
				newFactory.Load<DummyWithWorkflow>(dummy.PK).Logs.AddNew(Events.CustomisableEvent88);
			}
		}

		public void TestNullLogInSplitGroupErrorReports()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			factory.Save();

			var newsPublisher = new MockNewsPublisher();
			var subscriber = new MockSubscriber(logs =>
			{
				Db.Connection.ExecuteNonQuery($"DELETE FROM {StmJobQueueSchema.Constants.SqlSchemaName}.{StmJobQueueSchema.Constants.TableName} WHERE {StmJobQueueSchema.Constants.SJ_ALogReference} = '{log.PK}'");
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Some exception."), ((INeedRow)dummy).Row, Db.Connection), Factory);
			}, name: "AllCustomEvents");

			var transmitter = GetNewsTransmitter(subscriber);
			newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });
			transmitter.ProcessLogsFromDb(1);

			CombineAssertions(() =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertStartsWith("LastMessageReported", "Reloaded Log is null.", ErrorReporter.LastMessageReported);
				AssertType(typeof(AggregateException), ErrorReporter.LastExceptionReported);
			});

			ErrorReporter.Clear();
		}

		public void TestRetryAfterUnexpectedConcurrencyError()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter < 2)
				{
					throw new InvalidOperationException("First exception");
				}
				if (counter < 3)
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Rando Concurrency Exception"), ((INeedRow)dummy).Row, Db.Connection), Factory);
				}

				logs[0].Factory.Load<DummyWithWorkflow>(logs[0].SJ_ParentID).Logs.AddNew(Events.CustomisableEvent99);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("Since we had a concurrency error on the last try, we keep trying.", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(1, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent99Code)).Length);
		}

		public void TestEveryExceptionIsDifferent()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new InvalidOperationException("First exception");
				}
				if (counter == 2)
				{
					throw new ArgumentException("Second exception");
				}

				logs[0].Factory.Load<DummyWithWorkflow>(logs[0].SJ_ParentID).Logs.AddNew(Events.CustomisableEvent99);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals(@"This log failed to processes for more than one reason!
First exception
Second exception", ErrorReporter.LastExceptionReported.Message);
			AssertType<ArgumentException>(ErrorReporter.LastExceptionReported.InnerException);
			AssertEquals(2, counter);
			AssertEquals("We failed", 0, Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomisableEvent99Code)).Length);
			ErrorReporter.Clear();
		}

		public void TestAllConcurrencyExceptions()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Rando Concurrency Exception"), ((INeedRow)dummy).Row, Db.Connection), Factory);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("We just keep trying a whole bunch of times and then we give up.", retryAttempts + 1, counter);
			AssertEquals("No concurrency error has been reported.", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAllConcurrencyExceptionsWhenConcurrencyErrorReportEnable()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Rando Concurrency Exception"), ((INeedRow)dummy).Row, Db.Connection), Factory);
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("We just keep trying a whole bunch of times and then we give up.", retryAttempts + 1, counter);
			AssertEquals("ErrorReporter.ReportOnce tries to handle concurrency errors, instead we use ExceptionReporter which is a layer below and does not handle", 1, ExceptionReporterTestListener.Instance.Count);
			var reportedException = ExceptionReporterTestListener.Instance[0] as AggregateException;
			AssertEquals(retryAttempts + 1, reportedException.InnerExceptions.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFailOnTransientAndNonTransientExceptions()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new Exception();
				}
				else
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Rando Concurrency Exception"), ((INeedRow)dummy).Row, Db.Connection), Factory);
				}
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("We just keep trying a whole bunch of times and then we give up.", retryAttempts + 1, counter);
			AssertEquals("No concurrency error has been reported.", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFailOnTransientAndNonTransientExceptionsWhenConcurrencyErrorReportEnable()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new Exception();
				}
				else
				{
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception("Rando Concurrency Exception"), ((INeedRow)dummy).Row, Db.Connection), Factory);
				}
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("We just keep trying a whole bunch of times and then we give up.", retryAttempts + 1, counter);
			AssertEquals("ErrorReporter.ReportOnce tries to handle concurrency errors, instead we use ExceptionReporter which is a layer below and does not handle", 1, ExceptionReporterTestListener.Instance.Count);
			var reportedException = ExceptionReporterTestListener.Instance[0] as AggregateException;
			AssertEquals(retryAttempts + 1, reportedException.InnerExceptions.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFailOnZConcurrencyCheckFailureExceptionOnQoutedBooking()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new Exception();
				}
				else
				{
					throw new ZConcurrencyCheckFailureException("Another user has converted the booking into a shipment.", "Test", true);
				}
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals("We just keep trying a whole bunch of times and then we give up.", retryAttempts + 1, counter);
			AssertContains(@"Failed with this concurrency exception:
Another user has converted the booking into a shipment.", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertEquals("ZConcurrencyCheckFailureException and don't report Exception", 0, ExceptionReporterTestListener.Instance.Count);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestFailOnZCannotSaveException_ExceptionTypeIsBusinessFailure()
		{
			var retryAttempts = SystemDataRegistry.Instance.RetryAttemptsOnLogWalkerRecoverableErrors.Value;

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new Exception();
				}
				else
				{
					throw new ZCannotSaveException("Error when business failure occurred.", "Test", true, ExceptionType.BusinessFailure);
				}
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);

			AssertEquals("Retry Attempts", retryAttempts + 1, counter);
			AssertContains(@"Logs could not be processed due to an error:
Error when business failure occurred.", string.Join(System.Environment.NewLine, Logger.LogEntries));
			AssertEquals("No report of ZCannotSaveException", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestFailOnZCannotSaveException_ExceptionTypeIsUnhandled()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);

			var counter = 0;
			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new MockSubscriber(logs =>
			{
				counter++;
				if (counter == 1)
				{
					throw new InvalidOperationException("First exception");
				}
				if (counter == 2)
				{
					throw new ZCannotSaveException("Second exception", "Test");
				}
			}, name: "AllCustomEvents");

			Factory.Save();

			var transmitter = GetNewsTransmitter(subscriber1);
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			transmitter.ProcessLogsFromDb(1);
			AssertEquals(@"This log failed to processes for more than one reason!
First exception
Second exception", ErrorReporter.LastExceptionReported.Message);
			AssertType<ZCannotSaveException>(ErrorReporter.LastExceptionReported.InnerException);
			AssertEquals(2, counter);
			AssertContains("ZCannotSaveExceptions are considered unhandled by default in NewsTransmitter, if your exception is generating an Error Report but is a legitimate business failure that just needs to be Logged as an Error, then use the ExceptionType.BusinessFailure for the specific ZCannotSaveException that need it.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestInvalidOperationErrorAfterConcurrencyErrorLogsTheLatter()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);

			var first = true;
			var subscriber = new MockSubscriber(logs =>
			{
				if (first)
				{
					first = false;
					throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), Factory);
				}
				else
				{
					throw new InvalidOperationException();
				}
			});

			var transmitter = GetNewsTransmitter(subscriber);
			var queuedlog = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);

			transmitter.ProcessLogs(new[] { queuedlog });
			AssertEquals("Error reported once concurrency is thrown after retries", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = ExceptionReporterTestListener.Instance[0] as DeveloperNotificationException;
			AssertNotNull(exception);
			AssertContains(
$@"Subscriber Name: MockSubscriber,
Parent: {dummy.PK} Table: {dummy.TablePrefix}", exception.Message);
			AssertNotNull(exception.InnerException);
			AssertType<InvalidOperationException>(exception.InnerException.InnerException);

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		#region Partial batch

		public void TestPartialBatch()
		{
			var d1 = Factory.New<IDummyWithWorkflow>();
			((BusinessObject)d1).GetLogs().AddNew(Events.CustomisableEvent00, "shouldprocess");
			((BusinessObject)d1).GetLogs().AddNew(Events.CustomisableEvent00, "shouldprocess");
			((BusinessObject)d1).GetLogs().AddNew(Events.CustomisableEvent00, "noprocess");
			((BusinessObject)d1).GetLogs().AddNew(Events.CustomisableEvent00, "noprocess");

			Factory.Save();

			var sleepySubscriber = new PartialProcessingMockSubscriber(l =>
			{
				var split = l.Split(m => m.SJ_Reference.Contains("shouldprocess", StringComparison.OrdinalIgnoreCase));
				return new LogSubscriberResult(split.MatchingSet.ToArray(), split.NonMatchingSet.ToArray());
			},
			new[] { Events.CustomisableEvent00Code },
			new[] { DummyBizoSchema.Constants.TableName },
			"SleepySubscriber");
			ObjectFactory.Substitute("SystemLogSubscribers", new ILogSubscriber[] { sleepySubscriber });
			LogWalkerRunner.Master().Process(Logger, CancellationToken.None);
			LogWalkerRunner.Default().Process(Logger, CancellationToken.None);

			var logs = Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_ParentID, ((BusinessObject)d1).PK));

			foreach (var log in logs)
			{
				if (log.SJ_Reference.Contains("shouldprocess"))
				{
					AssertEquals(JobQueueStatus.StatusProcessed, log.SJ_Status);
				}
				else
				{
					AssertEquals(JobQueueStatus.StatusFailed, log.SJ_Status);
				}
			}
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		#endregion

		#region Cancel Batch Test

		public void TestCancelBatch()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber = new MockSubscriber(logs => throw new InvalidOperationException("This should never happen!"), batcher: new AlwaysSkipBatcher());
			var transmitter = GetNewsTransmitter(subscriber);
			var log = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);
			transmitter.ProcessLogs(new[] { log });
			AssertEquals("The exception never got thrown, but the log still got processed.", JobQueueStatus.StatusProcessed, log.SJ_Status);
		}

		public void TestCancelBatch_LocksAreFree()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);
			var subscriber = new MockSubscriber(logs => throw new InvalidOperationException("This should never happen!"), batcher: new AlwaysSkipBatcher());
			var transmitter = GetNewsTransmitter(subscriber);
			var log = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);
			transmitter.ProcessLogs(new[] { log });
			AssertNoExceptionThrown(() => Db.Connection.EnsureNoUndisposedLocks());
		}

		public void TestLogsGroupContextDispose()
		{
			var isDisposed = false;
			var action1 = new DisposableAction(() => throw new Exception("Exception Key"));
			var action2 = new DisposableAction(() => isDisposed = true);

			var logGroupContext = new LogsGroupContext(false, action1, action2);

			AssertExceptionThrown(typeof(AggregateException), () => logGroupContext.Dispose());
			AssertEquals("action2 is disposed even though action1 threw", true, isDisposed);
			AssertEquals("LogsGroupContext.DisposeError", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#endregion

		#region ProductivityMode

		public void TestProductivityWiseMode()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			var lwm = LogWalkerRunner.Master();
			var lwk = LogWalkerRunner.Default();
			var lwp = LogWalkerRunner.Purge();
			var logger = new LoggerForTesting();
			lwm.Process(logger, CancellationToken.None);
			lwk.Process(logger, CancellationToken.None);
			lwp.Process(logger, CancellationToken.None);
			var log = logger.ToString();
			AssertNotContains("Error", log);
			AssertNotContains("Exception", log);
		}

		#endregion

		#region Assertions

		void AssertQueuedLogsMatch(IQueuedLog queued, IQueuedLog faked)
		{
			CombineAssertions(() =>
			{
				AssertEquals("SJ_SE_NKEvent", faked.SJ_SE_NKEvent, queued.SJ_SE_NKEvent);
				AssertEquals("SJ_PostedTimeUtc", faked.SJ_PostedTimeUtc, queued.SJ_PostedTimeUtc);
				AssertEquals("SJ_EventTime", faked.SJ_EventTime, queued.SJ_EventTime);
				AssertEquals("SJ_EventTimeUtc", faked.SJ_EventTimeUtc, queued.SJ_EventTimeUtc);
				AssertEquals("SJ_GS_NKUser", faked.SJ_GS_NKUser, queued.SJ_GS_NKUser);
				AssertEquals("SJ_IsEstimate", faked.SJ_IsEstimate, queued.SJ_IsEstimate);
				AssertEquals("SJ_Reference", faked.SJ_Reference, queued.SJ_Reference);
				AssertEquals("SJ_ParentTableCode", faked.SJ_ParentTableCode, queued.SJ_ParentTableCode);
				AssertEquals("SJ_ParentID", faked.SJ_ParentID, queued.SJ_ParentID);
				AssertEquals("SJ_ALogReference", faked.SJ_ALogReference, queued.SJ_ALogReference);
				AssertEquals("SJ_Status", faked.SJ_Status, queued.SJ_Status);
				AssertEquals("SJ_IsDelayFired", faked.SJ_IsDelayFired, queued.SJ_IsDelayFired);
			});
		}

		#endregion

		#region Concurrent Save Tracking

		public void TestLogSubscriberWithExtraSaveCausingConcurrencyError()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			AddTrigger(dummy, Events.CustomisableEvent00);

			var subscriber = new MockSubscriber(logs =>
			{
				var factory = new BusinessObjectFactory() { NameForDebugging = "Extra Factory" };
				factory.Save();
				throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), null, null), Factory);
			});

			var transmitter = GetNewsTransmitter(subscriber);
			var queuedlog = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);

			transmitter.ProcessLogs(new[] { queuedlog });
			AssertEquals("Error reported once concurrency is thrown after retries", 1, ExceptionReporterTestListener.Instance.Count);
			AssertContains(
$@"Subscriber Name: MockSubscriber,
Parent: {dummy.PK} Table: {dummy.TablePrefix}

Main Factory Name: Extra Factory
Main Factory Allocation Path:
Suppressed for performance

Stack Trace:
   at",
			ExceptionReporterTestListener.Instance.GetExceptionMessage(0));
			var exception = ExceptionReporterTestListener.Instance[0] as ZSaveConcurrencyException;
			AssertNotNull(exception);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestLogSubscriberConcurrencyErrorLogNonWTELog()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			var subscriber = new MockSubscriber(logs =>
			{
				var ex = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)dummy).Row, Db.Connection), Factory);
				throw ex;
			});

			var transmitter = GetNewsTransmitter(subscriber);
			var queuedlog = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);

			transmitter.ProcessLogs(new[] { queuedlog });
			AssertEquals("No concurrency error has been reported.", 0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestLogSubscriberConcurrencyErrorLogNonWTELogWhenConcurrencyErrorReportEnable()
		{
			LogUtility.EnableLog(SystemDataRegistry.Instance.LogWalkerLogging, SystemDataRegistry.LogWalkerLoggingKeys.ConcurrencyErrorReport);
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			Factory.Save();

			var subscriber = new MockSubscriber(logs =>
			{
				var ex = new ZSaveConcurrencyException(new ZDataConcurrencyException(new Exception(), ((INeedRow)dummy).Row, Db.Connection), Factory);
				throw ex;
			});

			var transmitter = GetNewsTransmitter(subscriber);
			var queuedlog = new QueuedLogForTesting(dummy, Events.CustomisableEvent00Code);

			transmitter.ProcessLogs(new[] { queuedlog });
			AssertContains(
$@"Subscriber Name: MockSubscriber,
Parent: {dummy.PK} Table: {dummy.TablePrefix}", ExceptionReporterTestListener.Instance.GetExceptionMessage(0));
			var exception = ExceptionReporterTestListener.Instance[0] as AggregateException;
			AssertEquals(11, exception.InnerExceptions.Where(e => e is ZSaveConcurrencyException).Count());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
		}

		#endregion

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestReadQueueFiltersOn_SJ_ProcessOnOrAfter()
		{
			var subscriber = new MockSubscriber(name: "subscriber");
			var transmitter = new MockNewsTransmitter(subscriber, new LogSubscriber[] { subscriber }, new SubscriberParameters { Logger = Logger });

			Func<ZDateTime, ZDateTime?, bool, bool, StmJobQueue> createStmJobQueue = (ZDateTime eventTime, ZDateTime? eventTimeUtc, bool isDelayFired, bool shouldExist) =>
			{
				var job = Factory.New<StmJobQueue>();
				job.SJ_FilterName = subscriber.Name;
				job.SJ_PostedTimeUtc = ZDateTime.UtcNow;
				job.SJ_Status = JobQueueStatus.StatusQueued;
				job.SJ_EventTime = eventTime;
				job.SJ_EventTimeUtc = eventTimeUtc ?? eventTime;
				job.SJ_IsDelayFired = isDelayFired;
				job.SJ_Reference = shouldExist.ToString();
				job.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				return job;
			};

			//With EventTimeUtc: 4
			createStmJobQueue(ZDateTime.UtcNow.AddDays(10), ZDateTime.UtcNow, true, true);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(-10), ZDateTime.UtcNow, true, true);
			createStmJobQueue(ZDateTime.Now.AddDays(-10), ZDateTime.UtcNow.AddMinutes(5), true, false);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(-10), ZDateTime.UtcNow.AddDays(5), true, false);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(10), ZDateTime.UtcNow.AddHours(-5), true, true);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(-10), ZDateTime.UtcNow.AddDays(-5), true, true);

			//IsDelayFired=false: 6
			createStmJobQueue(ZDateTime.UtcNow, null, false, true);
			createStmJobQueue(ZDateTime.Now, null, false, true);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(-10), null, false, true);
			createStmJobQueue(ZDateTime.UtcNow.AddDays(10), null, false, true);
			createStmJobQueue(ZDateTime.Now.AddDays(10), ZDateTime.UtcNow.AddDays(10), false, true);
			createStmJobQueue(ZDateTime.Now.AddDays(-10), ZDateTime.UtcNow.AddDays(-10), false, true);

			Factory.Save();

			StmJobQueueBatch batch = null;
			try
			{
				batch = transmitter.ReadQueueForTest();

				AssertEquals("Invalid number of records returned", 10, batch.ItemsLoaded);
				Assert("Invalid records returned", batch.Values.All(j => j.Item.Reference == true.ToString()));
			}
			finally
			{
				batch?.Dispose();
			}
		}

		public void TestNoErrorReportWhenTryHandleException2()
		{
			var bizo = new BusinessObjectFactory();
			var saveInitiator = new Mock<ISaveInitiator>();
			var job = bizo.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(job, bizo);
			var header1 = jobHeader.ProcessHeaders.AddNew();
			header1.FH_CompletionStatement = "Workflow 1";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Workflow 2";

			var link1 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link1.FP_FH_HeaderTo = header2.PK;
			link1.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
			var link2 = header1.LinksFromMeToOthers_ForBinding.AddNew();
			link2.FP_FH_HeaderTo = header2.PK;
			link2.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;

			var exception = AssertExceptionThrown<ZSaveException>(() => bizo.Save());

			var causeZSaveException = true;
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var subscriber = new MockSubscriber(logs =>
			{
				var factory = logs[0].Factory;
				factory.Saving += (e) =>
				{
					if (causeZSaveException)
					{
						causeZSaveException = false;
						throw exception;
					}
				};
			});
			new MockNewsPublisher().PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber });

			var transmitter = GetNewsTransmitter(subscriber);
			transmitter.ProcessLogsFromDb(10);
			AssertNull("Should not report error before retries", UnitTestUserNotification.Instance.LastMessage.Text);
			ErrorReporter.Clear();
		}

		public void TestJobQueueStatus_ValidValues()
		{
			foreach (var status in JobQueueStatus.All())
			{
				var job = Factory.New<StmJobQueue>();
				job.SJ_PostedTimeUtc = ZDateTime.UtcNow;
				job.SJ_Status = status;
				job.SJ_EventTime = ZDateTime.Now;
				job.SJ_EventTimeUtc = ZDateTime.UtcNow;
				job.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			}
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestJobQueueStatus_InvalidValues()
		{
			var job = Factory.New<StmJobQueue>();
			job.SJ_PostedTimeUtc = ZDateTime.UtcNow;
			job.SJ_Status = "AAA";
			job.SJ_EventTime = ZDateTime.Now;
			job.SJ_EventTimeUtc = ZDateTime.UtcNow;
			job.SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var ex = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertContains("The INSERT statement conflicted with the CHECK constraint \"Constraint_SJ_Status\".", ex.Message);
		}

		public void TestParentTableCode_ShouldNotEmpty()
		{
			var job = Factory.New<StmJobQueue>();
			job.SJ_PostedTimeUtc = ZDateTime.UtcNow;
			job.SJ_Status = JobQueueStatus.StatusQueued;
			job.SJ_EventTime = ZDateTime.Now;
			job.SJ_EventTimeUtc = ZDateTime.UtcNow;
			job.SJ_ParentTableCode = "";
			var ex = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
			AssertContains("The INSERT statement conflicted with the CHECK constraint \"Constraint_SJ_ParentTableCode\".", ex.Message);
		}
	}
}
