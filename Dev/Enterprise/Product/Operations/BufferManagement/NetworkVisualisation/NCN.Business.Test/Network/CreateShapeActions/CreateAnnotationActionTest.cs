using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateAnnotationAction))]
	class CreateAnnotationActionTest : JobNetworkActionTestCase<CreateAnnotationAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(0, network.Entities.Count);

			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(1, network.Entities.Count);
			var annotation = network.Shapes[0];

			AssertEquals(ShapeTypeList.Codes.Annotation, annotation.BNS_ShapeType);
			AssertEquals("AliceBlue", annotation.BackColor);
			AssertEquals("New Annotation", annotation.Name);
			AssertEquals(diagram, annotation.AsEntity(network).Owner.Shape);
		}

		protected override void TestIsApplicableCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			CombineAssertions("Diagrams and shapes shown on Network Diagram Form", () =>
			{
				var root = CreateDiagram(jobHeader);
				AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, root.BNS_ShapeType);
				AssertNotNull("Precondition", root.ProcessHeader);

				var shape = CreateShape(root, name: "Shape");
				var buffer = CreateShape(root, shapeType: ShapeTypeList.Codes.Buffer, name: "Buffer");
				var annotation = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation, name: "Annotation");

				var networkViewModel = CreateNetworkViewModel(root);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed("Diagram", action.IsApplicableAfterActivatingEntity_ForTest(root));
				NetworkActionAccessibilityTest.AssertAllowed("Shape", action.IsApplicableAfterActivatingEntity_ForTest(shape));
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Buffer", "The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Annotation", "The shape should not be a buffer or annotation.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));
			});

			CombineAssertions("Diagrams and shapes shown on Workflow Relationship Designer tab", () =>
			{
				var defaultDiagram = jobHeader.GetDefaultDiagram();
				var defaultShape = CreateDefaultDiagramWorkflowShape(workflow);

				var networkViewModel = CreateNetworkViewModel(defaultDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed("Default root diagram", action.IsApplicableAfterActivatingEntity_ForTest(defaultDiagram));
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Default workflow", "The operation is not applicable to workflows.", action.IsApplicableAfterActivatingEntity_ForTest(defaultShape));
			});
		}

		protected override void TestIsEnabledCore()
		{
			NetworkActionAccessibilityTest.AssertAllowed(GetAction(CreateNetworkViewModel(CreateDiagram(Factory))).IsEnabled());
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Annotation", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates a new annotation and adds it to the diagram", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Bubble");
		}

		protected override CreateAnnotationAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateAnnotationAction(networkViewModel);
		}
	}
}
