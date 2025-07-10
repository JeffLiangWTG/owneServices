using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.LogWalker.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.LogWalker.Testing
{
	sealed class NewsPublisherTest : TestCaseWithFactory
	{
		public void TestCreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents_RowCountIncludesDeleted()
		{
			var publisher = new NewsPublisher();
			while (publisher.CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(Db.Connection, new[] { new MockSubscriber() }) > 0)
			{
			}

			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = stmEvent.TableName;
				log.SL_Parent = stmEvent.PK;
				log.SL_SE_NKEvent = Events.CustomisableEvent05Code;
			}

			Factory.Save();

			var filterlessQuery = new ZQuery();
			filterlessQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(filterlessQuery);
			AssertEquals("Even though nothing got created, something happened so record this.", 1, publisher.CreateLogQueueItemsForEverythingExceptWorkflowTriggerEvents(Db.Connection, new[] { new MockSubscriber() }));
			AssertArrayEqualsByElements("See, nothing got created.", itemsBeforeQueuing, Factory.Load<StmJobQueue>(filterlessQuery));
		}

		public void TestCreateLogQueueItemsForWorkflowTriggerEvents()
		{
			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.WorkflowTriggerEventCode;
				log.SL_Table = ProcessTasksSchema.Constants.TableName;
				log.SL_Reference = "WTE";
				log.SL_Parent = stmEvent.PK;
				log.SL_EventTime = new ZDateTime(2009, 09, 02, 13, 33, 22);
				log.SL_GS_NKUser = "U1";
				log.SL_IsEstimate = ZBool.True;
			}

			Factory.Save();
			var publisher = new NewsPublisher();
			AssertEquals("1 log created", 1, publisher.CreateLogQueueItemsForWorkflowTriggerEvents(Db.Connection));
			var queuedLogs = Factory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_ParentID, stmEvent.PK));
			AssertEquals("1 log queued", 1, queuedLogs.Length);
			var queuedLog1 = queuedLogs[0];

			AssertEquals("Log 1 EventTime", log.SL_EventTime, queuedLog1.SJ_EventTime);
			AssertEquals("Log 1 User", log.SL_GS_NKUser, queuedLog1.SJ_GS_NKUser);
			AssertEquals("Log 1 Department", log.SL_GE_NKDepartment, queuedLog1.SJ_GE_NKDepartment);
			AssertEquals("Log 1 Branch", log.SL_GB_NKBranch, queuedLog1.SJ_GB_NKBranch);
			AssertEquals("Log 1 IsCancelled", log.SL_IsCancelled, queuedLog1.SJ_IsCancelled);
			AssertEquals("Log 1 IsEstimate", log.SL_IsEstimate, queuedLog1.SJ_IsEstimate);
		}

		public void TestCreateNewLogQueueItems()
		{
			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log1.SL_Table = MockSubscriber.TestTableName;
				log1.SL_Reference = "LOG1";
				log1.SL_Parent = stmEvent.PK;
				log1.SL_EventTime = new ZDateTime(2009, 09, 02, 13, 33, 22);
				log1.SL_GS_NKUser = "U1";
				log1.SL_IsEstimate = ZBool.True;
			}

			var log2 = Factory.New<StmALog>();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log2.SL_Table = MockSubscriber.TestTableName;
				log2.SL_Reference = "LOG2";
				log2.SL_Parent = stmEvent.PK;
				log2.SL_EventTime = new ZDateTime(2008, 09, 02);
				log2.SL_IsEstimate = ZBool.False;
			}
			log2.Cancel();

			Factory.Save();

			var subscriber = new MockSubscriber();
			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, subscriber.Name);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("[PRE-CONDITION] No queued logs initially", 0, itemsBeforeQueuing.Length);

			var queuePublisher = new NewsPublisher();
			queuePublisher.CreateNewLogQueueItems(new[] { subscriber });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 1, queuedLogs.Length);

			var queuedLog1 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG1", MockSubscriber.TestSubscriberName);
			AssertNull("Cancelled log is not found", GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG2", MockSubscriber.TestSubscriberName));
			AssertNotNull("LOG1 found", queuedLog1);

			AssertEquals("Event", MockSubscriber.TestEventType, queuedLog1.SJ_SE_NKEvent);
			AssertEquals("Table Prefix", StmEventSchema.Constants.Prefix, queuedLog1.SJ_ParentTableCode);

			AssertEquals("Log 1 EventTime", log1.SL_EventTime, queuedLog1.SJ_EventTime);
			AssertEquals("Log 1 User", log1.SL_GS_NKUser, queuedLog1.SJ_GS_NKUser);
			AssertEquals("Log 1 Department", log1.SL_GE_NKDepartment, queuedLog1.SJ_GE_NKDepartment);
			AssertEquals("Log 1 Branch", log1.SL_GB_NKBranch, queuedLog1.SJ_GB_NKBranch);
			AssertEquals("Log 1 IsCancelled", log1.SL_IsCancelled, queuedLog1.SJ_IsCancelled);
			AssertEquals("Log 1 IsEstimate", log1.SL_IsEstimate, queuedLog1.SJ_IsEstimate);
		}

		public void TestCancellingEventDoesNotRequeue()
		{
			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();

			var initial = CountStmALogQueue();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log.SL_Table = MockSubscriber.TestTableName;
				log.SL_Reference = "LOG2";
				log.SL_Parent = evt.PK;
				log.SL_EventTime = new ZDateTime(2008, 09, 02);
				log.SL_IsEstimate = ZBool.False;
			}
			log.Cancel();

			Factory.Save();
			AssertEquals("Log not created by the trigger.", initial, CountStmALogQueue());
		}

		public void TestCreateNewLogQueueItems_ChangingReference()
		{
			var log1 = Factory.New<StmALog>();
			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log1.SL_Table = MockSubscriber.TestTableName;
				log1.SL_Reference = "LOG1";
				log1.SL_Parent = stmEvent.PK;
				log1.SL_EventTime = new ZDateTime(2009, 09, 02, 13, 33, 22);
				log1.SL_GS_NKUser = "U1";
				log1.SL_IsEstimate = ZBool.True;
			}

			var log2 = Factory.New<StmALog>();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log2.SL_Table = MockSubscriber.TestTableName;
				log2.SL_Reference = "LOG2";
				log2.SL_Parent = stmEvent.PK;
				log2.SL_EventTime = new ZDateTime(2008, 09, 02);
				log2.SL_IsEstimate = ZBool.False;
			}

			Factory.Save();

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, MockSubscriber.TestSubscriberName);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("[PRE-CONDITION] No queued logs initially", 0, itemsBeforeQueuing.Length);

			var queuePublisher = new NewsPublisher();
			queuePublisher.CreateNewLogQueueItems(new[] { new MockSubscriber() });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 2, queuedLogs.Length);

			var queuedLog1 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG1", MockSubscriber.TestSubscriberName);
			var queuedLog2 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG2", MockSubscriber.TestSubscriberName);

			AssertNotNull("LOG1 found", queuedLog1);
			AssertNotNull("LOG2 found", queuedLog2);

			queuePublisher.CreateNewLogQueueItems(new[] { new MockSubscriber() });

			AssertEquals("Number of Queued Logs", 2, queuedLogs.Length);

			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_Reference = "Im a different value | Such Scandal.";
			}

			queuePublisher.CreateNewLogQueueItems(new[] { new MockSubscriber() });

			AssertEquals("Number of Queued Logs", 2, queuedLogs.Length);
		}

		public void TestCreateNewLogQueueItems_WithMultipleEventSubscriber()
		{
			var testSubscriberEvents = new string[] { Events.Arrival.Code, Events.Departure.Code, Events.Booked.Code, Events.DeferredScheduledMessage.Code };
			var testSubscriber = new MockSubscriber("SUB1", testSubscriberEvents, new string[] { JobShipmentSchema.Constants.TableName });
			var testSubscribers = new LogSubscriber[] { testSubscriber };

			var shp = (BusinessObject)Factory.New<IForwardingShipment>();
			shp.FillWithValidTestData();
			var log1 = Factory.New<StmALog>();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = Events.Departure.Code;
				log1.SL_Table = JobShipmentSchema.Constants.TableName;
				log1.SL_Reference = "LOG1";
				log1.SL_Parent = shp.PK;
			}

			var log2 = Factory.New<StmALog>();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = Events.Arrival.Code;
				log2.SL_Table = JobShipmentSchema.Constants.TableName;
				log2.SL_Reference = "LOG2";
				log2.SL_Parent = shp.PK;
			}

			var log3 = Factory.New<StmALog>();
			using (log3.LockForUpdatingKeyFieldsForTesting())
			{
				log3.SL_SE_NKEvent = Events.DeferredScheduledMessage.Code;
				log3.SL_Table = JobShipmentSchema.Constants.TableName;
				log3.SL_Reference = "LOG3";
				log3.SL_Parent = shp.PK;
			}

			Factory.Save();
			var logs = new StmALog[] { log1, log2, log3 };

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, testSubscriber.Name);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("[PRE-CONDITION] No queued logs initially", 0, itemsBeforeQueuing.Length);

			var queuePublisher = new NewsPublisher();
			queuePublisher.CreateNewLogQueueItems(testSubscribers);

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 3, queuedLogs.Length);

			var queuedLog1 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG1", testSubscriber.Name);
			AssertEquals("Reference matches (LOG1)", log1.SL_Reference, queuedLog1.SJ_Reference);
			AssertEquals("Event matches (LOG1)", log1.SL_SE_NKEvent, queuedLog1.SJ_SE_NKEvent);
			AssertEquals("Table matches (LOG1)", JobShipmentSchema.Constants.Prefix, queuedLog1.SJ_ParentTableCode);
			Assert("Not delayed fired", !queuedLog1.SJ_IsDelayFired);

			var queuedLog2 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG2", testSubscriber.Name);
			AssertEquals("Reference matches (LOG2)", log2.SL_Reference, queuedLog2.SJ_Reference);
			AssertEquals("Event matches (LOG2)", log2.SL_SE_NKEvent, queuedLog2.SJ_SE_NKEvent);
			AssertEquals("Table matches (LOG2)", JobShipmentSchema.Constants.Prefix, queuedLog2.SJ_ParentTableCode);
			Assert("Not delayed fired", !queuedLog2.SJ_IsDelayFired);

			var queuedLog3 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, "LOG3", testSubscriber.Name);
			AssertEquals("Reference matches (LOG3)", log3.SL_Reference, queuedLog3.SJ_Reference);
			AssertEquals("Event matches (LOG3)", log3.SL_SE_NKEvent, queuedLog3.SJ_SE_NKEvent);
			AssertEquals("Table matches (LOG3)", JobShipmentSchema.Constants.Prefix, queuedLog3.SJ_ParentTableCode);
			Assert("Is delayed fired", queuedLog3.SJ_IsDelayFired);
		}

		public void TestCreateNewLogQueueItems_WithOverlappingCriteriaSubscribers()
		{
			var log1 = MockSubscriber.TestParent(Factory).Logs.AddNew(Events.All[MockSubscriber.TestEventType], "REF1");
			var log2 = MockSubscriber.TestParent(Factory).Logs.AddNew(Events.All[MockSubscriber.TestEventType], "REF2");
			Factory.Save();

			var subscriberName1 = "SUB1";
			var subscriberName2 = "SUB2";

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, new[] { subscriberName1, subscriberName2 });
			subscriberLogsQuery.ReLoadExistingRows = true;

			StmJobQueue[] itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("[PRE-CONDITION] No queued logs initially", 0, itemsBeforeQueuing.Length);

			LogSubscriber[] testSubscribers = new LogSubscriber[]
			{
				new MockSubscriber(subscriberName1),
				new MockSubscriber(subscriberName2)
			};

			var queuePublisher = new NewsPublisher();
			queuePublisher.CreateNewLogQueueItems(testSubscribers);

			StmJobQueue[] queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 4, queuedLogs.Length);

			StmJobQueue queuedLogRef1Sub1 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, log1.SL_Reference, subscriberName1);
			StmJobQueue queuedLogRef1Sub2 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, log1.SL_Reference, subscriberName2);
			StmJobQueue queuedLogRef2Sub1 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, log2.SL_Reference, subscriberName1);
			StmJobQueue queuedLogRef2Sub2 = GetLogFromQueueByReferenceAndSubscriberName(queuedLogs, log2.SL_Reference, subscriberName2);

			AssertNotNull("LOG REF1-SUB1 found", queuedLogRef1Sub1);
			AssertNotNull("LOG REF1-SUB2 found", queuedLogRef1Sub2);
			AssertNotNull("LOG REF2-SUB1 found", queuedLogRef2Sub1);
			AssertNotNull("LOG REF2-SUB2 found", queuedLogRef2Sub2);
			AssertEquals("Event matches", MockSubscriber.TestEventType, queuedLogRef1Sub1.SJ_SE_NKEvent);
			AssertEquals("Table Prefix matches", StmEventSchema.Constants.Prefix, queuedLogRef1Sub1.SJ_ParentTableCode);
		}

		public void TestCreateNewLogQueueItems_ExceptionWhenTablePrefixNotFoundForTableName()
		{
			var subscriber = new MockSubscriber("Mock", new[] { Events.EditedARecordCode }, new[] { "~InexistingTableName" });

			var newsPublisher = new NewsPublisher();
			AssertExceptionThrown(typeof(ApplicationException), "Table prefix not found for table [~InexistingTableName].", () =>
			{
				newsPublisher.CreateNewLogQueueItems(new[] { subscriber });
			});
		}

		public void TestIgnoreLogsMatchingEventFromOneSubscriberAndTableFromAnother()
		{
			var testSubscriber1 = new MockSubscriber("SUB1", new string[] { Events.Arrival.Code }, new string[] { DummyBizoSchema.Constants.TableName });
			var testSubscriber2 = new MockSubscriber("SUB2", new string[] { Events.QueueChanged.Code }, new string[] { StmPrintQueueSchema.Constants.TableName });
			var testSubscribers = new LogSubscriber[]
			{
				testSubscriber1,
				testSubscriber2
			};

			var log = Factory.New<StmALog>();
			var queue = Factory.New<IStmPrintQueue>();
			((BusinessObject)queue).FillWithValidTestData();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Arrival.Code;
				log.SL_Table = StmPrintQueueSchema.Constants.TableName;
				log.SL_Parent = queue.PK;
			}
			Factory.Save();

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(testSubscribers);

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, new[] { testSubscriber1.Name, testSubscriber2.Name });
			subscriberLogsQuery.ReLoadExistingRows = true;

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("No queued logs were created", 0, queuedLogs.Length);
		}

		public void TestCreateNewLogQueueItems_MultipleSubscribers()
		{
			var task = Factory.New<ProcessTask>();
			var log = CreateLog(task, AutoEvents.DeliveredCode);
			Factory.Save();

			var queuePublisher = new NewsPublisher();
			var testSubscriber1 = new MockSubscriber("TestSubscriber1", new[] { AutoEvents.DeliveredCode }, new[] { ProcessTasksSchema.Constants.TableName });
			var testSubscriber2 = new MockSubscriber("TestSubscriber2", new[] { AutoEvents.DeliveredCode }, new[] { ProcessTasksSchema.Constants.TableName });

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, new[] { testSubscriber1.Name, testSubscriber2.Name });
			subscriberLogsQuery.ReLoadExistingRows = true;
			var queuedJobs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Precondition", 0, queuedJobs.Length);

			queuePublisher.CreateNewLogQueueItems(new[] { testSubscriber1, testSubscriber2 });

			queuedJobs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("2 jobs should be created", 2, queuedJobs.Length);
			Assert(queuedJobs.Any(job => job.SJ_FilterName == "TestSubscriber1"));
			Assert(queuedJobs.Any(job => job.SJ_FilterName == "TestSubscriber2"));
		}

		StmALog CreateLog(BusinessObject parent, ZString eventCode)
		{
			StmALog log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = parent.PK;
				log.SL_Table = parent.TableName;
				log.SL_SE_NKEvent = eventCode;
			}
			return log;
		}

		public void TestCreateNewLogQueueItems_AllColumsArePopulated_BigReference()
		{
			var subscriber = new MockSubscriber("MockSubscriber", new[] { Events.Arrival.Code }, new[] { StmEventSchema.Constants.TableName });

			var evt = Factory.NewWithValidTestData<StmEvent>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Arrival.Code;
				log.SL_EventTime = ZDateTime.Now.AddMinutes(-10);
				log.SL_GS_NKUser = "DMI";
				log.SL_IsEstimate = false;
				log.SL_Reference = new string('a', StmALogSchema.SL_Reference.MaxLength);
				log.SL_Table = evt.TableName;
				log.SL_Parent = evt.PK;
			}

			Factory.Save();

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, subscriber.Name);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Pre-requisite: no queued logs initially", 0, itemsBeforeQueuing.Length);

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(new[] { subscriber });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 1, queuedLogs.Length);

			CombineAssertions(() =>
			{
				var queuedLog = queuedLogs[0];

				AssertEquals("MockSubscriber", queuedLog.SJ_FilterName);
				AssertEquals(log.SL_SE_NKEvent, queuedLog.SJ_SE_NKEvent);
				AssertEquals(log.SL_PostedTimeUtc, queuedLog.SJ_PostedTimeUtc);
				AssertEquals(log.SL_EventTime, queuedLog.SJ_EventTime);
				AssertEquals(log.SL_EventTimeUtc, queuedLog.SJ_EventTimeUtc);
				AssertEquals(log.SL_GS_NKUser, queuedLog.SJ_GS_NKUser);

				AssertEquals(log.SL_IsCancelled, queuedLog.SJ_IsCancelled);
				AssertEquals(log.SL_IsEstimate, queuedLog.SJ_IsEstimate);
				AssertEquals(log.SL_Reference, queuedLog.SJ_Reference);
				AssertEquals(StmEventSchema.Constants.Prefix, queuedLog.SJ_ParentTableCode);
				AssertEquals(log.SL_Parent, queuedLog.SJ_ParentID);

				AssertEquals(log.PK, queuedLog.SJ_ALogReference);
				AssertEquals(JobQueueStatus.StatusQueued, queuedLog.SJ_Status);
				AssertEquals(false, queuedLog.SJ_IsDelayFired);
			});
		}

		public void TestCreateNewLogQueueItems_AllColumsArePopulated()
		{
			var subscriber = new MockSubscriber("MockSubscriber", new[] { Events.Arrival.Code }, new[] { StmEventSchema.Constants.TableName });

			var stmEvent = Factory.NewWithValidTestData<StmEvent>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Arrival.Code;
				log.SL_EventTime = ZDateTime.Now.AddMinutes(-10);
				log.SL_GS_NKUser = "DMI";
				log.SL_IsEstimate = false;
				log.SL_Reference = "Hello";
				log.SL_Table = stmEvent.TableName;
				log.SL_Parent = stmEvent.PK;
			}

			Factory.Save();

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, subscriber.Name);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Pre-requisite: no queued logs initially", 0, itemsBeforeQueuing.Length);

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(new[] { subscriber });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 1, queuedLogs.Length);

			CombineAssertions(() =>
			{
				var queuedLog = queuedLogs[0];

				AssertEquals("MockSubscriber", queuedLog.SJ_FilterName);
				AssertEquals(log.SL_SE_NKEvent, queuedLog.SJ_SE_NKEvent);
				AssertEquals(log.SL_PostedTimeUtc, queuedLog.SJ_PostedTimeUtc);
				AssertEquals(log.SL_EventTime, queuedLog.SJ_EventTime);
				AssertEquals(log.SL_EventTimeUtc, queuedLog.SJ_EventTimeUtc);
				AssertEquals(log.SL_GS_NKUser, queuedLog.SJ_GS_NKUser);

				AssertEquals(log.SL_IsCancelled, queuedLog.SJ_IsCancelled);
				AssertEquals(log.SL_IsEstimate, queuedLog.SJ_IsEstimate);
				AssertEquals(log.SL_Reference, queuedLog.SJ_Reference);
				AssertEquals(StmEventSchema.Constants.Prefix, queuedLog.SJ_ParentTableCode);
				AssertEquals(log.SL_Parent, queuedLog.SJ_ParentID);

				AssertEquals(log.PK, queuedLog.SJ_ALogReference);
				AssertEquals(JobQueueStatus.StatusQueued, queuedLog.SJ_Status);
				AssertEquals(false, queuedLog.SJ_IsDelayFired);
			});
		}

		[StressTest]
		public void TestCreateNewLogQueueItems_MultipleNewLogs_AllProcessed()
		{
			var subscriber1 = new MockSubscriber("Subscriber1",
				new[] { Events.Arrival.Code, Events.Departure.Code },
				new[] { JobShipmentSchema.Constants.TableName, JobConsolSchema.Constants.TableName });

			var subscriber2 = new MockSubscriber("Subscriber2",
				new[] { Events.Arrival.Code },
				new[] { JobShipmentSchema.Constants.TableName, ProcessTasksSchema.Constants.TableName });

			var creationFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			void CreateMultipleLogs(int logsCount, string eventCode, BusinessObject parent)
			{
				parent.FillWithValidTestData();
				parent.Factory.Save();
				for (int i = 0; i < logsCount; i++)
				{
					var log = creationFactory.New<StmALog>();
					using (log.LockForUpdatingKeyFieldsForTesting())
					{
						log.SL_SE_NKEvent = eventCode;
						log.SL_Table = parent.TableName;
						log.SL_Parent = parent.PK;
						log.SL_Reference = "Load me by reference";
					}
				}
			}

			CreateMultipleLogs(16, Events.Arrival.Code, (BusinessObject)Factory.New<IForwardingShipment>());
			CreateMultipleLogs(32, Events.Arrival.Code, (BusinessObject)Factory.New<IForwardingConsol>());
			CreateMultipleLogs(64, Events.Departure.Code, (BusinessObject)Factory.New<IForwardingShipment>());
			CreateMultipleLogs(128, Events.Departure.Code, (BusinessObject)Factory.New<IForwardingConsol>());
			CreateMultipleLogs(1000, Events.EditedARecord.Code, Factory.New<RefCurrency>());
			CreateMultipleLogs(256, Events.Arrival.Code, Factory.New<ProcessTask>());

			creationFactory.Save();

			var allNewLogs = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Reference, "Load me by reference"));

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, new[] { subscriber1.Name, subscriber2.Name });
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Pre-requisite: no queued logs initially", 0, itemsBeforeQueuing.Length);

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(new[] { subscriber1, subscriber2 });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 512, queuedLogs.Length);

			Func<string, string, string, int> countQueuedLogsFor = (subscriberName, eventCode, tablePrefix) =>
			{
				var relevantQueuedLogs = queuedLogs
					.Where(q => q.SJ_FilterName == subscriberName && q.SJ_SE_NKEvent == eventCode && q.SJ_ParentTableCode == tablePrefix);

				var tableName = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(tablePrefix).TableName;

				AssertContainsExactElementsInAnyOrder("Queued logs have been created from correct StmALogs",
					allNewLogs.Where(log => log.SL_Table == tableName && log.SL_SE_NKEvent == eventCode).Select(log => log.PK),
					relevantQueuedLogs.Select(queuedLog => queuedLog.SJ_ALogReference));

				return relevantQueuedLogs.Count();
			};

			AssertEquals(16, countQueuedLogsFor("Subscriber1", "ARV", "JS"));
			AssertEquals(32, countQueuedLogsFor("Subscriber1", "ARV", "JK"));

			AssertEquals(64, countQueuedLogsFor("Subscriber1", "DEP", "JS"));
			AssertEquals(128, countQueuedLogsFor("Subscriber1", "DEP", "JK"));

			AssertEquals(16, countQueuedLogsFor("Subscriber2", "ARV", "JS"));
			AssertEquals(256, countQueuedLogsFor("Subscriber2", "ARV", "P9"));
		}

		public void TestCreateNewLogQueueItems_StmALogQueueIsConsumed()
		{
			var subscriber = new MockSubscriber("MockSubscriber", new[] { Events.Arrival.Code }, new[] { StmEventSchema.Constants.TableName });

			var evt = Factory.NewWithValidTestData<StmEvent>();
			Factory.Save();
			var initialCount = CountStmALogQueue();

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Table = evt.TableName;
				log.SL_Parent = evt.PK;
				log.SL_SE_NKEvent = Events.Arrival.Code;
			}

			Factory.Save();

			AssertEquals(initialCount + 1, CountStmALogQueue());

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, subscriber.Name);
			subscriberLogsQuery.ReLoadExistingRows = true;

			var itemsBeforeQueuing = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Pre-requisite: no queued logs initially", 0, itemsBeforeQueuing.Length);

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(new[] { subscriber });

			var queuedLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);
			AssertEquals("Number of Queued Logs", 1, queuedLogs.Length);

			AssertEquals("All logs are consumed", 0, CountStmALogQueue());
		}

		public void TestStmJobQueueEventTimeUtcHasValueForNoneWTE()
		{
			Db.Connection.ExecuteNonQuery($@"
			delete from dbo.StmALogQueue;
			delete from dbo.StmALogQueueWTE;");
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var log = dummy.Logs.AddNew(Events.CustomisableEvent00);
			((INeedRow)log).Row[StmALogSchema.SL_EventTimeUtc.Name] = DBNull.Value;

			var newsPublisher = new MockNewsPublisher();
			var subscriber1 = new Test.MockSubscriber(name: "AllCustomEvents");

			Factory.Save();

			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			var queuedLogs = Factory.Load<StmJobQueue>(new ZQuery(StmJobQueueSchema.SJ_FilterName, subscriber1.Name));
			AssertEquals(1, queuedLogs.Length);
			Assert("StmJobQueue.SJ_EventTimeUtc should not be null", queuedLogs[0].SJ_EventTimeUtc.IsValid);
		}

		public void TestStmJobQueueEventTimeUtcHasValueForWTE()
		{
			Db.Connection.ExecuteNonQuery($@"
delete from dbo.StmALogQueue;
delete from dbo.StmALogQueueWTE;
insert into dbo.StmALogQueueWTE (WTE_EventTime, WTE_EventTimeUTC, WTE_ParentID, WTE_PostedTimeUtc, WTE_ParentTableName, WTE_ALogReference) values (GETDATE(), NULL, newid(), GETDATE(), 'ProcessTasks', newid())
");

			var subscriber1 = new MockSubscriber(name: "Name");
			var newsPublisher = new MockNewsPublisher();
			AssertEquals(1, newsPublisher.PublishForTest(((IDbConnected)Factory).Connection, new[] { subscriber1 }));
			var queuedLogs = Factory.Load<StmJobQueue>(new ZQuery());
			AssertEquals(1, queuedLogs.Length);
			Assert("StmJobQueue.SJ_EventTimeUtc should not be null", queuedLogs[0].SJ_EventTimeUtc.IsValid);
		}

		#region Published Events

		public void TestCreateNewLogQueueItemsAndConfirmPendingOnes_CreateQueueItemsForPublishedLogs()
		{
			var subscriber = new MockSubscriber("Subscriber1", new[] { Events.Arrival.Code }, new[] { JobShipmentSchema.Constants.TableName });

			var initialCount = CountStmALogQueue();

			var subscription1 = CreateSubscription();
			var subscription2 = CreateSubscription();
			var subscription3 = CreateSubscription();

			var log1 = Factory.New<DummyWithWorkflow>().Logs.AddNew(Events.Arrival);
			var log2 = Factory.New<DummyWithWorkflow>().Logs.AddNew(Events.Departure);

			Factory.Save();

			AssertEquals(initialCount + 2, CountStmALogQueue());

			var query = $@"
select cast('{subscription1}' as UniqueIdentifier) A, cast('{log1.SL_Parent}' as UniqueIdentifier) B
union all
select cast('{subscription3}' as UniqueIdentifier), cast('{log1.SL_Parent}' as UniqueIdentifier)
union all
select cast('{subscription1}' as UniqueIdentifier), cast('{log2.SL_Parent}' as UniqueIdentifier)
union all
select cast('{subscription3}' as UniqueIdentifier), cast('{log2.SL_Parent}' as UniqueIdentifier)";

			DummyWorkflowDescriptor.Instance.ExpectGetSubscriptionsQuery(query);
			NewsPublisherEventMappingTable.UpdateIEventPublisherMappingsForTest();

			var publisher = new NewsPublisher();
			publisher.CreateNewLogQueueItems(new[] { subscriber });

			var subscriberLogsQuery = new ZQuery(StmJobQueueSchema.SJ_FilterName, "WorkflowEventPublish");
			subscriberLogsQuery.ReLoadExistingRows = true;
			var actualLogs = Factory.Load<StmJobQueue>(subscriberLogsQuery);

			var expectedLogs = new[]
			{
					CreateLog(log1, subscription1),
					CreateLog(log1, subscription3),
					CreateLog(log2, subscription1),
					CreateLog(log2, subscription3),
			};

			AssertContainsExactElementsInAnyOrder(
				new StmJobQueueCustomComparer(
					log => log.SJ_ParentTableCode,
					log => log.SJ_PostedTimeUtc,
					log => log.SJ_ALogReference,
					log => log.SJ_IsDelayFired,
					log => log.SJ_IsCancelled,
					log => log.SJ_FilterName,
					log => log.SJ_SE_NKEvent,
					log => log.SJ_IsEstimate,
					log => log.SJ_EventTime,
					log => log.SJ_EventTimeUtc,
					log => log.SJ_GS_NKUser,
					log => log.SJ_Reference,
					log => log.SJ_ParentID,
					log => log.SJ_TargetID),
					log => log.SJ_Status,
				expectedLogs,
				actualLogs);
		}

		ZGuid CreateSubscription()
		{
			var subscription = Factory.New<StmEventSubscription>();
			subscription.SES_RegistrarParentId = Guid.NewGuid();
			subscription.SES_AgentDescriptor = "AAA";
			subscription.SES_PublisherDescriptor = "DUM";
			subscription.SES_RegistrarDescriptor = "DUM";
			subscription.SES_SubscriberDescriptor = "BBB";

			return subscription.PK;
		}

		StmJobQueue CreateLog(StmALog triggeringLog, ZGuid subscription)
		{
			var log = Factory.New<StmJobQueue>();

			log.SJ_ParentTableCode = "Z0";
			log.SJ_PostedTimeUtc = triggeringLog.SL_PostedTimeUtc;
			log.SJ_ALogReference = triggeringLog.PK;
			log.SJ_JobConfirmed = true;
			log.SJ_IsCancelled = triggeringLog.SL_IsCancelled;
			log.SJ_FilterName = "WorkflowEventPublish";
			log.SJ_SE_NKEvent = triggeringLog.SL_SE_NKEvent;
			log.SJ_IsEstimate = triggeringLog.SL_IsEstimate;
			log.SJ_EventTime = triggeringLog.SL_EventTime;
			log.SJ_EventTimeUtc = triggeringLog.SL_EventTimeUtc;
			log.SJ_GS_NKUser = triggeringLog.SL_GS_NKUser;
			log.SJ_GE_NKDepartment = triggeringLog.SL_GE_NKDepartment;
			log.SJ_GB_NKBranch = triggeringLog.SL_GB_NKBranch;
			log.SJ_Reference = triggeringLog.SL_Reference;
			log.SJ_ParentID = triggeringLog.SL_Parent;
			log.SJ_TargetID = subscription;
			log.SJ_Status = "QUE";

			return log;
		}

		#endregion

		#region Implementation

		StmJobQueue GetLogFromQueueByReferenceAndSubscriberName(StmJobQueue[] queuedLogs, ZString logReferenceToFind, string subscriberNameToFind)
		{
			StmJobQueue result = null;

			foreach (StmJobQueue queuedLog in queuedLogs)
			{
				if (queuedLog.SJ_Reference == logReferenceToFind && queuedLog.SJ_FilterName == subscriberNameToFind)
				{
					result = queuedLog;
					break;
				}
			}

			return result;
		}

		int CountStmALogQueue()
		{
			string sql = @"
select count1 + count2
from 
(select count(*) count1 from dbo.StmALogQueue) t1,
(select count(*) count2 from dbo.StmALogQueueWTE) t2";

			int count = 0;
			using (var command = Db.Connection.Command(sql))
			using (var reader = command.ExecuteReader())
			{
				if (reader.Read())
				{
					count = reader.GetInt32(0);
				}
			}
			return count;
		}

		#endregion
	}
}
