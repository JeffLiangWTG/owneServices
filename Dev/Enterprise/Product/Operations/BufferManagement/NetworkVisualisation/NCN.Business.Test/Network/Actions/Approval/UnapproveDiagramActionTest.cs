using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(UnapproveDiagramAction))]
	class UnapproveDiagramActionTest : JobNetworkActionTestCase<UnapproveDiagramAction>
	{
		public void TestCanUnapproveOrphanApprovedShapes()
		{
			var diagram = CreateDiagram(Factory, name: "Joe");
			var subDiagram = CreateShape(diagram, name: "Treasurer");
			var shape = CreateShape(diagram, name: "Member for North Sydney");

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			AssertEquals(false, CustomNetworkActionIsEnabledForShape<ApproveDiagramAction>(networkViewModel, subDiagram));
			AssertEquals(false, CustomNetworkActionIsEnabledForShape<ApproveDiagramAction>(networkViewModel, shape));
			AssertEquals(false, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, subDiagram));
			AssertEquals(false, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, shape));

			var action = networkViewModel.GetCoreCustomNetworkAction_ForTesting<ApproveDiagramAction>();
			AssertNotNull(action);

			action.ExecuteAfterActivatingEntity_ForTest(diagram);

			AssertEquals(true, diagram.IsApproved);
			AssertEquals(true, subDiagram.IsApproved);
			AssertEquals(true, shape.IsApproved);

			diagram.UnApprove();

			AssertEquals(false, diagram.IsApproved);
			AssertEquals(true, subDiagram.IsApproved);
			AssertEquals(true, shape.IsApproved);

			AssertEquals(true, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, subDiagram));
			AssertEquals(true, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, shape));

			var unapproveAction = new UnapproveDiagramAction(networkViewModel);

			AssertEquals(true, unapproveAction.IsEnabledAfterActivatingEntity_ForTest(subDiagram).IsAllowed);
			AssertEquals(true, unapproveAction.IsEnabledAfterActivatingEntity_ForTest(shape).IsAllowed);

			unapproveAction.ExecuteAfterActivatingEntity_ForTest(subDiagram);
			unapproveAction.ExecuteAfterActivatingEntity_ForTest(shape);

			AssertEquals(false, subDiagram.IsApproved);
			AssertEquals(false, shape.IsApproved);

			AssertEquals(false, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, subDiagram));
			AssertEquals(false, CustomNetworkActionIsEnabledForShape<UnapproveDiagramAction>(networkViewModel, shape));
		}

		bool CustomNetworkActionIsEnabledForShape<T>(NetworkViewModel networkViewModel, BMNCNShape shape) where T : JobNetworkAction
		{
			return networkViewModel.GetCoreCustomNetworkAction_ForTesting<T>().IsEnabledAfterActivatingEntity_ForTest(shape).IsAllowed;
		}

		public void TestUnApprove_ShouldRefreshEntityState()
		{
			var diagram = CreateDiagram(Factory, name: "Joe");
			var subDiagram = CreateShape(diagram, name: "Treasurer");
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.ToggleApproval();

			var subDiagramEntity = network.Entities.GetInstance(subDiagram);

			AssertEquals(true, diagram.IsApproved);

			string changedProperty = null;
			subDiagramEntity.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

			networkViewModel.ToggleApproval();

			AssertEquals(false, diagram.IsApproved);
			AssertEquals(nameof(subDiagramEntity.EntityState), changedProperty);
		}

		protected override void TestExecuteCore()
		{
			var resource1 = CreateStaffInCurrentBranchDept("FRO", "Frodo Baggins");
			var resource2 = CreateStaffInCurrentBranchDept("SAM", "Samwise the Brave");

			Factory.Save();

			var diagram = Factory.New<BMNCNShape>();
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			var shape1 = networkViewModel.CreateNewShape(diagram);
			var shape2 = networkViewModel.CreateNewShape(diagram);
			var arrow = shape1.MakeVisiblePrerequisiteOf(shape2);

			network.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			using (Env.SetTemporaryUserContext(resource1.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				new ApproveDiagramAction(networkViewModel).ExecuteAfterActivatingEntity_ForTest(diagram);
				Factory.Save();
			}

			var log = diagram.Logs.GetAllLogs().Cast<StmALog>().Single(l => l.SL_Reference == "Diagram approved by [Frodo Baggins]");
			AssertNotNull(log);

			using (Env.SetTemporaryUserContext(resource2.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var action = GetAction(networkViewModel);
				action.ExecuteAfterActivatingEntity_ForTest(diagram);
				Factory.Save();
			}

			AssertEquals(false, diagram.IsApproved);
			AssertEquals(false, shape1.Shape.IsApproved);
			AssertEquals(false, shape2.Shape.IsApproved);
			AssertEquals(false, arrow.IsApproved);

			AssertEquals(ZString.Empty, diagram.BNS_GS_NKApprovedBy);
			AssertEquals(ZString.Empty, shape1.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(ZString.Empty, shape2.Shape.BNS_GS_NKApprovedBy);
			AssertEquals(ZString.Empty, arrow.BNA_GS_NKApprovedBy);

			log = diagram.Logs.GetAllLogs().Cast<StmALog>().Last(l => l.SL_Reference == "Diagram un-approved by [Samwise the Brave]");
			AssertNotNull(log);
		}

		public void TestExecute_ShouldUnApproveProcessHeaderAlso()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = CreateWorkflow(jobHeader, "workflow");
			var diagram = CreateDiagram(jobHeader);
			var shape = CreateShape(workflow, diagram);
			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertNull(jobHeader.ApprovedShape);
			AssertNull(workflow.ApprovedShape);

			networkViewModel.ToggleApproval();
			AssertNotNull(jobHeader.ApprovedShape);
			AssertNotNull(workflow.ApprovedShape);

			networkViewModel.ToggleApproval();
			AssertNull(jobHeader.ApprovedShape);
			AssertNull(workflow.ApprovedShape);
		}

		[TestDate(2019, 06, 01)]
		public void TestUnApproveButton_GetsDisabled_WhenDiagramIsUnApproved_BeforeSaving()
		{
			var diagram = Factory.New<BMNCNShape>();
			diagram.SwitchToScaled();
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var shape = networkViewModel.CreateNewShape(diagram);
			networkViewModel.ToggleApproval();
			Factory.Save();

			var action = GetAction(networkViewModel);

			using (NetworkVisualisationTestHelper.TemporarilyActivateEntityForNetworkActions(networkViewModel, diagram))
			{
				Assert(shape.IsApproved);
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabled());
				action.Execute();
				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be approved.", action.IsEnabled());
			}
		}

		protected override void TestIsApplicableCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("child shape", new string[] {
					"Cannot execute when the diagram is not in scaled mode.",
					"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(childShape));

			AssertEquals("Precondition: not scaled", false, diagram.IsScaled);
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons("non scaled root diagram", new string[] {
					"Cannot execute when the diagram is not in scaled mode.",
					"The shape should be approved." }, action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			diagram.SwitchToScaled();

			AssertEquals("Precondition: diagram is not approved, child shape has an unapproved parent", false, diagram.IsApproved);
			AssertEquals("Precondition: parent shape", diagram, childShape.ParentShape);

			childShape.ToggleApproval("DMK");
			AssertEquals("Precondition: child shape is approved", true, childShape.IsApproved);

			NetworkActionAccessibilityTest.AssertAllowed("child shape with an unapproved parent", action.IsApplicableAfterActivatingEntity_ForTest(childShape));
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("non approved root diagram", "The shape should be approved.", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			childShape.ToggleApproval();
			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: diagram is approved", true, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed("approved root diagram", action.IsApplicableAfterActivatingEntity_ForTest(diagram));

			AssertCannotExecuteInWorkflowRelationshipDesigner();
		}

		protected override void TestIsEnabledCore()
		{
			var diagram = CreateDiagram(Factory);
			var childShape = CreateShape(diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var action = GetAction(networkViewModel);

			diagram.SwitchToScaled();

			AssertEquals("Precondition: diagram is not approved, child shape has an unapproved parent", false, diagram.IsApproved);
			AssertEquals("Precondition: parent shape", diagram, childShape.ParentShape);

			childShape.ToggleApproval("DMK");
			AssertEquals("Precondition: child shape is approved", true, childShape.IsApproved);

			NetworkActionAccessibilityTest.AssertAllowed("child shape with an unapproved parent", action.IsEnabledAfterActivatingEntity_ForTest(childShape));

			childShape.ToggleApproval();
			networkViewModel.ToggleApproval();
			AssertEquals("Precondition: diagram is approved", true, diagram.IsApproved);
			NetworkActionAccessibilityTest.AssertAllowed("approved root diagram", action.IsEnabledAfterActivatingEntity_ForTest(diagram));
		}

		protected override void TestGetNameCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Un-approve Diagram", GetAction(CreateNetworkViewModel(diagram)).GetName());
		}

		protected override void TestGetDescriptionCore()
		{
			var diagram = CreateDiagram(Factory);
			AssertEquals("Marks this diagram as un-approved", GetAction(CreateNetworkViewModel(diagram)).GetDescription());
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Unapprove");
		}

		protected override UnapproveDiagramAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new UnapproveDiagramAction(networkViewModel);
		}
	}
}
