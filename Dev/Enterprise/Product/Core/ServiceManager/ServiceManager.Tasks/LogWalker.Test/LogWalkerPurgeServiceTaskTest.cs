using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.LogWalker.Test
{
	[TestedType(typeof(LogWalkerPurgeServiceTask))]
	class LogWalkerPurgeServiceTaskTest : ServiceTaskTestCase<LogWalkerPurgeServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
