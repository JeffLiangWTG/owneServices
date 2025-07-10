using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	class NetworkRibbonResourcesProviderTest : TestCase
	{
		public void TestShouldCacheResources()
		{
			var provider = new NetworkRibbonResourcesProvider();
			var resourcesAddress = @"pack://application:,,,/CargoWise.NetworkVisualisation.GUI;component/Ribbon/DefaultViewModel/DefaultNetworkRibbonResources.xaml";
			provider.AddRibbonResources(resourcesAddress);
			var drawing = provider.GetResource("Refresh");
			AssertNotNull(drawing);
		}
	}
}
