using System;
using Enterprise.Integration.RemoteDesktopServices;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class RemoteChannel : IRemoteChannel
	{
		public string[] RegisteredRemoteMessageTypes => InitializationMessageHandler.RegisteredRemoteMessageTypes;

		public Version RemoteVersion => InitializationMessageHandler.RemoteVersion;

		public ReturnElement SendMessage<MessageElement, ReturnElement>(string messageType, MessageElement message) => EnterpriseChannel.Instance.SendMessage<MessageElement, ReturnElement>(messageType, message);
	}
}
