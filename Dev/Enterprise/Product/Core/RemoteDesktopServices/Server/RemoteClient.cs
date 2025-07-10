using CargoWise.Common;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class RemoteClient
	{
		public RemoteClient(MessageChannel channel)
		{
			Argument.NotNull(channel, nameof(channel));
			Channel = channel;
		}

		public string FullyQualifiedMachineName
		{
			get
			{
				if (Channel == null)
				{
					return string.Empty;
				}
				return Channel.SendMessage<string>(EnterpriseChannelMessageTypes.GetFullyQualifiedMachineName, System.Array.Empty<byte>());
			}
		}

		public MessageChannel Channel { get; }
	}
}
