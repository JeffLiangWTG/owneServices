using CargoWise.NetworkVisualisation.Integration;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	public class NetworkRefresherTest : TestCase
	{
		public void TestShouldIncreaseRefreshCount()
		{
			var networkViewModel = new NetworkViewModel(new DummyNetwork());
			var refresher = networkViewModel.Network.Refresher;
			AssertType(typeof(NetworkRefresher), refresher);

			var refreshToken = refresher.RefreshToken;

			networkViewModel.Network.Refresh(RefreshType.Affinities);
			AssertNotEquals(refreshToken, refresher.RefreshToken);
		}
	}
}
