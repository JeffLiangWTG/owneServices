using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateShapeAction))]
	class CreateShapeActionTest : JobNetworkActionTestCase<CreateShapeAction>
	{
		protected override void TestExecuteCore()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(1, network.Entities.Count);

			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(1, jobHeader.ProcessHeaders.Count);

			var newShape = network.Shapes[1];
			AssertEquals(ShapeTypeList.Codes.Shape, newShape.BNS_ShapeType);
			AssertEquals("Bisque", newShape.BackColor);
			AssertNull(newShape.ProcessHeader);

			new ConvertToWorkflowsAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertEquals(jobHeader.PK, newShape.ProcessHeader.FH_FH_ParentHeader);
			AssertNotNull(newShape.ProcessHeader);
			AssertEquals(bucket, newShape.ProcessHeader.CurrentComponent);
		}

		protected override void TestIsApplicableCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var root = CreateDiagram(jobHeader);
			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, root.BNS_ShapeType);
			AssertNotNull("Precondition", root.ProcessHeader);

			var shape = CreateShape(root);
			var buffer = CreateShape(root, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(root);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(root));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			NetworkActionAccessibilityTest.AssertAllowed(GetAction(CreateNetworkViewModel(CreateDiagram(Factory))).IsEnabled());
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Shape", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates a new leaf shape that is not linked to a business entity", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Shape");
		}

		protected override CreateShapeAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateShapeAction(networkViewModel);
		}
	}
}
