using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.SystemToSystemTrust;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class SystemToSystemTrustHandler : ISystemToSystemTrustHandler
	{
		#region implementation

		public void SendMessage(string accessToken, string postUrl)
		{
			var trustMessage = new SystemToSystemTrustMessage() { AccessToken = accessToken, PostUrl = postUrl };

			if (IsRemote)
			{
				SendMessageRemote(trustMessage);
			}
			else
			{
				SendMessageLocal(trustMessage, WebUrlLauncher.Launch);
			}
		}

		#endregion

		static bool IsRemote => ObjectFactory.Get<TerminalService>().IsWTSSession && ObjectFactory.Get<TerminalService>().IsRemoteAppSession;

		static bool IsSupported => InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.SystemToSystemTrustMessage);

		static void SendMessageRemote(SystemToSystemTrustMessage message)
		{
			if (!IsSupported)
			{
				Globals.Message.ShowError(ZTerminalService.ClientPluginApplicationNotInstalledError);
			}
			else
			{
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.SystemToSystemTrustMessage, message);
			}
		}

#if DEBUG
		public
#endif
		static void SendMessageLocal(SystemToSystemTrustMessage message, SystemToSystemTrustMessageLauncher systemToSystemTrustMessageLauncher)
		{
			try
			{
				AddUrlAcl.EnsureCallbackUrlConfigured(SystemToSystemTrustMessageSender.SystemToSystemTrustMessageListenerUrl);
				SystemToSystemTrustMessageSender.SendMessage(message, systemToSystemTrustMessageLauncher);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message);
			}
		}
	}
}
