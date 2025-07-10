using System;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Threading.Testing
{
	sealed class ThreadSentryTest : TransactionedTestCase
	{
		public void TestCreationThreadIDSetInConstructor()
		{
			AssertEquals("CreationThreadID must be set to the currentThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
		}

		public void TestCreationThreadIDSetsOwnerThreadID()
		{
			AssertEquals("[Pre-Condition] CreationThreadID must be set to currentThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
			AssertEquals("OwnerThreadID must be set to the creationThreadID on factory creation", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.OwnerThread.ThreadID);
		}

		public void TestRelinquishThreadOwnership()
		{
			AssertEquals("[Pre-Condition] CreationThreadID must be set to currentThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
			AssertEquals("[Pre-Condition] OwnerThreadID must be set to the creationThreadID on factory creation", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.OwnerThread.ThreadID);

			factory.ThreadSentry.RelinquishThreadOwnership();

			AssertEquals("OwnerThreadID must be set to null on call to RelinquishThreadOwnership()", null, factory.ThreadSentry.OwnerThread.ThreadID);
			AssertEquals("Calling RelinquishThreadOwnership() must not modify Factory CreationThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
		}

		public void TestRelinquishThreadOwnership_ForciblyRelinquish()
		{
			AssertEquals("[Pre-Condition] CreationThreadID must be set to currentThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
			AssertEquals("[Pre-Condition] OwnerThreadID must be set to the creationThreadID on factory creation", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.OwnerThread.ThreadID);

			factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest();

			AssertEquals("OwnerThreadID must be set to null on call to RelinquishThreadOwnership()", null, factory.ThreadSentry.OwnerThread.ThreadID);
			AssertEquals("Calling RelinquishThreadOwnership() must not modify Factory CreationThreadID", Thread.CurrentThread.ManagedThreadId, factory.ThreadSentry.CreationThread.ThreadID);
		}

		public void TestAccessFactoryReportsErrorWhenFactoryIsOwned()
		{
			Task.Factory.StartNew(TaskDoActionOnFactoryWithoutTakingOwnership).Wait();
			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestAccessFactoryReportsErrorWhenFactoryOwnershipNull()
		{
			factory.ThreadSentry.RelinquishThreadOwnership();
			Task.Factory.StartNew(TaskDoActionOnFactoryWithoutTakingOwnership).Wait();
			AssertContains("Expected error report containing specified message", ThreadSentry.AttemptedAccessNullOwnedObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestTakeThreadOwnershipReportsErrorWhenFactoryIsOwned()
		{
			Task.Factory.StartNew(TaskTakeOwnershipOnFactory).Wait();
			AssertContains("Expected error report containing specified message", ThreadSentry.CanNotTakeThreadOwnershipOfAnOwnedObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestTakeThreadOwnershipReportsErrorWhenThreadAlreadyOwnsFactory()
		{
			factory.ThreadSentry.RelinquishThreadOwnership();

			Task.Factory.StartNew(TaskTakeOwnershipTwiceAndDoActionOnFactory).Wait();
			AssertContains("Expected error report containing specified message", ThreadSentry.ThisThreadAlreadyHasThreadOwnershipOfThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestTakeThreadOwnershipSuceedesWhenFactoryOwnershipNull()
		{
			factory.ThreadSentry.RelinquishThreadOwnership();

			var newThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					TaskTakeOwnershipAndDoActionOnFactory();
				}
			});

			newThread.Start();
			newThread.Join();

			AssertEquals("After calling RelinqishThreadOwnership() and then takeThreadOwnership() ThreadOwner ID should be equal to the new threads ManagedThreadID",
							newThread.ManagedThreadId, factory.ThreadSentry.OwnerThread.ThreadID);
		}

		public void TestRelinquishThreadOwnershipOnANullOwnedFactory()
		{
			factory.ThreadSentry.RelinquishThreadOwnership();

			factory.ThreadSentry.RelinquishThreadOwnership();
			AssertContains("Relinquishing thread ownership on an unowned factory should report an error with specified message", ThreadSentry.CanNotRelinquishOwnershipOfObjectWithNoOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipOnANullOwnedFactory_ForciblyRelinquish()
		{
			factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest();

			factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest();
			AssertContains("Relinquishing thread ownership on an unowned factory should report an error with specified message", ThreadSentry.CanNotRelinquishOwnershipOfObjectWithNoOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipOnAnotherFactory()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.RelinquishThreadOwnership()).Wait();
			AssertContains("Relinquishing thread ownership on another thread's factory should report an error with specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestRelinquishThreadOwnershipOnAnotherFactory_ForciblyRelinquish()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest()).Wait();
			AssertContains("Relinquishing thread ownership on another thread's factory should report an error with specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("ThreadSentry Parent: []", ErrorReporter.LastMessageReported);
			AssertNotContains("Label:", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_IfPassedIntoRelinquishThreadOwnership()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.RelinquishThreadOwnership("diagnosticLabel")).Wait();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_IfPassedIntoRelinquishThreadOwnership_ForciblyRelinquish()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest("diagnosticLabel")).Wait();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_ProvidedAsFunction_IfPassedIntoRelinquishThreadOwnership()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.RelinquishThreadOwnership(() => "diagnosticLabel")).Wait();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_ProvidedAsFunction_IfPassedIntoRelinquishThreadOwnership_ForciblyRelinquish()
		{
			Task.Factory.StartNew(() => factory.ThreadSentry.ForciblyRelinquishThreadOwnership_ForTest(() => "diagnosticLabel")).Wait();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyCurrentThreadOwnerCanRelinquishThreadOwnershipMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_IfPassedIntoEnsureCurrentThreadIsOwner()
		{
			var newThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory.ThreadSentry.EnsureCurrentThreadIsOwner("diagnosticLabel");
				}
			});

			newThread.Start();
			newThread.Join();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_ProvidedAsFunction_IfPassedIntoEnsureCurrentThreadIsOwner()
		{
			var newThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					factory.ThreadSentry.EnsureCurrentThreadIsOwner(() => "diagnosticLabel");
				}
			});

			newThread.Start();
			newThread.Join();

			AssertContains("Expected error report containing specified message", ThreadSentry.OnlyOwnerThreadCanAccessThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_IfPassedIntoTakeThreadOwnership()
		{
			factory.ThreadSentry.TakeThreadOwnership("diagnosticLabel");

			AssertContains("Expected error report containing specified message", ThreadSentry.ThisThreadAlreadyHasThreadOwnershipOfThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportError_IncludesDiagnosticLabel_ProvidedAsFunction_IfPassedIntoTakeThreadOwnership()
		{
			factory.ThreadSentry.TakeThreadOwnership(() => "diagnosticLabel");

			AssertContains("Expected error report containing specified message", ThreadSentry.ThisThreadAlreadyHasThreadOwnershipOfThisObjectMessage, ErrorReporter.LastMessageReported);
			AssertContains("Expected error report to have thread sentry parent and the specified label", "ThreadSentry Parent: [], Label: [diagnosticLabel]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		void TaskTakeOwnershipOnFactory()
		{
			factory.ThreadSentry.TakeThreadOwnership();
		}

		void TaskDoActionOnFactoryWithoutTakingOwnership()
		{
			using (Db.DisposableActionForDbConnection())
			{
				factory.New<DummyBusinessObject>();
			}
		}

		void TaskTakeOwnershipAndDoActionOnFactory()
		{
			factory.ThreadSentry.TakeThreadOwnership();
			factory.New<DummyBusinessObject>();
		}

		void TaskTakeOwnershipTwiceAndDoActionOnFactory()
		{
			using (Db.DisposableActionForDbConnection())
			{
				factory.ThreadSentry.TakeThreadOwnership();
				factory.ThreadSentry.TakeThreadOwnership();
				factory.New<DummyBusinessObject>();
			}
		}

		BusinessObjectFactory factory;

		protected override void SetUp()
		{
			base.SetUp();
			TestEntityFrameworkSettings.Get().ReportCrossThreadFactoryAccess = true;
			factory = new BusinessObjectFactory();
		}

		protected override void TearDown()
		{
			base.TearDown();
			factory = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
