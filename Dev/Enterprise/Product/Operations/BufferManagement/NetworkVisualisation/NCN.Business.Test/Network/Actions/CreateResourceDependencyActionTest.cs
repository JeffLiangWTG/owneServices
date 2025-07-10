using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CreateResourceDependencyAction))]
	class CreateResourceDependencyActionTest : JobNetworkActionTestCase<CreateResourceDependencyAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";
			childShape3.Name = "shape3";

			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, prereqAction, postreqAction);

			UnitTestUserNotification.Instance.ClearMessages();

			networkViewModel.SelectSingleEntity(null);

			CombineAssertions("prereq action, nothing is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", prereqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			CombineAssertions("postreq action, nothing is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", postreqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			networkViewModel.SelectSingleEntity(childShape1);

			CombineAssertions("prereq action, one shape is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", prereqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			CombineAssertions("postreq action, one shape is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", postreqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });

			CombineAssertions("postreq action, two shapes are selected", () =>
			{
				NetworkActionAccessibilityTest.AssertAllowed(postreqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape2));
				postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape2);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShape1, childShape2, diagram, true);
				AssertResourceDependencyExists(childShape2, childShape3, diagram, false);
			});

			CombineAssertions("postreq action, two shapes are selected, trying to add a dependency which already exists", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("A dependency already exists between these two shapes.", postreqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: A dependency already exists between these two shapes.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				AssertResourceDependencyExists(childShape1, childShape2, diagram, true);
				AssertResourceDependencyExists(childShape2, childShape3, diagram, false);
			});

			networkViewModel.SelectEntities(new[] { childShape2, childShape3 });

			CombineAssertions("prereq action, other two shapes are selected", () =>
			{
				NetworkActionAccessibilityTest.AssertAllowed(prereqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape2));
				prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape2);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShape2, childShape3, diagram, true);
			});
		}

		public void TestExecuteCore_ForUniformAction()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShapeLeft = networkViewModel.CreateNewShape(diagram);
			var childShapeCentre = networkViewModel.CreateNewShape(diagram);
			var childShapeRight = networkViewModel.CreateNewShape(diagram);
			var childShapeTop = networkViewModel.CreateNewShape(diagram);
			var childShapeBottom = networkViewModel.CreateNewShape(diagram);
			childShapeLeft.Name = "left";
			childShapeCentre.Name = "centre";
			childShapeRight.Name = "right";
			childShapeTop.Name = "top";
			childShapeBottom.Name = "bottom";

			var nodeLeft = networkViewModel.GetNodeForEntity(childShapeLeft);
			var nodeCentre = networkViewModel.GetNodeForEntity(childShapeCentre);
			var nodeRight = networkViewModel.GetNodeForEntity(childShapeRight);
			var nodeTop = networkViewModel.GetNodeForEntity(childShapeTop);
			var nodeBottom = networkViewModel.GetNodeForEntity(childShapeBottom);

			foreach (var node in new NodeViewModel[] { nodeLeft, nodeCentre, nodeRight, nodeTop, nodeBottom })
			{
				node.Width = 50;
				node.Height = 50;
				node.X = 200;
				node.Y = 200;
			}

			nodeLeft.X = 100;
			nodeRight.X = 300;

			nodeTop.Y = 100;
			nodeBottom.Y = 300;

			var action = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.Uniform);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, action);

			UnitTestUserNotification.Instance.ClearMessages();

			networkViewModel.SelectSingleEntity(null);

			CombineAssertions("nothing is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeLeft));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				action.ExecuteAfterActivatingEntity_ForTest(childShapeLeft);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
left: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			networkViewModel.SelectSingleEntity(childShapeLeft);

			CombineAssertions("one shape is selected", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeLeft));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				action.ExecuteAfterActivatingEntity_ForTest(childShapeLeft);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
left: Two entities must be selected.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			networkViewModel.SelectEntities(new[] { childShapeLeft, childShapeCentre });

			CombineAssertions("two shapes are selected, more to the left is active", () =>
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeLeft));
				action.ExecuteAfterActivatingEntity_ForTest(childShapeLeft);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, false);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, false);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, false);

				AssertEquals("left shape should stay at its place", 100d, nodeLeft.X);
				AssertEquals("centre shape should stay at its place", 200d, nodeCentre.X);
			});

			networkViewModel.SelectEntities(new[] { childShapeLeft, childShapeCentre });

			CombineAssertions("two shapes are selected, more to the left is active, trying to add a dependency which already exists", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("A dependency already exists between these two shapes.", action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeLeft));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				action.ExecuteAfterActivatingEntity_ForTest(childShapeLeft);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
