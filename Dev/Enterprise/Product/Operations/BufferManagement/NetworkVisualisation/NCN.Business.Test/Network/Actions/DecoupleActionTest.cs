using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.BufferManagement.NetworkVisualisation.Business.DecoupleAction;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(DecoupleAction))]
	class DecoupleActionTest : JobNetworkActionTestCase<DecoupleAction>
	{
		protected override void TestExecuteCore()
		{
			CreateTestData();

			AssertEquals(false, arrow1_3.BNA_IsDecouple);
			AssertEquals(false, arrow2_3.BNA_IsDecouple);

			var action = GetAction(networkViewModel);
			action.GetChildActionsAfterActivatingEntity_ForTest(shape3).ElementAt(0).AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(shape3);

			AssertEquals(true, arrow1_3.BNA_IsDecouple);
			AssertEquals(false, arrow2_3.BNA_IsDecouple);

			action.GetChildActionsAfterActivatingEntity_ForTest(shape3).ElementAt(0).AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(shape3);

			AssertEquals(true, arrow1_3.BNA_IsDecouple);
			AssertEquals(true, arrow2_3.BNA_IsDecouple);
		}

		public void TestExecute_ShouldReloadActions()
		{
			CreateTestData();

			var startingActionCount = networkViewModel.GetApplicableCoreCustomNetworkActions_ForTesting(shape3).Count();
			AssertNotEquals(0, startingActionCount);

			GetAction(networkViewModel).GetChildActionsAfterActivatingEntity_ForTest(shape3).ElementAt(0).AsJobNetworkAction().ExecuteForEntityWithoutAccessCheck(shape3);

			AssertEquals(startingActionCount, networkViewModel.GetApplicableCoreCustomNetworkActions_ForTesting(shape3).Count());
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"Cannot execute when the diagram is not in scaled mode.",
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"This action is not accessible to the root diagram.",
				"Cannot execute when the diagram is not in scaled mode.",
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			AssertEquals("Precondition: not approved", false, diagram.IsApproved);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"This action is not accessible to the root diagram.",
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: diagram is approved", true, diagram.IsApproved);
			AssertEquals("Precondition: child shape is approved", true, childShape.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			CreateTestData();

			var action = GetAction(networkViewModel);

			AssertEquals("No pre-requisites, so not enabled", false, action.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals("No pre-requisites, so not enabled", false, action.IsEnabledAfterActivatingEntity_ForTest(shape2).IsAllowed);
			AssertEquals("Has pre-requisites, so enabled", true, action.IsEnabledAfterActivatingEntity_ForTest(shape3).IsAllowed);

			arrow1_3.BNA_IsDecouple = arrow2_3.BNA_IsDecouple = true;

			AssertEquals("All pre-requisites are decoupled, so not enabled", false, action.IsEnabledAfterActivatingEntity_ForTest(shape3).IsAllowed);
		}

		protected override void TestGetNameCore()
		{
			CreateTestData();
			AssertEquals("Decouple", GetAction(networkViewModel).GetName());

			var action = GetAction(networkViewModel);

			var childActions = action.GetChildActionsAfterActivatingEntity_ForTest(shape3).ToArray();
			AssertEquals(2, childActions.Length);
			AssertEquals("From [shape1]", childActions[0].GetName());
			AssertEquals("From [shape2]", childActions[1].GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			CreateTestData();
			AssertEquals("Decouples this shape from the selected prerequisite shape, making it startable without the prerequisite being complete", GetAction(networkViewModel).GetDescription());

			var action = GetAction(networkViewModel);

			var childActions = action.GetChildActionsAfterActivatingEntity_ForTest(shape3).ToArray();
			AssertEquals(2, childActions.Length);
			AssertEquals("Decouples this shape from the selected prerequisite shape, making it startable without the prerequisite being complete", childActions[0].GetDescription());
			AssertEquals("Decouples this shape from the selected prerequisite shape, making it startable without the prerequisite being complete", childActions[1].GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestChildActionsCore()
		{
			CreateTestData();

			var action = GetAction(networkViewModel);

			var childActions = action.GetChildActionsAfterActivatingEntity_ForTest(shape3).ToArray();
			AssertEquals(2, childActions.Length);
		}

		protected override DecoupleAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new DecoupleAction(networkViewModel);
		}

		void CreateTestData()
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			networkViewModel = CreateNetworkViewModel(diagram);
			network = networkViewModel.GetJobNetwork();
			shape1 = networkViewModel.CreateNewShape(diagram).Shape;
			shape2 = networkViewModel.CreateNewShape(diagram).Shape;
			shape3 = networkViewModel.CreateNewShape(diagram).Shape;

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";

			arrow1_3 = network.CreateRelationship(shape1, shape3).AsAttachment();
			arrow2_3 = network.CreateRelationship(shape2, shape3).AsAttachment();

			network.SwitchToScaled();
			diagram.EarliestStartTimeUtc = ZDateTime.UtcNow;

			networkViewModel.ToggleApproval();
		}

		NetworkViewModel networkViewModel;
		IJobNetwork network;
		BMNCNShape shape1, shape2, shape3;
		BMNCNAttachment arrow1_3, arrow2_3;
	}

	[TestedType(typeof(DecoupleChildAction))]
	class DecoupleChildActionTest : JobNetworkActionTestCase<DecoupleChildAction>
	{
		protected override void TestExecuteCore()
		{
			Assert(true);
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("From [shape1]", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Decouples this shape from the selected prerequisite shape, making it startable without the prerequisite being complete", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override DecoupleChildAction GetActionCore(INetworkViewModel networkViewModel)
		{
			var diagram = CreateDiagram(Factory, name: "diagram");
			var nvm = CreateNetworkViewModel(diagram);
			var network = nvm.GetJobNetwork();
			var shape1 = nvm.CreateNewShape(diagram).Shape;
			var shape2 = nvm.CreateNewShape(diagram).Shape;

			shape1.Name = "shape1";
			shape2.Name = "shape2";

			var arrow = network.CreateRelationship(shape1, shape2).AsAttachment();

			return new DecoupleChildAction(networkViewModel, arrow);
		}
	}
}
