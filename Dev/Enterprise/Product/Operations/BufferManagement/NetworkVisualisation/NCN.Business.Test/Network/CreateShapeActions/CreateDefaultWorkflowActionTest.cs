using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateDefaultWorkflowAction))]
	class CreateDefaultWorkflowActionTest : JobNetworkActionTestCase<CreateDefaultWorkflowAction>
	{
		protected override void TestExecuteCore()
		{
			var system = CreateSystem("ORG");
			var bucket = CreateBucket(system);

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = jobHeader.GetDefaultDiagram();
			var shape = workflow.GetDefaultShape(diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals(1, network.Entities.Count);

			var action = GetAction(networkViewModel);

			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(2, network.Entities.Count);
			AssertEquals(2, jobHeader.ProcessHeaders.Count);

			var newShape = network.Shapes[1];
			AssertDefaultWorkflowConfig(newShape, bucket);

			action.ExecuteForEntityWithoutAccessCheck(newShape);

			var newerShape = network.Shapes[2];

			AssertDefaultWorkflowConfig(newerShape, bucket);
			AssertIsParent(newerShape.ProcessHeader, newShape.ProcessHeader);
		}

		static void AssertDefaultWorkflowConfig(BMNCNShape newShape, BMComponent expectedComponent)
		{
			AssertEquals(ShapeTypeList.Codes.DefaultWorkflow, newShape.BNS_ShapeType);
			AssertEquals("Bisque", newShape.BackColor);
			AssertNotNull(newShape.ProcessHeader);
			AssertEquals(true, newShape.ProcessHeader.IsWorkflow);

			AssertEquals(expectedComponent, newShape.ProcessHeader.CurrentComponent);
		}

		protected override void TestIsApplicableCore()
		{
			var system = CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			CombineAssertions("Linked diagrams and shapes shown on Network Diagram Form", () =>
			{
				var linkedDiagram = CreateDiagram(jobHeader);
				AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, linkedDiagram.BNS_ShapeType);
				AssertNotNull("Precondition", linkedDiagram.ProcessHeader);
				var networkViewModel = CreateNetworkViewModel(linkedDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is accessible in the Workflow Relationship Designer only.", action.IsApplicableAfterActivatingEntity_ForTest(linkedDiagram));

				var linkedShape = networkViewModel.CreateNewWorkflow(linkedDiagram);
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is accessible in the Workflow Relationship Designer only.", action.IsApplicableAfterActivatingEntity_ForTest(linkedShape));
			});

			CombineAssertions("Non linked diagrams and shapes shown on Network Diagram Form", () =>
			{
				var nonLinkedDiagram = CreateDiagram(Factory);
				var nonLinkedShape = CreateShape(nonLinkedDiagram);
				var buffer = CreateShape(nonLinkedDiagram, shapeType: ShapeTypeList.Codes.Buffer);
				var annotation = CreateShape(nonLinkedDiagram, shapeType: ShapeTypeList.Codes.Annotation);

				var networkViewModel = CreateNetworkViewModel(nonLinkedDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"This action is accessible in the Workflow Relationship Designer only.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(nonLinkedDiagram));

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"This action is accessible in the Workflow Relationship Designer only.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(nonLinkedShape));

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"This action is accessible in the Workflow Relationship Designer only.",
					"The shape should not be a buffer or annotation.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

				NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
					"This action is accessible in the Workflow Relationship Designer only.",
					"The shape should not be a buffer or annotation.",
					"The shape should be linked to a job or workflow." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));
			});

			CombineAssertions("Diagrams and shapes shown on Workflow Relationship Designer tab", () =>
			{
				var defaultDiagram = jobHeader.GetDefaultDiagram();
				var defaultShape = CreateDefaultDiagramWorkflowShape(workflow);

				var networkViewModel = CreateNetworkViewModel(defaultDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(defaultDiagram));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(defaultShape));
			});
		}

		protected override void TestIsEnabledCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			var defaultShape = CreateDefaultDiagramWorkflowShape(workflow);

			var networkViewModel = CreateNetworkViewModel(defaultDiagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(defaultDiagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(defaultShape));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Workflow", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates a new workflow for this job", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override CreateDefaultWorkflowAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateDefaultWorkflowAction(networkViewModel);
		}
	}
}
