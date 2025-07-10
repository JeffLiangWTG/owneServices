using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ConvertToWorkflowsAction))]
	class ConvertToWorkflowsActionTest : JobNetworkActionTestCase<ConvertToWorkflowsAction>
	{
		public void TestExecute_WhenDiagramContainsAnnotationsAndBuffers()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(diagram, "Gonna be a workflow");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var buffer = networkViewModel.SuggestAndAcceptAllBuffers().Single();
			var annotation = networkViewModel.CreateNewAnnotation(diagram).AsShape();

			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			GetAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(1, jobHeader.ProcessHeaders.Count);
			AssertEquals(ZGuid.Empty, buffer.BNS_RelatedEntityID);
			AssertEquals(ZGuid.Empty, annotation.BNS_RelatedEntityID);
		}

		public void TestExecute_LinksOnAttachmentsPlease()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = ":\\";
			childShape2.Name = ":/";

			var attachment = childShape1.MakeVisiblePrerequisiteOf(childShape2);

			AssertNull(childShape1.ProcessHeader);
			AssertNull(childShape2.ProcessHeader);
			AssertNull(diagram.ProcessHeader);
			AssertNull(attachment.ProcessHeaderLink);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var org = (OrgHeader)jobHeader.Parent;
			diagram.BNS_RelatedEntityID = jobHeader.PK;

			GetAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(org.PK, childShape1.ProcessHeader.FH_ParentId);
			AssertEquals(org.PK, childShape2.ProcessHeader.FH_ParentId);
			AssertNotNull(attachment.ProcessHeaderLink);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertCollectionContains(childShape1.ProcessHeader, jobHeader.ProcessHeaders);
			AssertCollectionContains(childShape2.ProcessHeader, jobHeader.ProcessHeaders);
		}

		protected override void TestExecuteCore()
		{
			VisualBoardsTestHelper.MakeCompletionStatementTaskType("ORG", "COM");

			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			var diagram = CreateDiagram(jobHeader);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "Awww yiss";
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape2.Name = "Breadcrumbs";
			childShape2.Shape.BNS_CompletionStatements =
				@"Such breadcrumbs
		very float
	pls throw in pond
			wow
quacky quacky
		wow";

			var action = GetAction(networkViewModel);
			action.ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Awww yiss");
			var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Breadcrumbs");

			AssertEquals(jobHeader.FH_ParentId, workflow1.FH_ParentId);
			AssertEquals(jobHeader.FH_ParentId, workflow2.FH_ParentId);
			AssertEquals(config.Bucket, workflow1.CurrentComponent);
			AssertEquals(config.Bucket, workflow2.CurrentComponent);

			AssertEquals(6, workflow2.CompletionStatementTasksIncludingChildWorkflowTasks.Count);
			AssertEquals("Such breadcrumbs", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[0].P9_NotesAsString);
			AssertEquals("		very float", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[1].P9_NotesAsString);
			AssertEquals("	pls throw in pond", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[2].P9_NotesAsString);
			AssertEquals("			wow", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[3].P9_NotesAsString);
			AssertEquals("quacky quacky", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[4].P9_NotesAsString);
			AssertEquals("		wow", workflow2.CompletionStatementTasksIncludingChildWorkflowTasks[5].P9_NotesAsString);
		}

		public void TestExecute_WhenDiagramLinkedToJobAfterShapesCreated()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			AssertEquals(0, jobHeader.ProcessHeaders.Count);

			var diagram = CreateDiagram(Factory);
			diagram.BNS_JobType = "ORG";

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.PickEntity(ModuleIDs.ProcessHeader, It.IsAny<bool>())).Returns(jobHeader);

			using (ObjectFactory.Substitute(controller.Object))
			{
				var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
				var network = networkViewModel.GetJobNetwork();

				var childShape1 = networkViewModel.CreateNewShape(diagram);
				childShape1.Name = "Awww yiss";
				var childShape2 = networkViewModel.CreateNewShape(diagram);
				childShape2.Name = "Breadcrumbs";

				Factory.Save();

				network.LinkEntity(diagram, ModuleIDs.ProcessHeader);
				GetAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

				AssertEquals(2, jobHeader.ProcessHeaders.Count);
				var workflow1 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Awww yiss");
				var workflow2 = jobHeader.ProcessHeaders.Single(w => w.FH_CompletionStatement == "Breadcrumbs");

				AssertEquals(jobHeader.FH_ParentId, workflow1.FH_ParentId);
				AssertEquals(jobHeader.FH_ParentId, workflow2.FH_ParentId);
				AssertEquals(config.Bucket, workflow1.CurrentComponent);
				AssertEquals(config.Bucket, workflow2.CurrentComponent);
			}
		}

		public void TestExecute_WhenExistingProcessHeaderSetOnShape_ShouldMoveToJob()
		{
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			childShape1.Name = "Awww yiss";
			childShape2.Name = "Breadcrumbs";

			AssertNull(childShape1.ProcessHeader);
			AssertNull(childShape2.ProcessHeader);
			AssertNull(diagram.ProcessHeader);

			var jobHeader = CreateJobHeader<OrgHeader>(false);
			var org = (OrgHeader)jobHeader.Parent;
			diagram.BNS_RelatedEntityID = jobHeader.PK;

			GetAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(diagram);

			AssertEquals(org.PK, childShape1.ProcessHeader.FH_ParentId);
			AssertEquals(org.PK, childShape2.ProcessHeader.FH_ParentId);

			AssertEquals(2, jobHeader.ProcessHeaders.Count);
			AssertCollectionContains(childShape1.ProcessHeader, jobHeader.ProcessHeaders);
			AssertCollectionContains(childShape2.ProcessHeader, jobHeader.ProcessHeaders);
		}

		public void TestCanExecute_ForChildShapeOfWorkflowShape()
		{
			var workflow = BMSTestHelper.CreateWorkflowAndParents<OrgHeader>(Factory, "This workflow is not a workflow");

			var diagram = CreateDiagram(workflow.JobHeader);
			var shape = CreateShape(workflow, diagram);
			var nestedShape = CreateShape(shape);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			AssertEquals(true, action.IsEnabledAfterActivatingEntity_ForTest(nestedShape).IsAllowed);
			AssertNull(nestedShape.ProcessHeader);

			action.ExecuteForEntityWithoutAccessCheck(nestedShape);

			AssertNotNull(nestedShape.ProcessHeader);
			AssertEquals(workflow.JobHeader, nestedShape.ProcessHeader.JobHeader);
			AssertIsParent(nestedShape.ProcessHeader, workflow);
		}

		public void TestShouldConvertMultipleSelectedShapes_WhenTheyAreAllNotConverted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);

			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity1 = networkViewModel.CreateNewShape(parentEntity);
			var childEntity2 = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);

			AssertNull("Precondition", childEntity1.ProcessHeader);
			AssertNull("Precondition", childEntity2.ProcessHeader);

			networkViewModel.SelectEntities(new INetworkEntity[] { childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());

			action.Execute();
			AssertNotNull(childEntity1.ProcessHeader);
			AssertNotNull(childEntity2.ProcessHeader);

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldConvertShapesThatAreNotConverted_WithoutBotheringUserWithNotification()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");

			var controller = new Mock<IBMNetworkEntityController>();

			controller.Setup(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()));

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity1 = networkViewModel.CreateNewShape(parentEntity);
			var childEntity2 = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);
			network.LinkEntity(childEntity1, workflow1);

			AssertNotNull("Precondition", childEntity1.ProcessHeader);
			AssertNull("Precondition", childEntity2.ProcessHeader);

			networkViewModel.SelectEntities(new INetworkEntity[] { childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());

			action.Execute();
			AssertNotNull(childEntity1.ProcessHeader);
			AssertNotNull(childEntity2.ProcessHeader);

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestExecuteForMultipleShapes_WhenBothParentAndChildShapesAreSelected()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");

			var controller = new Mock<IBMNetworkEntityController>();

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity1 = networkViewModel.CreateNewShape(parentEntity);
			var childEntity2 = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);
			network.LinkEntity(childEntity1, workflow1);

			AssertNotNull("Precondition", childEntity1.ProcessHeader);
			AssertNull("Precondition", childEntity2.ProcessHeader);

			networkViewModel.SelectEntities(new INetworkEntity[] { parentEntity, childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());

			action.Execute();
			AssertNotNull(childEntity1.ProcessHeader);
			AssertNotNull(childEntity2.ProcessHeader);

			controller.Verify(m => m.NotifyActionExecutedPartially(It.IsAny<INetworkActionAccessibility>()), Times.Never);
		}

		public void TestShouldBeDisabledAfterConverting()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);

			networkViewModel.SelectEntities(new INetworkEntity[] { childEntity });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
			action.Execute();
			AssertNotNull(childEntity.ProcessHeader);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be either linked to a job and have non-linked child shapes or be not linked itself and have a parent linked to a job or workflow.", action.IsEnabled());
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed("shape", action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertAllowed("diagram", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("buffer", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("annotation", new string[] {
					"The shape should not be a buffer or annotation." }, action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			AssertEquals("No child items", false, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);

			var childShape = networkViewModel.CreateNewShape(diagram);

			AssertEquals("Has child item but diagram is not linked to a job", false, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Child item is not linked to a workflow, but parent is not linked to a job", false, action.IsEnabledAfterActivatingEntity_ForTest(childShape).IsAllowed);

			diagram.BNS_RelatedEntityID = jobHeader.PK;

			AssertEquals("Has child item not linked to a workflow, and diagram is linked to a job", true, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Child item is not linked to a workflow", true, action.IsEnabledAfterActivatingEntity_ForTest(childShape).IsAllowed);

			var dummyProcessHeader = Factory.NewWithValidTestData<ProcessHeader>();
			childShape.Shape.BNS_RelatedEntityID = dummyProcessHeader.PK;

			AssertEquals("Has child item not linked to a real workflow", true, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Child item is not linked to a real workflow", true, action.IsEnabledAfterActivatingEntity_ForTest(childShape).IsAllowed);

			childShape.Shape.BNS_RelatedEntityID = workflow.PK;

			AssertEquals("Has child item but is already linked to workflow", false, action.IsEnabledAfterActivatingEntity_ForTest(diagram).IsAllowed);
			AssertEquals("Child item is already linked to a workflow", false, action.IsEnabledAfterActivatingEntity_ForTest(childShape).IsAllowed);
		}

		public void TestShouldBeAbleToConvertMultipleSelectedShapesIntoWorkflows_WhenAtLeastOneOfThemIsNotConverted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity1 = networkViewModel.CreateNewShape(parentEntity);
			var childEntity2 = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);
			network.LinkEntity(childEntity1, workflow);

			networkViewModel.SelectEntities(new INetworkEntity[] { childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecution());
		}

		public void TestShouldNotBeAbleToConvertMultipleSelectedShapesIntoWorkflows_WhenAllOfThemAreAlreadyConverted()
		{
			var jobHeader = CreateJobHeader<OrgHeader>(addDefaultProcessHeaderIfNone: false);
			var workflow1 = CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "workflow2");

			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);

			var parentEntity = networkViewModel.CreateNewShape(diagram);
			var childEntity1 = networkViewModel.CreateNewShape(parentEntity);
			var childEntity2 = networkViewModel.CreateNewShape(parentEntity);

			network.LinkEntity(parentEntity, jobHeader);
			network.LinkEntity(childEntity1, workflow1);
			network.LinkEntity(childEntity2, workflow2);

			networkViewModel.SelectEntities(new INetworkEntity[] { childEntity1, childEntity2 });

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] {
				new NetworkActionDenialReason(childEntity1.AsShape(), "The shape should be either linked to a job and have non-linked child shapes or be not linked itself and have a parent linked to a job or workflow.", needsNotification: false),
				new NetworkActionDenialReason(childEntity2.AsShape(), "The shape should be either linked to a job and have non-linked child shapes or be not linked itself and have a parent linked to a job or workflow.", needsNotification: false) }, action.IsEnabled());
		}

		protected override void TestGetNameCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var childShape = CreateShape(workflow, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			AssertEquals("Convert Shapes to Workflows", action.GetNameAfterActivatingEntity_ForTest(diagram));
			AssertEquals("Convert Shape to Workflow", action.GetNameAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestGetDescriptionCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var diagram = CreateDiagram(jobHeader);
			var childShape = CreateShape(workflow, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			AssertEquals("Converts all leaf shapes within the selected diagram into workflows on the linked job", action.GetDescriptionAfterActivatingEntity_ForTest(diagram));
			AssertEquals("Converts this shape into a workflow on the linked job", action.GetDescriptionAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Workflow");
		}

		protected override ConvertToWorkflowsAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ConvertToWorkflowsAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(MultipleSelectedEntitiesExecutionStrategy);
	}
}
