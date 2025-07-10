using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(LinkToDiagramAction))]
	class LinkToDiagramActionTest : LinkedEntityActionTestCase<LinkToDiagramAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertStaticName("Select Diagram...");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertStaticDescription("Choose a record from the Network Diagrams module to which this shape will be linked.");
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
			AssertIsEnabledForShapesOnly("The action can be applied to shapes only.");
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertExecute((network, shape) => network.Setup(m => m.LinkEntity(shape, ModuleIDs.NetworkDiagram)));
		}

		#endregion

		#region Implementation

		protected override LinkToDiagramAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new LinkToDiagramAction(networkViewModel);
		}

		#endregion
	}
}
