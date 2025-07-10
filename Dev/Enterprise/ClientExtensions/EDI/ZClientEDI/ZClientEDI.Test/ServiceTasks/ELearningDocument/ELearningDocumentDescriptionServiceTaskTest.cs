using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(ELearningDocumentDescriptionServiceTask))]
	class ELearningDocumentDescriptionServiceTaskTest : ServiceTaskTestCase<ELearningDocumentDescriptionServiceTask>
	{
		[ExpectNoExceptions]
		public void TestServiceTask()
		{
			AssertEquals("ELD", ELearningDocumentDescriptionServiceTask.Code);
		}

		public void TestDefinition()
		{
			AssertEquals("Service task should run every 2 hours", "2hours", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
