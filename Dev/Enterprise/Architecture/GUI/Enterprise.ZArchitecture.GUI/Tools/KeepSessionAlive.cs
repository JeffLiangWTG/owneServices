using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IKeepSessionAlive
	{
		void KeepAlive();
	}

	public class KeepSessionAlive : IKeepSessionAlive
	{
		public void KeepAlive()
		{
			if (EnterpriseChannel.Instance?.IsConnected ?? false)
			{
				EnterpriseChannel.Instance.SendMessage(EnterpriseChannelMessageTypes.RequestKeepAlive, string.Empty);
			}
		}
	}
}
