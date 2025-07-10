using System;

namespace Enterprise.Integration.RemoteDesktopServices
{
	public interface IRemoteChannel
	{
		ReturnElement SendMessage<MessageElement, ReturnElement>(string messageType, MessageElement message);

		string[] RegisteredRemoteMessageTypes { get; }

		Version RemoteVersion { get; }
	}
}
