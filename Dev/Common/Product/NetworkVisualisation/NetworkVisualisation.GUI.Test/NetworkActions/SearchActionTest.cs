using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
#pragma warning disable CS0618
	public class SearchActionTest : NodeNetworkActionsTestCase
	{
		public void TestEnabledness()
		{
			using (var control = new NetworkUserControl())
			{
				AssertActionIsAlwaysAllowed((networkViewModel) => new SearchAction(networkViewModel, control), AccessibilityLevel.Enabledness);
			}
		}

		protected override void TestGetIconNameCore()
		{
			var control = new NetworkUserControl();
			AssertActionHasCorrectIconName(networkViewModel => new SearchAction(networkViewModel, control), string.Empty);
		}

		public void TestAction()
		{
			using (var control = new NetworkUserControl())
			{
				var network = new DummyNetwork();
				(network.DiagramEntity as Entity).IsDiagramWithRibbon = true;
				var networkViewModel = new NetworkViewModel(network);
				var action = new SearchAction(networkViewModel, control);
				action.Execute();
				AssertEquals(true, control.RibbonSearchBoxExposed_ForTesting.IsFocused);
			}
		}
	}
#pragma warning restore CS0618
}
