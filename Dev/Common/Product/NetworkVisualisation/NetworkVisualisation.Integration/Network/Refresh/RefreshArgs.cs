using System;
using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public class RefreshArgs : EventArgs
	{
		public RefreshArgs(RefreshType type, params INetworkEntity[] entities)
		{
			RefreshType = type;
			Entities = entities;
		}

		public RefreshType RefreshType { get; private set; }
		public IEnumerable<INetworkEntity> Entities { get; private set; }
	}
}
