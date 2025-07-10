using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTaskGovernor))]
	public class SchedulerServiceTaskGovernorTest : TestCaseWithFactory
	{
		public void TestSetActive()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_IsActive = false;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			governor.SetActive(true);

			// Assert
			Assert(schedule.S5_IsActive);
		}

		public void TestSetBranchPk()
		{
			// Arrange
			var taskCode = "XYZ";

			var dummyBranchPk = Guid.NewGuid();

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_IsActive = false;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			governor.SetBranchPk(dummyBranchPk);

			// Assert
			AssertEquals(dummyBranchPk, schedule.S5_GB);
		}

		public void TestSetBranchFromCode()
		{
			// Arrange
			var taskCode = "XYZ";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "~TC";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "~TB";
			newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
			Factory.Save();

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_IsActive = false;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			governor.SetBranchFromCode(newBranch.GB_Code);

			// Assert
			AssertEquals(newBranch.PK, schedule.S5_GB);
		}

		public void TestSetNextRunTime()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_IsActive = false;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var nextRunTime = DateTimeOffset.UtcNow.AddHours(5).AddMinutes(30);

			// Act
			governor.SetNextRunTime(nextRunTime);

			// Assert
			AssertEquals(nextRunTime, schedule.S5_NextScheduledPrintRunTimeUtc.UtcToDateTimeOffset().ToDateTimeOffset());
		}

		public void TestSetLastRunTime()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var lastRunTime = DateTimeOffset.UtcNow.AddHours(5).AddMinutes(30);

			// Act
			governor.SetLastRunTime(lastRunTime);

			// Assert
			AssertEquals(lastRunTime, schedule.LastRunTime.UtcToDateTimeOffset().ToDateTimeOffset());
		}

		public void TestSetStartDate()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var startDate = DateTimeOffset.UtcNow.AddHours(5).AddMinutes(30);

			// Act
			governor.SetStartDate(startDate);

			// Assert
			AssertEquals(startDate, schedule.S5_StartDate.UtcToDateTimeOffset().ToDateTimeOffset());
		}

		public void TestSetSchedule()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			governor.SetSchedule(22, "H");

			// Assert
			AssertEquals(22, schedule.S5_TaskPeriodCount);
			AssertEquals("H", schedule.S5_TaskPeriod);
		}

		public void TestSetWeeklySchedule()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var scheduleDaysOfWeek = new[] { DayOfWeek.Wednesday, DayOfWeek.Thursday };

			// Act
			governor.SetWeeklySchedule(3, scheduleDaysOfWeek);

			// Assert
			AssertEquals(3, schedule.S5_TaskPeriodCount);
			AssertEquals("W", schedule.S5_TaskPeriod);
			AssertEquals("NNNYYNN", schedule.S5_DayList);
		}

		public void TestSetMonthlySchedule()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			governor.SetMonthlySchedule(3, 15);

			// Assert
			AssertEquals(3, schedule.S5_TaskPeriodCount);
			AssertEquals("M", schedule.S5_TaskPeriod);
			AssertEquals(15, schedule.Recurrence.DayOfMonth);
		}

		public void TestSetDailyStartTimeLocal()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var startTime = TimeSpan.FromMinutes(360);
			var offSet = TimeSpan.FromMinutes(25);

			// Act
			governor.SetDailyStartTimeLocal(startTime, offSet);

			// Assert
			AssertEquals(startTime + offSet, schedule.S5_DailyStartTime.TimeOfDay);
		}

		public void TestSetDailyStartTimeUtc()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var startTime = TimeSpan.FromMinutes(360);
			var offSet = TimeSpan.FromMinutes(25);

			// Act
			governor.SetDailyStartTimeUtc(startTime, offSet);

			// Assert
			AssertEquals(startTime + offSet, schedule.CalcDailyStartTimeUtc.TimeOfDay);
		}

		public void TestSetDailyEndTimeLocal()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var endTime = TimeSpan.FromMinutes(360);

			// Act
			governor.SetDailyEndTimeLocal(endTime);

			// Assert
			AssertEquals(endTime, schedule.S5_DailyEndTime.TimeOfDay);
		}

		public void TestSetDailyEndTimeUtc()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var endTime = TimeSpan.FromMinutes(360);

			// Act
			governor.SetDailyEndTimeUtc(endTime);

			// Assert
			AssertEquals(endTime, schedule.CalcDailyEndTimeUtc.TimeOfDay);
		}

		public void TestSetOverdueDuration()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			var overDue = TimeSpan.FromMinutes(25);

			// Act
			governor.SetOverdueDuration(overDue);

			// Assert
			AssertEquals((int)overDue.TotalSeconds, schedule.S5_OverdueDurationInSeconds);
		}

		public void TestUpdateStatus()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var expectedResult = schedule.UpdateStatus();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			var actualResult = governor.UpdateStatus();

			// Assert
			AssertEquals(expectedResult, actualResult);
		}

		public void TestReload()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			schedule.S5_ScheduleDescription = "Original description";
			Factory.Save();

			schedule.S5_ScheduleDescription = "Updated but not saved description";

			// Act
			governor.Reload();

			// Assert
			AssertEquals("Original description", governor.GovernedTask.Description);
		}

		public void TestReloadUpdatedValuesFromDb()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);
			schedule.S5_IsActive = false;
			Factory.Save();

			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var updatingGovernor = transactionAdapter.GetServiceTaskGovernor(schedule.PK.ToGuid());

			updatingGovernor.SetActive(true);
			transactionAdapter.Commit();

			// Act
			governor.Reload();

			// Assert
			Assert(governor.GovernedTask.IsActive);
		}

		public void TestOnErrorReported()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			// Assert
			AssertNoExceptionThrown(() =>
			{
				governor.OnErrorReported();
			});
		}

		public void TestResetScheduleToDefault()
		{
			// Arrange
			var taskCode = "XYZ";

			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = true;
			settings.DefaultScheduleRunEvery = "1hour";
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_IsActive = false;
			schedule.Factory.Save();

			var governor = new SchedulerServiceTaskGovernor(Factory, schedule);

			// Act
			var result = governor.ResetScheduleToDefault(true, Mock.Of<ILogger>());

			// Assert
			Assert(result);
		}
	}
}
