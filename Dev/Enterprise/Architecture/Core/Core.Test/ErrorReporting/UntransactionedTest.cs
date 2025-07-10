using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	class UntransactionedTest : TestCase
	{
		[UseSnapshotProtection]
		[TestDate(1950, 1, 1, 9, 0, 0)]
		public void TestReportStillCreatedIfExceptionReportedInRolledBackTransaction()
		{
			var beforecount = GetNumStmErrorReports();
			Db.Connection.BeginTransaction();
			var testReporter = new BaseExceptionReporterForTest()
			{
				SaveReportOutsideTransactionDuringTest = true
			};
			testReporter.SendReportForTesting(null, new ExceptionReportArgs(new ArgumentException(), "e", "a", "b"));
			var duringcount = GetNumStmErrorReports();
			AssertEquals(duringcount, beforecount + 1);
			Db.Connection.RollbackTransaction();
			var aftercount = GetNumStmErrorReports();
			AssertEquals(duringcount, aftercount);
		}

		int GetNumStmErrorReports()
		{
			using (var cmd = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM [{0}]", StmErrorReportSchema.Constants.TableName)))
			{
				return (int)cmd.ExecuteScalar();
			}
		}

		public void TestSqlLockLostExceptionLocksDisposed()
		{
			var ex = AssertExceptionThrown<SqlLockLostException>(() =>
			{
				Assert(Db.Connection.TryGetLock("TestSqlLockLostExceptionLocksDisposed", out var appLock));
				try
				{
					Db.Connection.CloseConnection();
					Db.Connection.EnsureIsOpen();
				}
				finally
				{
					appLock.Dispose();
				}
			});

			ExceptionReporter.Instance.HandleOrReport(ex);
			AssertEquals("SqlLockLostException should not be reported", 0, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("Connection to the database was lost, please try again.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestSqlLockLostExceptionLocksNotDisposed()
		{
			SqlApplicationLock appLock = null;
			try
			{
				var ex = AssertExceptionThrown<SqlLockLostException>(() =>
				{
					Assert(Db.Connection.TryGetLock("TestSqlLockLostExceptionLocksDisposed", out appLock));
					Db.Connection.CloseConnection();
					Db.Connection.EnsureIsOpen();
				});

				ExceptionReporter.Instance.HandleOrReport(ex);
				AssertEquals("SqlLockLostException should be reported", ex, ExceptionReporterTestListener.Instance[0].InnerException);
				ExceptionReporterTestListener.Instance.Clear();
			}
			finally
			{
				appLock?.Dispose();
			}
		}

		[UseSnapshotProtection]
		public void TestUnobservedTaskExceptionsAreReportedSilently()
		{
			using (Globals.SetIsUnitTestingProductionFunctionality())
			using (EnvProxy.Instance.SetTemporaryMasterUserContext(User.UnKnownUserName, Guid.Empty, Guid.Empty))
			{
				int beforecount = GetNumStmErrorReports();
				ExceptionReporter.Instance.TestingDoReportException.Value = true;
				var mockFormManager = new Mock<IExceptionReportingFormManager>();
				ObjectFactory.Substitute(mockFormManager.Object);
				var taskRef = StartTask(); //This task has to be unobserved. If we call Wait() on it, the exception is propagated up through Wait(). So, we'll have to sleep for larger periods until it finishes.
				int tries = 0;
				int maxtries = 4;
				while (taskRef.IsAlive && tries < maxtries)
				{
					Thread.Sleep((int)Math.Pow(10, tries));
					GC.Collect();
					GC.WaitForPendingFinalizers();
					++tries;
				}
				mockFormManager.Verify(o => o.ShowReportForm(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Action<ExceptionReportArgs>>()), Times.Never());
				AssertEquals(beforecount + 1, GetNumStmErrorReports());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1056:Do Not Use GC.Collect()")]
		[SuppressMessage("CargoWiseOne", "CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)")]
		[UseSnapshotProtection]
		public void TestBatchOfUnobservedTaskExceptionsDoNotExitApplicationForDeveloper()
		{
			using (Globals.TemporaryOverrideForIsTest(false))
			{
				var mockProgramRestarter = new Mock<IProgramRestarter>();
				ObjectFactory.Substitute(mockProgramRestarter.Object);
				ExceptionReporter.Instance.TestingDoReportException.Value = true;
				var beforecount = GetNumStmErrorReports();
				var taskRef = new WeakReference[10];
				for (var i = 0; i < 10; i++)
				{
					taskRef[i] = StartTask();
				}
				var tries = 0;
				var maxtries = 4;
				while (taskRef.Any(t => t.IsAlive) && tries < maxtries)
				{
					Thread.Sleep((int)Math.Pow(10, tries));
					GC.Collect();
					GC.WaitForPendingFinalizers();
					++tries;
				}
				AssertEquals("IsAlive", false, taskRef.Any(t => t.IsAlive));
				mockProgramRestarter.Verify(o => o.ShutdownEnterpriseWithMessage(It.IsAny<string>()), Times.Never());
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1056:Do Not Use GC.Collect()")]
		[SuppressMessage("CargoWiseOne", "CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)")]
		[UseSnapshotProtection]
		public void TestCriticalExceptionIsIgnoredUnobservedTaskException()
		{
			// Arrange
			ExceptionReporter.Instance.TestingDoReportException.Value = true;

			var taskRef = StartTask(() => new TestCriticalException());

			var originalErrorReportCount = GetNumStmErrorReports();

			var tryCount = 4;
			while (taskRef.IsAlive && tryCount > 0)
			{
				Thread.Sleep(TimeSpan.FromSeconds(1d));

				GC.Collect();
				GC.WaitForPendingFinalizers();

				--tryCount;
			}
			AssertEquals("IsAlive", false, taskRef.IsAlive);

			// Assert
			AssertEquals(originalErrorReportCount, GetNumStmErrorReports());
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference StartTask()
		{
			return StartTask(() => new Exception("FAIL"));
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference StartTask(Func<Exception> createExceptionFunc)
		{
			var task = new Task(() => { throw createExceptionFunc(); });
			task.Start();
			return new WeakReference(task);
		}

		class BaseExceptionReporterForTest : BaseExceptionReporter
		{
			public void SendReportForTesting(IErrorReporter reporter, ExceptionReportArgs reportArgs) => SendReport(reporter, reportArgs);
		}
	}
}
