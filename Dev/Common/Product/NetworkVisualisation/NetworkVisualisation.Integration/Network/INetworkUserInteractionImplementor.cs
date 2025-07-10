namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkUserInteractionImplementor
	{
		void ShowMessage(string message, string caption);

		void ShowError(string message, string caption);
	}
}