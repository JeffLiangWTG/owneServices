using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	sealed class LogSubscriberSaveTrackerTest : TestCaseWithFactory
	{
		public void TestAddValidFactoryToSave()
		{
			Factory.NameForDebugging = "TestFactory";
			Tracker.AddValidFactoryToSave("TestFactory");

			using (Tracker.TrackInvalidSaves())
			{
				Factory.Save();
			}

			Tracker.ReportConcurrencyErrorCausedByInvalidSave(CreateMockProcessableLogGroup(), new DeveloperNotificationException("Test"));

			AssertEquals("Should report no errors", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestTrackInvalidSaves()
		{
			BusinessObjectFactory newFactory = null;
			using (new DisposableAction(() => TestingState.IsRunningTests = false, () => TestingState.IsRunningTests = true))
			{
				newFactory = new BusinessObjectFactory();
				newFactory.NameForDebugging = "TestFactory";
			}

			using (Tracker.TrackInvalidSaves())
			{
				newFactory.Save();
			}

			Tracker.ReportConcurrencyErrorCausedByInvalidSave(CreateMockProcessableLogGroup(), new DeveloperNotificationException("Test"));

			AssertEquals("Should report the invalid save", 1, ExceptionReporterTestListener.Instance.Count);

			var expectedErrorMsg = $@"Subscriber Name: MockEventSubscriber,
Parent: 00000000-0000-0000-0000-000000000000 Table: 

{MainFactoryName}: TestFactory
Main Factory Allocation Path:
   at";
			var actualErrorMsg = ExceptionReporterTestListener.Instance.GetExceptionMessage(0);

			AssertContains(expectedErrorMsg, actualErrorMsg);
			AssertContains(StackTraceMsg, actualErrorMsg);
			AssertNotContains(ChildFactoryName, actualErrorMsg);
			AssertNotContains(TransactionParticipantType, actualErrorMsg);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestTrackInvalidSavesWithChildFactory()
		{
			BusinessObjectFactory newFactory = null;
			BusinessObjectFactory childFactory = null;
			Tracker.AddValidFactoryToSave("TestMainFactory");
			using (new DisposableAction(() => TestingState.IsRunningTests = false, () => TestingState.IsRunningTests = true))
			{
				newFactory = new BusinessObjectFactory();
				newFactory.NameForDebugging = "TestMainFactory";

				childFactory = new BusinessObjectFactory();
				childFactory.NameForDebugging = "TestChildFactory";

				newFactory.ChildFactories.Add(childFactory);
			}

			using (Tracker.TrackInvalidSaves())
			{
				newFactory.Save();
			}

			Tracker.ReportConcurrencyErrorCausedByInvalidSave(CreateMockProcessableLogGroup(), new DeveloperNotificationException("Test"));

			AssertEquals("Should report the invalid save", 1, ExceptionReporterTestListener.Instance.Count);

			var expectedErrorMsg1 = $@"Subscriber Name: MockEventSubscriber,
Parent: 00000000-0000-0000-0000-000000000000 Table: 

{MainFactoryName}: TestMainFactory
Main Factory Allocation Path:
   at";
			var expectedErrorMsg2 = $@"{ChildFactoryName}: TestChildFactory
Child Factory Allocation Path:
   at";
			var actualErrorMsg = ExceptionReporterTestListener.Instance.GetExceptionMessage(0);

			AssertContains(expectedErrorMsg1, actualErrorMsg);
			AssertContains(expectedErrorMsg2, actualErrorMsg);
			AssertContains(StackTraceMsg, actualErrorMsg);
			AssertNotContains(TransactionParticipantType, actualErrorMsg);

			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestTrackInvalidSavesWithNonFactoryTransactionParticipant()
		{
			Factory.NameForDebugging = "TestFactory";
			Tracker.AddValidFactoryToSave("TestFactory");
			var transactionParticipant = new DummySaveInTransactionAction();

			using (Tracker.TrackInvalidSaves())
			{
				BusinessObjectFactory.SaveTogether(Factory, transactionParticipant);
			}

			Tracker.ReportConcurrencyErrorCausedByInvalidSave(CreateMockProcessableLogGroup(), new DeveloperNotificationException("Test"));

			AssertEquals("Should report the invalid save", 1, ExceptionReporterTestListener.Instance.Count);

			var expectedErrorMsg1 = $@"Subscriber Name: MockEventSubscriber,
Parent: 00000000-0000-0000-0000-000000000000 Table:";
			var expectedErrorMsg2 = $"{TransactionParticipantType}: Enterprise.LogWalker.Testing.LogSubscriberSaveTrackerTest+DummySaveInTransactionAction";
			var actualErrorMsg = ExceptionReporterTestListener.Instance.GetExceptionMessage(0);

			AssertContains(expectedErrorMsg1, actualErrorMsg);
			AssertContains(expectedErrorMsg2, actualErrorMsg);
			AssertContains(StackTraceMsg, actualErrorMsg);
			AssertNotContains(MainFactoryName, actualErrorMsg);
			AssertNotContains(ChildFactoryName, actualErrorMsg);

			ExceptionReporterTestListener.Instance.Clear();
		}

		protected override void SetUp()
		{
			Tracker = new LogSubscriberSaveTracker();
		}

		ProcessableLogGroup CreateMockProcessableLogGroup()
		{
			var subscriber = new MockSubscriber();

			var logBatchKey = new LogBatchKey(0, Guid.NewGuid());
			var logList = new List<AppLockedItem<IQueuedLog>>();
			logList.Add(new AppLockedItem<IQueuedLog>(Factory.New<StmJobQueue>(), null));
			var logBatch = new LogBatch(logBatchKey, logList);

			return new ProcessableLogGroup(Factory, subscriber, 1, 1, logBatch, null, null);
		}

		class DummySaveInTransactionAction : SaveInTransactionAction
		{
			protected override bool IsInTransaction => false;

			protected override ITransactionManager BeginTransactionWithManager()
			{
				return new StubTransactionManager();
			}

			protected override IChangedTableNames SaveInTransaction()
			{
				return ChangedTableNames.Empty;
			}
		}

		LogSubscriberSaveTracker Tracker;
		
		const string MainFactoryName = "Main Factory Name";
		const string ChildFactoryName = "Child Factory Name";
		const string TransactionParticipantType = "Transaction Participant Type";
		const string StackTraceMsg = @"Stack Trace:
   at";
	}
}
