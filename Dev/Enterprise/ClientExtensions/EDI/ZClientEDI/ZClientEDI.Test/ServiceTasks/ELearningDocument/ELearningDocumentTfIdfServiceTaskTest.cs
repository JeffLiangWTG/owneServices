using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(ELearningDocumentTfIdfServiceTask))]
	public class ELearningDocumentTfIdfServiceTaskTest : ServiceTaskTestCase<ELearningDocumentTfIdfServiceTask>
	{
		public void TestServiceTask()
		{
			AssertEquals("EDT", ELearningDocumentTfIdfServiceTask.Code);
		}

		public void TestDefinition()
		{
			AssertEquals("3hours", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
