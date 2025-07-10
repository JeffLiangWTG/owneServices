using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(OpenAsDiagramAction))]
	class OpenAsDiagramActionTest : JobNetworkActionTestCase<OpenAsDiagramAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Open as Diagram", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Open this shape as a diagram in its own form", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var root = CreateDiagram(jobHeader);
			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, root.BNS_ShapeType);
			AssertNotNull("Precondition", root.ProcessHeader);

			var shape = CreateShape(root);
			var buffer = CreateShape(root, shapeType: ShapeTypeList.Codes.Buffer);
			var annotation = CreateShape(root, shapeType: ShapeTypeList.Codes.Annotation);

			var networkViewModel = CreateNetworkViewModel(root);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("Root", new string[] {
					"This action is not accessible to the root diagram.",
					"The action can be applied to shapes only." }, action.IsApplicableAfterActivatingEntity_ForTest(root));

			NetworkActionAccessibilityTest.AssertAllowed("Shape", action.IsApplicableAfterActivatingEntity_ForTest(shape));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Buffer", "The action can be applied to shapes only.", action.IsApplicableAfterActivatingEntity_ForTest(buffer));

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("Annotation", "The action can be applied to shapes only.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			var root = CreateDiagram(jobHeader);
			AssertEquals("Precondition", ShapeTypeList.Codes.Diagram, root.BNS_ShapeType);
			AssertNotNull("Precondition", root.ProcessHeader);

			var shape = CreateShape(root);

			var networkViewModel = CreateNetworkViewModel(root);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertAllowed("Shape", action.IsEnabledAfterActivatingEntity_ForTest(shape));
		}

		#endregion

		#region PreExecution Checks

		public void TestShouldRequireSavingBeforeExecution()
		{
			AssertRequiresSavingBeforeExecution();
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			var mocks = new MockRepository(MockBehavior.Default);

			var diagram = CreateDiagram(Factory);
			diagram.BNS_JobType = "ORG";
			var childShape = CreateShape(diagram);
			childShape.Name = "Link to me too";

			var controller = CreateMockableControllerWithMockableInteractionImplementor(mocks);
			var networkViewModel = CreateNetworkViewModel(diagram, controller: controller.Object);
			var network = networkViewModel.GetJobNetwork();

			controller
				.Setup(m => m.UserInteractionImplementor.HasUserConfirmed("The form will attempt to save before performing this operation.", "Confirmation Required"))
				.Returns(true);

			controller.Setup(m => m.ViewDiagram(childShape));
			controller.Setup(m => m.TriggerSaveAction()).Returns(ContinueWithSave.Yes);
			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertAllowed(action.CheckCanStartExecutionAfterActivatingEntity_ForTest(childShape));
			action.ExecuteAfterActivatingEntity_ForTest(childShape);
		}

		#endregion

		#region Overridden Test Methods

		public override void TestCanPerformOnApprovedDiagram()
		{
			var controller = CreateMockableController(Mocks);
			using (ObjectFactory.Substitute(controller.Object))
			{
				base.TestCanPerformOnApprovedDiagram();
			}
			controller.Verify(c => c.ViewDiagram(It.IsAny<INetworkEntity>()), Times.Never());
		}

		public override void TestCanPerformOnApprovedShape()
		{
			var controller = CreateMockableController(Mocks);
			controller.Setup(m => m.ViewDiagram(It.IsAny<INetworkEntity>()));
			using (ObjectFactory.Substitute(controller.Object))
			{
				base.TestCanPerformOnApprovedShape();
			}
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("PopOut");
		}

		#endregion

		#region Implementation

		protected override OpenAsDiagramAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new OpenAsDiagramAction(networkViewModel);
		}

		#endregion
	}
}
