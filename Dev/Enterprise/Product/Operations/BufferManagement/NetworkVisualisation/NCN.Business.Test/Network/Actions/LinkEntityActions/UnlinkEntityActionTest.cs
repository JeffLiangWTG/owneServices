using System;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(UnlinkEntityAction))]
	class UnlinkEntityActionTest : LinkedEntityActionTestCase<UnlinkEntityAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertGetName(nameForUnlinked: "Un-link Entity", nameForLinkedToProcessHeader: "Un-link this Workflow", nameForLinkedToDiagram: "Un-link this Diagram");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertGetDescription(descriptionForUnlinked: "Un-links the shape from its currently-linked entity.", descriptionForProcessHeader: "Un-links the shape from its currently-linked workflow.", descriptionForShape: "Un-links the shape from its currently-linked diagram.");
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Link");
		}

		#endregion

		#region Accessbility

		protected override void TestIsApplicableCore()
		{
			AssertApplicableToDiagram_WhenItIsLinkedOnly();
			AssertApplicableToShape_WhenItIsLinkedOnly();
		}

		protected override void TestIsEnabledCore()
		{
			AssertEnabledToLinkedShape();
			AssertEnabledToLinkedDiagram();
		}

		public void TestShouldBeDisabledAfterUnlinking()
		{
			NonLinkedDiagramTestConfig.NetworkViewModelForNormalDiagram.SelectEntities(new INetworkEntity[] { NonLinkedDiagramTestConfig.ShapeLinkedToDiagram });
			NetworkActionAccessibilityTest.AssertAllowed(NonLinkedDiagramTestConfig.ActionForNormalDiagram.CheckCanStartExecution());

			NonLinkedDiagramTestConfig.ActionForNormalDiagram.Execute();
			NetworkActionAccessibilityTest.AssertNotAllowedWithSingleReason("The shape should be linked to an entity", NonLinkedDiagramTestConfig.ActionForNormalDiagram.IsEnabled());
		}

		public void TestShouldBeAbleToUnlinkMultipleSelectedShapes_WhenAtLeastOneOfThemIsLinked()
		{
			NonLinkedDiagramTestConfig.NetworkViewModelForNormalDiagram.SelectEntities(new INetworkEntity[] { NonLinkedDiagramTestConfig.NonLinkedShape, NonLinkedDiagramTestConfig.ShapeLinkedToDiagram });
			NetworkActionAccessibilityTest.AssertAllowed(NonLinkedDiagramTestConfig.ActionForNormalDiagram.CheckCanStartExecution());
		}

		public void TestShouldNotBeAbleToUnlinkMultipleSelectedShapes_WhenAllOfThemShapesAreUnlinked()
		{
			NonLinkedDiagramTestConfig.NetworkViewModelForNormalDiagram.SelectEntities(new INetworkEntity[] { NonLinkedDiagramTestConfig.NonLinkedShape, NonLinkedDiagramTestConfig.NormalShape });
			NetworkActionAccessibilityTest.AssertNotAllowedWithExactReasons(new INetworkActionDenialReason[] {
				new NetworkActionDenialReason(NonLinkedDiagramTestConfig.NonLinkedShape, "The shape should be linked to an entity", needsNotification: false),
				new NetworkActionDenialReason(NonLinkedDiagramTestConfig.NormalShape, "The shape should be linked to an entity", needsNotification: false) }, NonLinkedDiagramTestConfig.ActionForNormalDiagram.IsEnabled());
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertExecute((network, shape) => network.Setup(m => m.UnlinkEntity(shape)));
		}

		#endregion

		#region Implementation

		protected override UnlinkEntityAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new UnlinkEntityAction(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(MultipleSelectedEntitiesExecutionStrategy);

		#endregion
	}
}
