using System.Collections.Generic;

namespace CargoWise.NetworkVisualisation.Integration
{
	public interface INetworkEntityCollection : INetworkActionResult
	{
		IEnumerable<INetworkEntity> Entities { get; }
	}
}