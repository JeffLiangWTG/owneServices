using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Business.Test;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class FitNodesActionTest : NodeNetworkActionsTestCase
	{
		public void TestDescription()
		{
			using (var config = NetworkGuiTestConfig.CreateWithNodes())
			{
				var action = config.FindOnlyRibbonAction<FitNodesAction>();
				AssertEquals("Fit all nodes to the view-port", action.GetDescription());

				config.Node1.IsSelected = true;
				AssertEquals("Fit selected node to the view-port", action.GetDescription());

				config.Node2.IsSelected = true;
				AssertEquals("Fit selected nodes to the view-port", action.GetDescription());
			}
		}

#pragma warning disable CS0618
		public void TestEnabledness()
		{
			using (var control = new NetworkUserControl())
			{
				AssertActionIsAlwaysAllowed((networkViewModel) => new FitNodesAction(networkViewModel, control), AccessibilityLevel.Enabledness);
			}
		}
#pragma warning restore CS0618

#pragma warning disable CS0618
		protected override void TestGetIconNameCore()
		{
			var network = new DummyNetwork();
			var networkViewModel = new NetworkViewModel(network);
			var control = new NetworkUserControl();
			var action = new FitNodesAction(networkViewModel, control);
			AssertEquals("Fit", action.GetIconName());
		}
#pragma warning restore CS0618
	}
}
