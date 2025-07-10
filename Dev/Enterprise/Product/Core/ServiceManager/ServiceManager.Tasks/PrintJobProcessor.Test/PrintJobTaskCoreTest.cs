using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.PrintJobProcessor.Testing
{
	sealed class PrintJobTaskCoreTest : TestCaseWithFactory
	{
		public void TestNotify()
		{
			var printTask = new PrintJobTaskCore_ForTestingNotify();
			printTask.RunTask();

			AssertEquals(1, printTask.LoggerForTest.LogEntries.Count());
			AssertEquals("test message", printTask.LoggerForTest.LogEntries.First());
		}

		public void TestFailingToAcquireLockReturnsEmptyList()
		{
			var sqlException = GetSqlException();
			var index = 0;
			var printJobTask = new PrintJobTaskCoreForTesting
			{
				ActionInAnotherInstance = () =>
				{
					if (index == 7)
					{
						throw sqlException;
					}

					++index;
				}
			};

			var jobs = Enumerable.Range(0, 10).Select(_ => Factory.NewWithValidTestData<StmPrintJob>()).ToArray();

			using (var mutexesGoHere = new DisposableList(0))
			{
				AssertEquals("No jobs should be seen as locked, since there was an sql exception", 0, printJobTask.TryToLockJobs(jobs, mutexesGoHere, Db.Connection).Length);
				AssertEquals(ErrorReporter.LastExceptionReported, sqlException);
			}

			ErrorReporter.Clear();
		}

		SqlException GetSqlException()
		{
			//SqlException cannot be instantiated directly due to private constructor
#if NETFRAMEWORK
			return (SqlException)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(SqlException));
#else
			return (SqlException)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(SqlException));
#endif
		}

		public void TestFailingWhileLockingJobsDoesNotIgnoreDisposables()
		{
			var index = 0;
			var printJobTask = new PrintJobTaskCoreForTesting
			{
				ActionInAnotherInstance = () =>
				{
					if (index == 7)
					{
						throw new Exception("Random Exception.");
					}

					++index;
				}
			};
			var jobs = Enumerable.Range(0, 10).Select(_ => Factory.NewWithValidTestData<StmPrintJob>()).ToArray();

			using var disposableList = new DisposableList(8);

			AssertExceptionThrown("Precondition: There must be a failure in locking", typeof(Exception), "Random Exception.", () => printJobTask.TryToLockJobs(jobs, disposableList, Db.Connection));
			AssertEquals("Precondition: Any job must have been locked successfully", true, Db.Connection.HasUndisposedSqlLocks(@lock => @lock.Key.StartsWith("PrintJobTask")));
			AssertEquals("Should have the (successfully locked) previous mutexes jobs still", true, disposableList.Any());
		}

		public void TestGetJobsOfGivenTypesQuery()
		{
			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;
			var printJobs = Enumerable.Range(0, 10).Select(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.EML), deliveryGroup.PK)).ToArray();

			_ = new PrintJobTaskCoreForTesting().TryGetJobsOfGivenTypesQueryExposed(new[] { PrintJobType.EML }, out var query);

			Factory.Save();

			using var cmd = new ZSqlConnectionInfo(Db.Connection, Db.DatabaseName).GetNewDbCommandForStoredProcedure(query.ParameterisedQueryText, query.Parameters);
			using var reader = cmd.ExecuteReader();
			reader.Read();

			AssertEquals("We expect to pick up the first job in the queue", printJobs[0].PK, reader[0]);
		}

		public void TestTryToLockJobsByDeliveryGroupGuid()
		{
			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var parentGuid1 = Guid.NewGuid();
			var parentGuid2 = Guid.NewGuid();
			var printJobs1 = Enumerable.Range(0, 5).Select(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.EML), deliveryGroup.PK, parentGuid1)).ToArray();
			var printJobs2 = Enumerable.Range(0, 5).Select(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.EML), deliveryGroup.PK, parentGuid2)).ToArray();

			Factory.Save();

			var printJobTask = new PrintJobTaskCoreForTesting();
			using var disposableList = new DisposableList(10);
			var newConnection = Db.NewAdminConnection();

			var lockedPrintJobs = printJobTask.TryToLockJobs(printJobs1, disposableList, Db.Connection);
			AssertEquals(5, lockedPrintJobs.Length);

			lockedPrintJobs = printJobTask.TryToLockJobs(printJobs1, disposableList, newConnection);
			AssertEquals(0, lockedPrintJobs.Length);

			lockedPrintJobs = printJobTask.TryToLockJobs(printJobs2, disposableList, newConnection);
			AssertEquals(0, lockedPrintJobs.Length);
		}

		public void TestJobStillExistsAndStatusIsInQUE()
		{
			var deliveryGroup = Factory.NewWithValidTestData<StmDeliveryGroup>();
			deliveryGroup.SB_IsProcessed = true;

			var printJobs = Enumerable.Range(0, 3).Select(_ => TestHelper.CreateTestPrintJobAndAddToQueue(nameof(PrintJobType.EML), deliveryGroup.PK)).ToArray();

			Factory.Save();

			var printJobTask = new PrintJobTaskCoreForTesting();
			using (var disposableList = new DisposableList(3))
			{
				var lockedPrintJobs = printJobTask.TryToLockJobs(printJobs, disposableList, Db.Connection);
				AssertEquals(3, lockedPrintJobs.Length);
			}

			using (var disposableList = new DisposableList(3))
			{
				printJobs[0].SP_Status = nameof(PrintJobStatus.FAL);
				printJobs[1].Delete();
				Factory.Save();

				var lockedPrintJobs = printJobTask.TryToLockJobs(printJobs, disposableList, Db.Connection);
				AssertEquals(1, lockedPrintJobs.Length);
			}
		}

		#region Implementation

		PrintJobTaskTestHelper TestHelper => testHelper ??= new PrintJobTaskTestHelper(Factory);

		PrintJobTaskTestHelper testHelper;

		#endregion
	}
}
