using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Customs.GB.Ccsuk.Connection
{
	public class IpAddressHelper
	{
		public IpAddressHelper(ILogger logger)
		{
			this.logger = logger;
			AllGoodAddresses = new List<CcsukIpaddressesSetting>();
			var allPairsOfAddressesInRegistry = GBCustomsDataRegistry.Instance.CcsukIpAddresses.Value;
			if (allPairsOfAddressesInRegistry != null && allPairsOfAddressesInRegistry.Count > 0)
			{
				allPairsOfAddressesInRegistry.Sort(CcsukIpaddressesSetting.Schema.Sequence);
				foreach (CcsukIpaddressesSetting registryIPPair in allPairsOfAddressesInRegistry)
				{
					Log(string.Format(CultureInfo.InvariantCulture, "Evaluating IP address pair [{0}]-->[{1}] (Name={2}, Transport={3})...", registryIPPair.LocalIpAddress, registryIPPair.CcsukParticipantIpAddress, registryIPPair.FriendlyName, registryIPPair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue));
					if (!registryIPPair.LocalIpAddress.IsEmpty && LocalIpAddressIsAvailable(registryIPPair.LocalIpAddress))
					{
						if (EnvProxy.IsHostedWithCargowise && registryIPPair.Transport == CcsukIpTransportList.Codes.IPC)
						{
							Log("...the IPC profile [" + registryIPPair.FriendlyName + "] cannot be used in a hosted environment");
						}
						else
						{
							Log("...found candidate pair for [" + registryIPPair.FriendlyName + "] via transport " + registryIPPair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue);
							AllGoodAddresses.Add(registryIPPair);
						}
					}
				}

				if (AllGoodAddresses.Count == 0)
				{
					throw new HostedServiceException("No acceptable participant IPs were given in the registry, cannot even try to connect");
				}
			}
			else
			{
				// Use legacy approach - do not check the validity of the local address.
				var participantIpAddressString = GBCustomsDataRegistry.Instance.CcsukNetworkIpToDeclareForCallBack_Legacy.Value;
				var localIpAddressString = GBCustomsDataRegistry.Instance.CcsukLocalIpForBindingListener_Legacy.Value;
				var pair = new CcsukIpaddressesSetting() { CcsukParticipantIpAddress = participantIpAddressString, LocalIpAddress = localIpAddressString, FriendlyName = "Legacy setting" };
				CurrentAddressesToUse = pair;
				AllGoodAddresses.Add(pair);
			}
		}

		int addressIndex = -1;
		public CcsukIpaddressesSetting GetNextAddress()
		{
			addressIndex++;
			if (addressIndex < AllGoodAddresses.Count)
			{
				CurrentAddressesToUse = AllGoodAddresses[addressIndex];
			}
			else
			{
				CurrentAddressesToUse = null;
			}
			return CurrentAddressesToUse;
		}

		CcsukIpaddressesSetting CurrentAddressesToUse
		{
			get; set;
		}

		List<CcsukIpaddressesSetting> AllGoodAddresses
		{
			get; set;
		}

		void Log(string p)
		{
			if (logger != null)
			{
				logger.Log(LogType.Information, p);
			}
		}

		bool LocalIpAddressIsAvailable(ZString proposedLocalAddress)
		{
			IPAddress address;
			var result = false;
			if (IPAddress.TryParse(proposedLocalAddress, out address))
			{
				if (!IPAddress.IsLoopback(address))
				{
					Socket testSocket = null;
					try
					{
						var endpoint = new IPEndPoint(address, 0);
						testSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
						testSocket.Bind(endpoint);
						result = true;
					}
					catch (SocketException)
					{
						Log("Could not bind, this local address cannot be used. This is not necessarily a problem yet, because all profiles are evaluated for suitability and it is normal that several are not suitable.");
					}
					finally
					{
						testSocket.Dispose();
					}
				}
				else
				{
					Log("This local IP address is a loopback address and is ignored");
				}
			}
			else
			{
				Log("This local IP address is not valid");  // Should never see this because the registry GUI has validation. But boot n braces. 
			}
			return result;
		}

		readonly ILogger logger;
	}
}
