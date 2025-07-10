using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(OpenLinkedEntityAction))]
	class OpenLinkedEntityActionTest : LinkedEntityActionTestCase<OpenLinkedEntityAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertGetName(nameForUnlinked: "Open Linked Entity", nameForLinkedToProcessHeader: "Open Linked Workflow", nameForLinkedToDiagram: "Open Linked Diagram");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertGetDescription(descriptionForUnlinked: "Open the linked entity in its own form.", descriptionForProcessHeader: "Open the linked workflow in its job's form.", descriptionForShape: "Open the linked diagram in its own form.");
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Link");
		}

		#endregion

		#region Accessbility

		protected override void TestIsApplicableCore()
		{
			AssertApplicableToDiagram_WhenItIsLinkedOnly();
			AssertApplicableToShape_WhenItIsLinkedOnly();
		}

		protected override void TestIsEnabledCore()
		{
			AssertEnabledToLinkedShape();
			AssertEnabledToLinkedDiagram();
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertExecute((network, shape) => network.Setup(m => m.OpenLinkedEntity(shape)));
		}

		#endregion

		#region Implementation

		protected override OpenLinkedEntityAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new OpenLinkedEntityAction(networkViewModel);
		}

		#endregion
	}
}
