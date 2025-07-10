using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	sealed class DbConsistencyCheckServiceTaskWithDatabaseRecoveryModelTest : TestCase
	{
		public void TestSkipDbConsistencyCheck()
		{
			using (SetTestParameters(DbRecoveryModel.Simple, isWiseTechServer: true))
			{
				var testLogger = RunDbccTask();

				AssertEquals(1, testLogger.Count);
				Assert("Must skip dbcc", testLogger[0].Contains("Skip database consistency check"));
				Assert("Must have info to enable dbcc", !testLogger[0].Contains("To enable database consistency check"));
			}

			using (SetTestParameters(DbRecoveryModel.Simple, isWiseTechServer: false))
			{
				var testLogger = RunDbccTask();

				AssertEquals(1, testLogger.Count);
				Assert("Must skip dbcc", testLogger[0].Contains("Skip database consistency check"));
				Assert("Must have info to enable dbcc", testLogger[0].Contains("To enable database consistency check"));
			}

			using (SetTestParameters(DbRecoveryModel.Full, isWiseTechServer: true))
			{
				var testLogger = RunDbccTask();

				AssertGreaterThan(testLogger.Count, 2);
				AssertEquals("Information|Check database does not have consistency/allocation errors", testLogger[0]);
				AssertEquals("Information|DbHealthCheck is completed", testLogger[testLogger.Count - 1]);
			}

			IDisposable SetTestParameters(DbRecoveryModel recoveryModel, bool isWiseTechServer)
			{
				var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;
				DataUtils.IsWiseTechGlobalDatabaseServerForTest = isWiseTechServer;
				var substitution = ObjectFactory.Substitute(DbRecoveryModelManagerMock.WithActual(recoveryModel));

				return new DisposableAction(() =>
				{
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
					substitution.Dispose();
				});
			}
		}

		TestServiceLogger RunDbccTask()
		{
			var dbccTask = new DbConsistencyCheckServiceTask();
			var testLogger = new TestServiceLogger();
			dbccTask.ServiceLogger = testLogger;
			dbccTask.RunTask();

			return testLogger;
		}
	}
}
