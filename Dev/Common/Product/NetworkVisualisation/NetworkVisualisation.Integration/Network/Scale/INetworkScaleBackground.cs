using System.Drawing;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkScaleBackground
	{
		int StartColumn { get; }
		int EndColumn { get; }
		Color Color { get; }
	}
}
