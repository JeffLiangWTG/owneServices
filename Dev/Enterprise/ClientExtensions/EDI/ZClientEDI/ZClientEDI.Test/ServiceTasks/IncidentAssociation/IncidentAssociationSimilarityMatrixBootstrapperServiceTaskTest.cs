using System.Collections.Generic;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(IncidentAssociationSimilarityMatrixBootstrapperServiceTask))]
	public class IncidentAssociationSimilarityMatrixBootstrapperServiceTaskTest : ServiceTaskTestCase<IncidentAssociationSimilarityMatrixBootstrapperServiceTask>
	{
		public void TestServiceTask()
		{
			AssertEquals("IAM", IncidentAssociationSimilarityMatrixBootstrapperServiceTask.Code);
			AssertEquals("IncidentAssociationMatrixBootstrapRequest", IncidentAssociationSimilarityMatrixBootstrapperRunner.StmFieldName);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
