using System.IO;
using System.Xml.Serialization;
using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.RemoteDesktopServices.Testing
{
	static class InitializationMessageHelper
	{
		public static void Handle(InitializationMessage initializationMessage)
		{
			using (var ms = new MemoryStream())
			{
				new XmlSerializer(typeof(InitializationMessage)).Serialize(ms, initializationMessage);
				ms.Position = 0;
				MessageHandlers.HandleMessage(EnterpriseChannel.Instance, EnterpriseChannelMessageTypes.Initialization, ms);
			}
		}

		public static void Handle()
		{
			Handle(new InitializationMessage(
				new[]
				{
					EnterpriseChannelMessageTypes.WCAAuthentication,
					EnterpriseChannelMessageTypes.ServerRDPVersion,
					EnterpriseChannelMessageTypes.UrlAuthenticationRequired,
					EnterpriseChannelMessageTypes.OpenFileSupported,
				},
				ClientVersion.Version.ToString()));
		}
	}
}
