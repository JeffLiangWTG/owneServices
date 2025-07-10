using System.Security.Principal;
using CargoWise.ApplicationManager.Common;
using CargoWise.Interop;

namespace Enterprise.RemoteDesktopServices.Server
{
	public static class AddUrlAcl
	{
		const string OIDCCallbackUrl = "http://127.0.0.1:80/CargowiseOne/Authorize/";

		public static void EnsureCallbackUrlConfigured(string callbackUrl = OIDCCallbackUrl)
		{
			using (var httpApi = new HttpApi())
			{
				if (httpApi.GetHttpServiceConfigUrlAclInfo(callbackUrl) == null)
				{
					AppManagerClientFactory.GetNewAppManager().Invoke(typeof(AddUrlAcl).Assembly.Location, typeof(AddUrlAclInvocable).FullName, callbackUrl, null);
				}
			}
		}

		class AddUrlAclInvocable : IAppManagerInvocable
		{
			public AppManagerResult Invoke(bool waitedForMutex, object state)
			{
				var securityDescriptor = string.Format("D:(A;;GX;;;{0})", new SecurityIdentifier(WellKnownSidType.WorldSid, null));
				using (var httpApi = new HttpApi())
				{
					httpApi.SetHttpServiceConfigUrlAclInfo((string)state, securityDescriptor);
				}

				return new AppManagerResult(AppManagerResultStatus.Success);
			}
		}
	}
}
