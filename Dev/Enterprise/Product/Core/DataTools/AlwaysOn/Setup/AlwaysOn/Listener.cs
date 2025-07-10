using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public class Listener : IListener
	{
		public Listener(string dnsName, int port, IDbServerInstance server, IAvailabilityGroup availabilityGroup, ListenerIp listenerIp = null, string id = null)
		{
			DnsName = dnsName;
			Port = port;
			Server = server;
			AvailabilityGroup = availabilityGroup;
			Id = id;
			IpAddresses = new List<ListenerIp>();

			if (listenerIp != null)
			{
				IpAddresses.Add(listenerIp);
			}
		}

		public void AddListenerIp(ListenerIp listenerIp)
		{
			IpAddresses.Add(listenerIp);
		}

		public string DnsName { get; set; }
		public int Port { get; set; }
		public IDbServerInstance Server { get; set; }
		public IAvailabilityGroup AvailabilityGroup { get; set; }
		public List<ListenerIp> IpAddresses { get; set; }
		public string Id { get; set; }
	}
}
