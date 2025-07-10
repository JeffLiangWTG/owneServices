using System;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(CopyOGDToPGAServiceTask))]
	sealed class CopyOGDToPGAServiceTaskTest : ServiceTaskTestCase<CopyOGDToPGAServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
