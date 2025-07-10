using System;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(PinShapeAction))]
	class PinShapeActionTest : JobNetworkActionTestCase<PinShapeAction>
	{
		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child of a non scaled diagram", new string[] {
					"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(shape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled diagram", new string[] {
				"This action is not accessible to the root diagram.",
				"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			AssertEquals("Precondition: not approved", false, diagram.IsApproved);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("scaled diagram", new string[] {
				"This action is not accessible to the root diagram." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(shape));

			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: diagram is approved", true, diagram.IsApproved);
			AssertEquals("Precondition: shape is approved", true, diagram.IsApproved);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("approved shape", "The shape should not be approved.", action.IsApplicableAfterActivatingEntity_ForTest(shape));

			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: diagram is not approved", false, diagram.IsApproved);
			AssertEquals("Precondition: shape is not approved", false, diagram.IsApproved);

			action.ExecuteAfterActivatingEntity_ForTest(shape);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("pinned non approved shape", "The shape should not be pinned.", action.IsApplicableAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertAllowed("buffer", action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			action.ExecuteAfterActivatingEntity_ForTest(buffer);

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("pinned non approved buffer", "The shape should not be pinned.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be an annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsEnabledAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertAllowed("buffer", action.IsEnabledAfterActivatingEntity_ForTest(buffer));
		}

		public void TestShouldBeDisabledAfterPinning()
		{
			diagram.SwitchToScaled();

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
				action.Execute();
				Assert(shape.IsPinned);
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should not be pinned.", action.IsEnabled());

				shape.UnPinShape(networkViewModel);
				Assert(!shape.IsPinned);
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
			}
		}

		public void TestShouldBeAbleToPinMultipleSelectedShape_WhenAtLeastOneOfThemIsNotPinned()
		{
			diagram.SwitchToScaled();

			var entity1 = networkViewModel.CreateNewShape(diagram);
			var entity2 = networkViewModel.CreateNewShape(diagram);
			var entity3 = networkViewModel.CreateNewShape(diagram);

			entity1.AsShape().PinShape(networkViewModel);
			entity3.AsShape().PinShape(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
		}

		public void TestShouldNotBeAbleToPinMultipleSelectedShapes_WhenAllOfThemArePinned()
		{
			diagram.SwitchToScaled();

			var entity1 = networkViewModel.CreateNewShape(diagram);
			var entity2 = networkViewModel.CreateNewShape(diagram);

			entity1.AsShape().PinShape(networkViewModel);
			entity2.AsShape().PinShape(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2 });

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] {
				new NetworkActionDenialReason(entity1.AsShape(), "The shape should not be pinned.", needsNotification: false),
				new NetworkActionDenialReason(entity2.AsShape(), "The shape should not be pinned.", needsNotification: false) }, action.IsEnabled());
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertEquals(false, shape.AsEntity(network).IsPinned);

			action.ExecuteForEntityWithoutAccessCheck(shape);

			AssertEquals(true, shape.AsEntity(network).IsPinned);
		}

		public void TestShouldPinMutlipleSelectedShape_WhenTheyAreNotPinned()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var shape1 = CreateShape(diagram, "Shape1");
			var shape2 = CreateShape(diagram, "Shape2");
			var shape3 = CreateShape(diagram, "Shape3");

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(c => c.UserInteractionImplementor).Returns(interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);

			Assert("Precondition", !entity1.IsPinned);
			Assert("Precondition", !entity2.IsPinned);
			Assert("Precondition", !entity3.IsPinned);

			var action = new PinShapeAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());

			action.Execute();
			Assert(entity1.IsPinned);
			Assert(entity2.IsPinned);
			Assert(entity3.IsPinned);
			controller.Verify(c => c.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
		}

		public void TestShouldPinEntitiesThatAreNotPinned_WithoutBotheringUserWithNotification()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var shape1 = CreateShape(diagram, "Shape1");
			var shape2 = CreateShape(diagram, "Shape2");
			var shape3 = CreateShape(diagram, "Shape3");

			var controller = new Mock<IBMNetworkEntityController>();

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);

			shape1.PinShape(networkViewModel);
			shape3.PinShape(networkViewModel);

			AssertEquals("Precondition", true, entity1.IsPinned);
			AssertEquals("Precondition", false, entity2.IsPinned);
			AssertEquals("Precondition", true, entity3.IsPinned);

			var action = new PinShapeAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());

			action.Execute();
			Assert(entity1.IsPinned);
			Assert(entity2.IsPinned);
			Assert(entity3.IsPinned);
			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never());
		}

		#endregion

		#region Main Properties

		protected override void TestGetNameCore()
		{
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Action shouldn't be applicable for non-scaled diagrams",
				"Cannot execute when the diagram is not in scaled mode.", action.IsApplicableAfterActivatingEntity_ForTest(shape));
			AssertEquals("Pin Shape", action.GetNameAfterActivatingEntity_ForTest(shape));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("Action should be applicable for scaled diagrams", action.IsApplicableAfterActivatingEntity_ForTest(shape));
			AssertEquals("Pin Shape", action.GetNameAfterActivatingEntity_ForTest(shape));
		}

		protected override void TestGetDescriptionCore()
		{
			AssertEquals("Pins this shape so it cannot be resized or moved.", action.GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Pin");
		}

		#endregion

		protected override PinShapeAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new PinShapeAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(MultipleSelectedEntitiesExecutionStrategy);

		protected override void SetUp()
		{
			base.SetUp();

			diagram = CreateDiagram(Factory, name: "Root");

			shape = CreateShape(diagram, "Shape");

			buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			networkViewModel = CreateNetworkViewModel(diagram);
			network = networkViewModel.GetJobNetwork();

			action = GetAction(networkViewModel);
		}

		BMNCNShape diagram;
		BMNCNShape shape;
		BMNCNShape buffer;
		BMNCNShape annotation;
		NetworkViewModel networkViewModel;
		IJobNetwork network;
		PinShapeAction action;
	}
}
