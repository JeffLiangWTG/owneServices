using System;
using System.IO;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.ZArchitecture.Modules
{
	public class RemoteDesktopServicesEnterpriseUrlHandler : MessageHandlerWithReturn<bool>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Part of the url")]
		protected override bool DoHandle(IEnterpriseChannel channel, Stream messageData)
		{
			var isUrlAuthenticationSupported = InitializationMessageHandler.IsUrlAuthenticationSupported;
			try
			{
				using (var reader = new StreamReader(messageData))
				{
					var url = reader.ReadToEnd();
					var waitForMainForm = !isUrlAuthenticationSupported || url.EndsWith("&Wait=True");
					return EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, waitForMainForm);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && !isUrlAuthenticationSupported)
			{
				return false; // Old version of RemoteDesktopServicesUrlHandlerService should silently try the next enterprise
			}
		}
	}
}
