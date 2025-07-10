using System;
using System.Globalization;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using static Enterprise.BufferManagement.NetworkVisualisation.Business.SetStatusAction;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(SetStatusAction))]
	class SetStatusActionTest : JobNetworkActionTestCase<SetStatusAction>
	{
		#region Child Actions

		public void TestAction_RecursToChildren()
		{
			var diagram = CreateDiagram(Factory);
			var child = CreateShape(diagram);
			var grandChild = CreateShape(child);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var suspended = new ShapeStatusList().Cast<CodeDescriptionPair>().Single(s => s.Code == ShapeStatusList.Codes.Suspended).CodeAndDescription;

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == suspended).AsJobNetworkAction().Execute();
			}

			AssertEquals(ShapeStatusList.Codes.Suspended, diagram.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Suspended, child.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Suspended, grandChild.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Unknown, annotation.BNS_Status);

			foreach (var pair in SetStatusAction.GetOrderedStatuses())
			{
				using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, child))
				{
					action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().Execute();
				}

				AssertEquals(ShapeStatusList.Codes.Suspended, diagram.BNS_Status);
				AssertEquals(pair.Code, child.BNS_Status);
				AssertEquals(pair.Code, grandChild.BNS_Status);
				AssertEquals(ShapeStatusList.Codes.Unknown, annotation.BNS_Status);
			}

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				action.GetChildActions().Single(a => a.AsJobNetworkAction().IsActivated()).AsJobNetworkAction().Execute();
			}

			AssertEquals("Executing the ticked action should set all statuses back to Unknown.", ShapeStatusList.Codes.Unknown, diagram.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Unknown, child.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Unknown, grandChild.BNS_Status);
			AssertEquals(ShapeStatusList.Codes.Unknown, annotation.BNS_Status);
		}

		public void TestActivated()
		{
			var diagram = CreateDiagram(Factory);
			var child = CreateShape(diagram);
			var grandChild = CreateShape(child);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var suspended = new ShapeStatusList().Cast<CodeDescriptionPair>().Single(s => s.Code == ShapeStatusList.Codes.Suspended).CodeAndDescription;

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals(false, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == suspended).AsJobNetworkAction().Execute();

				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals("The suspended action should be ticked.", pair.Code == ShapeStatusList.Codes.Suspended, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				AssertEquals(string.Format(CultureInfo.InvariantCulture, "Would you also like to mark all child unlinked shapes of New Diagram as {0}?", ShapeStatusList.Descriptions.Suspended), UnitTestUserNotification.Instance.LastMessage.Text);

				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == suspended).AsJobNetworkAction().Execute();

				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals("Firing again should untick.", false, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				AssertEquals("Would you like to clear the status of all unlinked child shapes of New Diagram?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActivated_WhenNoChildren_DoNotShowNotification()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var suspended = new ShapeStatusList().Cast<CodeDescriptionPair>().Single(s => s.Code == ShapeStatusList.Codes.Suspended).CodeAndDescription;

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals(false, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == suspended).AsJobNetworkAction().Execute();

				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals("The suspended action should be ticked.", pair.Code == ShapeStatusList.Codes.Suspended, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == suspended).AsJobNetworkAction().Execute();

				foreach (var pair in SetStatusAction.GetOrderedStatuses())
				{
					AssertEquals("Firing again should untick.", false, action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == pair.CodeAndDescription).AsJobNetworkAction().IsActivated());
				}

				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void TestChildActionsCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var shape1 = networkViewModel.CreateNewShape(diagram);

			var action = GetAction(networkViewModel);
			var actions = action.GetChildActionsAfterActivatingEntity_ForTest(shape1);

			AssertArrayEqualsByElements(SetStatusAction.GetOrderedStatuses().Select(s => s.CodeAndDescription).ToArray(), actions.Select(a => a.AsJobNetworkAction().GetNameAfterActivatingEntity_ForTest(shape1)).ToArray());
		}

		public void TestShouldSetStatusToMutlipleSelectedShapes()
		{
			var diagram = CreateDiagram(Factory, name: "Root", isScaled: true);

			var shape1 = CreateShape(diagram, "Shape1");
			var shape2 = CreateShape(diagram, "Shape2");
			var shape3 = CreateShape(diagram, "Shape3");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);

			var action = GetAction(networkViewModel);
			var wrkChildAction = action.GetChildActions().Single(a => a.GetName() == "WRK - Working");
			var susChildAction = action.GetChildActions().Single(a => a.GetName() == "SUS - Suspended (Temporary Pause)");

			networkViewModel.SelectEntities(new INetworkEntity[] { entity1, entity2, entity3 });

			NetworkActionAccessibilityTest.AssertAllowed(wrkChildAction.CheckCanStartExecution());
			AssertEquals(false, wrkChildAction.IsActivated());
			wrkChildAction.Execute();

			AssertEquals("WRK", shape1.BNS_Status);
			AssertEquals("WRK", shape2.BNS_Status);
			AssertEquals("WRK", shape3.BNS_Status);
			AssertEquals(true, wrkChildAction.IsActivated());

			NetworkActionAccessibilityTest.AssertAllowed(susChildAction.CheckCanStartExecution());
			AssertEquals(false, susChildAction.IsActivated());
			susChildAction.Execute();

			AssertEquals("SUS", shape1.BNS_Status);
			AssertEquals("SUS", shape2.BNS_Status);
			AssertEquals("SUS", shape3.BNS_Status);
			AssertEquals(true, susChildAction.IsActivated());

			NetworkActionAccessibilityTest.AssertAllowed(susChildAction.CheckCanStartExecution());
			susChildAction.Execute();

			AssertEquals("UNK", shape1.BNS_Status);
			AssertEquals("UNK", shape2.BNS_Status);
			AssertEquals("UNK", shape3.BNS_Status);
			AssertEquals(false, susChildAction.IsActivated());
		}

		public void TestShouldSetStatusToBothParentAndChildSelectedShapes_WithoutPrompting()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var parentShape = CreateShape(diagram, "Parent Shape");
			var childShape1 = CreateShape(parentShape, "Child Shape 1");
			var childShape2 = CreateShape(parentShape, "Child Shape 2");

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(c => c.UserInteractionImplementor).Returns(interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var parentEntity = parentShape.AsEntity(network);
			var childEntity1 = childShape1.AsEntity(network);
			var childEntity2 = childShape2.AsEntity(network);

			var action = GetAction(networkViewModel);
			var wrkChildAction = action.GetChildActions().Single(a => a.GetName() == "WRK - Working");
			var susChildAction = action.GetChildActions().Single(a => a.GetName() == "SUS - Suspended (Temporary Pause)");

			networkViewModel.SelectEntities(new INetworkEntity[] { parentEntity, childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(wrkChildAction.CheckCanStartExecution());
			wrkChildAction.Execute();

			AssertEquals("WRK", parentShape.BNS_Status);
			AssertEquals("WRK", childShape1.BNS_Status);
			AssertEquals("WRK", childShape2.BNS_Status);

			NetworkActionAccessibilityTest.AssertAllowed(susChildAction.CheckCanStartExecution());
			susChildAction.Execute();

			AssertEquals("SUS", parentShape.BNS_Status);
			AssertEquals("SUS", childShape1.BNS_Status);
			AssertEquals("SUS", childShape2.BNS_Status);

			NetworkActionAccessibilityTest.AssertAllowed(susChildAction.CheckCanStartExecution());
			susChildAction.Execute();

			AssertEquals("UNK", parentShape.BNS_Status);
			AssertEquals("UNK", childShape1.BNS_Status);
			AssertEquals("UNK", childShape2.BNS_Status);
			interactionImplementor.Verify(i => i.HasUserAnsweredYes(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

			controller.Verify(c => c.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never());
		}

		public void TestShouldSetStatusToBothParentAndChildSelectedShapes_WithoutPrompting_WhenSomeChildShapesAlreadyHaveThisStatus()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var parentShape = CreateShape(diagram, "Parent Shape");
			var childShape1 = CreateShape(parentShape, "Child Shape 1");
			var childShape2 = CreateShape(parentShape, "Child Shape 2");

			parentShape.BNS_Status = "SUS";
			childShape1.BNS_Status = "SUS";
			childShape2.BNS_Status = "WRK";

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(c => c.UserInteractionImplementor).Returns(interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var parentEntity = parentShape.AsEntity(network);
			var childEntity1 = childShape1.AsEntity(network);
			var childEntity2 = childShape2.AsEntity(network);

			var action = GetAction(networkViewModel);
			var wrkChildAction = action.GetChildActions().Single(a => a.GetName() == "WRK - Working");

			networkViewModel.SelectEntities(new INetworkEntity[] { parentEntity, childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(wrkChildAction.CheckCanStartExecution());
			wrkChildAction.Execute();

			AssertEquals("WRK", parentShape.BNS_Status);
			AssertEquals("WRK", childShape1.BNS_Status);
			AssertEquals("WRK", childShape2.BNS_Status);
			interactionImplementor.Verify(i => i.HasUserAnsweredYes(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowMessage(It.IsAny<string>(), It.IsAny<string>()), Times.Never());
			interactionImplementor.Verify(i => i.ShowError(It.IsAny<string>(), It.IsAny<string>()), Times.Never());

			controller.Verify(c => c.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never());
		}

		public void TestShouldSetStatusToGrandParentAndAllChildren_WhenhGrandParentAndGrandChildAreSelected()
		{
			var diagram = CreateDiagram(Factory, name: "Root");
			diagram.SwitchToScaled();

			var grandParentShape = CreateShape(diagram, "Grand Parent Shape");
			var parentShape = CreateShape(grandParentShape, "Parent Shape");
			var grandChildShape = CreateShape(parentShape, "Grand Child Shape");

			var interactionImplementor = new Mock<IBMNetworkUserInteractionImplementor>();
			var controller = new Mock<IBMNetworkEntityController>();

			interactionImplementor.Setup(i => i.HasUserAnsweredYes(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
			controller.Setup(c => c.UserInteractionImplementor).Returns(interactionImplementor.Object);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var grandParentEntity = grandParentShape.AsEntity(network);
			var parentEntity = parentShape.AsEntity(network);
			var grandChildEntity = grandChildShape.AsEntity(network);

			var action = GetAction(networkViewModel);
			var wrkChildAction = action.GetChildActions().Single(a => a.GetName() == "WRK - Working");

			networkViewModel.SelectEntities(new INetworkEntity[] { grandParentEntity, grandChildEntity });

			NetworkActionAccessibilityTest.AssertAllowed(wrkChildAction.CheckCanStartExecution());
			wrkChildAction.Execute();

			AssertEquals("WRK", grandParentShape.BNS_Status);
			AssertEquals("WRK", parentShape.BNS_Status);
			AssertEquals("WRK", grandChildShape.BNS_Status);
		}

		#endregion

		#region Buffer Penetration

		[TestDate(2015, 7, 14)]
		public void TestBufferPenetration_WhenShapeStatusSetManually_ShouldBeNoPenetration()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape = NetworkTestCase.CreateShape(diagram);

			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddDays(-2);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var viewModel = new BufferPenetrationViewModel(shape);

			AssertEquals(1, viewModel.PenetrationItemsCollection.Count);
			AssertEquals("212 %", viewModel.PenetrationItemsCollection[0].PenetrationPercent);

			var completeActionCodes = new[] { ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled };

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, shape))
			{
				foreach (var childAction in GetAction(networkViewModel).GetChildActions())
				{
					childAction.AsJobNetworkAction().Execute();
					Factory.Save();

					viewModel = new BufferPenetrationViewModel(shape);

					AssertEquals(1, viewModel.PenetrationItemsCollection.Count);

					var actionName = childAction.AsJobNetworkAction().GetName().ToString();

					if (completeActionCodes.Any(s => actionName.Contains(s)))
					{
						AssertEquals($"Should be no buffer penetration when shape has {actionName} status", string.Empty, viewModel.PenetrationItemsCollection[0].PenetrationPercent);
					}
					else
					{
						AssertEquals($"Should be buffer penetration when shape has {actionName} status", "212 %", viewModel.PenetrationItemsCollection[0].PenetrationPercent);
					}
				}
			}
		}

		#endregion

		#region Leveling Rules

		[TestDate(2019, 1, 7)]
		public void TestLevelingRules_WhenShapeStatusSetToClosedOrCancelledManually_ShouldNotConsiderShape()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			diagramShape.BackColor = string.Empty;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagramEntity = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, name: "Con the Fruiterer");

			var childShape1 = NetworkTestCase.CreateShape(diagramEntity, "Shape 1");
			childShape1.Width = 100;
			childShape1.Height = 100;
			childShape1.X = 0;
			childShape1.Y = 300;

			var childShape2 = NetworkTestCase.CreateShape(diagramEntity, "Shape 2");
			childShape2.Width = 100;
			childShape2.Height = 100;
			childShape2.X = 0;
			childShape2.Y = 300;

			descriptor.GetScaleSetForColumns(10);

			var childEntity1 = network.Entities.Single(e => e.Name == "Shape 1");
			var childEntity2 = network.Entities.Single(e => e.Name == "Shape 2");

			networkViewModel.CreateNodeViewModelsForEntitiesToLetNetworkActionsWork();

			CombineAssertions("Both nodes should initially mutually violate each other", () =>
			{
				AssertEquals(1, childEntity1.EntityNotifications.Count());
				AssertEquals(1, childEntity2.EntityNotifications.Count());

				AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity1.EntityNotifications.First().Message);
				AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity2.EntityNotifications.First().Message);
			});

			var action = GetAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, childShape1))
			{
				var closed = new ShapeStatusList().Cast<CodeDescriptionPair>().Single(s => s.Code == ShapeStatusList.Codes.Closed).CodeAndDescription;
				var cancelled = new ShapeStatusList().Cast<CodeDescriptionPair>().Single(s => s.Code == ShapeStatusList.Codes.Cancelled).CodeAndDescription;

				var closedAction = action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == closed).AsJobNetworkAction();
				var cancelledAction = action.GetChildActions().Single(a => a.AsJobNetworkAction().GetName() == cancelled).AsJobNetworkAction();

				closedAction.Execute();
				descriptor.GetScaleSetForColumns(10);

				CombineAssertions("Since we have manually closed the first node, the leveling rule should no longer apply", () =>
				{
					AssertEquals(0, childEntity1.EntityNotifications.Count());
					AssertEquals(0, childEntity2.EntityNotifications.Count());
				});

				closedAction.Execute();
				descriptor.GetScaleSetForColumns(10);

				CombineAssertions("Unclosing the first node should make the leveling rule apply again", () =>
				{
					AssertEquals(1, childEntity1.EntityNotifications.Count());
					AssertEquals(1, childEntity2.EntityNotifications.Count());

					AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity1.EntityNotifications.First().Message);
					AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity2.EntityNotifications.First().Message);
				});

				cancelledAction.Execute();
				descriptor.GetScaleSetForColumns(10);

				CombineAssertions("Since we have manually cancelled the first node, the leveling rule should no longer apply", () =>
				{
					AssertEquals(0, childEntity1.EntityNotifications.Count());
					AssertEquals(0, childEntity2.EntityNotifications.Count());
				});

				cancelledAction.Execute();
				descriptor.GetScaleSetForColumns(10);

				CombineAssertions("Uncancelling the first node should make the leveling rule apply again (#MLRAA)", () =>
				{
					AssertEquals(1, childEntity1.EntityNotifications.Count());
					AssertEquals(1, childEntity2.EntityNotifications.Count());

					AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity1.EntityNotifications.First().Message);
					AssertEquals("Warning - Leveling Rule: There are more shapes in this time slot than are allowed by the Leveling Rule named [Con the Fruiterer].", childEntity2.EntityNotifications.First().Message);
				});
			}
		}

		#endregion

		#region Implementation

		protected override void TestExecuteCore()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			AssertNull(action.ExecuteForEntityWithoutAccessCheck(diagram));
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
			AssertEquals("Set Status", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Marks this shape with a status", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Status");
		}

		protected override SetStatusAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new SetStatusAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(MultipleSelectedEntitiesExecutionStrategy);

		#endregion
	}

	[TestedType(typeof(ChildStatusAction))]
	class ChildStatusActionTest : JobNetworkActionTestCase<ChildStatusAction>
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

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("ASN - Assigned", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Set status to Assigned.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		public void TestShouldGetActivatedAfterExecution()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var entity = childShape.AsEntity(network);
			var action = GetAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, entity))
			{
				AssertEquals("Precondition", false, action.IsActivated());

				NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
				action.Execute();

				AssertEquals(true, action.IsActivated());
			}
		}

		protected override ChildStatusAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ChildStatusAction(networkViewModel, new CodeDescriptionPair(ShapeStatusList.Codes.Assigned, ShapeStatusList.Descriptions.Assigned));
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Statuses_ASN");
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(SetStatusActionExecutionStrategy);
	}
}
