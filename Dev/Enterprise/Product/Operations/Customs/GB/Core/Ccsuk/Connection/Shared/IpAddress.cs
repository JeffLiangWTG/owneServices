using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class IpAddress
	{
		public IpAddress(string address)
		{
			this.Address = address;
		}

		public string IpAddressFormatted
		{
			get
			{
				char space = ' ';
				return Address.PadRight(15, space);
			}
		}

		const string PortReleaseGuid = "8875BD8E-4D5B-11DE-B2F4-691756D89593";
		public static int FindNextAvailablePortInRangeOnAddress(int startPort, int endPort, IPAddress localAddress)
		{
			int candidatePort = startPort;

			var mutex = new Mutex(false, string.Concat("Global/", PortReleaseGuid));
			mutex.WaitOne();
			try
			{
				IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
				IPEndPoint[] endPoints = ipGlobalProperties.GetActiveTcpListeners();

				while (candidatePort <= endPort)
				{
					if (endPoints.Any(ep => ep.Address.Equals(localAddress) && ep.Port == candidatePort))
					{
						// Something is already bound.... this port is no good
						candidatePort++;
					}
					else
					{
						break;  // Nothing is bound to this port on the selected address... it's free to use
					}
				}
				return candidatePort <= endPort ? candidatePort : -1;
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		public string Address { get; private set; }
	}
}
