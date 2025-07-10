using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(CoreCustomActions))]
	class CoreCustomActionsTest : JobNetworkActionTestCase<CoreCustomActions>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertStaticName("Actions");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertStaticDescription(null);
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestChildActionsCore()
		{
			AssertContainsExactElementsInAnyOrder(new Type[]
				{
					typeof(ExtendedNetworkAction),
					typeof(CloneDiagramAction),
					typeof(OpenShapeListAction),
					typeof(ConvertToWorkflowsAction),
					typeof(SetStatusAction),
					typeof(ValidateWorkflowLoopsAction),
					typeof(AcceptBufferAction),
					typeof(UnapproveDiagramAction),
					typeof(ApproveDiagramAction),
					typeof(ApproveNonApprovedShapesAction),
					typeof(SwitchToScaledModeAction),
					typeof(ToggleResourceDependencyVisibilityAction),
					typeof(SuggestBufferAction),
					typeof(AddBufferAction),
					typeof(CreateProjectBufferAction),
					typeof(DecoupleAction),
					typeof(PinShapeAction),
					typeof(UnpinShapeAction),
					typeof(CreateResourceDependencyAction),
					typeof(CreateResourceDependencyAction),
					typeof(PushAllEntitiesAction),
					typeof(MoveToOtherSectionAction),
					typeof(CreateJobAction)
				},
				GetAction().GetChildActions().Select(a => a.GetType()));
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			{
				var diagram = CreateDiagram(Factory);
				var childShape = CreateShape(diagram);

				var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

				var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

				var networkViewModel = CreateNetworkViewModel(diagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(diagram));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(childShape));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(buffer));

				NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("There are no custom actions defined for this entity.", action.IsApplicableAfterActivatingEntity_ForTest(annotation));
			}
			{
				var jobHeader = CreateJobHeader<OrgHeader>();
				var workflow = jobHeader.ProcessHeaders[0];

				var defaultDiagram = jobHeader.GetDefaultDiagram();
				var defaultWorkflow = workflow.GetDefaultShape(defaultDiagram);

				var networkViewModel = CreateNetworkViewModel(defaultDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(defaultDiagram));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsApplicableAfterActivatingEntity_ForTest(defaultWorkflow));
			}
		}

		protected override void TestIsEnabledCore()
		{
			{
				var diagram = CreateDiagram(Factory);
				var childShape = CreateShape(diagram);

				var buffer = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Buffer);

				var annotation = CreateShape(diagram, shapeType: ShapeTypeList.Codes.Annotation);

				var networkViewModel = CreateNetworkViewModel(diagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(diagram));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(childShape));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(buffer));
			}
			{
				var jobHeader = CreateJobHeader<OrgHeader>();
				var workflow = jobHeader.ProcessHeaders[0];

				var defaultDiagram = jobHeader.GetDefaultDiagram();
				var defaultWorkflow = workflow.GetDefaultShape(defaultDiagram);

				var networkViewModel = CreateNetworkViewModel(defaultDiagram);
				var action = GetAction(networkViewModel);

				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(defaultDiagram));
				NetworkActionAccessibilityTest.AssertAllowed(action.IsEnabledAfterActivatingEntity_ForTest(defaultWorkflow));
			}
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertNoNeedToTestItAsItIsContainerForOtherNetworkActions();
		}

		#endregion

		#region Implementation

		protected override CoreCustomActions GetActionCore(INetworkViewModel networkViewModel)
		{
			return new CoreCustomActions(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(CommonNetworkActionExecutionStrategy);

		#endregion
	}
}
