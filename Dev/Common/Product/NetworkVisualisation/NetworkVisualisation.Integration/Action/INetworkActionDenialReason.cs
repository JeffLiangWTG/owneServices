namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkActionDenialReason
	{
		INetworkEntity Entity { get; }
		string Explanation { get; }
		bool NeedsNotification { get; }
	}
}