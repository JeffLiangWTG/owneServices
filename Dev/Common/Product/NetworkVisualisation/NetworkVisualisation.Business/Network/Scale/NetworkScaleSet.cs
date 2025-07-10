using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NetworkScaleSet : INetworkScaleSet
	{
		public NetworkScaleSet(IReadOnlyList<INetworkScalePoint> scalePoints, IReadOnlyList<INetworkScaleBackground> backgroundPoints, int presentColumn)
		{
			ScalePoints = scalePoints;
			BackgroundPoints = backgroundPoints;
			IndexOfColumnInPresent = presentColumn;
		}

		public IReadOnlyList<INetworkScalePoint> ScalePoints { get; }
		public IReadOnlyList<INetworkScaleBackground> BackgroundPoints { get; }
		public int IndexOfColumnInPresent { get; }

		#region Empty

		public static INetworkScaleSet Empty
		{
			get { return new NetworkScaleSet(new List<INetworkScalePoint>().AsReadOnly(), new List<INetworkScaleBackground>().AsReadOnly(), -1); }
		}

		#endregion
	}
}
