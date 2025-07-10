using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(LinkFromClipboardAction))]
	class LinkFromClipboardActionTest : JobNetworkActionTestCase<LinkFromClipboardAction>
	{
		#region Main Properties

		protected override void TestGetNameCore()
		{
			Assert("Impossible to test it here without access to GUI - tested in NetworkDiagramFormIntegrationTest (Linked Entity Menu region)", true);
		}

		protected override void TestGetDescriptionCore()
		{
			Assert("Impossible to test it here without access to GUI - tested in NetworkDiagramFormIntegrationTest (Linked Entity Menu region)", true);
		}

		protected override void TestGetIconCore()
		{
			AssertActionHasCorrectIcon("Link");
		}

		#endregion

		#region Accessibility

		protected override void TestIsApplicableCore()
		{
			Assert("Impossible to test it here without access to GUI - tested in NetworkDiagramFormIntegrationTest (Linked Entity Menu region)", true);
		}

		protected override void TestIsEnabledCore()
		{
			Assert("Impossible to test it here without access to GUI - tested in NetworkDiagramFormIntegrationTest (Linked Entity Menu region)", true);
		}

		#endregion

		#region Execution

		protected override void TestExecuteCore()
		{
			Assert("Impossible to test it here without access to GUI - tested in NetworkDiagramFormIntegrationTest (Linked Entity Menu region)", true);
		}

		#endregion

		#region Implementation

		protected override LinkFromClipboardAction GetActionCore(INetworkViewModel networkViewModel)
		{
			return new LinkFromClipboardAction(networkViewModel);
		}

		#endregion
	}
}
