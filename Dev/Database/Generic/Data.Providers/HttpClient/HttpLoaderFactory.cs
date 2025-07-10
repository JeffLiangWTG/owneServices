using System;
using CargoWise.Data.SqlProxy.Interface;

namespace CargoWise.Data.HttpClient
{
	public static class HttpLoaderFactory
	{
		public static void SetGlowLoaderServerListeningPort(string port)
		{
			SqlProxyUrls.ListeningPort = port;
		}

		public static bool IsHttpServerConfigured => SqlProxyUrls.ListeningPort != null;

		internal static SqlProxyClient GetClient()
		{
			if (!IsHttpServerConfigured)
			{
				throw new InvalidOperationException("GlowLoader server is not configured yet.");
			}

			return SqlProxyClient.CreateHttpClient();
		}

		public static void ResetGlowLoaderService()
		{
			SqlProxyUrls.ListeningPort = null;
		}
	}
}
