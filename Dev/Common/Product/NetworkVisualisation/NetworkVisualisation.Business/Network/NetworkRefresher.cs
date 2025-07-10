using System;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class NetworkRefresher : INetworkRefresher
	{
		public void AssociateWithNetwork(INetwork network)
		{
			if (Network == null)
			{
				Network = network;
			}
			else
			{
				throw new InvalidOperationException("This refresher should not handle multiple networks.");
			}
		}

		public INetwork Network { get; private set; }

		public INetwork GetReloadedNetwork()
		{
			return Network;
		}

		public string RefreshToken { get; private set; }

		public void Refresh(RefreshArgs args)
		{
			RefreshToken = Guid.NewGuid().ToString();
			Refreshed?.Invoke(this, args);
		}

		public event EventHandler<RefreshArgs> Refreshed;
	}
}
