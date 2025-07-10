using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(InterchangeProcessorServiceTask))]
	sealed class InterchangeProcessorServiceTaskTest : ServiceTaskTestCase<InterchangeProcessorServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
