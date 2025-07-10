namespace Enterprise.RemoteDesktopServices.Server
{
	public partial class EnterpriseChannel
	{
		enum ConnectReason
		{
			FirstAttempOnInitializing,
			SecondAttempOnInitializing,
			RemoteConnectOnServerSessionSwitch,
		}
	}
}
