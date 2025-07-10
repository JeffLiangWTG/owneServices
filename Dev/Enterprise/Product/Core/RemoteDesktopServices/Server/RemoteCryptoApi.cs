using System;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.Cryptoki.Common.ClientServerApi;

namespace Enterprise.RemoteDesktopServices.Server
{
	public static class RemoteCryptoApi
	{
		public static bool IsRemote
		{
			get { return ObjectFactory.Get<TerminalService>().IsWTSSession && ObjectFactory.Get<TerminalService>().IsRemoteAppSession; }
		}

		public static bool IsSupported
		{
			get { return InitializationMessageHandler.RegisteredRemoteMessageTypes.Contains(EnterpriseChannelMessageTypes.CryptoApi); }
		}

		public static ICryptoApi Instance
		{
			get { return IsRemote ? new CryptoApiClient(Send) : new CryptoApi(); }
		}

		public static byte[] Send(byte[] request)
		{
			if (!IsSupported)
			{
				throw new IOException(FormattableString.Invariant($"'{nameof(EnterpriseChannelMessageTypes.CryptoApi)}' is not supported by the plugin installed on the client machine."));
			}

			return EnterpriseChannel.Instance.SendMessage<byte[], byte[]>(EnterpriseChannelMessageTypes.CryptoApi, request);
		}
	}
}
