using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(StmServiceTask))]
	class StmServiceTaskTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanGetBranchFromForeignKey()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();

			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.SST_GB_Branch = branch.PK;
			var query = new ZQuery(task.PKSchemaColumn, task.PK);

			var retrievedTask = Factory.LoadTop1<StmServiceTask>(query);
			AssertEquals(branch, retrievedTask.Branch);
		}

		public void TestForNobranchOnServiceTask()
		{
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			Factory.Save();

			var query = new ZQuery(task.PKSchemaColumn, task.PK);

			var retrievedTask = Factory.LoadTop1<StmServiceTask>(query);
			AssertEquals(retrievedTask.SST_GB_Branch, ZGuid.Empty);
		}

		public void TestFileBasedLogViewer()
		{
			// Arrange
			var serviceTaskSchedule = Factory.New<StmServiceTask>();

			// Act
			serviceTaskSchedule.SST_ServiceTaskCode = "111";

			// Assert
			AssertNotNull(serviceTaskSchedule.LogViewer.FileBasedLogViewer);
			AssertEquals(serviceTaskSchedule.LogViewer.FileBasedLogViewer.TaskType, serviceTaskSchedule.SST_ServiceTaskCode);
			Assert(serviceTaskSchedule.LogViewer.FileBasedLogViewer.TaskTypeReadOnly);
		}

		public void TestSearchBasedLogViewer()
		{
			// Arrange
			var serviceTaskSchedule = Factory.New<StmServiceTask>();

			// Act
			serviceTaskSchedule.SST_ServiceTaskCode = "111";

			// Assert
			AssertNotNull(serviceTaskSchedule.LogViewer.SearchBasedLogViewer);
			AssertEquals(serviceTaskSchedule.LogViewer.SearchBasedLogViewer.ServiceTaskCode, serviceTaskSchedule.SST_ServiceTaskCode);
		}

		[TestDate(2024, 11, 1, 11, 0, 0)]
		public void TestUpdatingNextRunTimeCalculatorUpdatesNextRunTime()
		{
			var originalNextRunTime = new ZDateTimeOffset(2024, 11, 1, 11, 30, 0);
			CombineAssertions(() =>
			{
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorSeconds() { Period = 10 }, new NextRunTimeCalculatorMinutes() { Period = 10 }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 1, 11, 10, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorSeconds() { Period = 10 }, new NextRunTimeCalculatorMinutes() { Period = 10 }, new ZDateTimeOffset(2024, 11, 1, 11, 30, 0), new ZDateTimeOffset(2024, 11, 1, 11, 10, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMinutes() { Period = 10 }, new NextRunTimeCalculatorHours() { Period = 1 }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 1, 12, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMinutes() { Period = 10 }, new NextRunTimeCalculatorHours() { Period = 1 }, new ZDateTimeOffset(2024, 11, 1, 13, 0, 0), new ZDateTimeOffset(2024, 11, 1, 12, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorHours() { Period = 1 }, new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = TimeSpan.FromHours(7) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 2, 7, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorHours() { Period = 1 }, new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = TimeSpan.FromHours(7) }, new ZDateTimeOffset(2024, 11, 1, 12, 0, 0), new ZDateTimeOffset(2024, 11, 2, 7, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorDays() { Period = 1 }, new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new[] { DayOfWeek.Monday }, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorDays() { Period = 1 }, new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new[] { DayOfWeek.Monday }, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 7, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new[] { DayOfWeek.Tuesday }, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = TimeSpan.FromHours(7) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 7, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new[] { DayOfWeek.Tuesday }, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = TimeSpan.FromHours(7) }, new ZDateTimeOffset(2024, 11, 5, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 7, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorWorkingDays(), new NextRunTimeCalculatorMonthsByDate() { DayOfOccurrence = 2, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 2, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorWorkingDays(), new NextRunTimeCalculatorMonthsByDate() { DayOfOccurrence = 2, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 5, 11, 0, 0), new ZDateTimeOffset(2024, 11, 2, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByDate() { DayOfOccurrence = 2, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByDate() { DayOfOccurrence = 2, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = TimeSpan.FromHours(11) }, new ZDateTimeOffset(2024, 11, 5, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 11, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorMonthsByLastDay() { ScheduledRunTime = TimeSpan.FromHours(1), Period = 1 }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 30, 1, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = TimeSpan.FromHours(11) }, new NextRunTimeCalculatorMonthsByLastDay() { ScheduledRunTime = TimeSpan.FromHours(1), Period = 1 }, new ZDateTimeOffset(2024, 11, 5, 11, 0, 0), new ZDateTimeOffset(2024, 11, 30, 1, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1 }, new NextRunTimeCalculatorYearsByDate() { Day = 10, Month = 11, ScheduledRunTime = TimeSpan.FromHours(10) }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 10, 10, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorMonthsByLastDay() { Period = 1 }, new NextRunTimeCalculatorYearsByDate() { Day = 10, Month = 11, ScheduledRunTime = TimeSpan.FromHours(10) }, new ZDateTimeOffset(2024, 11, 5, 11, 0, 0), new ZDateTimeOffset(2024, 11, 10, 10, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorYearsByDate() { Day = 10, Month = 11, ScheduledRunTime = TimeSpan.FromHours(10) }, new NextRunTimeCalculatorYearsByDayOfMonth() { DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 11, ScheduledRunTime = TimeSpan.FromHours(10), WeekOfTheMonth = 1 }, new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 10, 0, 0));
				AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(new NextRunTimeCalculatorYearsByDate() { Day = 10, Month = 11, ScheduledRunTime = TimeSpan.FromHours(10) }, new NextRunTimeCalculatorYearsByDayOfMonth() { DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 11, ScheduledRunTime = TimeSpan.FromHours(10), WeekOfTheMonth = 1 }, new ZDateTimeOffset(2024, 11, 3, 11, 0, 0), new ZDateTimeOffset(2024, 11, 4, 10, 0, 0));
			});
		}

		void AssertUpdatingNextRunTimeCalculatorUpdatesNextRunTime(INextRunTimeCalculator originalCalculator, INextRunTimeCalculator newCalculator, ZDateTimeOffset originalNextRunTime, ZDateTimeOffset expectedNextRunTime)
		{
			// Arrange
			var task = Factory.NewWithValidTestData<StmServiceTask>();
			task.NextRunTimeCalculator = originalCalculator;
			task.SST_NextRunTime = originalNextRunTime;

			// Act
			task.NextRunTimeCalculator = newCalculator;

			// Assert
			AssertEquals($"Failed conversion from {originalCalculator.GetType()} to {newCalculator.GetType()}", expectedNextRunTime, task.SST_NextRunTime);
		}

		public void TestUpdatingActiveFlagRecalculatesNextRunTime()
		{
			Test(new ZDateTimeOffset(2023, 11, 1, 11, 0, 0), new NextRunTimeCalculatorSeconds() { Period = 1 });
			Test(new ZDateTimeOffset(2024, 10, 1, 11, 0, 0), new NextRunTimeCalculatorMinutes() { Period = 1 });
			Test(new ZDateTimeOffset(2025, 11, 3, 11, 0, 0), new NextRunTimeCalculatorHours() { Period = 1 });
			Test(new ZDateTimeOffset(2024, 11, 1, 11, 1, 0), new NextRunTimeCalculatorDays() { Period = 1 });
			Test(new ZDateTimeOffset(2023, 11, 1, 11, 0, 0), new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new[] { DayOfWeek.Tuesday }, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) });
			Test(new ZDateTimeOffset(2024, 11, 7, 11, 0, 0), new NextRunTimeCalculatorWorkingDays());
			Test(new ZDateTimeOffset(2024, 11, 1, 12, 0, 1), new NextRunTimeCalculatorMonthsByDate() { DayOfOccurrence = 2, Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) });
			Test(new ZDateTimeOffset(2025, 11, 1, 11, 0, 0), new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = TimeSpan.FromHours(11) });
			Test(new ZDateTimeOffset(2024, 11, 4, 11, 0, 0), new NextRunTimeCalculatorMonthsByLastDay() { Period = 1 });
			Test(new ZDateTimeOffset(2024, 11, 1, 11, 0, 0), new NextRunTimeCalculatorYearsByDate() { Day = 10, Month = 11, ScheduledRunTime = TimeSpan.FromHours(10) });

			void Test(ZDateTimeOffset nextRunTime, INextRunTimeCalculator calculator)
			{
				// Arrange
				var task = Factory.NewWithValidTestData<StmServiceTask>();
				task.SST_Active = false;
				task.NextRunTimeCalculator = calculator;
				task.SST_NextRunTime = nextRunTime;
				AssertEquals("Pre-condition", nextRunTime, task.SST_NextRunTime);

				// Act
				task.SST_Active = true;

				// Assert
				AssertNotEquals(nextRunTime, task.SST_NextRunTime);
			}
		}

		[TestDate(2025, 01, 01, 12, 0, 0)]
		public void TestSetSST_ActiveFromFalseToTrue_WithPeriodExceedingMaxValue_DoestNotThrow()
		{
			// Arrange
			var serviceTask = Factory.New<StmServiceTask>();
			const string expectedErrorMessage = "Period Count is out of bounds for associated TimeSpan";

			serviceTask.SST_Active = false;
			serviceTask.NextRunTimeCalculator = new NextRunTimeCalculatorWeeks
			{
				Period = 99999999,
				DaysOfOccurrence = [DayOfWeek.Wednesday],
				ScheduledRunTime = TimeSpan.FromHours(9)
			};

			// Act & Assert
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() =>
				{
					serviceTask.SST_Active = true;
				});
				AssertEquals(true, serviceTask.Recurrence.HasErrors);
				Assert(serviceTask.Recurrence.Notifications.Any(n => n.Message.Contains(expectedErrorMessage)));
			});
		}

		public void TestNoErrorsUsingLogViewerWithFileLoggingEnabledAndInvalidElasticConfiguration()
		{
			// Arrange
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.FSL, true);

			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (SystemDataRegistry.Instance.ElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var task = Factory.NewWithValidTestData<StmServiceTask>();

				// Act
				_ = task.LogViewer;

				// Assert
				var errors = UnitTestUserNotification.Instance.PreviousMessages.Where(x => !x.ToString().Trim().Equals("None")).ToList();
				AssertEquals($"There are errors: \n{string.Join("\n", errors)}", 0, errors.Count);
			}
		}

		[TestDate(2025, 1, 1)]
		[ExpectNoExceptions]
		public void TestNextRunTimeUpdatedAndThenSaveSendsToHost()
		{
			// Arrange
			var task = Factory.NewWithValidTestData<StmServiceTaskForTest>();
			task.SST_ServiceTaskCode = "TS1";
			var nextRunTime = task.SST_NextRunTime;

			// Act
			task.NextRunTime = new ZDateTime(2026, 1, 1);
			Factory.Save();

			// Assert
			AssertNotEquals(nextRunTime, task.SST_NextRunTime);
			task.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("TS1"), Times.Once);
			task.StatusProviderForTest.Verify(sp => sp.SetServiceTaskNextRuntime("TS1", It.IsAny<DateTime>(), null), Times.Once);
		}

		[TestDate(2025, 1, 1)]
		[ExpectNoExceptions]
		public void TestNextRunTimeNotUpdatedAndThenSaveDoesNotSendToHost()
		{
			// Arrange
			var task = Factory.NewWithValidTestData<StmServiceTaskForTest>();
			task.SST_ServiceTaskCode = "TS1";
			var nextRunTime = task.SST_NextRunTime;

			// Act
			Factory.Save();

			// Assert
			AssertEquals(nextRunTime, task.SST_NextRunTime);
			task.StatusProviderForTest.Verify(sp => sp.RequestTaskConfigurationReload("TS1"), Times.Once);
			task.StatusProviderForTest.Verify(sp => sp.SetServiceTaskNextRuntime("TS1", It.IsAny<DateTime>(), null), Times.Never);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
				a.DefaultSchedule == Mock.Of<IDefaultSchedule>());
			var hostedServiceProviderMock = Mock.Of<IClientHostedServiceAttributeProvider>(a =>
				a.GetClientHostedServiceAttribute(It.IsAny<string>()) == hostedServiceMock);
			hostedServiceAttributeProviderDisposable = ObjectFactory.Substitute(hostedServiceProviderMock);
		}

		protected override void TearDown()
		{
			hostedServiceAttributeProviderDisposable?.Dispose();
			base.TearDown();
		}

		IDisposable hostedServiceAttributeProviderDisposable;

		public class StmServiceTaskUiExtensionsTest : TestCaseWithFactory
		{
			public void TestIsScheduleReadOnly()
			{
				// Arrange
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

				void Test(bool expected, bool isScheduleReadOnly, bool isReadonlyForWiseCloud, bool isHostedWithCargoWise, bool isSupport)
				{
					using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(isScheduleReadOnly: isScheduleReadOnly, isReadyOnlyForWiseCloudClient: isReadonlyForWiseCloud).Object))
					{
						var taskSchedule = Factory.New<StmServiceTask>();

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
				}
			}

			public void TestIsNudgeable()
			{
				using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
				using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
							provider.BusinessObjectBindings == new[] { new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo", Array.Empty<string>(), null), new HostedServiceBusinessObjectBindingAttribute("222", "DummyBizo", Array.Empty<string>(), null), new HostedServiceBusinessObjectBindingAttribute("333", "DummyBizo", Array.Empty<string>(), null), new HostedServiceBusinessObjectBindingAttribute("111", "DummyBizo2", Array.Empty<string>(), null) })))
				{
					var scheduleTask = Factory.New<StmServiceTask>();
					scheduleTask.SST_ServiceTaskCode = "111";

					var schemaResolver = new Mock<IApplicationSchemaResolver>();
					schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
					schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

					AssertEquals(2, scheduleTask.ServiceTaskBindingsCount);
					AssertEquals("DummyBizo,DummyBizo2", scheduleTask.ServiceTaskBindingTypesString);
					Assert("Service task having business object bindings is nudge-able", scheduleTask.IsNudgeable);
				}
			}

			public void TestIsNotNudgeable()
			{
				using (SystemDataRegistry.Instance.ServiceTaskBusinessObjectBindingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
				using (ObjectFactory.Substitute(Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(provider =>
							provider.BusinessObjectBindings == Array.Empty<IHostedServiceBusinessObjectBinding>())))
				{
					var scheduleTask = Factory.New<StmServiceTask>();
					scheduleTask.SST_ServiceTaskCode = "111";

					var schemaResolver = new Mock<IApplicationSchemaResolver>();
					schemaResolver.Setup(x => x.GetTableSchema("DummyBizo")).Returns(DummyBizoSchema.Instance);
					schemaResolver.Setup(x => x.GetTableSchema("DummyBizo2")).Returns(DummyBizoSchema.Instance);

					AssertEquals(0, scheduleTask.ServiceTaskBindingsCount);
					AssertEquals(string.Empty, scheduleTask.ServiceTaskBindingTypesString);
					AssertEquals("Service task having business object bindings is nudge-able", false, scheduleTask.IsNudgeable);
				}
			}

			public void TestSST_Active_ReadOnly()
			{
				// Arrange
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

				void Test(bool expected, bool isMandatory, bool isScheduleReadOnly, bool isReadonlyForWiseCloud, bool isHostedWithCargoWise, bool isSupport)
				{
					using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(isMandatory: isMandatory, isScheduleReadOnly: isScheduleReadOnly, isReadyOnlyForWiseCloudClient: isReadonlyForWiseCloud).Object))
					{
						var taskSchedule = Factory.New<StmServiceTask>();

						using (SetHostedLocation())
						using (EnvProxy.Instance.SetTemporaryUserContext(isSupport ? User.SupportUserName : User.ServiceUserName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
						{
							// Act
							var result = taskSchedule.SST_Active_ReadOnly;

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
				}
			}

			[TestDate(2024, 1, 1, 11, 0, 0)]
			public void TestNextRunTimeEstimator()
			{
				CombineAssertions(() =>
				{
					Test(new NextRunTimeCalculatorSeconds { Period = 1 }, ZDateTime.UtcNow.AddSeconds(1), ZDateTime.UtcNow.AddSeconds(2));
					Test(new NextRunTimeCalculatorMinutes { Period = 1 }, ZDateTime.UtcNow.AddMinutes(1), ZDateTime.UtcNow.AddMinutes(2));
					Test(new NextRunTimeCalculatorHours { Period = 1 }, ZDateTime.UtcNow.AddHours(1), ZDateTime.UtcNow.AddHours(2));
					Test(new NextRunTimeCalculatorDays { Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(1), ZDateTime.UtcNow.AddDays(2));
					Test(new NextRunTimeCalculatorWeeks { Period = 1, DaysOfOccurrence = new [] { DayOfWeek.Monday }, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(7), ZDateTime.UtcNow.AddDays(14));
					Test(new NextRunTimeCalculatorMonthsByDate { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(31), ZDateTime.UtcNow.AddDays(60));
					Test(new NextRunTimeCalculatorMonthsByDayOfWeek { DayOfTheWeek = DayOfWeek.Monday, WeekOfTheMonth = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(35), ZDateTime.UtcNow.AddDays(63));
					Test(new NextRunTimeCalculatorMonthsByLastDay { Period = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(30), ZDateTime.UtcNow.AddDays(59));
					Test(new NextRunTimeCalculatorYearsByDate() { Day = 1, Month = 1, ScheduledRunTime = TimeSpan.FromHours(11) }, ZDateTime.UtcNow.AddDays(366), ZDateTime.UtcNow.AddDays(731));
				});

				void Test(INextRunTimeCalculator calculator, ZDateTime expectedFirstDate, ZDateTime expectedSecondDate)
				{
					using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
					{
						var task = Factory.NewWithValidTestData<StmServiceTask>();
						task.NextRunTimeCalculator = calculator;
						task.SST_NextRunTime = ZDateTimeOffset.UtcNow;

						// Act
						var estimator = task.NextRunTimeEstimator;

						// Assert
						AssertEquals(20, estimator.NextRunTimeList.Count);
						AssertEquals(calculator.GetType().Name + " First date", expectedFirstDate, estimator.NextRunTimeList[0].NextRunTime);
						AssertEquals(calculator.GetType().Name + " Second date", expectedSecondDate, estimator.NextRunTimeList[1].NextRunTime);
					}
				}
			}

			public void TestDefaultsOnLoad_SecondaryProcessesMaxCount()
			{
				var testCases = new List<int>
				{
					0,
					1,
					16,
					99,
					100
				};

				CombineAssertions(() =>
				{
					testCases.ForEach((testCase) =>
					{
						using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(allowsMultipleInstances: true).Object))
						{
							// Arrange
							var task = Factory.NewWithValidTestData<StmServiceTask>();
							var test = task.SecondaryProcessesMaxCount;
							task.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds { Period = 1 };
							task.SecondaryProcessesMaxCount = testCase;

							Factory.Save();

							// Act
							var loadedTask = new BusinessObjectFactory().Load<StmServiceTask>(task.PK);

							// Assert
							AssertEquals(testCase, loadedTask.SecondaryProcessesMaxCount);
						}
					});
				});
			}

			public void TestDefaultsOnLoad_SecondaryProcessesMaxCountDescription()
			{
				var testCases = new List<(int inputValue, string expectedResult)>
				{
					(0, "0"),
					(1, "1"),
					(16, "16"),
					(99, "99"),
					(100, "SYSTEM MANAGED")
				};

				CombineAssertions(() =>
				{
					testCases.ForEach((testCase) =>
					{
						using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(allowsMultipleInstances: true).Object))
						{
							// Arrange
							var task = Factory.NewWithValidTestData<StmServiceTask>();
							task.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds { Period = 1 };
							task.SecondaryProcessesMaxCount = testCase.inputValue;

							Factory.Save();

							// Act
							var loadedTask = new BusinessObjectFactory().Load<StmServiceTask>(task.PK);

							// Assert
							AssertEquals(testCase.expectedResult, loadedTask.SecondaryProcessesMaxCountDescription);
						}
					});
				});
			}

			public void TestDefaultsOnLoad_ConfigString()
			{
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
				{
					// Arrange
					var testConfigString = "<config>TESTING</config>";
					var task = Factory.NewWithValidTestData<StmServiceTask>();
					task.NextRunTimeCalculator = new NextRunTimeCalculatorSeconds { Period = 1 };
					task.ConfigString = testConfigString;

					Factory.Save();

					// Act
					var loadedTask = new BusinessObjectFactory().Load<StmServiceTask>(task.PK);

					// Assert
					AssertEquals(testConfigString, loadedTask.ConfigString);
				}
			}

			public void TestIsNotAutoLogged()
			{
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTaskForTest>();

					// Assert
					Assert(!task.IsAutoLoggedForTesting);
				}
			}

			public void TestNoAuditLogsGeneratedForStmServiceTask()
			{
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider().Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();
					task.SST_Active = false;
					task.NextRunTime = ZDateTime.UtcNow.AddDays(1);
					Factory.Save();

					// Act
					task.SST_Active = true;
					task.NextRunTime = ZDateTime.UtcNow.AddDays(2);
					Factory.Save();

					// Assert
					var query = new ZQuery(StmALogSchema.SL_Parent, task.PK);
					var logs = Factory.Load<StmALog>(query);
					AssertEquals("No audit logs should be generated for schedule task", 0, logs.Length);
				}
			}

			public void TestDefaultSchedule_Seconds()
			{
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(defaultScheduleRunEvery: "30seconds").Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorSeconds>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.SecondsRange);
						AssertEquals(30, task.DefaultSchedule.Period);
					});
				}
			}

			public void TestDefaultSchedule_Minutes()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "25minutes",
						defaultScheduleStartAtUtc: "2hours",
						defaultScheduleEndAtUtc: "5hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorMinutes>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.MinutesRange);
						AssertEquals(25, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromHours(2), task.DefaultSchedule.CalcDailyStartTimeUtc.TimeOfDay);
						AssertEquals(TimeSpan.FromHours(5), task.DefaultSchedule.CalcDailyEndTimeUtc.TimeOfDay);
					});
				}
			}

			public void TestDefaultSchedule_Hours()
			{
				using (ObjectFactory.Substitute(GetMockServiceTaskAttributeProvider(defaultScheduleRunEvery: "10hours").Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorHours>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.HoursRange);
						AssertEquals(10, task.DefaultSchedule.Period);
					});
				}
			}

			public void TestDefaultSchedule_Days()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "5days",
						defaultScheduleStartAtUtc: "11hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorDays>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.DaysRange);
						AssertEquals(5, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromHours(11), task.DefaultSchedule.RecurringStartTimeUtc.TimeOfDay);
					});
				}
			}

			public void TestDefaultSchedule_Weeks()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "3weeks",
						defaultScheduleDaysOfWeek: [DayOfWeek.Monday, DayOfWeek.Tuesday],
						defaultScheduleStartAtUtc: "185minutes")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorWeeks>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.WeeksRange);
						AssertEquals(3, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromMinutes(185), task.DefaultSchedule.RecurringStartTimeUtc.TimeOfDay);
					});
				}
			}

			public void TestDefaultSchedule_Months()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "2months",
						defaultScheduleDayOfMonth: 28,
						defaultScheduleStartAtUtc: "19hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorMonthsByDate>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.MonthsRange);
						AssertEquals(2, task.DefaultSchedule.Period);
						AssertEquals(28, task.DefaultSchedule.DayOfMonth);
						AssertEquals(TimeSpan.FromHours(19), task.DefaultSchedule.RecurringStartTimeUtc.TimeOfDay);
					});
				}
			}

			public void TestDefaultSchedule_Years()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "1year",
						defaultScheduleStartAtUtc: "23hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorYearsByDate>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.YearsRange);
						AssertEquals(1, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromHours(23), task.DefaultSchedule.RecurringStartTimeUtc.TimeOfDay);
					});
				}
			}

			public void TestDefaultSchedule_LocalStartEndTime()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "25minutes",
						defaultScheduleStartAtLocal: "2hours",
						defaultScheduleEndAtLocal: "5hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorMinutes>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.MinutesRange);
						AssertEquals(25, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromHours(2).ToString(@"hh\:mm"), task.DefaultSchedule.CalcDailyStartTimeLocalText);
						AssertEquals(TimeSpan.FromHours(5).ToString(@"hh\:mm"), task.DefaultSchedule.CalcDailyEndTimeLocalText);
					});
				}
			}

			public void TestDefaultSchedule_LocalRecurringTime()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "5days",
						defaultScheduleStartAtLocal: "12hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorDays>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.DaysRange);
						AssertEquals(5, task.DefaultSchedule.Period);
						AssertEquals(TimeSpan.FromHours(12).ToString(@"hh\:mm"), task.DefaultSchedule.RecurringStartTimeLocalText);
					});
				}
			}

			public void TestDefaultSchedule_RandomOffset()
			{
				using (ObjectFactory.Substitute(
					GetMockServiceTaskAttributeProvider(
						defaultScheduleRunEvery: "5days",
						defaultScheduleStartAtUtc: "1hours",
						defaultScheduleRandomStartOffset: "3hours")
					.Object))
				{
					// Arrange
					var task = Factory.NewWithValidTestData<StmServiceTask>();

					// Act
					// Assert
					CombineAssertions(() =>
					{
						AssertType<NextRunTimeCalculatorDays>(task.DefaultSchedule.TaskNextRunTimeCalculator);
						Assert(task.DefaultSchedule.DaysRange);
						AssertEquals(5, task.DefaultSchedule.Period);
						AssertGreaterThanOrEqualTo(task.DefaultSchedule.RecurringStartTimeUtc, TimeSpan.FromHours(1));
						AssertLessThanOrEqualTo(task.DefaultSchedule.RecurringStartTimeUtc, TimeSpan.FromHours(4));
					});
				}
			}

			Mock<IClientHostedServiceAttributeProvider> GetMockServiceTaskAttributeProvider(
				bool isMandatory = false,
				bool isScheduleReadOnly = false,
				bool isReadyOnlyForWiseCloudClient = false,
				bool allowsMultipleInstances = false,
				string defaultScheduleRunEvery = "15minutes",
				DayOfWeek[] defaultScheduleDaysOfWeek = null,
				int defaultScheduleDayOfMonth = 0,
				string defaultScheduleStartAtUtc = "",
				string defaultScheduleEndAtUtc = "",
				string defaultScheduleStartAtLocal = "",
				string defaultScheduleEndAtLocal = "",
				string defaultScheduleRandomStartOffset = "")
			{
				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.ConfigControlTypeAssemblyName == "A" &&
					a.ConfigControlTypeName == "B" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == Mock.Of<IDefaultSchedule>(d =>
						d.RunEvery == defaultScheduleRunEvery &&
						d.DaysOfWeek == defaultScheduleDaysOfWeek &&
						d.DayOfMonth == defaultScheduleDayOfMonth &&
						d.StartAtUtc == defaultScheduleStartAtUtc &&
						d.EndAtUtc == defaultScheduleEndAtUtc &&
						d.StartAtLocal == defaultScheduleStartAtLocal &&
						d.EndAtLocal == defaultScheduleEndAtLocal &&
						d.RandomStartOffset == defaultScheduleRandomStartOffset
						) &&
					a.IsMandatory == isMandatory &&
					a.IsScheduleReadOnly == isScheduleReadOnly &&
					a.IsReadOnlyForWiseCloudClient == isReadyOnlyForWiseCloudClient &&
					a.AllowsMultipleInstances == allowsMultipleInstances);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				return hostedServiceProviderMock;
			}
		}

		public class ScheduleConfigTest : TestCaseWithFactory
		{
			public void TestCalculatorsCanBeSavedToDatabase()
			{
				// Arrange
				var testTime = new TimeSpan(6, 30, 0);
				CombineAssertions(() =>
				{
					TestFunction(new NextRunTimeCalculatorSeconds() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorMinutes() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorHours() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorDays() { Period = 1, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorWorkingDays() { ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Wednesday }, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 1, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 10, DayOfOccurrence = 31, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = 20, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Sunday, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 4, DayOfTheWeek = DayOfWeek.Saturday, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Day = 1, Month = 1, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Day = 31, Month = 12, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { DayOfTheWeek = DayOfWeek.Sunday, WeekOfTheMonth = 1, MonthOfTheYear = 1, ScheduledRunTime = testTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { DayOfTheWeek = DayOfWeek.Saturday, WeekOfTheMonth = 4, MonthOfTheYear = 12, ScheduledRunTime = testTime });
				});

				void TestFunction(INextRunTimeCalculator nextRunTimeCalculator)
				{
					var task = GetServiceTask();

					// Act
					task.NextRunTimeCalculator = nextRunTimeCalculator;

					// Assert
					AssertNoExceptionThrown(() => Factory.Save());
					task.Delete();
				}
			}

			[UseSnapshotProtection(skipTransaction: true)]
			public void TestInvalidCalculatorsAreRejectedFromDatabase()
			{
				// Arrange
				var sampleTime = new TimeSpan(6, 30, 0);
				CombineAssertions(() =>
				{
					TestFunction(new NextRunTimeCalculatorSeconds());
					TestFunction(new NextRunTimeCalculatorSeconds() { Period = 0 });
					TestFunction(new NextRunTimeCalculatorSeconds() { Period = -1 });
					TestFunction(new NextRunTimeCalculatorSeconds() { Period = -10 });
					TestFunction(new NextRunTimeCalculatorMinutes());
					TestFunction(new NextRunTimeCalculatorMinutes() { Period = 0 });
					TestFunction(new NextRunTimeCalculatorMinutes() { Period = -1 });
					TestFunction(new NextRunTimeCalculatorMinutes() { Period = -10 });
					TestFunction(new NextRunTimeCalculatorHours());
					TestFunction(new NextRunTimeCalculatorHours() { Period = 0 });
					TestFunction(new NextRunTimeCalculatorHours() { Period = -1 });
					TestFunction(new NextRunTimeCalculatorHours() { Period = -10 });
					TestFunction(new NextRunTimeCalculatorDays());
					TestFunction(new NextRunTimeCalculatorDays() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorDays() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorDays() { Period = 0, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorDays() { Period = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorDays() { Period = -10, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks());
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorWeeks() { DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday } });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = 0, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday }, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = -1, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday }, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorWeeks() { Period = -10, DaysOfOccurrence = new DayOfWeek[] { DayOfWeek.Monday }, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate());
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1 });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByLastDay() { Period = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 0, DayOfOccurrence = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 10, DayOfOccurrence = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 0, DayOfOccurrence = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = -1, DayOfOccurrence = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = -10, DayOfOccurrence = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 0, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = -10, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDate() { Period = 1, DayOfOccurrence = 32, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek());
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 1 });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { DayOfTheWeek = DayOfWeek.Monday });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = -1, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = -10, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorMonthsByDayOfWeek() { WeekOfTheMonth = 5, DayOfTheWeek = DayOfWeek.Monday, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate());
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1 });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Day = 1 });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 0, Day = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = -1, Day = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 13, Day = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 0, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDate() { Month = 1, Day = 32, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth());
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1 });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { DayOfTheWeek = DayOfWeek.Monday });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { MonthOfTheYear = 1 });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 0, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = -1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 54, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 0, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = -1, ScheduledRunTime = sampleTime });
					TestFunction(new NextRunTimeCalculatorYearsByDayOfMonth() { WeekOfTheMonth = 1, DayOfTheWeek = DayOfWeek.Monday, MonthOfTheYear = 13, ScheduledRunTime = sampleTime });
				});

				void TestFunction(INextRunTimeCalculator nextRunTimeCalculator)
				{
					var task = GetServiceTask();

					// Act
					task.NextRunTimeCalculator = nextRunTimeCalculator;
					task.SST_ServiceTaskCode = "TS1";

					// Assert
					AssertExceptionThrown<ZSaveException>(() => Factory.Save());
				}
			}

			public void TestSavedCalculatorsCanBeReceivedFromDatabase()
			{
				// Arrange
				CombineAssertions(() =>
				{
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorSeconds Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorHours Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"12:00:00\"><DaysOfOccurrence><DayOfWeek>Monday</DayOfWeek><DayOfWeek>Wednesday</DayOfWeek><DayOfWeek>Friday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"2\" ScheduledRunTime=\"10:00:00\"><DaysOfOccurrence><DayOfWeek>Tuesday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"4\" ScheduledRunTime=\"14:00:00\"><DaysOfOccurrence><DayOfWeek>Thursday</DayOfWeek><DayOfWeek>Sunday</DayOfWeek></DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=\"12:00:00\" /></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"1\" DayOfOccurrence=\"1\" ScheduledRunTime=\"12:00:00\"></NextRunTimeCalculatorMonthsByDate></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"3\" DayOfOccurrence=\"10\" ScheduledRunTime=\"10:00:00\"></NextRunTimeCalculatorMonthsByDate></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=\"4\" DayOfOccurrence=\"31\" ScheduledRunTime=\"13:00:00\"></NextRunTimeCalculatorMonthsByDate></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=\"10\" ScheduledRunTime=\"13:00:00\"></NextRunTimeCalculatorMonthsByLastDay></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"12\" Day=\"31\" ScheduledRunTime=\"12:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"1\" Day=\"1\" ScheduledRunTime=\"10:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=\"5\" Day=\"10\" ScheduledRunTime=\"16:00:20\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"3\" DayOfTheWeek=\"Monday\" MonthOfTheYear=\"10\" ScheduledRunTime=\"12:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"1\" DayOfTheWeek=\"Sunday\" MonthOfTheYear=\"3\" ScheduledRunTime=\"04:30:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth WeekOfTheMonth=\"2\" DayOfTheWeek=\"Tuesday\" MonthOfTheYear=\"8\" ScheduledRunTime=\"16:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/><SecondaryProcessesMaxCount>1</SecondaryProcessesMaxCount></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/><ConfigString>1,2,3</ConfigString></ScheduleConfig>");
				});

				void TestFunction(string configuration)
				{
					var task = GetServiceTask();

					// Act
					task.SST_Configuration = configuration;
					Factory.Save();

					// Assert
					AssertNoExceptionThrown(() =>
					{
						_ = Factory.LoadTop1<StmServiceTask>(new ZQuery(task.PKSchemaColumn, task.PK));
					});
					task.Delete();
				}
			}

			public void TestInvalidScheduleConfigIsRejectedFromDB()
			{
				// Arrange
				CombineAssertions(() => {
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"12:00:00\"/><NextRunTimeCalculatorSeconds Period=\"2\" StartTime=\"10:00:00\" EndTime=\"15:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorDays Period=\"1\" ScheduledRunTime=\"23:59:59\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><NextRunTimeCalculatorWeeks Period=\"1\" ScheduledRunTime=\"10:00:00\"/></ScheduleConfig>");
					TestFunction("<ScheduleConfig><SecondaryProcessesMaxCount>999</SecondaryProcessesMaxCount></ScheduleConfig>");
					TestFunction("<ScheduleConfig><ConfigString/></ScheduleConfig>");
				});

				void TestFunction(string scheduleConfig)
				{
					var task = GetServiceTask();

					// Act
					task.SST_Configuration = scheduleConfig;

					// Assert
					var exception = AssertExceptionThrown<ZSaveException>(() => Factory.Save());
				}
			}

			StmServiceTask GetServiceTask()
			{
				var defaultScheduleMock = Mock.Of<IDefaultSchedule>(d =>
					d.RunEvery == "15minutes");

				var hostedServiceMock = Mock.Of<IHostedServiceAttribute>(a =>
					a.Description == "some description" &&
					a.Category == "some category" &&
					a.ConfigControlTypeAssemblyName == "A" &&
					a.ConfigControlTypeName == "B" &&
					a.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					a.DefaultSchedule == defaultScheduleMock);

				var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceMock);

				using (ObjectFactory.Substitute(hostedServiceProviderMock.Object))
				{
					var task = Factory.NewWithValidTestData<StmServiceTask>();
					task.SST_ServiceTaskCode = "TS1";

					return task;
				}
			}
		}

		public class StmServiceTaskForTest : StmServiceTask
		{
			readonly Mock<IServiceTaskScheduleStatusProvider> StatusProviderMock;

			public StmServiceTaskForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
				StatusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
				StatusProvider = StatusProviderMock.Object;
			}

			public bool IsAutoLoggedForTesting
			{
				get { return IsAutoLogged; }
			}

			public Mock<IServiceTaskScheduleStatusProvider> StatusProviderForTest { get { return StatusProviderMock; } }
		}
	}
}