left: A dependency already exists between these two shapes.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, false);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, false);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, false);
			});

			networkViewModel.SelectEntities(new[] { childShapeCentre, childShapeRight });

			CombineAssertions("two shapes are selected, more to the right is active", () =>
			{
				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeRight));
				action.ExecuteAfterActivatingEntity_ForTest(childShapeRight);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, true);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, false);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, false);

				AssertEquals("centre shape should stay at its place", 200d, nodeCentre.X);
				AssertEquals("right shape should stay at its place", 300d, nodeRight.X);
			});

			networkViewModel.SelectEntities(new[] { childShapeCentre, childShapeRight });

			CombineAssertions("two shapes are selected, more to the right is active, trying to add a dependency which already exists", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("A dependency already exists between these two shapes.", action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeRight));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				action.ExecuteAfterActivatingEntity_ForTest(childShapeRight);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
right: A dependency already exists between these two shapes.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, true);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, false);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, false);
			});

			networkViewModel.SelectEntities(new[] { childShapeTop, childShapeCentre });

			CombineAssertions("two shapes on the same vertical line are selected, more to the top is active", () =>
			{
				AssertEquals("Precondition", childShapeTop.X, childShapeCentre.X);

				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeTop));
				action.ExecuteAfterActivatingEntity_ForTest(childShapeTop);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, true);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, false);

				AssertEquals("top shape should stay at its place", 200d, nodeTop.X);
				AssertGreaterThan("centre shape should move to the right", nodeCentre.X, 200d);
			});

			nodeCentre.X = 200;
			networkViewModel.SelectEntities(new[] { childShapeCentre, childShapeBottom });

			CombineAssertions("two shapes on the same vertical line are selected, more to the bottom is active", () =>
			{
				AssertEquals("Precondition", childShapeCentre.X, childShapeBottom.X);

				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShapeBottom));
				action.ExecuteAfterActivatingEntity_ForTest(childShapeBottom);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertResourceDependencyExists(childShapeLeft, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeRight, diagram, true);
				AssertResourceDependencyExists(childShapeTop, childShapeCentre, diagram, true);
				AssertResourceDependencyExists(childShapeCentre, childShapeBottom, diagram, true);

				AssertEquals("centre shape should stay at its place", 200d, nodeCentre.X);
				AssertGreaterThan("bottom shape should move to the right", nodeBottom.X, 200d);
			});
		}

		public void TestShouldExecuteForScaledModeOnly()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals("Precondition", false, diagram.IsScaled);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";

			networkViewModel.SelectEntities(new INetworkEntity[] { childShape1, childShape2 });

			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);

			CombineAssertions("prereq action", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute when the diagram is not in scaled mode.", prereqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Cannot execute when the diagram is not in scaled mode.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});

			CombineAssertions("postreq action", () =>
			{
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Cannot execute when the diagram is not in scaled mode.", postreqAction.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape1));
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);
				AssertEquals(@"This action cannot be executed for the given shape(s) due to the following reasons:
shape1: Cannot execute when the diagram is not in scaled mode.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			});
		}

		public void TestExecute_ShouldWorkThroughMenuItem()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";

			Factory.Save();

			networkViewModel.SelectEntities(new INetworkEntity[] { childShape1, childShape2 });
			var createResourceDependencyMenuItem = networkViewModel.GetSecondLevelNetworkActionsMenuItem_ForTesting(childShape1, "Actions", "Create Resource Dependency from [shape1] to [shape2]");
			AssertNotNull(createResourceDependencyMenuItem);
			createResourceDependencyMenuItem.Action.Execute();
			AssertResourceDependencyExists(childShape1, childShape2, diagram, true);
		}

		public void TestExecute_ShouldShuntOvelappingEntityLater()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";

			network.SwitchToScaled();
			((INetworkEntity)childShape1).X = 100;
			((INetworkEntity)childShape2).X = 200;

			AssertEquals(300.0, ((INetworkEntity)childShape1).Width);
			AssertEquals(300.0, ((INetworkEntity)childShape2).Width);

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);

			AssertResourceDependencyExists(childShape1, childShape2, diagram, true);

			AssertEquals("Prereq shape position should not be affected", 100.0, ((INetworkEntity)childShape1).X);
			AssertEquals("Postreq shape should be shunted right to remove overlap", 400.0, ((INetworkEntity)childShape2).X);
		}

		public void TestExecute_ShouldNotShuntNonOvelappingEntity()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";

			network.SwitchToScaled();
			((INetworkEntity)childShape1).X = 100;
			((INetworkEntity)childShape2).X = 500;

			AssertEquals(300.0, ((INetworkEntity)childShape1).Width);
			AssertEquals(300.0, ((INetworkEntity)childShape2).Width);

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			prereqAction.ExecuteAfterActivatingEntity_ForTest(childShape1);

			AssertResourceDependencyExists(childShape1, childShape2, diagram, true);

			AssertEquals("Prereq shape position should not be affected", 100.0, ((INetworkEntity)childShape1).X);
			AssertEquals("Non-overlappting postreq shape position should not be affected", 500.0, ((INetworkEntity)childShape2).X);
		}

		public void TestExecute_ForShapesWithDifferentOwners()
		{
			var diagram = Factory.New<BMNCNShape>();
			var subDiagram = Factory.New<BMNCNShape>();
			subDiagram.MakeChildOf(diagram);

			diagram.SwitchToScaled();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(subDiagram);
			childShape1.Name = "shape1";
			childShape2.Name = "shape2";

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);
			postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape2);

			AssertResourceDependencyExists(childShape1, childShape2, diagram, true);
		}

		public void TestExecute_ShouldUseRefreshTypeResourceDependencyAdded()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			diagram.SwitchToScaled();

			var refreshes = new List<RefreshType>();
			network.Refreshed += (s, e) => refreshes.Add(e.RefreshType);

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);

			networkViewModel.SelectEntities(new[] { childShape1, childShape2 });
			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);

			AssertArrayEqualsByElements(new[] { RefreshType.EntitiesReloaded, RefreshType.EntityAdded, RefreshType.EntitiesReloaded, RefreshType.EntityAdded }, refreshes.ToArray());
			postreqAction.ExecuteAfterActivatingEntity_ForTest(childShape2);

			AssertArrayEqualsByElements(new[] { RefreshType.EntitiesReloaded, RefreshType.EntityAdded, RefreshType.EntitiesReloaded, RefreshType.EntityAdded, RefreshType.ResourceDependencyAdded }, refreshes.ToArray());
			AssertResourceDependencyExists(childShape1, childShape2, diagram, true);
		}

		static void AssertResourceDependencyExists(INetworkEntity prereq, INetworkEntity postreq, INetworkEntity owner, bool shouldExist)
		{
			var dependencyAttachment = prereq.AsShape().DependencyAttachments.SingleOrDefault(a => a.IsResourceDependency && a.BNA_BNS_ToShape == postreq.EntityPK);
			var reverseAttachment = postreq.AsShape().DependencyAttachments.SingleOrDefault(a => a.IsResourceDependency && a.BNA_BNS_ToShape == prereq.EntityPK);

			if (shouldExist)
			{
				AssertNotNull("Resource dependency should exist", dependencyAttachment);
				AssertEquals("Owner PK", owner.EntityPK, dependencyAttachment.BNA_BNS_Owner);

				AssertNull("Reverse resource dependency should not exist", reverseAttachment);
			}
			else
			{
				AssertNull("Resource dependency should not exist", dependencyAttachment);
				AssertNull("Reverse resource dependency should not exist", reverseAttachment);
			}
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child of a non scaled diagram", new string[] {
					"Cannot execute when the diagram is not in scaled mode." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled diagram", new string[] {
				"Cannot execute when the diagram is not in scaled mode.",
				"This action is not accessible to the root diagram." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed("child of a scaled diagram", action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var child1 = networkViewModel.CreateNewShape(diagram);
			var child2 = networkViewModel.CreateNewShape(diagram);
			var child3 = networkViewModel.CreateNewShape(diagram);

			var action = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, action);

			diagram.SwitchToScaled();

			networkViewModel.SelectSingleEntity(child1);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Two entities must be selected.", action.IsEnabledAfterActivatingEntity_ForTest(child1));

			networkViewModel.SelectEntities(new[] { child1, child2 });
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(child1));

			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);
			postreqAction.ExecuteAfterActivatingEntity_ForTest(child2);
			AssertResourceDependencyExists(child1, child2, diagram, true);

			networkViewModel.SelectEntities(new[] { child1, child2 });
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("A dependency already exists between these two shapes.", action.IsEnabledAfterActivatingEntity_ForTest(child1));

			networkViewModel.SelectEntities(new[] { child1, child3 });
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(child1));
		}

		protected override void TestGetNameCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);

			diagram.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";

			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(networkViewModel, prereqAction, postreqAction);

			networkViewModel.SelectSingleEntity(shape1);
			AssertEquals(false, prereqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals(false, postreqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);

			networkViewModel.SelectSingleEntity(shape2);
			AssertEquals(false, prereqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals(false, postreqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals("Create Resource Dependency", prereqAction.GetNameAfterActivatingEntity_ForTest(shape1));
			AssertEquals("Create Resource Dependency", postreqAction.GetNameAfterActivatingEntity_ForTest(shape1));

			networkViewModel.SelectEntities(new[] { shape1, shape2 });
			AssertEquals(true, prereqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals(true, postreqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals("Create Resource Dependency from [shape1] to [shape2]", prereqAction.GetNameAfterActivatingEntity_ForTest(shape1));
			AssertEquals("Create Resource Dependency from [shape2] to [shape1]", postreqAction.GetNameAfterActivatingEntity_ForTest(shape1));

			networkViewModel.SelectSingleEntity(shape1);
			AssertEquals(false, prereqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals(false, postreqAction.IsEnabledAfterActivatingEntity_ForTest(shape1).IsAllowed);
			AssertEquals("Create Resource Dependency", prereqAction.GetNameAfterActivatingEntity_ForTest(shape1));
			AssertEquals("Create Resource Dependency", postreqAction.GetNameAfterActivatingEntity_ForTest(shape1));
		}

		[ExpectNoExceptions]
		public void TestGetName_ShouldNotRaiseException()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);

			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var shape3 = networkViewModel.CreateNewShape(diagram);

			shape1.Name = "shape1";
			shape2.Name = "shape2";
			shape3.Name = "shape3";

			var prereqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
			networkViewModel.SelectEntities(new[] { shape1, shape2, shape3 });
			AssertEquals("Create Resource Dependency", prereqAction.GetNameAfterActivatingEntity_ForTest(shape1));

			var postreqAction = new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PostRequisite);
			networkViewModel.SelectEntities(new[] { shape2, shape3 });
			AssertEquals("Create Resource Dependency", postreqAction.GetNameAfterActivatingEntity_ForTest(shape1));
		}

		[ExpectNoExceptions]
		public void TestUpdatingActionProperties_ShouldNotRaiseException_WhenRemovingMultipleShapes()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.SwitchToScaled();

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			networkViewModel.Refresh();

			var shapeEntity1 = shape1.AsEntity(network);
			var shapeEntity2 = shape2.AsEntity(network);

			var actionWhichUpdatesItsPropertiesOnNetworkEvents = GetAction(networkViewModel);

			networkViewModel.SelectEntities(new INetworkEntity[] { shapeEntity1, shapeEntity2 });

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shapeEntity2))
			{
				NetworkActionAccessibilityTest.AssertAllowed("Updatable action should be applicable otherwise it will use default property values instead of recalculating them", actionWhichUpdatesItsPropertiesOnNetworkEvents.IsApplicable());

				var removeAction = new RemoveAndDeleteAction(networkViewModel);
				NetworkActionAccessibilityTest.AssertAllowed(removeAction.CheckCanStartExecution());
				removeAction.Execute();
				Assert(!networkViewModel.Nodes.Any());
			}
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Adds a resource dependency between two selected shapes.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("ArrowRight");
		}

		protected override CreateResourceDependencyAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CreateResourceDependencyAction(networkViewModel, DependencyDirection.PreRequisite);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(CommonNetworkActionExecutionStrategy);
	}
}
