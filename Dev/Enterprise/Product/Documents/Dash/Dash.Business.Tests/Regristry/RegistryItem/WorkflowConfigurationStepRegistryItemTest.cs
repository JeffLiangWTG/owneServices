using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.Business.Testing
{
	[TestedType(typeof(WorkflowConfigurationStepRegistryItem))]
	sealed class WorkflowConfigurationStepRegistryItemTest : StronglyTypedRegistryItemTestCase<WorkflowConfigurationStepCollection>
	{
		protected override StronglyTypedRegistryItem<WorkflowConfigurationStepCollection, WorkflowConfigurationStepCollection> GetNewRegistryItem()
		{
			return new WorkflowConfigurationStepRegistryItem("", null, null, null, RegistryStorageFlags.System, WorkflowConfigurationStepTest.GetCodesProviderForTesting());
		}
	}
}
