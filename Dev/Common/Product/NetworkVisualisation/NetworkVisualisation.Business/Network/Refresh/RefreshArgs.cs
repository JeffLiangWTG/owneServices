using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class RefreshArgsExtensions
	{
		public static void Refresh(this INetworkRefresher refresher, RefreshType type, params INetworkEntity[] entities)
		{
			refresher.Refresh(new RefreshArgs(type, entities));
		}

		public static void Refresh(this INetwork network, RefreshType type, params INetworkEntity[] entities)
		{
			network.Refresher.Refresh(new RefreshArgs(type, entities));
		}
	}
}
