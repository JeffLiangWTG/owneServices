using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateJobActionCollection))]
	class CreateJobActionCollectionTest : JobNetworkActionTestCase<CreateJobActionCollection>
	{
		protected override CreateJobActionCollection GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateJobActionCollection(networkViewModel);
		}

		protected override void TestGetDescriptionCore()
		{
			AssertEquals("Creates and opens a new job", GetAction().GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Job");
		}

		protected override void TestGetNameCore()
		{
			AssertEquals("Create Job", GetAction().GetName());
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			CreateSystem("ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var linkedDiagram = CreateDiagram(jobHeader);
			var networkViewModel1 = CreateNetworkViewModel(linkedDiagram);

			var linkedWorkflowShape = networkViewModel1.CreateNewWorkflow(linkedDiagram);

			var unLinkedDiagram = Factory.New<BMNCNShape>();
			var networkViewModel2 = CreateNetworkViewModel(unLinkedDiagram);

			var unLinkedWorkflowShape = networkViewModel2.CreateNewShape(unLinkedDiagram);

			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel1).IsEnabledAfterActivatingEntity_ForTest(linkedWorkflowShape).IsAllowed);

			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedDiagram).IsAllowed);
			AssertEquals(true, GetAction(networkViewModel2).IsEnabledAfterActivatingEntity_ForTest(unLinkedWorkflowShape).IsAllowed);
		}

		public override void TestChildActions()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);

			var action = GetAction(networkViewModel);
			var actions = action.GetChildActionsAfterActivatingEntity_ForTest(shape1);

			var actionNames = new[]
			{
				"Create Work Item",
				"Create Project",
				"Create Job"
			};

			AssertArrayEqualsByElements(actionNames, actions.Select(a => a.AsJobNetworkAction().GetNameAfterActivatingEntity_ForTest(shape1)).ToArray());
		}

		protected override void TestExecuteCore()
		{
			Assert(true);
		}
	}
}
