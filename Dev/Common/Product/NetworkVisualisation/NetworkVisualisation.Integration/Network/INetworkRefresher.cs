using System;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkRefresher
	{
		INetwork GetReloadedNetwork();

		string RefreshToken { get; }

		void Refresh(RefreshArgs args);

		event EventHandler<RefreshArgs> Refreshed;
	}
}