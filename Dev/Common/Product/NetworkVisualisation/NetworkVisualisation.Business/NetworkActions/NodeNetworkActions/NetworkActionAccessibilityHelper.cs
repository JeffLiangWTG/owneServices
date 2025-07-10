using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public static class NetworkActionAccessibilityHelper
	{
		public static NetworkActionAccessibility CheckEntityIsRoot(INetworkEntity entity) => new NetworkActionAccessibility(
			entity.IsRoot(),
			entity,
			() => Res.GetString("57A5DA54-4930-411C-A726-6C4C87CCE48F", "The shape should be the root diagram."));

		public static NetworkActionAccessibility CheckEntityIsNotRoot(INetworkEntity entity) => new NetworkActionAccessibility(
			!entity.IsRoot(),
			entity,
			() => Res.GetString("03002401-F502-4614-BCAB-32FA6D5A2193", "The shape should not be the root diagram."));
	}
}
