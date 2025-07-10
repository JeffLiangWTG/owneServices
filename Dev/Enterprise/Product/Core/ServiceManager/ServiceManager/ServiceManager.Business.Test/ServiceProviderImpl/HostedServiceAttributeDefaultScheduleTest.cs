using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Scheduler.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.ServiceManager.Runner.Testing.BasicServiceProviderTest;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace Enterprise.ServiceManager.Business.Testing
{
	class HostedServiceAttributeDefaultScheduleTest : TestCaseWithFactory
	{
		public void TestHostedServiceAttributeDefaultSchedulePropertiesAppliedToTask()
		{
			CombineAssertions(() =>
			{
				Test("20seconds", 20, ScheduleRecurrenceType.Second, null, null);
				Test("15minutes", 15, ScheduleRecurrenceType.Minute, null, null);
				Test("2hours", 2, ScheduleRecurrenceType.Hourly, null, null);
				Test("1days", 1, ScheduleRecurrenceType.Daily, null, null);
				Test("2weeks", 2, ScheduleRecurrenceType.Weekly, null, DayOfWeek.Sunday);
				Test("1month", 1, ScheduleRecurrenceType.Monthly, 1, null);
				Test("1year", 1, ScheduleRecurrenceType.Yearly, null, null);
			});

			void Test(string scheduleOccurence, int scheduleCount, string scheduleType, int? dayOfMonth, params DayOfWeek[] daysOfWeek)
			{
				// Arrange
				var taskCode = "###";
				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = scheduleOccurence,
					DefaultScheduleDayOfMonth = dayOfMonth ?? 0,
					DefaultScheduleDaysOfWeek = daysOfWeek
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();

				var taskBizO = LoadTask(taskCode);

				AssertEquals(scheduleCount, taskBizO.S5_TaskPeriodCount);
				AssertEquals(scheduleType, taskBizO.S5_TaskPeriod);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		public void TestDefaultsApplied()
		{
			// Arrange
			var taskCode = "###";
			var testDate = new DateTime(2024, 07, 23, 12, 30, 0, DateTimeKind.Utc);
			var dateTimeMock = new Mock<IDateTimeProvider>();
			dateTimeMock.Setup(d => d.CurrentDateTimeUtc).Returns(testDate);

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
			{
				DefaultScheduleRunEvery = "15minutes"
			};

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);

			var scheduleConfigurer = new DefaultScheduleConfigurer(dateTimeMock.Object);

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

			// Assert
			adapter.Commit();

			var taskBizO = LoadTask(taskCode);

			Assert(!taskBizO.S5_IsActive);
			AssertEquals(testDate, taskBizO.S5_NextScheduledPrintRunTimeUtc.ToDateTime());
			AssertEquals(new ZDateTime(testDate), taskBizO.S5_NextScheduledPrintRunTimeUtc);
			AssertEquals(new ZDateTime(testDate), taskBizO.S5_StartDate);
		}

		public void TestRandomStartOffsetLocal()
		{
			CombineAssertions(() =>
			{
				Test("1hour", TimeSpan.FromHours(1), "30minutes", TimeSpan.FromMinutes(30));
				Test("15minutes", TimeSpan.FromMinutes(15), "225minutes", TimeSpan.FromMinutes(225));
				Test("5hour", TimeSpan.FromHours(5), "12hours", TimeSpan.FromHours(12));
				Test("16hour", TimeSpan.FromHours(16), "30minutes", TimeSpan.FromMinutes(30));
				Test("200minutes", TimeSpan.FromMinutes(200), "225minutes", TimeSpan.FromMinutes(225));
				Test("0seconds", TimeSpan.Zero, "24hours", TimeSpan.FromHours(24));
			});

			void Test(string startAt, TimeSpan expectedStart, string randomStartOffset, TimeSpan offsetTimespan)
			{
				// Arrange
				var taskCode = "###";
				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "15minutes",
					DefaultScheduleStartAtLocal = startAt,
					DefaultScheduleRandomStartOffset = randomStartOffset,
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();
				var taskBizO = LoadTask(taskCode);

				AssertGreaterThanOrEqualTo(taskBizO.S5_DailyStartTime.TimeOfDay, expectedStart);
				AssertLessThanOrEqualTo(taskBizO.S5_DailyStartTime.TimeOfDay, expectedStart + offsetTimespan);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		public void TestRandomStartOffsetUtc()
		{
			CombineAssertions(() =>
			{
				Test("1hour", TimeSpan.FromHours(1), "30minutes", TimeSpan.FromMinutes(30));
				Test("15minutes", TimeSpan.FromMinutes(15), "225minutes", TimeSpan.FromMinutes(225));
				Test("5hour", TimeSpan.FromHours(5), "12hours", TimeSpan.FromHours(12));
				Test("16hour", TimeSpan.FromHours(16), "30minutes", TimeSpan.FromMinutes(30));
				Test("200minutes", TimeSpan.FromMinutes(200), "225minutes", TimeSpan.FromMinutes(225));
				Test("0seconds", TimeSpan.Zero, "24hours", TimeSpan.FromHours(24));
			});

			void Test(string startAt, TimeSpan expectedStart, string randomStartOffset, TimeSpan offsetTimespan)
			{
				// Arrange
				var taskCode = "###";
				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "15minutes",
					DefaultScheduleStartAtUtc = startAt,
					DefaultScheduleRandomStartOffset = randomStartOffset,
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();
				var taskBizO = LoadTask(taskCode);

				AssertGreaterThanOrEqualTo(taskBizO.CalcDailyStartTimeUtc.TimeOfDay, expectedStart);
				AssertLessThanOrEqualTo(taskBizO.CalcDailyStartTimeUtc.TimeOfDay, expectedStart + offsetTimespan);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		public void TestDoNotRunTillNextDueTimeIfOverdue()
		{
			CombineAssertions(() =>
			{
				Test("", 0);
				Test("50seconds", 50);
				Test("2hours", (int)TimeSpan.FromHours(2).TotalSeconds);
				Test("1day", (int)TimeSpan.FromDays(1).TotalSeconds);
			});

			void Test(string overdueDuration, int expectedValue)
			{
				// Arrange
				var taskCode = "~TA";

				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(ProviderForTesting))
				{
					DefaultScheduleRunEvery = "15seconds",
					DefaultScheduleDoNotRunTillNextDueTimeIfOverdue = overdueDuration,
					ActiveByDefault = true,
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();
				var task1 = LoadTask(taskCode);

				AssertEquals(expectedValue, task1.S5_OverdueDurationInSeconds);

				task1.Delete();
				Factory.Save();
			}
		}

		public void TestWeekly()
		{
			// Arrange
			var taskCode = "###";

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
			{
				DefaultScheduleRunEvery = "2weeks",
				DefaultScheduleDaysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }
			};

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);

			var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

			// Assert
			adapter.Commit();
			var taskBizO = LoadTask(taskCode);

			AssertEquals(2, taskBizO.S5_TaskPeriodCount);
			AssertEquals(ScheduleRecurrenceType.Weekly, taskBizO.S5_TaskPeriod);
			Assert(!taskBizO.Recurrence.Monday);
			Assert(!taskBizO.Recurrence.Tuesday);
			Assert(!taskBizO.Recurrence.Wednesday);
			Assert(!taskBizO.Recurrence.Thursday);
			Assert(!taskBizO.Recurrence.Friday);
			Assert(taskBizO.Recurrence.Saturday);
			Assert(taskBizO.Recurrence.Sunday);
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestWeeklyNoDays()
		{
			// Arrange
			var taskCode = "###";

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
			{
				DefaultScheduleRunEvery = "2weeks"
			};

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);

			var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);
		}

		public void TestMonthly()
		{
			CombineAssertions(
				() =>
				{
					Enumerable.Range(1, 28).ForEach(i => Test(i));
				});

			void Test(int dayOfMonth)
			{
				// Arrange
				var taskCode = "###";

				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "2months",
					DefaultScheduleDayOfMonth = dayOfMonth
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();
				var taskBizO = LoadTask(taskCode);

				AssertEquals(2, taskBizO.S5_TaskPeriodCount);
				AssertEquals(dayOfMonth, taskBizO.Recurrence.DayOfMonth);
				AssertEquals(ScheduleRecurrenceType.Monthly, taskBizO.S5_TaskPeriod);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		public void TestMonthlyInvalidOptions()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Cannot set monthly schedule when provided an invalid day", typeof(InvalidOperationException), () => Test(-1));
				AssertExceptionThrown("Cannot set monthly schedule for day that exceeds the shortest month", typeof(InvalidOperationException), () => Test(29));
			});

			void Test(int dayOfMonth)
			{
				// Arrange
				var taskCode = "###";

				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "1month",
					DefaultScheduleDayOfMonth = dayOfMonth
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);
			}
		}

		public void TestStartAtEndAt()
		{
			// Arrange
			var taskCode = "###";

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
			{
				DefaultScheduleRunEvery = "1day",
				DefaultScheduleStartAtLocal = "9hours",
				DefaultScheduleEndAtLocal = "17hours"
			};

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);

			var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

			// Assert
			adapter.Commit();
			var taskBizO = LoadTask(taskCode);

			AssertEquals(9, taskBizO.S5_DailyStartTime.TimeOfDay.Hours);
			AssertEquals(17, taskBizO.S5_DailyEndTime.TimeOfDay.Hours);
		}

		public void TestSetNextRunTime()
		{
			var testDate = new DateTime(2008, 6, 10, 12, 30, 0);

			CombineAssertions(() =>
			{
				Test("10hours", "", new ZDateTime(2008, 6, 10, 10, 0, 0));
				Test("13hours", "", new ZDateTime(2008, 6, 10, 13, 0, 0));
				Test("10hours", "13hours", new ZDateTime(2008, 6, 10, 10, 0, 0));
				Test("8hours", "11hours", new ZDateTime(2008, 6, 10, 8, 0, 0));
				Test("14hours", "17hours", new ZDateTime(2008, 6, 10, 14, 0, 0));
				Test("20hours", "10hours", new ZDateTime(2008, 6, 10, 20, 0, 0));
				Test("20hours", "13hours", new ZDateTime(2008, 6, 10, 12, 30, 0));
			});

			void Test(string defaultStartAtLocal, string defaultEndAtLocal, ZDateTime expectedNextRunTimeUtc)
			{
				// Arrange
				var taskCode = "###";

				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "15minutes",
					DefaultScheduleStartAtLocal = defaultStartAtLocal,
					DefaultScheduleEndAtLocal = defaultEndAtLocal,
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var currentTimeMock = new Mock<IDateTimeProvider>();
				currentTimeMock
					.Setup(c => c.CurrentDateTimeUtc)
					.Returns(testDate);

				var scheduleConfigurer = new DefaultScheduleConfigurer(currentTimeMock.Object);

				// Act
				scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

				// Assert
				adapter.Commit();
				var taskBizO = LoadTask(taskCode);

				AssertEquals(expectedNextRunTimeUtc, taskBizO.S5_NextScheduledPrintRunTimeUtc);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		public void TestActiveByDefault()
		{
			// Arrange
			var taskCode = "###";

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
			{
				ActiveByDefault = true,
			};

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);
			var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

			// Assert
			adapter.Commit();
			var taskBizO = LoadTask(taskCode);

			Assert(taskBizO.S5_IsActive);
		}

		public void TestNotActiveByDefault()
		{
			// Arrange
			var taskCode = "###";

			var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object));

			using var adapter = new SchedulerServiceTaskTransactionAdapter();
			var governor = adapter.GetNewServiceTaskGovernor(config);
			var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

			// Act
			scheduleConfigurer.SetDefaultScheduleForTask(governor, config);

			// Assert
			adapter.Commit();
			var taskBizO = LoadTask(taskCode);

			Assert(!taskBizO.S5_IsActive);
		}

		public void TestDefaultScheduleForDisplay()
		{
			CombineAssertions(() =>
			{
				Test("1hour", TimeSpan.FromHours(1), "30minutes");
				Test("15minutes", TimeSpan.FromMinutes(15), "225minutes");
				Test("5hour", TimeSpan.FromHours(5), "12hours");
				Test("16hour", TimeSpan.FromHours(16), "30minutes");
				Test("200minutes", TimeSpan.FromMinutes(200), "225minutes");
				Test("0seconds", TimeSpan.Zero, "24hours");
			});

			void Test(string startAt, TimeSpan expectedStart, string randomStartOffset)
			{
				// Arrange
				var taskCode = "###";

				var config = new HostedServiceAttribute(taskCode, "#DESC#", "TST", typeof(object))
				{
					DefaultScheduleRunEvery = "15minutes",
					DefaultScheduleStartAtUtc = startAt,
					DefaultScheduleRandomStartOffset = randomStartOffset,
				};

				using var adapter = new SchedulerServiceTaskTransactionAdapter();
				var governor = adapter.GetNewServiceTaskGovernor(config);

				var scheduleConfigurer = new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider());

				// Act
				scheduleConfigurer.SetDefaultScheduleForTaskForDisplay(governor, config);

				// Assert
				adapter.Commit();
				var taskBizO = LoadTask(taskCode);

				AssertEquals(expectedStart, taskBizO.CalcDailyStartTimeUtc.TimeOfDay);

				taskBizO.Delete();
				Factory.Save();
			}
		}

		ServiceTaskSchedule LoadTask(string code)
		{
			var query = new ZQuery();
			query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, StmServiceHostSchema.Constants.Prefix);
			query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleType, code);
			return Factory.LoadTop1<ServiceTaskSchedule>(query);
		}
	}
}
