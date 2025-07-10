using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(SwitchToScaledModeAction))]
	class SwitchToScaledModeActionTest : JobNetworkActionTestCase<SwitchToScaledModeAction>
	{
		#region PreExecution Checks

		public void TestShouldRequireSavingBeforeExecution()
		{
			AssertRequiresSavingBeforeExecution();
		}

		public void TestShouldRequireUserConfirmationBeforeExecution()
		{
			AssertRequiresSpecificConfirmationBeforeExecution("Are you sure you want to create a new copy of this diagram in scaled mode?");
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader2.ProcessHeaders[0];
			var workflow3 = jobHeader2.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link2_3 = workflow2.GetOrCreateDependencyLink(workflow3);
			var task1 = CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code, 60 * 4, estVariationFactor: 1); // 4 hours
			var task2 = CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code, 60 * 13, estVariationFactor: 1); // 1.6 days
			var task3 = CreateTask(workflow3, GlbStaff.CurrentUser.GS_Code, 0, estVariationFactor: 1); // 0 days

			diagram.BNS_Name = "Dat Diagram";

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.Is<INetworkEntity>(p => p.Name == "Dat Diagram (scaled copy)")));

			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			var subDiagram = CreateShape(jobHeader2, network.DiagramEntity, "subDiagram");

			var workflowShape1 = CreateShape(workflow1, network.DiagramEntity, "workflowShape1");
			var workflowShape2 = CreateShape(workflow2, subDiagram, "workflowShape2");
			var workflowShape3 = CreateShape(workflow3, subDiagram, "workflowShape3");
			var arrow1_2 = CreateDependencyAttachment(diagram, link1_2, workflowShape1.Shape, workflowShape2.Shape);
			var arrow2_3 = CreateDependencyAttachment(diagram, link2_3, workflowShape2.Shape, workflowShape3.Shape);

			SetShapeOffset(workflowShape1, network.DiagramEntity, 310, 50);
			SetShapeOffset(subDiagram, network.DiagramEntity, 201, 50);
			SetShapeSize(subDiagram, network.DiagramEntity, 1000, 100);
			SetShapeOffset(workflowShape2, subDiagram, 490, 10);
			SetShapeOffset(workflowShape3, subDiagram, 510, 50);

			var annotation1 = CreateShape(diagram, "Dat Annotation1", shapeType: ShapeTypeList.Codes.Annotation);
			var annotation2 = CreateShape(subDiagram, "Dat Annotation2", shapeType: ShapeTypeList.Codes.Annotation);

			AssertEquals(false, diagram.IsScaled);

			Factory.Save();

			var startingShapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var startingAttachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			var action = GetAction(networkViewModel);
			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			action.FactoryForSpawnedNetwork_ExposedForTest.Save();

			AssertEquals("Are you sure you want to create a new copy of this diagram in scaled mode?", UnitTestUserNotification.Instance.LastMessage.Text);

			var newFactory = new BusinessObjectFactory();

			var clone = newFactory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Dat Diagram (scaled copy)"));
			AssertNotNull(clone);

			AssertEquals(true, clone.IsScaled);
			AssertEquals("Should default to 1 day scale", new ZInt(60 * 8).GetDateTimeFromMinutes(), clone.Scale);
			AssertEquals("Should default to 1 day resolution increment", new ZInt(60 * 8).GetDateTimeFromMinutes(), clone.ResolutionIncrement);
			AssertEquals(ShapeTypeList.Codes.Diagram, clone.BNS_ShapeType);

			var cloneNetwork = CreateNetwork(clone);

			var cloneSubDiagram = cloneNetwork.Entities.ShapeEntities.Single(s => s.RelatedEntityPK == jobHeader2.PK);
			var cloneWorkflowShape1 = cloneNetwork.Entities.ShapeEntities.Single(s => s.RelatedEntityPK == workflow1.PK);
			var cloneWorkflowShape2 = cloneNetwork.Entities.ShapeEntities.Single(s => s.RelatedEntityPK == workflow2.PK);
			var cloneWorkflowShape3 = cloneNetwork.Entities.ShapeEntities.Single(s => s.RelatedEntityPK == workflow3.PK);

			AssertEquals(false, clone.IsReadOnly);
			AssertEquals(true, cloneSubDiagram.Shape.IsReadOnly);
			AssertEquals(true, cloneWorkflowShape1.Shape.IsReadOnly);
			AssertEquals(true, cloneWorkflowShape2.Shape.IsReadOnly);
			AssertEquals(true, cloneWorkflowShape3.Shape.IsReadOnly);

			CombineAssertions("Cloned diagram X offsets", () =>
			{
				AssertEquals("cloneWorkflowShape1", 0d, ((INetworkEntity)cloneWorkflowShape1).X);
				AssertEquals("cloneWorkflowShape2", 100d, ((INetworkEntity)cloneWorkflowShape2).X);
				AssertEquals("cloneWorkflowShape3", 300d, ((INetworkEntity)cloneWorkflowShape3).X);
				AssertEquals("cloneSubDiagram", 0d, ((INetworkEntity)cloneSubDiagram).X);
			});

			AssertEquals("Should round planned duration up to the nearest day", 60 * BMConstants.WorkingHoursPerDay, cloneWorkflowShape1.ExplicitDurationMinutes);
			AssertEquals("Should round planned duration up to the nearest day", 60 * BMConstants.WorkingHoursPerDay * 2, cloneWorkflowShape2.ExplicitDurationMinutes);
			AssertEquals("Should default planned duration to 3x resolution increment", 60 * BMConstants.WorkingHoursPerDay * 3, cloneWorkflowShape3.ExplicitDurationMinutes);

			AssertEquals(startingShapeCount + 7, newFactory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals("Should be one extra attachment for each arrow, and one for the 'switch to scaled' record", startingAttachmentCount + 3, newFactory.GetDatabaseCount(typeof(BMNCNAttachment)));
		}

		public void TestExecute_AnsweringNo_ShouldNotClone()
		{
			var workflow = jobHeader.ProcessHeaders[0];
			diagram.BNS_Name = "Dat Diagram";

			var workflowShape = CreateShape(workflow, diagram);

			Factory.Save();

			var startingShapeCount = Factory.GetDatabaseCount(typeof(BMNCNShape));
			var startingAttachmentCount = Factory.GetDatabaseCount(typeof(BMNCNAttachment));

			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);
			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals("The form will attempt to save before performing this operation. Are you sure you want to create a new copy of this diagram in scaled mode?", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(startingShapeCount, Factory.GetDatabaseCount(typeof(BMNCNShape)));
			AssertEquals(startingAttachmentCount, Factory.GetDatabaseCount(typeof(BMNCNAttachment)));
		}

		public void TestSwitchToScaledAttachmentTypeCreated_WhenSwitchToScale()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			diagram.BNS_Name = "Switch to Scaled Test Diagram";
			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			Factory.Save();

			controller.Setup(m => m.UserInteractionImplementor.HasUserConfirmed(
					"Are you sure you want to create a new copy of this diagram in scaled mode?",
					"Confirmation Required"))
				.Returns(true);

			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			var action = GetAction(networkViewModel);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			action.FactoryForSpawnedNetwork_ExposedForTest.Save();

			// Use a new factory here to load the scale diagram.
			var newFactory = new BusinessObjectFactory();

			var scaledDiagram = newFactory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Switch to Scaled Test Diagram (scaled copy)"));
			AssertNotNull(scaledDiagram);

			var attachmentQuery = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_FromShape, diagram.PK);
			var loadedAttachments = newFactory.Load<BMNCNAttachment>(attachmentQuery); // Load the attachment onto newFactory and not Factory because we want to delete the attachment on scaledDiagram. Hence, use the same factory here.
			AssertEquals(1, loadedAttachments.Length);

			AssertEquals(AttachmentTypeList.Codes.SwitchToScaled, loadedAttachments[0].BNA_Type);
			AssertEquals(scaledDiagram.PK, loadedAttachments[0].BNA_BNS_ToShape);

			scaledDiagram.Delete();
			newFactory.Save();
			Assert(scaledDiagram.IsDeleted);

			loadedAttachments = Factory.Load<BMNCNAttachment>(attachmentQuery);  // Here we are using Factory instead because we want to check if the SCL relationship in the other factory is also deleted.
			AssertEquals("When scaled diagram is deleted, SCL relationship attachment is also deleted.", 0, loadedAttachments.Length);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);
			action.FactoryForSpawnedNetwork_ExposedForTest.Save();

			newFactory = new BusinessObjectFactory();
			var secondScaledDiagram = newFactory.LoadTop1<BMNCNShape>(new ZQuery(BMNCNShapeSchema.BNS_Name, "Switch to Scaled Test Diagram (scaled copy)"));
			AssertNotNull(secondScaledDiagram);

			var anotherAttachmentQuery = new ZQuery(BMNCNAttachmentSchema.BNA_BNS_FromShape, diagram.PK);
			loadedAttachments = Factory.Load<BMNCNAttachment>(anotherAttachmentQuery);
			AssertEquals(1, loadedAttachments.Length);

			AssertEquals(AttachmentTypeList.Codes.SwitchToScaled, loadedAttachments[0].BNA_Type);
			AssertEquals(secondScaledDiagram.PK, loadedAttachments[0].BNA_BNS_ToShape);

			diagram.Delete();  // Here, we are deleting the initial (unscaled) diagram in the orginal Factory, so we use Factory.Save() after.
			Factory.Save();
			Assert(diagram.IsDeleted);

			loadedAttachments = newFactory.Load<BMNCNAttachment>(anotherAttachmentQuery); // Check if the SCL relationship attachment is also deleted. the Scaled Diagram is in loaded by newFactory as seen above.
			AssertEquals("When initial non-scaled diagram is deleted, SCL relationship attachment (related to second scaled diagram) is also deleted.", 0, loadedAttachments.Length);
		}

		public void TestShouldNotSaveClonedDiagram()
		{
			AssertSpawningActionDoesNotSaveSpawnedDiagram();
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"This action is accessible to the root diagram only." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"Cannot execute when the diagram is in scaled mode." }, action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		#endregion

		#region Main Action Attributes

		protected override void TestGetNameCore()
		{
			AssertEquals("Switch to Scaled Mode", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			AssertEquals("Clones a scaled-mode version of the current diagram. In scaled mode, entities have a Planned Duration relative to their size, and a starting time relative to their position from the left side.", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		#endregion

		#region Implementation

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Scale");
		}

		protected override SwitchToScaledModeAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new SwitchToScaledModeAction(networkViewModel);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateSystem("ORG");
			jobHeader = CreateJobHeader<OrgHeader>();
			diagram = CreateDiagram(jobHeader);
		}

		ProcessJobHeader jobHeader;
		BMNCNShape diagram;

		#endregion
	}
}
