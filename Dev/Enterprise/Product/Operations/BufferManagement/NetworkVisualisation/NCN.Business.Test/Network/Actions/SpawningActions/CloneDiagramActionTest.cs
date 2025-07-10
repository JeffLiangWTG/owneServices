using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CloneDiagramAction))]
	class CloneDiagramActionTest : JobNetworkActionTestCase<CloneDiagramAction>
	{
		#region PreExecution Checks

		public void TestShouldNotExecuteAndNotifyToTheUser_IfThereAreValidationErrors()
		{
			AssertDoesNotExecuteAndNotifyToTheUser_IfThereAreValidationErrors();
		}

		public void TestShouldNotRequireSavingBeforeExecution()
		{
			AssertDoesNotRequireSavingBeforeExecution();
		}

		public void TestShouldRequireUserConfirmationBeforeExecution()
		{
			AssertRequiresSpecificConfirmationBeforeExecution("Are you sure you want to create a new copy of this diagram?");
		}

		#endregion

		#region Execute

		public void TestExecute_ForNonSavedDiagram()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			controller.Setup(m => m.TriggerSaveAction());
			var diagram = CreateDiagram(Factory);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			var shape = networkViewModel.CreateNewShape(diagram);
			shape.Name = "Jimminy Jillikers!";

			AssertEquals(true, diagram.HasChanges);
			AssertEquals(false, shape.IsInDatabase);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			new CloneDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			var newFactory = Factory.CreateNewFactory();
			var loadedShape = newFactory.Load<BMNCNShape>(shape.PK);

			AssertNull(loadedShape);

			AssertEquals("The diagram should still not be saved", true, diagram.HasChanges);
			AssertEquals("Are you sure you want to create a new copy of this diagram?", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void TestExecuteCore()
		{
			BMSTestHelper.CreateSystem(Factory, "DUM");
			var jobHeader1 = CreateJobHeader<DummyWithWorkflow>();
			var jobHeader2 = CreateJobHeader<DummyWithWorkflow>();
			var workflow1 = jobHeader1.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders[0];
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = CreateDiagram(jobHeader1);
			var subDiagram = CreateShape(jobHeader2, diagram);
			diagram.BNS_Name = "Dat Diagram";

			var workflowShape1 = CreateShape(workflow1, diagram);
			var workflowShape2 = CreateShape(workflow2, subDiagram);
			var workflowAttachment = CreateDependencyAttachment(diagram, link, workflowShape1, workflowShape2);

			var annotation1 = CreateShape(diagram, "Dat Annotation1", ShapeTypeList.Codes.Annotation);
			var annotation2 = CreateShape(subDiagram, "Dat Annotation2", ShapeTypeList.Codes.Annotation);

			Factory.Save();

			var startingShapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var startingAttachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(p => p.Name == "Dat Diagram (copy)")));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForSpawnedNetwork);

			var clone = factoryForSpawnedNetwork.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Dat Diagram (copy)"));
			AssertNotNull(clone);

			AssertEquals(ShapeTypeList.Codes.Diagram, clone.BNS_ShapeType);
			AssertEquals(startingShapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals(startingAttachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));
		}

		public void TestExecute_ForDefaultDiagram_ShouldCloneDiagramAndOpenInNewForm()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Reticulating splines");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Calibrating personality matricies");
			var link = workflow1.GetOrCreateDependencyLink(workflow2);

			var defaultDiagram = jobHeader.GetDefaultDiagram();
			defaultDiagram.BNS_Name = "Dat Diagram";

			AssertEquals(ShapeTypeList.Codes.DefaultDiagram, defaultDiagram.BNS_ShapeType);

			Factory.Save();

			var startingShapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var startingAttachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(p => p.Name == "Dat Diagram (copy)")));
			var networkViewModel = CreateNetworkViewModel(defaultDiagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();
			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(defaultDiagram);

			var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForSpawnedNetwork);

			var clone = factoryForSpawnedNetwork.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Dat Diagram (copy)"));
			AssertNotNull(clone);

			AssertEquals(ShapeTypeList.Codes.Diagram, clone.BNS_ShapeType);
			AssertEquals(startingShapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals(startingAttachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));

			AssertEquals(2, clone.ChildShapes.Count);
			AssertEquals(1, clone.ChildShapes[0].DependencyAttachments.Count);
			AssertEquals(1, clone.ChildShapes[1].DependencyAttachments.Count);
		}

		public override void TestCanPerformOnApprovedDiagram()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				base.TestCanPerformOnApprovedDiagram();
			}
		}

		public void TestShouldNotSaveClonedDiagram()
		{
			AssertSpawningActionDoesNotSaveSpawnedDiagram();
		}

		#endregion

		#region Creating Copies Of Scaled Diagrams

		public void TestShouldNotPromptTheUserAboutBuffers_AndShouldCreateDiagramCopy_ForScaledDiagramsWithNoBuffers()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"))
				.Returns(true);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForSpawnedNetwork);

			var cloneDiagram = factoryForSpawnedNetwork.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Diagram 1 (copy)"));
			AssertNotNull(cloneDiagram);

			var cloneNetworkViewModel = CreateNetworkViewModel(cloneDiagram);
			var cloneNetwork = cloneNetworkViewModel.GetJobNetwork();

			AssertContainsExactElementsInAnyOrder("Should copy shapes and annotations",
				new[] { "Shape 1", "Shape 2", "Annotation 1" }, cloneNetwork.Shapes.Select(s => s.BNS_Name));
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?", "Buffers cannot be recreated on diagram copies"), Times.Never);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}

		public void TestShouldPromptTheUserAboutBuffers_AndShouldCreateDiagramCopyWithNoBuffers_ForScaledDiagramsWithBuffers_WhenUserAnsweredYes()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var buffer1 = CreateShape(diagram, "Buffer 1", shapeType: ShapeTypeList.Codes.Buffer);

			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_Owner = diagram.PK;
			attachment.BNA_BNS_FromShape = shape2.PK;
			attachment.BNA_BNS_ToShape = buffer1.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"))
				.Returns(true);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?", "Buffers cannot be recreated on diagram copies"))
				.Returns(true);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			AssertEquals("Precondition: should contain one attachement within the diagram", 1, diagram.AllAttachments.Count);
			AssertEquals("Precondition: should contain one attachement for the buffer", 1, buffer1.AsEntity(network).Attachments.Count);

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNotNull(factoryForSpawnedNetwork);

			var cloneDiagram = factoryForSpawnedNetwork.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Diagram 1 (copy)"));
			AssertNotNull(cloneDiagram);

			var cloneNetworkViewModel = CreateNetworkViewModel(cloneDiagram);
			var cloneNetwork = cloneNetworkViewModel.GetJobNetwork();

			AssertContainsExactElementsInAnyOrder("Should copy shapes and annotations only with no buffers",
				new[] { "Shape 1", "Shape 2", "Annotation 1" }, cloneNetwork.Shapes.Select(s => s.BNS_Name));
			AssertEquals("Should contain no attachements for the buffer", 0, cloneDiagram.AllAttachments.Count);
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?", "Buffers cannot be recreated on diagram copies"), Times.Once);
		}

		public void TestShouldPromptTheUserAboutBuffers_AndShouldCancel_ForScaledDiagramWithBuffers_WhenUserAnsweredNo()
		{
			var diagram = CreateDiagram(Factory, "Diagram 1");
			var shape1 = CreateShape(diagram, "Shape 1");
			var shape2 = CreateShape(diagram, "Shape 2");
			var annotation1 = CreateShape(diagram, "Annotation 1", shapeType: ShapeTypeList.Codes.Annotation);
			var buffer1 = CreateShape(diagram, "Buffer 1", shapeType: ShapeTypeList.Codes.Buffer);

			var attachment = Factory.New<BMNCNAttachment>();
			attachment.BNA_BNS_Owner = diagram.PK;
			attachment.BNA_BNS_FromShape = shape2.PK;
			attachment.BNA_BNS_ToShape = buffer1.PK;
			attachment.BNA_Type = AttachmentTypeList.Codes.Dependency;

			Factory.Save();

			var controller = CreateMockableControllerWithMockableInteractionImplementor(Mocks);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"))
				.Returns(true);
			controller
				.Setup(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?", "Buffers cannot be recreated on diagram copies"))
				.Returns(false);

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			var factoryForSpawnedNetwork = action.FactoryForSpawnedNetwork_ExposedForTest;
			AssertNull("Should cancel diagram creation", factoryForSpawnedNetwork);
			controller.Verify(m => m.UserInteractionImplementor.HasUserConfirmed("Are you sure you want to create a new copy of this diagram?", "Confirmation Required"), Times.Once);
			controller.Verify(m => m.UserInteractionImplementor.HasUserAnsweredYes("The original diagram contains buffers which will not be created on the diagram copy. Do you want to proceed?", "Buffers cannot be recreated on diagram copies"), Times.Once);
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);
			var action = GetAction(CreateNetworkViewModel(diagram));

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("This action is accessible to the root diagram only.", action.IsApplicableAfterActivatingEntity_ForTest(childShape));
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			NetworkActionAccessibilityTest.AssertAllowed(GetAction(CreateNetworkViewModel(diagram)).IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		#endregion

		#region Main Action Attributes

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Clone Diagram", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Clones the current diagram and opens it in a new form", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Copy");
		}

		#endregion

		#region Implementation

		protected override CloneDiagramAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CloneDiagramAction(networkViewModel);
		}

		#endregion
	}
}
