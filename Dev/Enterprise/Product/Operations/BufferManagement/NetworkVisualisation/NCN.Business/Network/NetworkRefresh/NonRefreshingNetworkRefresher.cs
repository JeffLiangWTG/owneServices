using System;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	/// <summary>
	/// A refresher used temporarily while initialising network controls (before setting data context) and for testing purposes
	/// </summary>
	public class NonRefreshingNetworkRefresher : INetworkRefresher
	{
		public INetwork GetReloadedNetwork()
		{
			throw new NotImplementedException();
		}

		string INetworkRefresher.RefreshToken => null;

		public void Refresh(RefreshArgs args)
		{
			// Do nothin.
		}

		public event EventHandler<RefreshArgs> Refreshed { add { } remove { } }
	}
}
