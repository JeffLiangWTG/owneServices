using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Client.EDI.BorderWise;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.BorderWise
{
	[TestedType(typeof(BorderWiseDirectSyncServiceTask))]
	class BorderWiseDirectSyncServiceTaskTest : ServiceTaskTestCase<BorderWiseDirectSyncServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("15Minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestDefaultScheduleAttribute()
		{
			var attribute = GetHostedServiceAttributes().Single();

			CombineAssertions(() =>
				{
					AssertEquals("1day", attribute.DefaultScheduleRunEvery);
					AssertEquals("7hours", attribute.DefaultScheduleStartAtLocal);
				});
		}

		public void TestRunTask_WhenConnectionStringNotSet_ShouldLogWarning()
		{
			var serviceTask = new BorderWiseDirectSyncServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertMultilineASCIIEquals("Warning|No connection string has been specified in registry item UMP Database Connection String.", logger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
