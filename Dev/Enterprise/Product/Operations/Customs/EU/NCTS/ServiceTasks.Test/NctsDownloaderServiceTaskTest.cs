using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks.Test
{
	[TestedType(typeof(NctsDownloaderServiceTask))]
	sealed class NctsDownloaderServiceTaskTest : ServiceTaskTestCase<NctsDownloaderServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
