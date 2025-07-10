using CargoWise.Cryptoki.Common.ClientServerApi;

namespace Enterprise.RemoteDesktopServices.Server
{
	public static class RemoteCryptoApi
	{
		public static ICryptoApi Instance => new CryptoApi();
	}
}
