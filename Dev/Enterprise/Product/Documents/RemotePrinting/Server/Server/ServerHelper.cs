using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using CargoWise.Data;
using CargoWiseOne.WebInfrastructure;
using Enterprise.RemotePrinting.Server.RPSCore;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.RemotePrinting.Server
{
	public static class ServerHelper
	{
		public static string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection)
		{
			var forceToUseHttps = RegistryData.WebPrintForceToUseHTTPSForWebPrintRequests(connection);
			return GetCurrentWebServiceAddress(contextUrl, connection, forceToUseHttps);
		}

		public static string GetCurrentWebServiceAddress(Uri contextUrl, DbConnection connection, bool forceToUseHttps)
		{
			return webServiceAddressCache.GetOrAdd((WebAppPath.ForCurrentAppDomain(), forceToUseHttps), key => GetWebServiceAddressForWebAppPath(key.Item1, contextUrl, key.Item2));
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		static readonly ConcurrentDictionary<(WebAppPath, bool), string> webServiceAddressCache = new ConcurrentDictionary<(WebAppPath, bool), string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static string GetWebServiceAddressForWebAppPath(WebAppPath appPath, Uri contextUrl, bool forceToUseHttps)
		{
			var hostIpAddress = GetHostIpAddress();
			var virtualAppPath = appPath.VirtualAppPath;

			var (protocol, port, hostName) = new SiteConfigRetriever(appPath.SiteId, virtualAppPath, hostIpAddress, contextUrl, forceToUseHttps).GetSiteConfig();

			if (string.IsNullOrEmpty(protocol) || port == -1 || hostName == null)
			{
				var url = contextUrl ?? HttpContext.Current.Request.Url;
				protocol = url.Scheme;
				port = url.Port;
				hostName = url.Host;
			}

			var uriHostPart = hostIpAddress == null || hostIpAddress.Equals(IPAddress.Any)
				? (string.IsNullOrEmpty(hostName) ? HttpContext.Current.Request.Url.Host : hostName)
				: hostIpAddress.ToString();

			var result = new UriBuilder(protocol, uriHostPart, port, virtualAppPath).ToString();

			if (!string.IsNullOrEmpty(hostName))
			{
				result += "|host:" + hostName;
			}

			return result;
		}

		static IPAddress GetHostIpAddress()
		{
			if (cachedHostIpAddress == null)
			{
				var hostName = Dns.GetHostName();

				IPHostEntry ipHostInfo = Dns.GetHostEntry(hostName);

				cachedHostIpAddress =
					ipHostInfo.AddressList.FirstOrDefault(adr => adr.AddressFamily == AddressFamily.InterNetwork) // Ip4
					?? ipHostInfo.AddressList.FirstOrDefault(adr => adr.AddressFamily == AddressFamily.InterNetworkV6) // Ip6
					?? ipHostInfo.AddressList.FirstOrDefault();
			}
			return cachedHostIpAddress;
		}

		[ThreadStatic]
		static IPAddress cachedHostIpAddress;
	}
}
