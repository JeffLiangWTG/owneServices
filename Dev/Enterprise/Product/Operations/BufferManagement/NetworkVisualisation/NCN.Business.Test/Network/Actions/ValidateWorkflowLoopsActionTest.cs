using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ValidateWorkflowLoopsAction))]
	class ValidateWorkflowLoopsActionTest : JobNetworkActionTestCase<ValidateWorkflowLoopsAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);
			AssertNull(action.ExecuteForEntityWithoutAccessCheck(diagram));
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed("Root", action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Shape", "This action is accessible to the root diagram only.", action.IsApplicableAfterActivatingEntity_ForTest(shape));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Buffer", "This action is accessible to the root diagram only.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Annotation", "This action is accessible to the root diagram only.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Validate Workflow Loops", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Performs validation on linked workflows to determine if there are any loops", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Approve");
		}

		protected override ValidateWorkflowLoopsAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ValidateWorkflowLoopsAction(networkViewModel);
		}
	}
}
