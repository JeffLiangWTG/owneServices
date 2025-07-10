namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkScalePoint
	{
		int Column { get; }
		string Label { get; }
		string ToolTip { get; }
	}
}
