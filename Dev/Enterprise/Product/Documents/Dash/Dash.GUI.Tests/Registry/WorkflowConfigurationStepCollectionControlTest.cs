using CargoWise.EntityFramework;
using Enterprise.Dash.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Dash.GUI.Tests
{
	[TestedType(typeof(WorkflowConfigurationStepCollectionControl))]
	sealed class WorkflowConfigurationStepCollectionControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new WorkflowConfigurationStepCollection(TestHelper.GetCodesProviderForTesting());
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((WorkflowConfigurationStepCollectionControl)control).workflowConfigurationStepGrid.ReadOnly;
		}
	}
}
