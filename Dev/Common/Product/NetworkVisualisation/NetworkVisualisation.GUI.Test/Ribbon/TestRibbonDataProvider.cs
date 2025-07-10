using CargoWise.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
#pragma warning disable CS0618
	public class TestRibbonDataProvider : IRibbonDataProvider
	{
		public RibbonViewModel GetRibbonViewModel(NetworkViewModel networkViewModel, NetworkUserControl control)
		{
			return new DefaultNetworkRibbonViewModel(networkViewModel, control);
		}
	}
#pragma warning restore CS0618
}
