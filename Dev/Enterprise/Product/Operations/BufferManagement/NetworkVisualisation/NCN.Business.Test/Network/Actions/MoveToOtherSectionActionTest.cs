using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class MoveToOtherSectionActionTest : NetworkTestCase
	{
		#region Name & Description

		public void TestActionName_ForScheduledSection()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			AssertEquals("Move to Non-Scheduled Section", action.GetName());
		}

		public void TestActionName_ForNonScheduledSection()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			shape.IsNonScheduled = true;
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			AssertEquals("Move to Scheduled Section", action.GetName());
		}

		public void TestDescription_ForScheduledSection()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			AssertEquals("Moves this item to the non-scheduled section of the diagram.", action.GetDescription());
		}

		public void TestDescription_ForNonScheduledSection()
		{
			var diagram = CreateDiagram(Factory);
			var shape = CreateShape(diagram);
			shape.IsNonScheduled = true;
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			AssertEquals("Moves this item to the scheduled section of the diagram.", action.GetDescription());
		}

		#endregion

		#region Action Execution

		public void TestMoveToNonScheduledSection_ShouldMakeEntityNonScheduled()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);
			AssertEquals(false, shape.IsNonScheduled);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = (JobNetwork)viewModel.Network;
			var entity = shape.AsEntity(network);
			viewModel.SelectSingleEntity(entity);
			var action = new MoveToOtherSectionAction(viewModel);

			action.ExecuteForEntityWithoutAccessCheck(entity);

			AssertEquals(true, shape.IsNonScheduled);
		}

		public void TestMoveToScheduledSection_ShouldMakeEntityScheduled()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);
			shape.IsNonScheduled = true;
			AssertEquals(true, shape.IsNonScheduled);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = (JobNetwork)viewModel.Network;
			var entity = shape.AsEntity(network);
			viewModel.SelectSingleEntity(entity);
			var action = new MoveToOtherSectionAction(viewModel);

			action.ExecuteForEntityWithoutAccessCheck(entity);

			AssertEquals(false, shape.IsNonScheduled);
		}

		public void TestExecute_ShouldReturnNull()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = (JobNetwork)viewModel.Network;
			var entity = shape.AsEntity(network);
			viewModel.SelectSingleEntity(entity);
			var action = new MoveToOtherSectionAction(viewModel);

			var result = action.ExecuteForEntityWithoutAccessCheck(entity);
			AssertNull("The execution must return null so that the diagram controls don't treat the shape as if it is new. SAD!", result);
		}

		#endregion

		#region Applicability/Enabled

		public void TestAction_ForShapesOnDiagramWithoutNonScheduledSection_ShouldNotBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			AssertEquals(false, diagram.ShouldShowNonScheduledSection);

			var shape = CreateShape(diagram);
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(shape).IsAllowed;
			AssertEquals("The action should not be allowed for shapes on diagrams that aren't showing the non-scheduled section. SAD!", false, isAllowed);
		}

		public void TestAction_ForNonRootShapes_ShouldBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(shape).IsAllowed;
			AssertEquals("The action should be allowed for regular shapes. SAD!", true, isAllowed);
		}

		public void TestAction_ForAnnotations_ShouldBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram, shapeType: DiagramShapeTypeList.Codes.Annotation);
			var viewModel = CreateNetworkViewModel(diagram);
			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(shape).IsAllowed;
			AssertEquals("The action should be allowed for annotations. SAD!", true, isAllowed);
		}

		public void TestAction_ForRootDiagrams_ShouldNotBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(diagram).IsAllowed;
			AssertEquals("The action should not be allowed for root diagrams. SAD!", false, isAllowed);
		}

		public void TestAction_ForShapesShownAsDiagrams_ShouldNotBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);
			var network = CreateNetwork(shape);
			var viewModel = CreateNetworkViewModel(shape);
			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(shape.AsEntity(network)).IsAllowed;
			AssertEquals("The action should not be allowed for regular shapes that are shown as diagrams. SAD!", false, isAllowed);
		}

		public void TestAction_ForPinnedShape_ShouldBeApplicableButNotEnabled()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var viewModel = CreateNetworkViewModel(diagram);

			var shape = CreateShape(diagram);
			shape.PinShape(viewModel);

			var entity = shape.AsEntity(viewModel.GetJobNetwork());
			viewModel.AddAllEntitiesIntoDiagram(new[] { entity }, new Location(100, 100));
			viewModel.SelectSingleEntity(entity);

			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(entity).IsAllowed;
			AssertEquals("The action should be applicable for pinned shapes so that it's visible in the context menu even though it's disabled. SAD!", true, isAllowed);

			var enabledResult = action.IsEnabledForEntity(entity);
			AssertEquals("The action should not be enabled for pined shapes. SAD!", false, enabledResult.IsAllowed);
			AssertContainsExactElementsInAnyOrder(new[] { "The action is not applicable to pinned shapes." }, enabledResult.DenialReasons.Select(x => x.Explanation));
		}

		public void TestAction_ForApprovedShape_ShouldBeApplicableButNotEnabled()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;
			var viewModel = CreateNetworkViewModel(diagram);

			var shape = CreateShape(diagram);

			var approveAction = new ApproveDiagramAction(viewModel);
			approveAction.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(true, shape.IsApproved);

			var entity = shape.AsEntity(viewModel.GetJobNetwork());
			viewModel.AddAllEntitiesIntoDiagram(new[] { entity }, new Location(100, 100));
			viewModel.SelectSingleEntity(entity);

			var action = new MoveToOtherSectionAction(viewModel);

			var isAllowed = action.IsApplicableToEntity(entity).IsAllowed;
			AssertEquals("The action should be applicable for approved shapes so that it's visible in the context menu even though it's disabled. SAD!", true, isAllowed);

			var enabledResult = action.IsEnabledForEntity(entity);
			AssertEquals("The action should not be enabled for approved shapes. SAD!", false, enabledResult.IsAllowed);
			AssertContainsExactElementsInAnyOrder(new[] { "The shape should not be approved." }, enabledResult.DenialReasons.Select(x => x.Explanation));
		}

		public void TestAction_ForChildShape_ShouldNotBeApplicable()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape = CreateShape(diagram);
			var childShape = CreateShape(shape);
			var grandchildShape = CreateShape(childShape);
			var viewModel = CreateNetworkViewModel(diagram);

			var action = new MoveToOtherSectionAction(viewModel);

			viewModel.SelectSingleEntity(shape.AsEntity((JobNetwork)viewModel.Network));
			var isAllowed = action.IsApplicableToEntity(shape).IsAllowed;
			AssertEquals("The action should be allowed for top-level shapes that have children. SAD!", true, isAllowed);

			viewModel.SelectSingleEntity(childShape.AsEntity((JobNetwork)viewModel.Network));
			isAllowed = action.IsApplicableToEntity(childShape).IsAllowed;
			AssertEquals("The action should not be allowed for child shapes, even if they have children of their own. SAD!", false, isAllowed);

			viewModel.SelectSingleEntity(grandchildShape.AsEntity((JobNetwork)viewModel.Network));
			isAllowed = action.IsApplicableToEntity(grandchildShape).IsAllowed;
			AssertEquals("The action should not be allowed for child shapes, even if they don't have any children. SAD!", false, isAllowed);
		}

		public void TestAction_ForBuffer_ShouldNotBeApplicable()
		{
			var diagramShape = CreateDiagram(Factory, isScaled: true);
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var shape1 = CreateShape(diagram, name: "Exersize Daily");
			var shape2 = CreateShape(diagram, name: "Eat Healthily");
			var shape3 = CreateShape(diagram, name: "Die Anyway");

			shape1.MakeVisiblePrerequisiteOf(shape3);
			shape2.MakeVisiblePrerequisiteOf(shape3);

			shape1.Width = 200;
			shape2.Width = 100;
			shape3.Width = 200;

			network.RefreshSchedules();
			var buffer = networkViewModel.SuggestAndAcceptAllBuffers().Single(s => s.BufferType == BufferTypeList.Codes.Feeding).AsEntity(network);

			var action = new MoveToOtherSectionAction(networkViewModel);
			var isAllowed = action.IsApplicableToEntity(buffer).IsAllowed;
			AssertEquals("The action should not be allowed for buffers. SAD!", false, isAllowed);
		}

		#endregion

		#region Removal of Arrows

		public void TestAction_ForShapeWithArrows_ShouldPromptUserToConfirmDelete_AnswerOk()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 4");

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);
			var shape4 = CreateShape(workflow4, diagram);
			var attachment1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var attachment2 = shape1.MakeVisiblePrerequisiteOf(shape3, diagram);
			var attachment3 = shape3.MakeVisiblePrerequisiteOf(shape4, diagram);

			AssertEquals(2, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(0, workflow2.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow3.LinksFromMeToOthers.Count());
			AssertEquals(0, workflow4.LinksFromMeToOthers.Count());

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);

			// Not adding a UnitTestUserNotification answer to prove that OK is the default option.
			action.ExecuteAfterActivatingEntity_ForTest(shape1);

			AssertEquals(@"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("The user agreed, so the arrows between the target shape and other shapes should be deleted. SAD!", true, attachment1.IsDeleted);
			AssertEquals("The user agreed, so the arrows between the target shape and other shapes should be deleted. SAD!", true, attachment2.IsDeleted);
			AssertEquals("The third attachment didn't touch the target shape, so it should have been left alone. SAD!", false, attachment3.IsDeleted);

			AssertEquals("The prereq links should not be deleted. SAD!", 2, workflow1.LinksFromMeToOthers.Count());
			AssertEquals("The prereq links for unrelated shapes should not have been deleted. SAD!", 1, workflow3.LinksFromMeToOthers.Count());
		}

		public void TestAction_ForShapeWithArrows_ShouldPromptUserToConfirmDelete_AnswerCancel()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 2");
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 3");
			var workflow4 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow 4");

			var diagram = CreateDiagram(jobHeader, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);
			var shape4 = CreateShape(workflow4, diagram);
			var attachment1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var attachment2 = shape1.MakeVisiblePrerequisiteOf(shape3, diagram);
			var attachment3 = shape3.MakeVisiblePrerequisiteOf(shape4, diagram);

			AssertEquals(2, workflow1.LinksFromMeToOthers.Count());
			AssertEquals(0, workflow2.LinksFromMeToOthers.Count());
			AssertEquals(1, workflow3.LinksFromMeToOthers.Count());
			AssertEquals(0, workflow4.LinksFromMeToOthers.Count());

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);
			NetworkActionTestHelper.MakeActionsRefreshOnSelectionChanged(viewModel, action);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			action.Execute();
			action.ExecuteAfterActivatingEntity_ForTest(shape1);

			AssertEquals(@"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("The user did not agree, so the arrows between the target shape and other shapes should not be deleted. SAD!", false, attachment1.IsDeleted);
			AssertEquals("The user did not agree, so the arrows between the target shape and other shapes should not be deleted. SAD!", false, attachment2.IsDeleted);
			AssertEquals("The third attachment didn't touch the target shape, so it should have been left alone. SAD!", false, attachment3.IsDeleted);

			AssertEquals("The prereq links should not have been deleted as well. SAD!", 2, workflow1.LinksFromMeToOthers.Count());
			AssertEquals("The prereq links for unrelated shapes should not have been deleted. SAD!", 1, workflow3.LinksFromMeToOthers.Count());
		}

		public void TestAction_WithArrowsWhollyContainedInsideChildShapesOfSelectedEntity_ShouldNotBeRemoved_AndUserShouldNotBePropmpted()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var topShape = CreateShape(diagram);
			var middleShape1 = CreateShape(topShape);
			var middleShape2 = CreateShape(topShape);
			var bottomShape1 = CreateShape(middleShape1);
			var bottomShape2 = CreateShape(middleShape2);

			var attachment1 = middleShape1.MakeVisiblePrerequisiteOf(middleShape2, diagram);
			var attachment2 = bottomShape1.MakeVisiblePrerequisiteOf(bottomShape2, diagram);
			var attachment3 = bottomShape1.MakeVisiblePrerequisiteOf(middleShape2, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);
			action.ExecuteAfterActivatingEntity_ForTest(topShape);

			AssertEquals("The selected shape should have been moved to the non-scheduled section. SAD!", true, topShape.IsNonScheduled);
			AssertEquals("The child shapes should have been moved to the non-scheduled section. SAD!", true, middleShape1.IsNonScheduled);
			AssertEquals("The child shapes should have been moved to the non-scheduled section. SAD!", true, middleShape2.IsNonScheduled);
			AssertEquals("The child shapes should have been moved to the non-scheduled section. SAD!", true, bottomShape1.IsNonScheduled);
			AssertEquals("The child shapes should have been moved to the non-scheduled section. SAD!", true, bottomShape2.IsNonScheduled);

			AssertNullOrEmpty("No message should be shown to the user because all the shapes with arrows can be moved together inside the moving shape. SAD!", UnitTestUserNotification.Instance.LastMessage?.Text);
			AssertEquals("The arrows should not have been deleted. SAD!", false, attachment1.IsDeleted);
			AssertEquals("The arrows should not have been deleted. SAD!", false, attachment2.IsDeleted);
			AssertEquals("The arrows should not have been deleted. SAD!", false, attachment3.IsDeleted);
		}

		public void TestAction_ForShapeWithChildren_WithArrowsFromChildrenToShapesOutsideSelectedEntity_ShouldPromptAndRemove()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var parentShape = CreateShape(diagram);
			var childShape = CreateShape(parentShape);
			var standaloneShape = CreateShape(diagram);

			var attachment = childShape.MakeVisiblePrerequisiteOf(standaloneShape, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			action.ExecuteAfterActivatingEntity_ForTest(parentShape);

			AssertEquals(@"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The arrow goes from a child of the selected shape to a shape which is not a child of the selected shape, so it should be deleted. SAD!", true, attachment.IsDeleted);
		}

		public void TestAction_ForShapeWithChildrenAndCrazyArrowsEverywhere_WhenGivenUserConsent_ShouldDeleteOnlyNecessaryArrows()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var parentShape = CreateShape(diagram, "Parent Shape");
			var childShape1 = CreateShape(parentShape, "Child Shape 1");
			var childShape2 = CreateShape(parentShape, "Child Shape 2");
			var standaloneShape = CreateShape(diagram, "Standalone Shape");

			var attachment1 = parentShape.MakeVisiblePrerequisiteOf(standaloneShape, diagram);
			var attachment2 = childShape1.MakeVisiblePrerequisiteOf(standaloneShape, diagram);
			var attachment3 = childShape1.MakeVisiblePrerequisiteOf(childShape2, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var action = new MoveToOtherSectionAction(viewModel);

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			action.ExecuteAfterActivatingEntity_ForTest(parentShape);

			AssertEquals(@"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("The arrow goes from the selected shape to a shape which is not a child of the selected shape, so it should be deleted. SAD!", true, attachment1.IsDeleted);
			AssertEquals("The arrow goes from a child of the selected shape to a shape which is not a child of the selected shape, so it should be deleted. SAD!", true, attachment2.IsDeleted);
			AssertEquals("The arrow is contained within the parent shape that is moving so it needn't be deleted. SAD!", false, attachment3.IsDeleted);
		}

		public void TestAction_ForMultipleSelectedShapesAndArrows_ShouldMoveAllSelectedShapesAndRemoveArrows()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = CreateShape(diagram, "First the Worst");
			var shape2 = CreateShape(diagram, "Second the Best");
			var shape3 = CreateShape(diagram, "Third the One with the Hairy Chest");
			var shape4 = CreateShape(diagram, "Fourth the One who is Happy to Place");

			var attachment1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var attachment2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var attachment3 = shape3.MakeVisiblePrerequisiteOf(shape4, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);
			var entity4 = shape4.AsEntity(network);

			var action = new MoveToOtherSectionAction(viewModel);

			viewModel.SelectEntities(new INetworkEntity[] { entity1, entity2 });

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			action.ExecuteAfterActivatingEntity_ForTest(shape1);

			AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Select(m =>
				m.Text == @"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?").Count());

			AssertEquals("The first shape should be in the non-scheduled section, and yet...", true, entity1.IsNonScheduled);
			AssertEquals("The second shape should be in the non-scheduled section, and yet...", true, entity2.IsNonScheduled);
			AssertEquals("The third shape should NOT be in the non-scheduled section, and yet...", false, entity3.IsNonScheduled);
			AssertEquals("The fourth shape should NOT be in the non-scheduled section, and yet...", false, entity4.IsNonScheduled);

			AssertEquals("This arrow goes from the first selected shape to the second selected shape, so it should not be deleted, and yet...", false, attachment1.IsDeleted);
			AssertEquals("This arrow goes from the second selected shape to the first non-selected shape, so it should be deleted, and yet...", true, attachment2.IsDeleted);
			AssertEquals("This arrow goes from the first non-selected shape to the second non-selected shape, so it should not be deleted, and yet...", false, attachment3.IsDeleted);
		}

		public void TestAction_ForMultipleSelectedShapesAndArrows_ShouldMoveAllSelectedShapesAndPreserveArrowsBetweenMovedShapes()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = CreateShape(diagram, "First the Worst");
			var shape2 = CreateShape(diagram, "Second the Best");
			var shape3 = CreateShape(diagram, "Third the One with the Hairy Chest");
			var shape4 = CreateShape(diagram, "Fourth the One who is Happy to Place");

			var attachment1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var attachment2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var attachment3 = shape3.MakeVisiblePrerequisiteOf(shape4, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);
			var entity4 = shape4.AsEntity(network);

			var action = new MoveToOtherSectionAction(viewModel);

			viewModel.SelectEntities(new INetworkEntity[] { entity2, entity3 });

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			action.ExecuteAfterActivatingEntity_ForTest(shape3);

			AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Select(m =>
				m.Text == @"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?").Count());

			AssertEquals("The first shape should NOT be in the non-scheduled section, and yet...", false, entity1.IsNonScheduled);
			AssertEquals("The second shape should be in the non-scheduled section, and yet...", true, entity2.IsNonScheduled);
			AssertEquals("The third shape should be in the non-scheduled section, and yet...", true, entity3.IsNonScheduled);
			AssertEquals("The fourth shape should NOT be in the non-scheduled section, and yet...", false, entity4.IsNonScheduled);

			AssertEquals("This arrow goes from 1 -> 2 so it should be deleted, and yet...", true, attachment1.IsDeleted);
			AssertEquals("This arrow goes from 2 -> 3, so it should NOT be deleted, and yet...", false, attachment2.IsDeleted);
			AssertEquals("This arrow goes from 3 -> 4, so it should be deleted, and yet...", true, attachment3.IsDeleted);
		}

		public void TestAction_ForMultipleSelectedShapesAndArrows_WithChildShapes()
		{
			var diagram = CreateDiagram(Factory, isScaled: true);
			diagram.ShouldShowNonScheduledSection = true;

			var shape1 = CreateShape(diagram, "Parent");
			var childShape1 = CreateShape(shape1, "Child 1");
			var childShape2 = CreateShape(shape1, "Child 2");

			var shape2 = CreateShape(diagram, "Move");
			var shape3 = CreateShape(diagram, "Stay 1");
			var shape4 = CreateShape(diagram, "Stay 2");

			var attachment1 = shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			var attachment2 = shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			var attachment3 = childShape1.MakeVisiblePrerequisiteOf(childShape2, shape1);
			var attachment4 = childShape2.MakeVisiblePrerequisiteOf(shape4, diagram);

			var viewModel = CreateNetworkViewModel(diagram);
			var network = viewModel.GetJobNetwork();

			var entity1 = shape1.AsEntity(network);
			var entityChild1 = childShape1.AsEntity(network);
			var entityChild2 = childShape2.AsEntity(network);
			var entity2 = shape2.AsEntity(network);
			var entity3 = shape3.AsEntity(network);
			var entity4 = shape4.AsEntity(network);

			var action = new MoveToOtherSectionAction(viewModel);

			viewModel.SelectEntities(new INetworkEntity[] { entity1, entity2 });

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
			action.ExecuteAfterActivatingEntity_ForTest(shape1);

			AssertEquals(2, UnitTestUserNotification.Instance.PreviousMessages.Select(m =>
				m.Text == @"In order to move this shape, all arrows to shapes not being moved must be deleted. Any associated pre-requisite links between workflows will be unaffected.
Do you want to delete the attached arrow(s)?").Count());

			AssertEquals("The parent shape SHOULD be in the non-scheduled section, and yet...", true, entity1.IsNonScheduled);
			AssertEquals("The first child shape SHOULD be in the non-scheduled section, and yet...", true, entityChild1.IsNonScheduled);
			AssertEquals("The second child shape SHOULD be in the non-scheduled section, and yet...", true, entityChild1.IsNonScheduled);

			AssertEquals("The second shape SHOULD be in the non-scheduled section, and yet...", true, entity2.IsNonScheduled);

			AssertEquals("The third shape should NOT be in the non-scheduled section, and yet...", false, entity3.IsNonScheduled);
			AssertEquals("The fourth shape should NOT be in the non-scheduled section, and yet...", false, entity4.IsNonScheduled);

			AssertEquals("The link between the parent and move shape SHOULD be preserved, and yet...", false, attachment1.IsDeleted);
			AssertEquals("The link between the move shape and first stay shape should NOT be preserved, and yet...", true, attachment2.IsDeleted);
			AssertEquals("The link between the child shapes SHOULD be preserved, and yet...", false, attachment3.IsDeleted);
			AssertEquals("The link between the second child shape and second stay shape should NOT be preserved, and yet...", true, attachment4.IsDeleted);
		}

		#endregion
	}

	[TestedType(typeof(MoveToOtherSectionAction))]
	class MoveToOtherSectionActionTestCase : JobNetworkActionTestCase<MoveToOtherSectionAction>
	{
		protected override void TestExecuteCore()
		{
			Assert("Tested above.", true);
		}

		protected override void TestIsApplicableCore()
		{
			Assert("Tested above.", true);
		}

		protected override void TestIsEnabledCore()
		{
			Assert("Tested above.", true);
		}

		protected override void TestGetNameCore()
		{
			Assert("Tested above.", true);
		}

		protected override void TestGetDescriptionCore()
		{
			Assert("Tested above.", true);
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override MoveToOtherSectionAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new MoveToOtherSectionAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType()
		{
			return typeof(MultipleSelectedEntitiesExecutionStrategy);
		}
	}
}
