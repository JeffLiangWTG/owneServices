using System;
using System.Linq;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(LinkEntityActions))]
	class LinkEntityActionsTest : JobNetworkActionTestCase<LinkEntityActions>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			AssertStaticName("Linked Entity");
		}

		protected override void TestGetDescriptionCore()
		{
			AssertStaticDescription("The open or change the linked business entity this shape represents");
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon(string.Empty);
		}

		protected override void TestChildActionsCore()
		{
			AssertContainsExactElementsInAnyOrder(new Type[]
				{
					typeof(LinkFromClipboardAction),
					typeof(LinkToWorkflowAction),
					typeof(LinkToDiagramAction),
					typeof(OpenLinkedEntityAction),
					typeof(UnlinkEntityAction),
				},
				GetAction().GetChildActions().Select(a => a.GetType()));
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			AssertIsApplicableToNonDefaultDiagramsAndShapesOnly();
		}

		protected override void TestIsEnabledCore()
		{
			AssertIsEnabledForNonDefaultDiagramsAndShapes();
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			AssertNoNeedToTestItAsItIsContainerForOtherNetworkActions();
		}

		#endregion

		#region Implementation

		protected override LinkEntityActions GetActionCore(INetworkViewModel networkViewModel)
		{
			return new LinkEntityActions(networkViewModel);
		}

		protected override Type GetExpectedExecutionStrategyType() => typeof(CommonNetworkActionExecutionStrategy);

		#endregion
	}
}
