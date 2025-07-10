using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.DataContracts;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(ServiceTaskSchedule))]
	class ServiceTaskScheduleTest : EnterpriseBusinessObjectTestCase
	{
		[TestedType(typeof(ServiceTaskSchedule.Loader))]
		class LoaderTest : LoaderTestCase
		{
			protected override BusinessObject.Loader GetNewLoaderToTest()
			{
				return new ServiceTaskSchedule.Loader(Factory);
			}

			public void TestLoader_GetInstancesOfNameService()
			{
				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = "xxx";
				Factory.Save();

				var loader = new ServiceTaskSchedule.Loader(Factory);

				var loadedServiceTask = loader.GetInstancesOfNameService("xxx");
				AssertNotNull(loadedServiceTask);
				AssertEquals("ServiceTaskSchedule S5_ScheduleType is set correctly", "xxx", loadedServiceTask.S5_ScheduleType);
				AssertEquals("ServiceTaskSchedule S5_ParentTableCode", "SH", loadedServiceTask.S5_ParentTableCode);
			}
		}

		[TestDate(2018, 07, 07, 23, 0, 0)]
		public void TestWithin1Point5Hours_AllTimesOfDay()
		{
			var serviceTaskSchedule = Factory.NewWithValidTestData<ServiceTaskSchedule>();
			serviceTaskSchedule.S5_NextScheduledPrintRunTimeUtc = new ZDateTime(2019, 07, 07, 23, 00, 00);
			serviceTaskSchedule.S5_TaskPeriod = ScheduleRecurrenceType.Daily;

			var dummy = "";

			for (var i = 0; i < 48; ++i)
			{
				serviceTaskSchedule.S5_NextScheduledPrintRunTimeUtc += TimeSpan.FromMinutes(30);
				TestDateAttribute.Date = serviceTaskSchedule.S5_NextScheduledPrintRunTimeUtc.ToDateTime();
				TestDateAttribute.Date = TestDateAttribute.Date.AddYears(-1);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(30);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(20);
				Assert(!serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-130);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-30);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-20);
				Assert(serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(-20);
				Assert(!serviceTaskSchedule.IsValidTimeOfDayToRun(out dummy));
			}
		}

		public void TestFileBasedLogViewer()
		{
			// Arrange
			var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();

			// Act
			serviceTaskSchedule.S5_ScheduleType = "111";

			// Assert
			AssertNotNull(serviceTaskSchedule.LogViewer.FileBasedLogViewer);
			AssertEquals(serviceTaskSchedule.LogViewer.FileBasedLogViewer.TaskType, serviceTaskSchedule.S5_ScheduleType);
			Assert(serviceTaskSchedule.LogViewer.FileBasedLogViewer.TaskTypeReadOnly);
		}

		public void TestSearchBasedLogViewer()
		{
			// Arrange
			var serviceTaskSchedule = Factory.New<ServiceTaskSchedule>();

			// Act
			serviceTaskSchedule.S5_ScheduleType = "111";

			// Assert
			AssertNotNull(serviceTaskSchedule.LogViewer.SearchBasedLogViewer);
			AssertEquals(serviceTaskSchedule.LogViewer.SearchBasedLogViewer.ServiceTaskCode, serviceTaskSchedule.S5_ScheduleType);
		}

		public void TestScheduleCannotSaveIfBranchIsInactive()
		{
			// Arrange
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "USFDW";

			var schedule = Factory.New<ServiceTaskSchedule>();
			schedule.S5_ScheduleType = "111";
			schedule.S5_GB = branch.PK;
			schedule.S5_IsActive = false;
			Factory.Save();

			// Act
			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var branchInAnotherFactory = anotherFactory.Load<GlbBranch>(branch.PK);
			branchInAnotherFactory.GB_IsActive = false;
			anotherFactory.Save();

			schedule.S5_IsActive = true;
			schedule.RunPreSaveValidation();

			// Assert
			AssertHasRowError(schedule, "You cannot activate a service task when the branch is inactive.");
		}

		public void TestBranchName()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			Assert(scheduleTask.S5_GB.IsValid);
			AssertEquals(scheduleTask.Branch.GB_Code, scheduleTask.BranchName);
		}

		public void TestS5_GB_ReadOnly()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			CombineAssertions(() =>
			{
				AssertEquals("It's not ReadOnly default", false, scheduleTask.S5_GBInfo.ReadOnly);

				var settings = new HostedServiceAttribute
				{
					CanRunInAnyBranch = true,
				};
				scheduleTask.SetStaticServiceAttributesDebugOnly(settings);
				AssertEquals("It's ReadOnly when CanRunInAnyBranch is true", true, scheduleTask.S5_GBInfo.ReadOnly);
			});
		}

		[TestDate(2008, 2, 7, 13, 0, 0)]
		public void TestIsValidTimeOfDayToRun()
		{
			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Hourly;
			Assert(scheduleTask.IsValidTimeOfDayToRun(out var message));

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date;
			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date;
			Assert(scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(10);
			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(18);
			Assert(scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(15);
			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(20);
			Assert(!scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(22);
			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(2);
			Assert(!scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(21);
			scheduleTask.S5_DailyEndTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(14);
			Assert(scheduleTask.IsValidTimeOfDayToRun(out message));
		}

		[TestDate(2008, 2, 2, 13, 0, 0)]
		public void TestIsValidTimeOfDayToRunForDailyTasks1()
		{
			AssertIsValidTimeOfDayToRunForDailyTasks1(ScheduleRecurrenceType.Daily, false);
			AssertIsValidTimeOfDayToRunForDailyTasks1(ScheduleRecurrenceType.Weekly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks1(ScheduleRecurrenceType.Monthly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks1(ScheduleRecurrenceType.Yearly, true);
		}

		void AssertIsValidTimeOfDayToRunForDailyTasks1(ZString taskPeriod, ZBool isValidTimeIfWeekDaysOnly)
		{
			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_TaskPeriod = taskPeriod;
			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(12);
			Assert(scheduleTask.IsValidTimeOfDayToRun(out var message));

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-91);
			AssertEquals("NextRunTime more than 1.5 hours old => IsValidTimeOfDayToRun?", false, scheduleTask.IsValidTimeOfDayToRun(out message));
			AssertEquals("Next Run Time UTC (2008-02-02 11:29:00) time of day is more than 1.5 hours away from current UTC (2008-02-02 13:00:00) time of day. To avoid this problem in the future, always set Next Run Time to within an hour of Scheduled Run Time, or use 'Schedule Now' to skip this check.",
				message);

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-89);
			AssertEquals("NextRunTime less than 1.5 hours old => IsValidTimeOfDayToRun?", true, scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_WeekDaysOnly = true;
			var isValid = scheduleTask.IsValidTimeOfDayToRun(out message);
			AssertEquals("Task.WeekDaysOnly = TRUE => IsValidTimeOfDayToRun?", isValidTimeIfWeekDaysOnly, isValid);
			if (!isValid)
			{
				AssertEquals("This task is scheduled to run on weekdays only, and an attempt has been made to run it on the weekend (Next Run Time UTC = 2008-02-02 11:31:00, current UTC = 2008-02-02 13:00:00). To avoid this problem in the future, always set Next Run Time to an appropriate day of the week, or use 'Schedule Now' to skip this check.",
					message);
			}

			scheduleTask.S5_WeekDaysOnly = false;
			AssertEquals("Task.WeekDaysOnly = FALSE => IsValidTimeOfDayToRun?", true, scheduleTask.IsValidTimeOfDayToRun(out message));
		}

		[TestDate(2008, 2, 4, 0, 30, 0)]
		public void TestIsValidTimeOfDayToRunForDailyTasks2()
		{
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Daily, false);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Weekly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Monthly, true);
			AssertIsValidTimeOfDayToRunForDailyTasks2(ScheduleRecurrenceType.Yearly, true);
		}

		void AssertIsValidTimeOfDayToRunForDailyTasks2(ZString taskPeriod, ZBool isValidTimeIfWeekDaysOnly)
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			scheduleTask.S5_TaskPeriod = taskPeriod;
			scheduleTask.S5_DailyStartTime = ZDateTime.MinSmallDateTimeValue.Date.AddHours(12);
			Assert(scheduleTask.IsValidTimeOfDayToRun(out var message));

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-91);
			AssertEquals("NextRunTime more than 1.5 hours old => IsValidTimeOfDayToRun?", false, scheduleTask.IsValidTimeOfDayToRun(out message));
			AssertEquals("Next Run Time UTC (2008-02-03 22:59:00) time of day is more than 1.5 hours away from current UTC (2008-02-04 00:30:00) time of day. To avoid this problem in the future, always set Next Run Time to within an hour of Scheduled Run Time, or use 'Schedule Now' to skip this check.",
				message);

			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.MinSmallDateTimeValue.Date.AddHours(23).AddMinutes(30);
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(-89);
			AssertEquals("NextRunTime less than 1.5 hours old => IsValidTimeOfDayToRun?", true, scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_WeekDaysOnly = true;
			var isValid = scheduleTask.IsValidTimeOfDayToRun(out message);
			AssertEquals("Task.WeekDaysOnly = TRUE => IsValidTimeOfDayToRun?", isValidTimeIfWeekDaysOnly, isValid);
			if (!isValid)
			{
				AssertEquals("This task is scheduled to run on weekdays only, and an attempt has been made to run it on the weekend (Next Run Time UTC = 2008-02-03 23:01:00, current UTC = 2008-02-04 00:30:00). To avoid this problem in the future, always set Next Run Time to an appropriate day of the week, or use 'Schedule Now' to skip this check.",
					message);
			}

			scheduleTask.S5_WeekDaysOnly = false;
			AssertEquals("Task.WeekDaysOnly = FALSE => IsValidTimeOfDayToRun?", true, scheduleTask.IsValidTimeOfDayToRun(out message));
		}

		[TestDate(2008, 2, 4, 6, 5, 0)]
		public void TestIsValidTimeOfDayToRunIgnoresDailyEndTimeWhenIsNotApplicable()
		{
			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_DailyStartTime = new ZDateTime(2008, 2, 4, 5, 30, 0);
			scheduleTask.S5_DailyEndTime = new ZDateTime(2008, 2, 4, 6, 00, 0);

			// Task Periods that have Daily End Time exposed
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Second;
			AssertEquals("End Time should make current time invalid", false, scheduleTask.IsValidTimeOfDayToRun(out var message));
			AssertEquals("The current UTC (2008-02-04 06:05:00) is not between Start Time UTC (05:30:00) and End Time UTC (06:00:00). To avoid this problem in the future, always set Next Run Time within the bounds of Start Time and End Time, or use 'Schedule Now' to skip this check.",
				message);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
			AssertEquals("End Time should make current time invalid", false, scheduleTask.IsValidTimeOfDayToRun(out message));
			AssertEquals("The current UTC (2008-02-04 06:05:00) is not between Start Time UTC (05:30:00) and End Time UTC (06:00:00). To avoid this problem in the future, always set Next Run Time within the bounds of Start Time and End Time, or use 'Schedule Now' to skip this check.",
				message);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Hourly;
			AssertEquals("End Time should make current time invalid", false, scheduleTask.IsValidTimeOfDayToRun(out message));
			AssertEquals("The current UTC (2008-02-04 06:05:00) is not between Start Time UTC (05:30:00) and End Time UTC (06:00:00). To avoid this problem in the future, always set Next Run Time within the bounds of Start Time and End Time, or use 'Schedule Now' to skip this check.",
				message);

			// Task Periods that do not have Daily End Time exposed
			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Daily;
			AssertEquals("End Time should be ignored as it's not exposed on the form", true, scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Weekly;
			AssertEquals("End Time should be ignored as it's not exposed on the form", true, scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Monthly;
			AssertEquals("End Time should be ignored as it's not exposed on the form", true, scheduleTask.IsValidTimeOfDayToRun(out message));

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Yearly;
			AssertEquals("End Time should be ignored as it's not exposed on the form", true, scheduleTask.IsValidTimeOfDayToRun(out message));
		}

		public void TestIsAutoLogged()
		{
			var scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			Assert(scheduleTask.IsAutoLoggedForTesting);
			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			{
				Assert(!scheduleTask.IsAutoLoggedForTesting);
			}
			Assert(scheduleTask.IsAutoLoggedForTesting);
		}

		public void TestSaveServiceStateWithIncorrectString_ShouldProduceError()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			scheduleTask.S5_ScheduleState = Encoding.ASCII.GetBytes("<xml></xml>");
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			var str = "0x505AB3A9C8CDB1B3D1079100";
			scheduleTask.S5_ScheduleState = Encoding.ASCII.GetBytes(str);
			var error = "The string that is being saved to S5_Schedule State is not XML Parsable." + System.Environment.NewLine + "It might be compressed." + System.Environment.NewLine + "String: " + str;
			AssertEquals(error, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestScheduleSchedulePeriodDuration()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();
			scheduleTask.SetStaticServiceAttributesDebugOnly(new HostedServiceAttribute());

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Second;
			scheduleTask.S5_TaskPeriodCount = 10;
			AssertEquals(TimeSpan.FromSeconds(10), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
			scheduleTask.S5_TaskPeriodCount = 5;
			AssertEquals(TimeSpan.FromMinutes(5), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Hourly;
			scheduleTask.S5_TaskPeriodCount = 3;
			AssertEquals(TimeSpan.FromHours(3), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Daily;
			scheduleTask.S5_TaskPeriodCount = 1;
			AssertEquals(TimeSpan.FromDays(1), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Weekly;
			scheduleTask.S5_TaskPeriodCount = 2;
			AssertEquals(TimeSpan.FromDays(14), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Monthly;
			scheduleTask.S5_TaskPeriodCount = 3;
			AssertEquals(TimeSpan.FromDays(84), scheduleTask.SchedulePeriodDuration);

			scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Yearly;
			scheduleTask.S5_TaskPeriodCount = 2;
			AssertEquals(TimeSpan.FromDays(365), scheduleTask.SchedulePeriodDuration);
		}

		public void TestSettings()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();

			AssertNull(scheduleTask.ServiceSettings.ConfigString);
			AssertEquals("", scheduleTask.ConfigString);
			AssertEquals(0, scheduleTask.ServiceSettings.SecondaryProcessesMaxCount);
			AssertEquals(0, scheduleTask.SecondaryProcessesMaxCount);

			scheduleTask.ConfigString = "SomeConfigurationString";
			scheduleTask.SecondaryProcessesMaxCount = 5;
			AssertEquals("SomeConfigurationString", scheduleTask.ServiceSettings.ConfigString);
			AssertEquals("SomeConfigurationString", scheduleTask.ConfigString);
			AssertEquals(5, scheduleTask.ServiceSettings.SecondaryProcessesMaxCount);
			AssertEquals(5, scheduleTask.SecondaryProcessesMaxCount);
		}

		public void TestSecondaryProcessesMaxCount()
		{
			var scheduleTask = Factory.New<ServiceTaskSchedule>();

			AssertNotNull(scheduleTask.ServiceSettings);
			AssertEquals(0, scheduleTask.SecondaryProcessesMaxCount);

			scheduleTask.SecondaryProcessesMaxCount = 5;

			AssertEquals(5, scheduleTask.ServiceSettings.SecondaryProcessesMaxCount);
			AssertEquals(5, scheduleTask.SecondaryProcessesMaxCount);

			var settings = scheduleTask.ServiceSettings;
			settings.SecondaryProcessesMaxCount = 0;
			scheduleTask.ServiceSettings = settings;

			AssertEquals(0, scheduleTask.ServiceSettings.SecondaryProcessesMaxCount);
			AssertEquals(0, scheduleTask.SecondaryProcessesMaxCount);
		}

		public void TestBindingsCountAndType()
		{
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null)
			})))
			{
				var scheduleTask = Factory.New<ServiceTaskSchedule>();
				scheduleTask.S5_ScheduleType = "111";

				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				AssertEquals(2, scheduleTask.ServiceTaskBindingsCount);
				AssertEquals("DummyBizo,DummyBizo2", scheduleTask.ServiceTaskBindingTypesString);
			}
		}

		public void TestServiceTaskSchedule_HavingBizOBindings_ServiceTaskBusinessObjectBindingEnabled_IsNudged()
		{
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null)
			})))
			{
				var scheduleTask = Factory.New<ServiceTaskSchedule>();
				scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Yearly;
				scheduleTask.S5_TaskPeriodCount = 99;

				scheduleTask.S5_ScheduleType = "111";

				var settings = new HostedServiceAttribute();
				settings.DefaultScheduleRunEvery = "99years";
				scheduleTask.SetStaticServiceAttributesDebugOnly(settings);

				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;

				try
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;
					Assert("ServiceTaskBusinessObjectBindingEnabled is true", ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled);

					AssertEquals(2, scheduleTask.ServiceTaskBindingsCount);
					AssertEquals("DummyBizo,DummyBizo2", scheduleTask.ServiceTaskBindingTypesString);

					Assert("Service task having business object bindings is nudge-able", scheduleTask.IsNudgeable);
					Assert("Service task is not yet in the database", !scheduleTask.IsInDatabase);
					AssertEquals("Nudge-able service task is scheduled on year during initialisation", ScheduleRecurrenceType.Yearly, scheduleTask.S5_TaskPeriod);
					AssertEquals("Nudge-able service task is scheduled on 99 interval during initialisation", 99, scheduleTask.S5_TaskPeriodCount);

					Factory.Save();

					Assert("Service task is in the database", scheduleTask.IsInDatabase);
					AssertEquals("Nudge-able service task is scheduled on year during initialisation", ScheduleRecurrenceType.Yearly, scheduleTask.S5_TaskPeriod);
					AssertEquals("Nudge-able service task is scheduled on 99 interval during initialisation", 99, scheduleTask.S5_TaskPeriodCount);

					settings = new HostedServiceAttribute();
					settings.DefaultScheduleRunEvery = "45seconds";
					scheduleTask.SetStaticServiceAttributesDebugOnly(settings);
					scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Second;
					scheduleTask.S5_TaskPeriodCount = 45;
					Factory.Save();

					AssertEquals("Nudge-able service task is scheduled on minute", ScheduleRecurrenceType.Minute, scheduleTask.S5_TaskPeriod);
					AssertEquals("Nudge-able service task is scheduled on 15 minutes interval", 15, scheduleTask.S5_TaskPeriodCount);

					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = false;
					var newFactory = new BusinessObjectFactory();
					var baseSchedule = newFactory.Load<StmScheduleTask>(scheduleTask.PK);

					AssertEquals("Base service task S5_TaskPeriod stays unchanged", ScheduleRecurrenceType.Second, baseSchedule.S5_TaskPeriod);
					AssertEquals("Base service task S5_TaskPeriodCount stays unchanged", 45, baseSchedule.S5_TaskPeriodCount);
				}
				finally
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
				}
			}
		}

		public void TestServiceTaskSchedule_HavingBizOBindings_ServiceTaskBusinessObjectBindingNotEnabled_NoNudging()
		{
			using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
				provider.BusinessObjectBindings == new[]
			{
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null),
				new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null)
			})))
			{
				var scheduleTask = Factory.New<ServiceTaskSchedule>();
				scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Yearly;
				scheduleTask.S5_TaskPeriodCount = 99;

				scheduleTask.S5_ScheduleType = "111";

				var schemaResolver = new Mock<IApplicationSchemaResolver>();
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
				schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

				var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;

				try
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = false;
					Assert("ServiceTaskBusinessObjectBindingEnabled is false", !ObjectFactory.Get<ISystemDataRegistry>().ServiceTaskBusinessObjectBindingEnabled);

					AssertEquals(2, scheduleTask.ServiceTaskBindingsCount);
					AssertEquals("DummyBizo,DummyBizo2", scheduleTask.ServiceTaskBindingTypesString);

					Assert("Service task having business object bindings is NOT nudge-able", !scheduleTask.IsNudgeable);
					AssertNotEquals("Service task S5_TaskPeriod stays the same", ScheduleRecurrenceType.Minute, scheduleTask.S5_TaskPeriod);
					AssertNotEquals("Service task S5_TaskPeriodCount stays the same", 15, scheduleTask.S5_TaskPeriodCount);

					Factory.Save();
					var baseSchedule = Factory.Load<ServiceTaskSchedule>(scheduleTask.PK);

					AssertEquals("Base service task S5_TaskPeriod stays unchanged", ScheduleRecurrenceType.Yearly, baseSchedule.S5_TaskPeriod);
					AssertEquals("Base service task S5_TaskPeriodCount stays unchanged", 99, baseSchedule.S5_TaskPeriodCount);
				}
				finally
				{
					SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
				}
			}
		}

		public void TestServiceTaskSchedule_HavingNudgedAttribute_NudgedAttributeReturnsTrue_IsNudged()
		{
			var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;

			try
			{
				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;
				using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeNudgeURL\\ServiceTaskNudging"))
				{
					var scheduleTask = Factory.New<ServiceTaskSchedule>();
					scheduleTask.S5_ScheduleType = "EHI";
					scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
					scheduleTask.S5_TaskPeriodCount = 1;
					Factory.Save();

					AssertEquals("Service task S5_TaskPeriod should be the nudging amount", ScheduleRecurrenceType.Minute, scheduleTask.S5_TaskPeriod);
					AssertEquals("Service task S5_TaskPeriodCount should be the nudging amount", 15, scheduleTask.S5_TaskPeriodCount);
				}
			}
			finally
			{
				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
			}
		}

		public void TestServiceTaskSchedule_HavingNudgedAttribute_NudgedAttributeReturnsFalse_NotNudged()
		{
			var serviceTaskBusinessObjectBindingEnabled = SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled;

			try
			{
				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = true;
				using (eHubMessagingRegistry.Instance.EHINudgeURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
				{
					var scheduleTask = Factory.New<ServiceTaskSchedule>();
					scheduleTask.S5_ScheduleType = "EHI";
					scheduleTask.S5_TaskPeriod = ScheduleRecurrenceType.Minute;
					scheduleTask.S5_TaskPeriodCount = 1;
					Factory.Save();

					AssertEquals("Base service task S5_TaskPeriod should stay as configured", ScheduleRecurrenceType.Minute, scheduleTask.S5_TaskPeriod);
					AssertEquals("Base service task S5_TaskPeriodCount should stay as configured", 1, scheduleTask.S5_TaskPeriodCount);
				}
			}
			finally
			{
				SystemDataRegistryForTest.Get().ServiceTaskBusinessObjectBindingEnabled = serviceTaskBusinessObjectBindingEnabled;
			}
		}

		public void TestCalculateNextRunTime_SetsNextRuntimeToNow()
		{
			// Arrange
			var logger = new Mock<ILogger>();

			var schedule = CreateTaskSchedule("AAA");
			schedule.S5_TaskPeriod = "H";
			schedule.S5_TaskPeriodCount = 1;
			var reference = ZDateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = reference;
			Factory.Save();

			//Act
			var result = schedule.CalculateNextRunTime(logger.Object);

			//Assert
			AssertEquals(reference.AddHours(1), result);
		}

		[TestDate(2015, 09, 23, 14, 00, 00)]
		public void TestCalculateNextRunTime_WhenCurrentIsEmpty()
		{
			// Arrange
			var logger = new Mock<ILogger>();

			var schedule = CreateTaskSchedule("AAA");
			schedule.S5_TaskPeriod = "H";
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			Factory.Save();

			//Act
			var expectedTime = ZDateTime.UtcNow.AddHours(1);
			var expectedNoteLog = "Next runtime was out of the expected range, so it was recalculated based off 'now'";
			var result = schedule.CalculateNextRunTime(logger.Object);

			//Assert
			AssertEquals(expectedTime, result);
			logger.VerifyLog(LogLevel.Debug, expectedNoteLog, Times.Once);
		}

		[TestDate(2015, 09, 23, 14, 00, 00)]
		public void TestSetNextRunTimeBasedOnRecurrenceShouldUpdateScheduledRuntimeIfInFutureBasedOnNow()
		{
			// Arrange
			var logger = new Mock<ILogger>();
			var schedule = CreateTaskSchedule("AAA");
			schedule.S5_TaskPeriod = "H";
			schedule.S5_TaskPeriodCount = 1;
			var reference = ZDateTime.UtcNow.AddYears(1);
			schedule.S5_NextScheduledPrintRunTimeUtc = reference;
			Factory.Save();

			//Act
			var expectedTime = ZDateTime.UtcNow.AddHours(1);
			var expectedNoteLog = "Next runtime was out of the expected range, so it was recalculated based off 'now'";
			var result = schedule.CalculateNextRunTime(logger.Object);

			//Assert
			AssertEquals(expectedTime, result);
			logger.VerifyLog(LogLevel.Debug, expectedNoteLog, Times.Once);
		}

		[TestDate(2015, 12, 11, 12, 00, 00)]
		public void TestSetNextRunTimeBasedOnRecurrence_ShoudCalculateBasedOnNowIfExpired()
		{
			// Arrange
			var logger = new Mock<ILogger>();

			var schedule = CreateTaskSchedule("AAA");
			schedule.S5_TaskPeriod = "H";
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.BrettsBirthday;
			Factory.Save();

			//Act
			var expectedTime = ZDateTime.UtcNow.AddHours(1);
			var expectedNoteLog = "Next runtime was out of the expected range, so it was recalculated based off 'now'";
			var result = schedule.CalculateNextRunTime(logger.Object);

			//Assert
			AssertEquals(expectedTime, result);
			logger.VerifyLog(LogLevel.Debug, expectedNoteLog, Times.Once);
		}

		public void TestSetNextRunTime_IsSentToHost()
		{
			var prevAppName = DbConnection.ApplicationName;
			try
			{
				DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
				var testStartTime = DateTime.UtcNow.AddHours(1);
				var schedule = CreateTaskSchedule("AAA");
				schedule.S5_TaskPeriod = "H";
				schedule.S5_TaskPeriodCount = 1;
				schedule.S5_NextScheduledPrintRunTimeUtc = testStartTime;
				Factory.Save();
				schedule.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("AAA"), Times.Never);
				schedule.StatusProviderForTest.Verify(sp => sp.SetServiceTaskNextRuntime("AAA", testStartTime, null), Times.Never);
				AssertEquals(testStartTime, schedule.S5_NextScheduledPrintRunTimeUtc);

				DbConnection.ApplicationName = "SomeOtherApp";
				schedule.S5_NextScheduledPrintRunTimeUtc = testStartTime.AddHours(5);
				Factory.Save();
				schedule.StatusProviderForTest.Verify(sp => sp.SetServiceTaskNextRuntime("AAA", testStartTime.AddHours(5), null), Times.Once());
				schedule.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("AAA"), Times.Once);
				AssertEquals(testStartTime.AddHours(5), schedule.S5_NextScheduledPrintRunTimeUtc);
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		public void TestRequestTaskConfigurationReload_IsSentToHost()
		{
			var prevAppName = DbConnection.ApplicationName;
			try
			{
				DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
				var testStartTime = ZDateTime.UtcNow.AddHours(1);
				var schedule = CreateTaskSchedule("AAA");
				schedule.S5_TaskPeriod = "H";
				schedule.S5_TaskPeriodCount = 1;
				Factory.Save();
				schedule.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("AAA"), Times.Never);
				AssertEquals(true, schedule.S5_IsActive);

				DbConnection.ApplicationName = null;
				schedule.S5_IsActive = false;
				Factory.Save();

				schedule.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("AAA"), Times.Once);
				AssertEquals(false, schedule.S5_IsActive);

				DbConnection.ApplicationName = "SomeOtherApp";
				schedule.S5_IsActive = true;
				Factory.Save();
				schedule.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("AAA"), Times.Exactly(2));
				AssertEquals(true, schedule.S5_IsActive);
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		ServiceTaskScheduleForTesting CreateTaskSchedule(string aaa)
		{
			var schedule = Factory.New<ServiceTaskScheduleForTesting>();
			schedule.S5_ScheduleType = aaa;
			schedule.S5_ScheduleDescription = "Desc 1";
			schedule.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now;
			schedule.S5_IsActive = true;
			return schedule;
		}

		[TestDate(2016, 2, 15, 12, 00, 00)]
		public void TestExpiredNextRunTime()
		{
			//Arrange
			var initialTime = new DateTime(2015, 06, 22, 14, 00, 00);

			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = initialTime;
			scheduleTask.S5_ScheduleType = "ABC";
			scheduleTask.S5_TaskPeriod = "M";
			scheduleTask.S5_TaskPeriodCount = 1;
			Factory.Save();

			//Act
			var result = scheduleTask.NextRunTimeExpired;

			//Assert
			AssertEquals(true, result);
		}

		[TestDate(2016, 2, 15, 12, 00, 00)]
		public void TestNextRunTimeInFuture()
		{
			//Arrange
			var initialTime = ZDateTime.UtcNow.AddYears(1);

			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = initialTime;
			scheduleTask.S5_ScheduleType = "ABC";
			scheduleTask.S5_TaskPeriod = "M";
			scheduleTask.S5_TaskPeriodCount = 1;
			Factory.Save();

			//Act
			var result = scheduleTask.NextRunTimeIsInFuture;

			//Assert
			AssertEquals(true, result);
		}

		public void TestUpdateStatusFromHostWithRedundantPeer()
		{
			UpdateStatus(DbConnectionConstants.ApplicationNames.ServiceHost, numberOfHosts: 2, expectUpdateToOccur: true);
		}

		public void TestUpdateStatusFromHostWithNoPeer()
		{
			UpdateStatus(DbConnectionConstants.ApplicationNames.ServiceHost, numberOfHosts: 1, expectUpdateToOccur: false);
		}

		public void TestUpdateStatusFromClient()
		{
			UpdateStatus(null, numberOfHosts: 1, expectUpdateToOccur: true);
		}

		public void UpdateStatus(string applicationName, int numberOfHosts, bool expectUpdateToOccur)
		{
			var prevAppName = DbConnection.ApplicationName;
			try
			{
				DbConnection.ApplicationName = applicationName;
				var testStartTime = DateTime.UtcNow.AddHours(1);
				var scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
				var taskCode = "TST";
				scheduleTask.S5_ScheduleType = taskCode;
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = testStartTime.AddHours(1);
				AssertEquals(testStartTime.AddHours(1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				Factory.Save();

				var serviceHostsCache = new Mock<IServiceHostsCache>();
				var serviceHosts = new List<IServiceHostClient>();
				for (var i = 0; i < numberOfHosts; i++)
				{
					var hostClient = new Mock<IServiceHostClient>();
					hostClient.Setup(hc => hc.HostName).Returns(new ServiceHostName("Host" + i.ToString()));
					serviceHosts.Add(hostClient.Object);
				}
				serviceHostsCache.Setup(sc => sc.ConfiguredServiceHosts).Returns(serviceHosts);
				scheduleTask.SetServiceHostsCache(serviceHostsCache.Object);

				var initialTaskStatus = new TaskInstanceStatus();
				initialTaskStatus.NextRunTime = testStartTime.AddHours(2);
				scheduleTask.StatusProviderForTest.Setup(sp => sp.GetServiceTaskStatus(taskCode)).Returns(initialTaskStatus);

				scheduleTask.UpdateStatus();
				if (expectUpdateToOccur)
				{
					scheduleTask.StatusProviderForTest.Verify(sp => sp.GetServiceTaskStatus(taskCode), Times.AtLeastOnce);
					AssertEquals(testStartTime.AddHours(2), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				}
				else
				{
					scheduleTask.StatusProviderForTest.Verify(sp => sp.GetServiceTaskStatus(taskCode), Times.Never);
					AssertEquals(testStartTime.AddHours(1), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				}
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		public void TestOnStatusUpdateComplete_InHost()
		{
			var prevAppName = DbConnection.ApplicationName;
			DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
			try
			{
				AssertOnStatusUpdateComplete();
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		public void TestOnStatusUpdateComplete_InClient()
		{
			AssertOnStatusUpdateComplete();
		}

		void AssertOnStatusUpdateComplete()
		{
			var inHost = (string.Equals(DbConnection.ApplicationName, DbConnectionConstants.ApplicationNames.ServiceHost, StringComparison.OrdinalIgnoreCase));
			var olderNextRuntime = new ZDateTime(2010, 3, 8);
			var newerNextRuntime = new ZDateTime(2020, 3, 8);
			var newerLastRuntime = new ZDateTime(2019, 8, 27);
			var newerLastErrorTime = new ZDateTime(2019, 1, 23);
			var olderLastErrorTime = new ZDateTime(2010, 12, 11);
			var webStatusNewerRunTime = new TaskInstanceStatus() { StatusString = "status newer", PlaceInQueueString = "place in queue newer", ProcessIDsString = "process id newer", RegisteredOnHosts = "registered on hosts newer", RunningCount = 2, SecondsInQueueString = "seconds in queue newer", SecondsRunningString = "seconds running newer", NextRunTime = newerNextRuntime.ToDateTime(), LastRunTime = newerLastRuntime.ToDateTime(), LastErrorTime = newerLastErrorTime.ToDateTime(), ErrorCountLast24Hours = 33 };
			var webStatusOlderRunTime = new TaskInstanceStatus() { StatusString = "status older", PlaceInQueueString = "place in queue older", ProcessIDsString = "process id older", RegisteredOnHosts = "registered on hosts older", RunningCount = 3, SecondsInQueueString = "seconds in queue older", SecondsRunningString = "seconds running older", NextRunTime = olderNextRuntime.ToDateTime(), LastRunTime = new DateTime(2011, 4, 5), LastErrorTime = olderLastErrorTime.ToDateTime(), ErrorCountLast24Hours = 45 };
			var initialTime = new ZDateTime(new DateTime(2017, 5, 5));
			ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
			scheduleTask.S5_NextScheduledPrintRunTimeUtc = initialTime;
			scheduleTask.S5_ScheduleType = "ABC";
			scheduleTask.S5_IsActive = false;
			Factory.Save();

			scheduleTask.OnStatusUpdateComplete(webStatusNewerRunTime);
			AssertEquals("status newer", scheduleTask.StatusString);
			AssertEquals("place in queue newer", scheduleTask.PlaceInQueueString);
			AssertEquals("process id newer", scheduleTask.ProcessIDsString);
			AssertEquals("registered on hosts newer", scheduleTask.RegisteredOnHosts);
			AssertEquals(2, scheduleTask.RunningCount);
			AssertEquals("seconds in queue newer", scheduleTask.SecondsInQueueString);
			AssertEquals("seconds running newer", scheduleTask.SecondsRunningString);
			AssertEquals(newerNextRuntime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(newerLastRuntime, scheduleTask.LastRunTime);

			// Error tracking should not replicate between service hosts, restart of hosts should clear these properties.
			AssertEquals(inHost ? ZDateTime.Empty : newerLastErrorTime, scheduleTask.LastErrorTime);
			AssertEquals(inHost ? 0 : 33, scheduleTask.ErrorCountLast24Hours);

			scheduleTask.OnStatusUpdateComplete(webStatusOlderRunTime);
			AssertEquals("status older", scheduleTask.StatusString);
			AssertEquals("place in queue older", scheduleTask.PlaceInQueueString);
			AssertEquals("process id older", scheduleTask.ProcessIDsString);
			AssertEquals("registered on hosts older", scheduleTask.RegisteredOnHosts);
			AssertEquals(3, scheduleTask.RunningCount);
			AssertEquals("seconds in queue older", scheduleTask.SecondsInQueueString);
			AssertEquals("seconds running older", scheduleTask.SecondsRunningString);
			AssertEquals(inHost ? newerNextRuntime : olderNextRuntime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(newerLastRuntime, scheduleTask.LastRunTime);

			// Error tracking should not replicate between service hosts, restart of hosts should clear these properties.
			AssertEquals(inHost ? ZDateTime.Empty : olderLastErrorTime, scheduleTask.LastErrorTime);
			AssertEquals(inHost ? 0 : 45, scheduleTask.ErrorCountLast24Hours);
		}

		public void TestOnStatusUpdateCompleteEmptyNextRuntime()
		{
			var prevAppName = DbConnection.ApplicationName;
			try
			{
				DbConnection.ApplicationName = DbConnectionConstants.ApplicationNames.ServiceHost;
				var newerNextRuntime = new ZDateTime(2020, 3, 8);
				var newerLastRuntime = new ZDateTime(2019, 8, 27);
				var webStatusNewerRunTime = new TaskInstanceStatus() { StatusString = "status newer", PlaceInQueueString = "place in queue newer", ProcessIDsString = "process id newer", RegisteredOnHosts = "registered on hosts newer", RunningCount = 2, SecondsInQueueString = "seconds in queue newer", SecondsRunningString = "seconds running newer", NextRunTime = newerNextRuntime.ToDateTime(), LastRunTime = newerLastRuntime.ToDateTime() };
				var webStatusEmptyRunTime = new TaskInstanceStatus() { StatusString = "status older", PlaceInQueueString = "place in queue older", ProcessIDsString = "process id older", RegisteredOnHosts = "registered on hosts older", RunningCount = 3, SecondsInQueueString = "seconds in queue older", SecondsRunningString = "seconds running older", NextRunTime = new DateTime?(), LastRunTime = new DateTime?() };
				var initialTime = new ZDateTime(new DateTime(2017, 5, 5));
				ServiceTaskSchedule scheduleTask = Factory.New<ServiceTaskScheduleForTesting>();
				scheduleTask.S5_NextScheduledPrintRunTimeUtc = initialTime;
				scheduleTask.S5_ScheduleType = "ABC";
				scheduleTask.S5_IsActive = false;
				Factory.Save();

				scheduleTask.OnStatusUpdateComplete(webStatusNewerRunTime);
				AssertEquals("status newer", scheduleTask.StatusString);
				AssertEquals("place in queue newer", scheduleTask.PlaceInQueueString);
				AssertEquals("process id newer", scheduleTask.ProcessIDsString);
				AssertEquals("registered on hosts newer", scheduleTask.RegisteredOnHosts);
				AssertEquals(2, scheduleTask.RunningCount);
				AssertEquals("seconds in queue newer", scheduleTask.SecondsInQueueString);
				AssertEquals("seconds running newer", scheduleTask.SecondsRunningString);
				AssertEquals(newerNextRuntime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				AssertEquals(newerLastRuntime, scheduleTask.LastRunTime);

				scheduleTask.OnStatusUpdateComplete(webStatusEmptyRunTime);
				AssertEquals("status older", scheduleTask.StatusString);
				AssertEquals("place in queue older", scheduleTask.PlaceInQueueString);
				AssertEquals("process id older", scheduleTask.ProcessIDsString);
				AssertEquals("registered on hosts older", scheduleTask.RegisteredOnHosts);
				AssertEquals(3, scheduleTask.RunningCount);
				AssertEquals("seconds in queue older", scheduleTask.SecondsInQueueString);
				AssertEquals("seconds running older", scheduleTask.SecondsRunningString);
				AssertEquals(newerNextRuntime, scheduleTask.S5_NextScheduledPrintRunTimeUtc);
				AssertEquals(newerLastRuntime, scheduleTask.LastRunTime);
			}
			finally
			{
				DbConnection.ApplicationName = prevAppName;
			}
		}

		public void TestUIBoundGetterInvocationDoesNotModifyObjectState()
		{
			// Arrange
			var taskScheduleStatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
			taskScheduleStatusProviderMock.Setup(provider => provider.GetServiceTaskStatus(It.IsAny<string>()))
				.Returns(new TaskInstanceStatus
				{
					StatusString = "Failed",
					PlaceInQueueString = "88",
					ProcessIDsString = "proc2",
					RunningCount = 5,
					SecondsRunningString = "23",
					RegisteredOnHosts = "localhost",
					LastRunTime = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
					NextRunTime = DateTime.UtcNow.AddHours(2)
				});

			using (ObjectFactory.Substitute(taskScheduleStatusProviderMock.Object))
			{
				var schedule = Factory.New<ServiceTaskSchedule>();
				schedule.S5_ScheduleType = "TST";
				schedule.S5_ScheduleDescription = "Desc 1";
				schedule.S5_IsActive = true;
				schedule.S5_TaskPeriod = "H";
				schedule.S5_TaskPeriodCount = 1;

				schedule.StatusString = "Running";
				schedule.PlaceInQueueString = "A";
				schedule.ProcessIDsString = "B";
				schedule.RunningCount = 2;
				schedule.SecondsRunningString = "15";
				schedule.RegisteredOnHosts = "host1";
				schedule.LastRunTime = ZDateTime.BrettsBirthday;

				var publicGetters = schedule.GetType()
					.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
					.Where(property => typeof(IZType).IsAssignableFrom(property.PropertyType));

				//Act
				publicGetters.ForEach(property => property.GetValue(schedule));

				//Assert
				CombineAssertions(() =>
				{
					AssertEquals("Running", schedule.StatusString);
					AssertEquals("A", schedule.PlaceInQueueString);
					AssertEquals("B", schedule.ProcessIDsString);
					AssertEquals(2, schedule.RunningCount);
					AssertEquals("15", schedule.SecondsRunningString);
					AssertEquals("host1", schedule.RegisteredOnHosts);
					AssertEquals(ZDateTime.BrettsBirthday, schedule.LastRunTime);
					AssertEquals(ZDateTime.Empty, schedule.S5_NextScheduledPrintRunTimeUtc);
					AssertEquals(false, schedule.NextRunTimeHasBeenSet);

					taskScheduleStatusProviderMock.Verify(provider => provider.GetServiceTaskStatus("TST"), Times.Never);
				});
			}
		}

		public class ServiceTaskScheduleForTesting : ServiceTaskSchedule
		{
			readonly Mock<IServiceTaskScheduleStatusProvider> StatusProviderMock;
			public ServiceTaskScheduleForTesting(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				StatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
				StatusProvider = StatusProviderMock.Object;
			}

			public void SetServiceHostsCache(IServiceHostsCache serviceHostsCache)
			{
				ServiceHostsCache = serviceHostsCache;
			}

			public Mock<IServiceTaskScheduleStatusProvider> StatusProviderForTest { get { return StatusProviderMock; } }

			public bool IsAutoLoggedForTesting
			{
				get { return IsAutoLogged; }
			}
		}

		public class ServiceTaskScheduleUiExtensionsTest : TestCaseWithFactory
		{
			public void TestIsScheduleReadOnly()
			{
				// Arrange
				var taskSchedule = Factory.New<ServiceTaskSchedule>();
				var settings = new HostedServiceAttribute
				{
					ConfigControlTypeAssemblyName = "A",
					ConfigControlTypeName = "B",
				};
				taskSchedule.SetStaticServiceAttributesDebugOnly(settings);

				void Test(bool expected, bool isScheduleReadOnly, bool isReadonlyForWiseCloud, bool isHostedWithCargoWise, bool isSupport)
				{
					settings.IsScheduleReadOnly = isScheduleReadOnly;
					settings.IsReadOnlyForWiseCloudClient = isReadonlyForWiseCloud;

					using (SetHostedLocation())
					using (EnvProxy.Instance.SetTemporaryUserContext(isSupport ? User.SupportUserName : User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						// Act
						var result = taskSchedule.IsScheduleReadOnly;

						// Assert
						AssertEquals(expected, result);
					}

					IDisposable SetHostedLocation()
					{
						var originalHostedLocation = EnvProxy.HostedLocation;
						EnvProxy.SetHostedLocationForTest(isHostedWithCargoWise ? "SYD" : "NCW");
						return new DisposableAction(() => EnvProxy.SetHostedLocationForTest(originalHostedLocation));
					}
				}

				CombineAssertions(() =>
				{
					Test(true, true, false, true, false);
					Test(true, false, true, true, false);
					Test(false, false, true, true, true);
					Test(false, false, true, false, false);
					Test(false, false, true, false, true);
					Test(false, false, false, true, false);
					Test(false, false, false, false, false);
				});
			}

			public void TestNextRunTime_ReadOnly()
			{
				// Arrange
				var taskSchedule = Factory.New<ServiceTaskSchedule>();
				var settings = new HostedServiceAttribute
				{
					ConfigControlTypeAssemblyName = "A",
					ConfigControlTypeName = "B",
				};
				taskSchedule.SetStaticServiceAttributesDebugOnly(settings);

				void Test(bool expected, bool isScheduleReadOnly, bool isReadonlyForWiseCloud, bool isHostedWithCargoWise, bool isSupport)
				{
					settings.IsScheduleReadOnly = isScheduleReadOnly;
					settings.IsReadOnlyForWiseCloudClient = isReadonlyForWiseCloud;

					using (SetHostedLocation())
					using (EnvProxy.Instance.SetTemporaryUserContext(isSupport ? User.SupportUserName : User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						// Act
						var result = taskSchedule.NextRunTime_ReadOnly;

						// Assert
						AssertEquals(expected, result);
					}

					IDisposable SetHostedLocation()
					{
						var originalHostedLocation = EnvProxy.HostedLocation;
						EnvProxy.SetHostedLocationForTest(isHostedWithCargoWise ? "SYD" : "NCW");
						return new DisposableAction(() => EnvProxy.SetHostedLocationForTest(originalHostedLocation));
					}
				}

				CombineAssertions(() =>
				{
					Test(true, true, false, true, false);
					Test(true, false, true, true, false);
					Test(false, false, true, true, true);
					Test(false, false, true, false, false);
					Test(false, false, true, false, true);
					Test(false, false, false, true, false);
					Test(false, false, false, false, false);
				});
			}

			public void TestS5_IsActive_ReadOnly()
			{
				// Arrange
				var taskSchedule = Factory.New<ServiceTaskSchedule>();
				var settings = new HostedServiceAttribute
				{
					ConfigControlTypeAssemblyName = "A",
					ConfigControlTypeName = "B",
				};
				taskSchedule.SetStaticServiceAttributesDebugOnly(settings);

				void Test(bool expected, bool isMandatory, bool isScheduleReadOnly, bool isReadonlyForWiseCloud, bool isHostedWithCargoWise, bool isSupport)
				{
					settings.IsMandatory = isMandatory;
					settings.IsScheduleReadOnly = isScheduleReadOnly;
					settings.IsReadOnlyForWiseCloudClient = isReadonlyForWiseCloud;

					using (SetHostedLocation())
					using (EnvProxy.Instance.SetTemporaryUserContext(isSupport ? User.SupportUserName : User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
					{
						// Act
						var result = taskSchedule.S5_IsActive_ReadOnly;

						// Assert
						AssertEquals(expected, result);
					}

					IDisposable SetHostedLocation()
					{
						var originalHostedLocation = EnvProxy.HostedLocation;
						EnvProxy.SetHostedLocationForTest(isHostedWithCargoWise ? "SYD" : "NCW");
						return new DisposableAction(() => EnvProxy.SetHostedLocationForTest(originalHostedLocation));
					}
				}

				CombineAssertions(() =>
				{
					Test(true, true, false, false, true, false);
					Test(true, false, true, false, true, false);
					Test(true, false, false, true, true, false);
					Test(false, false, false, true, true, true);
					Test(false, false, false, true, false, false);
					Test(false, false, false, true, false, true);
					Test(false, false, false, false, true, false);
					Test(false, false, false, false, false, false);
				});
			}
		}
	}
}
