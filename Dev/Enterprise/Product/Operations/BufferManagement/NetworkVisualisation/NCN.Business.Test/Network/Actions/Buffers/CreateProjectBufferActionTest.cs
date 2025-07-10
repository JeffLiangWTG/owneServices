using System.Linq;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateProjectBufferAction))]
	class CreateProjectBufferActionTest : JobNetworkActionTestCase<CreateProjectBufferAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var shape3 = NetworkTestCase.CreateShape(diagram);
			var shape4 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertNull(network.Shapes.OfType<BMNCNBufferShape>().SingleOrDefault());

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().SingleOrDefault();

			AssertNotNull(buffer);
			AssertEquals(BufferTypeList.Codes.Project, buffer.BufferType);
			AssertEquals("Project Buffer", buffer.BNS_Name);

			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void TestIsApplicableCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader, name: "diagram");
			var shape = CreateShape(workflow, diagram, "shape");

			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child of a non scaled diagram", new string[] {
				"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(shape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled diagram", new string[] {
				"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var shape3 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("Should be enabled for diagram", action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		public void TestShouldBeApplicableButDisabled_WhenThereIsAlreadyProjectBufferOnDiagram()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var shape3 = NetworkTestCase.CreateShape(diagram);
			var shape4 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertNull(network.Shapes.OfType<BMNCNBufferShape>().SingleOrDefault());

			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());

			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().SingleOrDefault();
			AssertNotNull(buffer);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicable());
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There is already a project buffer on this diagram.", action.IsEnabled());
		}

		public void TestShouldBeEnabledForLastShapeOnCriticalChain_AndApplicableButDisabledForOtherShapes()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);
			var shape3 = NetworkTestCase.CreateShape(diagram);
			var shape4 = NetworkTestCase.CreateShape(diagram);
			var shape5 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			shape3.IsCriticalPath = true;
			Assert("Precondition: shape 3 should not have postrequisites", !shape3.PostRequisiteLinks.Any());

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertNull(network.Shapes.OfType<BMNCNBufferShape>().SingleOrDefault());

			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape1));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be either the root diagram or the last shape on the critical path.", action.IsEnabledAfterActivatingEntity_ForTest(shape1));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape2));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be either the root diagram or the last shape on the critical path.", action.IsEnabledAfterActivatingEntity_ForTest(shape2));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape4));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be either the root diagram or the last shape on the critical path.", action.IsEnabledAfterActivatingEntity_ForTest(shape4));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(shape5));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be either the root diagram or the last shape on the critical path.", action.IsEnabledAfterActivatingEntity_ForTest(shape5));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(shape3));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Create Project Buffer", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Creates a project buffer at the end of the Critical Chain on this diagram.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Buffer");
		}

		protected override CreateProjectBufferAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateProjectBufferAction(networkViewModel);
		}
	}
}
