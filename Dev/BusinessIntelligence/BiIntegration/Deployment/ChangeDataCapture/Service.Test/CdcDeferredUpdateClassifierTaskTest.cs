using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ChangeDataCapture.Service.Testing
{
	[TestedType(typeof(CdcDeferredUpdateClassifierTask))]
	class CdcDeferredUpdateClassifierTaskTest : ServiceTaskTestCase<CdcDeferredUpdateClassifierTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new CdcDeferredUpdateClassifierTask(Db.NewAdminConnection(), new TestServiceLogger());
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

		public void TestRunAsServiceTask()
		{
			using (var testConnection = Db.NewAdminConnection())
			{
				var logger = new TestServiceLogger();
				var testTask = new CdcDeferredUpdateClassifierTask(Db.NewAdminConnection(), logger);

				testTask.isHostedWithCargoWise = false;

				testTask.RunTask(new System.Threading.CancellationToken());
				var log = logger.ToString();
				AssertContains("Debug|Logic hasn't been implemented yet", log);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
