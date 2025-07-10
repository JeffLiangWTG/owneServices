using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.Integration;
using Enterprise.LogWalker.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Testing
{
	sealed class OperationsManagerTest : TestCaseWithFactory
	{
		public void TestQueueNewLogsAndProcessQueue()
		{
			Db.Connection.ExecuteNonQuery("delete dbo.stmalogqueue");
			var testNotifier = new LoggerForTesting() { AllowDebug = true };
			var testSubscriber = new MockSubscriber();
			var testSubscribers = new LogSubscriber[] { testSubscriber };
			var testManager = new OperationsManagerForTesting(testSubscribers);

			testManager.QueueAndProcessLogs(testNotifier);
			AssertEquals("Notifications event count", 5, testNotifier.NotifiedEventList.Count);
			Assert("1st notification description not as expected\r\n" + testNotifier.NotifiedEventList[0],
				testNotifier.NotifiedEventList[0].StartsWith("Registered Subscribers:"));
			AssertEquals("2nd notification description", "Cleaning up old logs.", testNotifier.NotifiedEventList[1]);
			AssertEquals("3rd notification description", "Cleaned:0 PRS log(s), 0 FAL log(s), 0 dead log(s).", testNotifier.NotifiedEventList[2]);

			Assert("4th notification description not as expected\r\n" + testNotifier.NotifiedEventList[3],
				testNotifier.NotifiedEventList[3].StartsWith("0 log event(s) queued."));
			AssertEquals("5th notification description", "LogWalker cycle completed.", testNotifier.NotifiedEventList[4]);
			AssertEquals("No logs should have been queued yet", true, testSubscriber.LastProcessedLog_Pk.IsEmpty);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log.SL_Table = MockSubscriber.TestTableName;
				log.SL_Parent = Factory.NewWithValidTestData<StmEvent>().PK;
				log.SL_Reference = "TestQueueNewLogsAndProcessQueue";
			}
			Factory.Save();

			testManager.QueueAndProcessLogs(testNotifier);
			Assert("Notified event count should be >= 9", testNotifier.NotifiedEventList.Count >= 9);
			Assert("6th notification description not as expected\r\n" + testNotifier.NotifiedEventList[5],
				testNotifier.NotifiedEventList[5].StartsWith("Registered Subscribers:"));
			AssertEquals("7th notification description", "Cleaning up old logs.", testNotifier.NotifiedEventList[6]);
			AssertEquals("8th notification description", "Cleaned:0 PRS log(s), 0 FAL log(s), 0 dead log(s).", testNotifier.NotifiedEventList[7]);
			Assert("9th notified event description not as expected\r\n" + testNotifier.NotifiedEventList[8],
				testNotifier.NotifiedEventList[8].StartsWith("1 log event(s) queued."));

			// Assert Last Queued Log Properties
			AssertEquals("Queued log - ParentID", log.SL_Parent, testSubscriber.LastProcessedLog_ParentId);
			AssertEquals("Queued log - EventTime", log.SL_EventTime, testSubscriber.LastProcessedLog_EventTime);
			AssertEquals("Queued log - User", log.SL_GS_NKUser, testSubscriber.LastProcessedLog_User);
			AssertEquals("Queued log - Reference", log.SL_Reference, testSubscriber.LastProcessedLog_Reference);
			AssertEquals("Queued log - FilterName", testSubscriber.Name, testSubscriber.LastProcessedLog_FilterName);
			AssertEquals("Queued log - Event", testSubscriber.EventTypes[0], testSubscriber.LastProcessedLog_Event);
			AssertEquals("Queued log - Table Code", ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(testSubscriber.TableNames[0]), testSubscriber.LastProcessedLog_TablePrefix);

			// Assert Log MArked As Processed
			AssertEquals("Queue Log PK Empty?", false, testSubscriber.LastProcessedLog_Pk.IsEmpty);
			StmJobQueue queueLog = Factory.Load<StmJobQueue>(testSubscriber.LastProcessedLog_Pk);
			AssertEquals("Log should be marked as processed", JobQueueStatus.StatusProcessed, queueLog.SJ_Status);
		}

		public void TestStmJobQueueDBHitPerformance()
		{
			var testNotifier = new LoggerForTesting() { AllowDebug = true };
			var parent = Factory.New<DummyBusinessObject>();
			var subscriber1 = new MockSubscriber("mock1", new[] { Events.CustomisableEvent00Code }, new[] { DummyBizoSchema.Constants.TableName });
			var subscriber2 = new MockSubscriber("mock2", new[] { Events.CustomisableEvent01Code }, new[] { DummyBizoSchema.Constants.TableName });
			var subscriber3 = new MockSubscriber("mock3", new[] { Events.CustomisableEvent02Code }, new[] { DummyBizoSchema.Constants.TableName });
			MockSubscriber.QueueNewLogForGivenSubscriber(Factory, "mock1", parent);

			Factory.Save();
			var testManager = new OperationsManagerForTesting(new[] { subscriber1, subscriber2, subscriber3 });
			var allowedHits = new Dictionary<string, int>()
			{
				{ StmJobQueueSchema.Constants.TableName, 2 }
			};

			using (AssertDbHitsForAllFactories(allowedHits, ignoreUnspecified: true, useOnlyNewFactories: true))
			{
				testManager.QueueAndProcessLogs(testNotifier);
			}
		}

		public void TestLogTypeWithAndWithoutAction()
		{
			Db.Connection.ExecuteNonQuery("delete dbo.stmalogqueue");
			var testNotifier = new LoggerForLogWalkerTest();
			var testSubscriber = new MockSubscriber();
			var testSubscribers = new LogSubscriber[] { testSubscriber };
			var testManager = new OperationsManagerForTesting(testSubscribers);
			var testArchive = new NewsArchive(new[] { testSubscriber });

			testManager.QueueAndProcessLogs(testNotifier);

			AssertEquals("Cleaned:0 PRS log(s), 0 FAL log(s), 0 dead log(s).", testNotifier.LogEntries[2].Message);
			Assert(testNotifier.LogEntries[2].LogType == LogType.Debug);
			AssertEquals("0 log event(s) queued.", testNotifier.LogEntries[3].Message);
			Assert(testNotifier.LogEntries[3].LogType == LogType.Debug);

			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = MockSubscriber.TestEventType;
				log.SL_Table = MockSubscriber.TestTableName;
				log.SL_Parent = Factory.NewWithValidTestData<StmEvent>().PK;
			}

			Factory.Save();

			testManager.QueueAndProcessLogs(testNotifier);

			AssertEquals("1 log event(s) queued.", testNotifier.LogEntries[8].Message);
			Assert(testNotifier.LogEntries[8].LogType == LogType.Information);

			testArchive.CleanupOldLogs(CancellationToken.None, testNotifier);

			AssertEquals("Cleaned:1 PRS log(s), 0 FAL log(s), 0 dead log(s).", testNotifier.LogEntries[12].Message);
			Assert(testNotifier.LogEntries[12].LogType == LogType.Information);
		}

		public void TestQueueAndProcessLogsCannotBePassedNullLogger()
		{
			//Setup
			LogSubscriber[] testSubscribers = new LogSubscriber[] { new MockSubscriber(), new MockSubscriber() };
			OperationsManagerForTesting testManager = new OperationsManagerForTesting(testSubscribers);

			//Assert
			AssertExceptionThrown<ArgumentNullException>(() => testManager.QueueAndProcessLogs(null));
		}

		public void TestControllerThrowsExceptionIfTwoSubscribersHaveTheSameName()
		{
			LogSubscriber[] testSubscribers = new LogSubscriber[] { new MockSubscriber(), new MockSubscriber() };
			OperationsManagerForTesting testManager = new OperationsManagerForTesting(testSubscribers);

			try
			{
				testManager.QueueAndProcessLogs(new LoggerForTesting());
				Fail("An exception should have been thrown");
			}
			catch (Exception ex)
			{
				string expectedMessage = String.Format("There is already a subscriber called [{0}].", MockSubscriber.TestSubscriberName);
				AssertEquals("Unexpected exception caught", expectedMessage, ex.Message);
			}
		}
	}
}
