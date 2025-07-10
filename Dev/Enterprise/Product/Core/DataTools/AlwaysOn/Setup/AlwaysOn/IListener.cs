using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public interface IListener
	{
		string DnsName { get; }

		string Id { get; }

		int Port { get; }

		IDbServerInstance Server { get; }

		IAvailabilityGroup AvailabilityGroup { get; }

		List<ListenerIp> IpAddresses { get; }
	}
}
