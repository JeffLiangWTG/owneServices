using System;
using System.Linq;
using System.Threading;

using Enterprise.RemoteDesktopServices.MessageElements;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class InitializationMessageHandler : XmlMessageHandler<InitializationMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, InitializationMessage message)
		{
			if (RegisteredRemoteMessageTypes == null || RegisteredRemoteMessageTypes.Length == 0)
			{
				RemoteInitializationMessage = message;
				EnvProxy.RegisteredRemoteMessageTypesCopyForGlobal = message.supportedMessageTypes;
				InitializationCompleted.Set();
			}
		}

		public static string[] RegisteredRemoteMessageTypes
		{
			get { return RemoteInitializationMessage.supportedMessageTypes; }
			set { RemoteInitializationMessage.supportedMessageTypes = value; }
		}

		public static Version RemoteVersion
		{
			get
			{
				var version = RemoteInitializationMessage.version;
				return string.IsNullOrEmpty(version) ? null : new Version(version);
			}
		}

		public static InitializationMessage RemoteInitializationMessage
		{
			get { return remoteInitializationMessage ?? new InitializationMessage(Array.Empty<string>(), ""); }
			set { remoteInitializationMessage = value; }
		}

		// NOTE: To be compatible with old client versions
		public static bool IsUrlAuthenticationSupported
		{
			get { return RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.UrlAuthenticationRequired); }
		}

		public static bool IsDragDropLiteSupported
		{
			get { return RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.ServerRDPVersion); }
		}

		public static EventWaitHandle InitializationCompleted
		{
			get { return initializationCompleted; }
		}

		static readonly EventWaitHandle initializationCompleted = new ManualResetEvent(false);
		static InitializationMessage remoteInitializationMessage;
	}
}
