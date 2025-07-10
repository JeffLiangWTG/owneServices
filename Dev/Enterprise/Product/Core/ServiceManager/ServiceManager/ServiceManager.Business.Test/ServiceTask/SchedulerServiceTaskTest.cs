using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SchedulerServiceTask))]
	class SchedulerServiceTaskTest : TestCaseWithFactory
	{
		public void TestIsActive()
		{
			// Arrange
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "XYZ";
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);

			// Act
			schedule.S5_IsActive = true;
			schedule.Factory.Save();

			// Assert
			Assert(new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ").IsActive);

			// Act
			schedule.S5_IsActive = false;
			schedule.Factory.Save();

			// Assert
			Assert(!new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ").IsActive);
		}

		public void TestGetCodeAndDescription()
		{
			// Arrange
			var taskCode = "XYZ";
			var taskDescription = "XYZ Description";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_ScheduleDescription = taskDescription;
			schedule.Factory.Save();

			// Act
			var task = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ");

			// Assert
			AssertEquals(taskDescription, task.Description);
			AssertEquals(taskCode, task.Code);
		}

		public void TestGetBranchAndBranchName()
		{
			// Arrange
			var taskCode = "XYZ";
			var branchName = "~TB";

			var newCompany = Factory.New<GlbCompany>();
			newCompany.GC_Code = "~TC";
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Barbados;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = branchName;
			newBranch.GB_RN_NKCountryCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, newCompany.GC_RN_NKCountryCode)).RL_RN_NKCountryCode;
			Factory.Save();

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			schedule.S5_GB = newBranch.PK;
			schedule.Factory.Save();

			// Act
			var task = new SchedulerServiceTaskLoader(new Lazy<BusinessObjectFactory>(() => Factory)).Load("XYZ");

			// Assert
			AssertEquals(newBranch.PK, task.BranchPk);
			AssertEquals(branchName, task.BranchName);
		}

		public void TestGetMultilingualDescription()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleDescription = "Test Services";
			var scheduleMultilingualDescription = schedule.S5_ScheduleDescriptionMultilingual;

			// Act
			var task = new SchedulerServiceTask(schedule);

			//Assert
			AssertEquals(scheduleMultilingualDescription.ToString(), task.MultilingualDescription);
		}

		public void TestGetNextRunTime()
		{
			// Arrange
			var testDate = new ZDateTime(2024, 8, 16, 10, 30, 0, DateTimeKind.Utc);
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_NextScheduledPrintRunTimeUtc = testDate;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(testDate.ToDateTime(), task.NextRunTime.UtcDateTime);
		}

		public void TestGetDailyStartTime()
		{
			// Arrange
			var testDate = new ZDateTime(2024, 8, 16, 10, 30, 0, DateTimeKind.Utc);
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_DailyStartTime = testDate;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(testDate.TimeOfDay, task.DailyStartTime);
		}

		public void TestGetDailyEndTime()
		{
			// Arrange
			var testDate = new ZDateTime(2024, 8, 16, 10, 30, 0, DateTimeKind.Utc);
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_DailyEndTime = testDate;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(testDate.TimeOfDay, task.DailyEndTime);
		}

		public void TestGetBranchErrorMessage()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();

			schedule.S5_GB = ZGuid.Invalid;
			var expectedErrorMessage = schedule.S5_GBInfo.GetErrors().FirstOrDefault()?.Message;
			var task = new SchedulerServiceTask(schedule);

			// Act
			var actualErrorMessage = task.BranchErrorMessage;

			// Assert
			AssertEquals(expectedErrorMessage, actualErrorMessage);
		}

		public void TestGetConfigString()
		{
			// Arrange
			var testConfig = "TestConfigString";
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.ConfigString = testConfig;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(testConfig, task.ConfigString);
		}

		public void TestGetSettingsXml()
		{
			// Arrange
			var testSettingsString = "<xml></xml>";
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_ScheduleState = Encoding.ASCII.GetBytes(testSettingsString);

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(testSettingsString, task.SettingsXml);
		}

		public void TestGetScheduleDaysOfWeek()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_DayList = "NYNNNYN";

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertContainsExactElementsInAnyOrder(new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Friday }, task.ScheduleDaysOfWeek);

			// Act
			schedule.S5_DayList = "NNYYNNY";

			// Assert
			AssertContainsExactElementsInAnyOrder(new DayOfWeek[] { DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday }, task.ScheduleDaysOfWeek);
		}

		public void TestGetScheduleDayOfMonth()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.Recurrence.DayOfMonth = 20;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(20, task.ScheduleDayOfMonth);
		}

		public void TestGetScheduleMonth()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_MonthNumber = 12;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(12, task.ScheduleMonth);
		}

		public void TestGetScheduleDayNumber()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_DayNumber = 15;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(15, task.ScheduleDayNumber);
		}

		public void TestGetScheduleStartDate()
		{
			// Arrange
			var testDate = new ZDateTime(2024, 8, 16, 10, 30, 0, DateTimeKind.Utc);
			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_StartDate = testDate;

			// Act
			var task = new SchedulerServiceTask(schedule);

			AssertEquals(testDate.ToDateTime(), task.ScheduleStartDate.UtcDateTime);
		}

		public void TestGetScheduleOccurrence()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_TaskPeriod = "M";

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals("M", task.ScheduleOccurrence);
		}

		public void TestGetScheduleFrequency()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_TaskPeriodCount = 20;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals(20, task.ScheduleFrequency);
		}

		public void TestGetScheduleWeekdaysOnly()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_WeekDaysOnly = true;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			Assert(task.WeekDaysOnly);

			// Act
			schedule.S5_WeekDaysOnly = false;

			// Assert
			Assert(!task.WeekDaysOnly);
		}

		public void TestGetCategory()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			schedule.S5_TypeOfDocument = "CAT";

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			AssertEquals("CAT", task.Category);
		}

		public void TestGetThreadSafeReader()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var settings = new HostedServiceAttribute();
			settings.IsMandatory = false;
			settings.DefaultScheduleRunEvery = "15minutes";
			schedule.SetStaticServiceAttributesDebugOnly(settings);
			var task = new SchedulerServiceTask(schedule);

			// Act
			var threadSafeTaskReader = task.ThreadSafeTaskReader;

			// Assert
			Assert(threadSafeTaskReader is ServiceTaskScheduleThreadSafeReader);

			if (threadSafeTaskReader is IDisposable disposableReader)
			{
				disposableReader.Dispose();
			}
		}

		public void TestGetNullThreadSafeReader()
		{
			// Arrange
			var task = new SchedulerServiceTask(null);

			// Act
			var threadSafeTaskReader = task.ThreadSafeTaskReader;

			// Assert
			Assert(threadSafeTaskReader is NullServiceTaskScheduleThreadSafeReader);

			if (threadSafeTaskReader is IDisposable disposableReader)
			{
				disposableReader.Dispose();
			}
		}

		public void TestPreRunValidation()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();

			using (schedule.SuspendValidationTesting())
			{
				schedule.S5_TaskPeriod = "W";
				schedule.S5_TaskPeriodCount = 1;
				schedule.S5_DayList = "NNNNNNN";
				schedule.S5_DayListInfo.ClearAllNotifications();
				var task = new SchedulerServiceTask(schedule);

				AssertNoErrors("Pre-condition: no errors expected", schedule.S5_DayListInfo);

				// Act
				task.PreRunValidation();

				// Assert
				AssertHasError(schedule.S5_DayListInfo, "Please select at least one day.");
			}
		}

		public void TestEnsureThreadSafety()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var task = new SchedulerServiceTask(schedule);

			// Act
			// Assert
			AssertNoExceptionThrown(() => task.EnsureThreadSafety());
		}

		public void TestEnsureThreadSafetyIncorrectThread()
		{
			// Arrange
			var factory = new BusinessObjectFactory();
			var schedule = factory.NewWithValidTestData<ServiceTaskSchedule>();

			var newThreadTask = Task.Run(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var task = new SchedulerServiceTask(schedule);
					task.EnsureThreadSafety();
				}
			});

			newThreadTask.Wait();

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("Attempted to access an object owned by another thread.", ExceptionReporterTestListener.Instance[0].Message);
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestCalculateNextRunTime()
		{
			// Arrange
			var schedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			var expectedValue = schedule.CalculateNextRunTime(Mock.Of<ILogger>());
			var task = new SchedulerServiceTask(schedule);

			// Act
			var actualValue = task.CalculateNextRunTime(Mock.Of<ILogger>());

			// Assert
			AssertEquals(expectedValue.UtcToDateTimeOffset(), actualValue);
		}

		public void TestGetAssignedGovernor()
		{
			// Arrange
			var taskCode = "XYZ";
			var taskDescription = "XYZ Description";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			schedule.S5_ScheduleDescription = taskDescription;
			schedule.S5_IsActive = false;

			// Act
			var task = new SchedulerServiceTask(schedule);

			// Assert
			Assert(task.AssignedGovernor is not null);

			// Act
			task.AssignedGovernor?.SetActive(true);

			// Assert
			Assert(task.IsActive);
		}

		public void TestAssignedGovernorIsCached()
		{
			// Arrange
			var taskCode = "XYZ";
			var taskDescription = "XYZ Description";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = taskCode;
			schedule.S5_ScheduleDescription = taskDescription;

			var task = new SchedulerServiceTask(schedule);

			// Act
			var governor1 = task.AssignedGovernor;
			governor1.SetActive(true);

			var governor2 = task.AssignedGovernor;

			// Assert
			AssertEquals(governor1, governor2);
		}
	}
}
