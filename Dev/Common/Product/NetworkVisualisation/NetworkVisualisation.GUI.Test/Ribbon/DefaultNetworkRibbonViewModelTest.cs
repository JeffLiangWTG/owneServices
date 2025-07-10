using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class DefaultNetworkRibbonViewModelTest : NetworkRibbonViewModelTestCase
	{
		protected override INetwork GetNetwork()
		{
			return new DummyNetwork();
		}

		protected override IRibbonDataProvider GetRibbonDataProvider()
		{
			return new TestRibbonDataProvider();
		}
	}
}
