using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(LinkToWorkflowAction))]
	class LinkToWorkflowActionTest : LinkedEntityActionTestCase<LinkToWorkflowAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertStaticName("Select Workflow...");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertStaticDescription("Choose a record from the Job Workflows module to which this shape will be linked.");
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Link");
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			AssertIsApplicableToNonDefaultDiagramsAndShapesOnly();
		}

		protected override void TestIsEnabledCore()
		{
			AssertIsEnabledForNonDefaultDiagramsAndShapes();
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertExecute((network, shape) => network.Setup(m => m.LinkEntity(shape, ModuleIDs.ProcessHeader)));
		}

		#endregion

		#region Implementation

		protected override LinkToWorkflowAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new LinkToWorkflowAction(networkViewModel);
		}

		#endregion
	}
}
