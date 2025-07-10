using System;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class JobNetworkRefresher : INetworkRefresher
	{
		public void AddedToNetwork(JobNetwork jobNetwork)
		{
			if (this.network == null)
			{
				this.network = jobNetwork;
			}
			else
			{
				throw new InvalidOperationException("This refresher should not handle multiple networks.");
			}
		}

		JobNetwork network;

		public INetwork GetReloadedNetwork()
		{
			if (!network.DiagramEntity.IsDeleted)
			{
				return network;
			}
			else
			{
				return new DummyNetwork { Name = Res.GetString("45e5f898-f6d8-436d-9324-db02969d6213", "Network was deleted"), DiagramEntity = new Entity() };
			}
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
