using System.Linq;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(ApproveNonApprovedShapesAction))]
	class ApproveNonApprovedShapesActionTest : JobNetworkActionTestCase<ApproveNonApprovedShapesAction>
	{
		protected override void TestExecuteCore()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(true, shape1.IsApproved);
			AssertEquals(true, shape2.IsApproved);

			var shape3 = networkViewModel.CreateNewShape(diagram);
			var shape4 = networkViewModel.CreateNewShape(diagram);

			var arrow3_4 = shape3.MakeVisiblePrerequisiteOf(shape4);

			AssertEquals(false, shape3.IsApproved);
			AssertEquals(false, shape4.IsApproved);
			AssertEquals(false, arrow3_4.IsApproved);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(true, shape1.IsApproved);
			AssertEquals(true, shape2.IsApproved);
			AssertEquals(true, shape3.IsApproved);
			AssertEquals(true, shape4.IsApproved);
			AssertEquals(true, arrow3_4.IsApproved);
		}

		public void TestExecute_ShouldAlsoSetRelatedBuffers()
		{
			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			var buffer = network.Shapes.OfType<BMNCNBufferShape>().Single();

			AssertEquals(true, shape1.IsApproved);
			AssertEquals(true, shape2.IsApproved);

			var shape3 = networkViewModel.CreateNewShape(diagram);
			var shape4 = networkViewModel.CreateNewShape(diagram);

			var arrow3_4 = shape3.MakeVisiblePrerequisiteOf(shape4);
			var arrow4_1 = shape4.MakeVisiblePrerequisiteOf(shape1);

			AssertEquals(0, shape3.Shape.GetRelatedBuffers().Count());
			AssertEquals(0, shape4.Shape.GetRelatedBuffers().Count());

			AssertEquals(false, shape3.IsApproved);
			AssertEquals(false, shape4.IsApproved);
			AssertEquals(false, arrow3_4.IsApproved);

			GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(buffer, shape3.Shape.GetRelatedBuffers().ElementAt(0));
			AssertEquals(buffer, shape4.Shape.GetRelatedBuffers().ElementAt(0));
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"Cannot execute when the diagram is not in scaled mode.",
				"This action is accessible to the root diagram only.",
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new string[] {
				"Cannot execute when the diagram is not in scaled mode.",
				"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			AssertEquals("Precondition: not approved", false, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be approved.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: approved", true, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var resource = CreateStaffInCurrentBranchDept("SAM", "Samwise the Brave");
			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
			AssertEquals("Precondition", true, diagram.IsApproved);

			var action = GetAction(networkViewModel);
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The diagram should contain unapproved shapes.", action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			var shape3 = networkViewModel.CreateNewShape(diagram);
			NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				GetAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
				Factory.Save();
			}

			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The diagram should contain unapproved shapes.", action.IsEnabledAfterActivatingEntity_ForTest(diagram));

			var log = diagram.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_Reference == "Additional shapes approved by [Samwise the Brave]");
			AssertNotNull(log);

			var arrow = (NetworkAttachment)network.CreateRelationship(shape1, shape2);
			AssertEquals(false, arrow.Attachment.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed("Non-approved arrow exists - should be enabled", action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Approve Non-approved Shapes and Arrows", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Approves shapes and arrows that have been added since the diagram was last approved", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Approve");
		}

		protected override ApproveNonApprovedShapesAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new ApproveNonApprovedShapesAction(networkViewModel);
		}
	}
}
