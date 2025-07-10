using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	[TestedType(typeof(CaptureTask))]
	class CaptureTaskTest : ServiceTaskTestCase<CaptureTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new CaptureTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.SecondsRange);
			AssertEquals("TaskPeriodCount", 30, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		}

		public void TestAllowMultipleInstances()
		{
			var thisClassAttribute = GetHostedServiceAttributes().SingleOrDefault();
			if (thisClassAttribute != null)
			{
				AssertEquals("AllowsMultipleInstances", false, thisClassAttribute.AllowsMultipleInstances);
			}
		}

		public void TestEnableTraceFlagLog()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var logger = new TestServiceLogger();
				var testTask = new CaptureTaskForTest(logger);

				testTask.isHostedWithCargoWise = false;

				testTask.RunTask();
				var log = logger.ToString();
				AssertContains("Error|This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.", log);
			}
		}

		public void TestHostedServiceException()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var logger = new TestServiceLogger();
				var testTask = new CaptureTaskForTest(logger);

				testTask.isHostedWithCargoWise = true;

				var expectedMessage = "This database does not permit CDC Allow Alter Meta Data - Please enable TF15006. Please search for Technical Advisory Notes for more information.";
				AssertExceptionThrown<HostedServiceException>("HostedServiceException should be thrown when trace flag needs to be enabled", expectedMessage, () => testTask.RunTask());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
