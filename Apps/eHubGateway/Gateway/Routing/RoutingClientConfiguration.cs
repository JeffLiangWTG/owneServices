using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CargoWise.eHub.Gateway.Routing
{
	public class RoutingClientsConfiguration
	{
		public RoutingClientsConfiguration(bool enabled, List<RoutingClientConfigurationItem> list)
		{
			this.Enabled = enabled;
			Clients = new ReadOnlyDictionary<string, RoutingClientConfigurationItem>(list.ToDictionary(x => x.ClientId));
		}

		public ReadOnlyDictionary<string, RoutingClientConfigurationItem> Clients { get; set; }
		public bool Enabled { get; set; } = false;
	}


	public class RoutingClientConfigurationItem
	{
		public string ClientId { get; set; }
		public RoutingMethod RoutingMethod { get; set; }
		public string RouteId { get; set; }
	}

	public enum RoutingMethod
	{
		Standard = 0,
		OCM = 1
	}
}
