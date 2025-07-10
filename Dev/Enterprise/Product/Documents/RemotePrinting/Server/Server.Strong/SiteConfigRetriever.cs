using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using CargoWiseOne.WebInfrastructure;
using Microsoft.Web.Administration;

namespace Enterprise.RemotePrinting.Server
{
	public class SiteConfigRetriever : ExecuteOrInvokeViaAppManager
	{
		SiteConfigRetriever()
		{
		}

		public SiteConfigRetriever(long siteId, string virtualAppPath, IPAddress hostIpAddress, Uri contextUrl, bool forceToUseHttps)
		{
			this.siteId = siteId.ToString(CultureInfo.InvariantCulture);
			this.virtualAppPath = virtualAppPath;
			this.hostIpAddress = hostIpAddress;
			this.contextUrl = contextUrl;
			this.forceToUseHttps = forceToUseHttps;
		}

		string siteId;
		string virtualAppPath;
		IPAddress hostIpAddress;
		readonly Uri contextUrl;
		bool forceToUseHttps;

		public (string protocol, int port, string hostName) GetSiteConfig()
		{
			const int MaxRetries = 3;
			var timeout = 10;

			for (var retries = 0; retries <= MaxRetries; retries++)
			{
				try
				{
					ExecuteOrInvoke();
					break;
				}
				catch (FileLoadException) when (retries < MaxRetries) // Ensure that last iteration will not be caught (e.g. use <= and <)
				{
					// If it fails due to the files being in use, we should not throw before trying to open few time.
					SleepAFewTimes();
				}
				catch (COMException)
				{
					// This condition is to ensure that the last iteration should not sleep a few times 
					if (retries < MaxRetries)
					{
						SleepAFewTimes();
					}
				}
				catch (Exception ex) when (ex.GetType().Name == "RemotingException")
				{
					return (null, -1, null);
				}
			}

			if (!string.IsNullOrEmpty(MessageOverride))
			{
				var siteConfigParts = MessageOverride.Split(':');
				if (siteConfigParts.Length == 3 && int.TryParse(siteConfigParts[1], out var port))
				{
					return (siteConfigParts[0], port, string.Join(":", siteConfigParts.Skip(2)));
				}
			}

			return (null, -1, null);

			void SleepAFewTimes()
			{
				Thread.Sleep(timeout);
				timeout *= 10; // 10, 100, 1000
			}
		}

		protected override string[] GetState()
		{
			return new[] { siteId, virtualAppPath, hostIpAddress.ToString(), forceToUseHttps.ToString() };
		}

		protected override void ReadState(string[] state)
		{
			if (state != null && state.Length == 4)
			{
				siteId = state[0];
				virtualAppPath = state[1];
				if (!IPAddress.TryParse(state[2], out hostIpAddress))
				{
					hostIpAddress = null;
				}

				_ = bool.TryParse(state[3], out forceToUseHttps);
			}
			else
			{
				throw new InvalidOperationException("State should be exactly length 4");
			}
		}

		public override void Execute()
		{
			var path = siteId.TrimEnd('/') + "/" + virtualAppPath.TrimStart('/');
			var appPath = WebAppPath.Parse(path);

			using (var serverManager = new ServerManager())
			{
				Site site = appPath.GetSite(serverManager);
				if (site != null)
				{
					var siteBinding = GetBestSiteBinding(site, hostIpAddress ?? IPAddress.Any);
					if (siteBinding != null)
					{
						MessageOverride = siteBinding.Protocol + ":" + siteBinding.EndPoint?.Port + ":" + siteBinding.Host;
					}
				}
			}
		}

		[SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Readability")]
		Binding GetBestSiteBinding(Site site, IPAddress ipAddress)
		{
			var preferredScheme = PreferredScheme;

			var siteBinding = site.Bindings.FirstOrDefault(binding =>
				string.Equals(binding.Protocol, preferredScheme, StringComparison.OrdinalIgnoreCase) &&
				binding.EndPoint != null &&
				binding.EndPoint.Address.Equals(ipAddress));

			if (siteBinding == null)
			{
				siteBinding = site.Bindings.FirstOrDefault(binding =>
					string.Equals(binding.Protocol, preferredScheme, StringComparison.OrdinalIgnoreCase) &&
					binding.EndPoint != null &&
					binding.EndPoint.Address.Equals(IPAddress.Any));
			}

			if (siteBinding == null)
			{
				siteBinding = site.Bindings.FirstOrDefault(binding =>
					binding.EndPoint != null &&
					binding.EndPoint.Address.Equals(ipAddress));
			}

			if (siteBinding == null)
			{
				siteBinding = site.Bindings.FirstOrDefault(binding =>
					binding.EndPoint != null &&
					binding.EndPoint.Address.Equals(IPAddress.Any));
			}

			if (siteBinding == null)
			{
				siteBinding = site.Bindings.FirstOrDefault(binding =>
					string.Equals(binding.Protocol, preferredScheme, StringComparison.OrdinalIgnoreCase));
			}

			if (siteBinding == null)
			{
				siteBinding = site.Bindings.FirstOrDefault();
			}

			return siteBinding;
		}

		string PreferredScheme => forceToUseHttps ? Uri.UriSchemeHttps : (contextUrl?.Scheme ?? Uri.UriSchemeHttp);
	}
}
