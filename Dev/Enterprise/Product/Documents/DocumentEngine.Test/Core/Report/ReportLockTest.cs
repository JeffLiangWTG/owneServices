using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.Environment.Semaphore;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class ReportLockTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFormattedCurrentLocks()
		{
			var currentDepartment = Env.CurrentDepartment.PK;
			var currentBranch = Env.CurrentBranch.PK;
			new ServiceManagerEnvProvider(usePooledConnection: true).Enable();
			Env.SetTemporaryUserContext(User.ServiceUserCode, currentBranch, currentDepartment);
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			using (var mutex = new ReportMutex())
			using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
			{
				var rep1 = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), "Report 1", Enterprise.MasterFiles.Business.ContactType.All, false);
				var rep2 = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), "Report with long name to test formatting", Enterprise.MasterFiles.Business.ContactType.All, false);
				Assert("Should be able to lock first mutex", mutex.Lock(rep1));
				Assert("Should NOT be able to lock second mutex", !mutex.Lock(rep2));

				var message = "Mutex Should contain - Start Time UTC: but actually has {0}";
				var lockMessage = mutex.GetFormattedCurrentLocks();
				var lockMessageRegexPattern = $"Start Time UTC: .+, Host Name: .+, Type: PRC";
				var lockMessageRegex = new Regex(lockMessageRegexPattern);
				Assert(string.Format(message, lockMessage), lockMessageRegex.IsMatch(lockMessage));

				mutex.Unlock(rep1);
				mutex.Unlock(rep2);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:Do Not Use DateTime Parse Method", Justification = "UTC DateTime Format For Test")]
		public void TestFormattedCurrentLocks_OrderByStartTime()
		{
			var currentDepartment = Env.CurrentDepartment.PK;
			var currentBranch = Env.CurrentBranch.PK;
			new ServiceManagerEnvProvider(usePooledConnection: true).Enable();
			Env.SetTemporaryUserContext(User.ServiceUserCode, currentBranch, currentDepartment);
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2);

			using (var mutex = new ReportMutex())
			using (SemaphoreDbManager.ForceToUseTheSameDbConnection_ForTestOnly())
			{
				var rep1 = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), "Report 1", Enterprise.MasterFiles.Business.ContactType.All, false);
				var rep2 = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles), "Report with long name to test formatting", Enterprise.MasterFiles.Business.ContactType.All, false);
				Assert("Should be able to lock first mutex", mutex.Lock(rep1));
				Thread.Sleep(1000);
				using ((ServiceTaskEnvironment.semaphoreProvider as ServiceTaskSemaphoreProvider).ResetInternalHeartbeatForTest())
				{
					Assert("Should be able to lock second mutex", mutex.Lock(rep2));

					var lockMessage = mutex.GetFormattedCurrentLocks();
					var startTimeRegex = new Regex(@"(?<=Start Time UTC:).{14}");
					var startTimes = startTimeRegex.Matches(lockMessage).ToList<Match>().Select(m => DateTime.Parse(m.Value, null, DateTimeStyles.AssumeUniversal)).ToList();
					AssertEquals("Should get two lock records", 2, startTimes.Count);
					Assert("Should be ordered by start time ascendingly", startTimes[0] < startTimes[1]);

					mutex.Unlock(rep1);
					mutex.Unlock(rep2);
				}
			}
		}

		public void TestLockReport()
		{
			SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			using (ReportMutex mutex1 = new ReportMutex())
			using (ReportMutex mutex2 = new ReportMutex())
			{
				AssertEquals("No report should be running now", 0, EnvProxy.Instance.SemaphoreProvider.GetActiveSemaphoreHandles(new ReportMutexSemaphore()).Length);

				Assert("Start Report1", mutex1.Lock(Report1));
				Assert("Start Report2 - should fail as there's no available slots", !mutex2.Lock(Report2));

				Assert("End Report1", mutex1.Unlock(Report1));
				Assert("Start Report2", mutex2.Lock(Report2));

				Assert("End Report2", mutex2.Unlock(Report2));
			}
		}

		public void TestReportUnlockedOnDispose()
		{
			SystemDataRegistryForTest.Get().ReportMaxConnections = 1;
			using (ReportMutex mutex = new ReportMutex())
			{
				// Lock and do not explicitly unlock
				mutex.Lock(Report1);
			}

			using (ReportMutex mutex = new ReportMutex())
			{
				Assert("First mutex wasn't unlocked on dispose", mutex.Lock(Report2));
			}
		}

		Report Report1;
		Report Report2;

		protected override void SetUp()
		{
			base.SetUp();
			Report1 = new Report(new DocumentPack(), null);
			Report2 = new Report(new DocumentPack(), null);
		}
	}
}
