using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkScaleSet
	{
		IReadOnlyList<INetworkScalePoint> ScalePoints { get; }
		IReadOnlyList<INetworkScaleBackground> BackgroundPoints { get; }

		int IndexOfColumnInPresent { get; }
	}
}
