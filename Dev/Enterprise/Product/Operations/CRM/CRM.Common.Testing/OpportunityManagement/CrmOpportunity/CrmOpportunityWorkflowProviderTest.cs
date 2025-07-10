using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.CRM.Common.Testing
{
	[TestedType(typeof(CrmOpportunity))]
	sealed class CrmOpportunityWorkflowProviderTest : WorkflowProviderTest<CrmOpportunity, CrmOpportunityProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode;
	}
}
