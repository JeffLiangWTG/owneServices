using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.Bi.Product.ServiceTask.Maintenance.Testing
{
	[TestedType(typeof(BiMaintenanceTask))]
	class BiMaintenanceTaskTransactionedTest : ServiceTaskTestCase<BiMaintenanceTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new BiMaintenanceTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			Assert("TaskPeriod", taskSchedule.Recurrence.DaysRange);
			AssertEquals("TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", false, taskSchedule.Recurrence.RecurringStartTimeUtc.IsEmpty);
			AssertEquals("DailyStartTime", "01:00", taskSchedule.Recurrence.RecurringStartTimeLocalText);
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(BiMaintenanceTask).GetMethod(nameof(BiMaintenanceTask.CheckCanLoadDataWarehouseServer));
			Assert("HostedServiceRequirement for EDW DB is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));

			var hostedServiceRequirementAttributedMethods = typeof(BiMaintenanceTask).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
						.Where(m => m.IsDefined(typeof(HostedServiceRequirementAttribute), inherit: false));

			AssertEquals("Only one hosted Service Requirement should exist", 1, hostedServiceRequirementAttributedMethods.Count());
		}

		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public void TestMaintenanceNudgeTimeLastIndexRebuildDateSet()
		{
			var dbName = Db.EdwDatabaseName;

			using (var biConnection = Db.NewAdminConnection(dbName))
			{
				var logger = new LoggerForTest();
				var task = new BiMaintenanceTask();
				task.ServiceLogger = logger;

				BiMasterState.DeleteParameter(biConnection, BiConstants.BimNudgeTimeParamName);
				BiMasterState.SetParameter(biConnection, BiConstants.LastIndexRebuildUtcDt, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
				task.RunTask();

				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("BIM_NUDGE_TIME is not set. Skipping Business Intelligence Maintenance."));
			}
		}

		[UseSnapshotProtection(Db.EdwDatabaseSuffix)]
		public void TestMaintenanceNudgeTimeEmptyLastIndexRebuildDate()
		{
			var dbName = Db.EdwDatabaseName;

			using (var biConnection = Db.NewAdminConnection(dbName))
			{
				var logger = new LoggerForTest();
				var task = new BiMaintenanceTask();
				task.ServiceLogger = logger;

				BiMasterState.DeleteParameter(biConnection, BiConstants.BimNudgeTimeParamName);
				BiMasterState.DeleteParameter(biConnection, BiConstants.LastIndexRebuildUtcDt);
				task.RunTask();

				Assert(string.Join("\r\n", logger.LogEntries), logger.LogEntries.Contains("Index has not been rebuilt. Running Business Intelligence Maintenance."));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
