using System;
using CargoWise.Application;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.AuditDataServices.Glow
{
	static class GlowServiceClient
	{
		public static bool IsServiceUriSpecified => !string.IsNullOrEmpty(ServiceUri?.Trim('/'));

		public static string ServiceUri => GlowRegistry.Instance.GlowServiceUri;

		public static IGlowServiceClient GetClient()
		{
			if (IsServiceUriSpecified)
			{
				var clientFactory = ObjectFactory.Get<IGlowServiceClientFactory>();
				return clientFactory.Create(new Uri(ServiceUri));
			}

			return null;
		}
	}
}
