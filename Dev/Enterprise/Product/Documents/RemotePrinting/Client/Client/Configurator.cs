using System;
using System.Net;

namespace Enterprise.RemotePrinting.Client
{
	public static class Configurator
	{
		public static WebClient GetWebService(string name)
		{
			return GetWebService(ConnectionRegistryManager.Instance.GetWebClientConfiguration(name));
		}

		public static WebClient GetWebService(WebClientConfiguration config)
		{
			WebClient client = new WebClient(new RemotePrintingServiceAdaptor(config));
			client.SetWebServiceUrlAndCredentials(config);
			return client;
		}

		public static WebClientConfiguration GetProxyDefaultSystemSettings()
		{
			var proxy = WebRequest.DefaultWebProxy;
			var uri = new Uri("http://www.cargowise.com");

			var proxyUri = proxy.GetProxy(uri);
			var config = new WebClientConfiguration()
			{
				ProxyAddress = proxyUri.Host,
				ProxyPort = proxyUri.Port
			};

			//If no default system proxy is defined, GetProxy(uri) returns uri.
			if (config.ProxyAddress == uri.Host)
			{
				config.ProxyAddress = string.Empty;
				config.ProxyPort = 0;
			}
			return config;
		}

		public static IWebProxy GetProxy(WebClientConfiguration config)
		{
			if (config.ProxyEnabled)
			{
				var proxy = config.ProxyUseDefaultSystemSettings ? WebRequest.DefaultWebProxy : new WebProxy(config.ProxyAddress, config.ProxyPort);

				if (!string.IsNullOrEmpty(config.ProxyUser))
				{
					proxy.Credentials = new NetworkCredential(config.ProxyUser, ProtectedDataHelper.Unprotect(config.ProxyPwd));
				}

				return proxy;
			}
			else
			{
				var defaultProxyConfig = Configurator.GetProxyDefaultSystemSettings();
				if (!string.IsNullOrEmpty(defaultProxyConfig.ProxyAddress))
				{
					return new WebProxy(); // Use empty proxy to ignore default proxy configuration
				}
			}

			return null;
		}
	}
}
