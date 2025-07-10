using System;
using System.Collections.Generic;
using CargoWise.Application;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks.Test
{
	[TestedType(typeof(NctsLiabilityUpdaterServiceTask))]
	sealed class NctsLiabilityUpdaterServiceTaskTest : ServiceTaskTestCase<NctsLiabilityUpdaterServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestDisableServiceTaskAfterSuccessfulRun()
		{
			var governorMock = new Mock<IServiceManagerGovernor>();

			using (ObjectFactory.Substitute(governorMock.Object))
			{
				InitialiseAndRunTaskSchedule(new NctsLiabilityUpdaterServiceTask());

				AssertNoExceptionThrown(() =>
				{
					governorMock.Verify(g => g.SetServiceTaskIsActive(NctsLiabilityUpdaterServiceTask.Code, false), Times.Once, "Service task only need to run once");
				});
			}
		}
	}
}
