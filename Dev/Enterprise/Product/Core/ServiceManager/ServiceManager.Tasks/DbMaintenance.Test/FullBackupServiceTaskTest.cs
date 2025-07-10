using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DbBackup.Engine;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(FullBackupServiceTask))]
	sealed class FullBackupServiceTaskTest : ServiceTaskTestCase<FullBackupServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestInitialiseScheduleNotHosted()
		{
			InitialiseSchedule(string.Empty);
		}

		public void TestInitialiseScheduleHosted()
		{
			InitialiseSchedule("SYD");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void InitialiseSchedule(string hostLocation)
		{
			var originalHostedPlace = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest(hostLocation);

			try
			{
				var fullBackupTask = new FullBackupServiceTask();
				var attribute = GetHostedServiceAttributes().SingleOrDefault();
				ServiceTaskFrequencyTestHelper.ParseFrequency(attribute.DefaultScheduleRunEvery, out var periodCount, out var period);

				InitialiseTaskSchedule(fullBackupTask, out StmServiceTask taskSchedule);
				AssertEquals("ScheduleTask - IsActive", attribute.ActiveByDefault, (bool)taskSchedule.SST_Active);
				AssertEquals("ScheduleTask - TaskPeriod", ScheduleRecurrenceType.Weekly, period);
				AssertContainsExactElementsInAnyOrder("ScheduleTask - DayList", new[] { DayOfWeek.Sunday }, attribute.DefaultScheduleDaysOfWeek);
				AssertEquals("ScheduleTask - TaskPeriodCount", 1, periodCount);
				AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest(originalHostedPlace);
			}
		}

		public void TestFullBackupRunsUnconditionally()
		{
			CombineAssertions(() =>
			{
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Test, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Production, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Demo, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Education, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.Training, hostedOnWiseCloud: false);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: true);
				Test(DatabaseTypes.Codes.WisecloudTrial, hostedOnWiseCloud: false);
			});
			return;

			void Test(string databaseType, bool hostedOnWiseCloud)
			{
				// Arrange
				var keyForTest = ObjectFactory.Get<IProductRegistration>().KeyForTest;
				var savedData = keyForTest.DatabaseTypeForTest;
				var savedIsWiseTechGlobalDatabaseServerForTest = DataUtils.IsWiseTechGlobalDatabaseServerForTest;

				var loggerMock = new Mock<ILogger>();
				var dbMaintenanceProxyMock = new Mock<IDbMaintenanceProxy>(MockBehavior.Strict);
				dbMaintenanceProxyMock.Setup(x => x
					.RunFullBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>())).Verifiable();

				using (ObjectFactory.Substitute(dbMaintenanceProxyMock.Object))
				using (new DisposableAction(() =>
				{
					keyForTest.DatabaseTypeForTest = savedData;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = savedIsWiseTechGlobalDatabaseServerForTest;
				}))
				{
					keyForTest.DatabaseTypeForTest = databaseType;
					DataUtils.IsWiseTechGlobalDatabaseServerForTest = hostedOnWiseCloud;

					var backupTask = new FullBackupServiceTask { ServiceLogger = loggerMock.Object };

					// Act
					backupTask.RunTask();

					// Assert
					AssertNoExceptionThrown(() =>
					{
						dbMaintenanceProxyMock.Verify(x => x
							.RunFullBackup(It.IsAny<ILogger>(), It.IsAny<IEmailNotificationSender>()), Times.Once);
						dbMaintenanceProxyMock.VerifyNoOtherCalls();
					});
				}
			}
		}
	}
}
