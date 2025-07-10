using System.Security.Principal;
using CargoWise.ApplicationManager.Common;
using CargoWise.Interop;

namespace Enterprise.BlazorWinFormsInterop
{
	public static class HttpServiceConfig
	{
		public const string UrlAclUrlPrefix = "http://+:7070/cargowise/blazorwinformintegration/";

		public static void EnsureHttpServiceConfig()
		{
			using (var httpApi = new HttpApi())
			{
				if (httpApi.GetHttpServiceConfigUrlAclInfo(UrlAclUrlPrefix) == null)
				{
					AppManagerClientFactory.GetNewAppManager().Invoke(typeof(HttpServiceConfig).Assembly.Location, typeof(HttpServiceConfigInvocable).FullName, null, null);
				}
			}
		}

		class HttpServiceConfigInvocable : IAppManagerInvocable
		{
			public AppManagerResult Invoke(bool waitedForMutex, object state)
			{
				var securityDescriptor = string.Format("D:(A;;GX;;;{0})", new SecurityIdentifier(WellKnownSidType.WorldSid, null));
				using (var httpApi = new HttpApi())
				{
					httpApi.SetHttpServiceConfigUrlAclInfo(UrlAclUrlPrefix, securityDescriptor);
				}
				return new AppManagerResult(AppManagerResultStatus.Success);
			}
		}
	}
}
