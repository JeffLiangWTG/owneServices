using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.StabilityChecker.ServiceTasks.Testing
{
	[TestedType(typeof(StabilityCheckerServiceTask))]
	sealed class StabilityCheckerServiceTaskTest : ServiceTaskTestCase<StabilityCheckerServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
