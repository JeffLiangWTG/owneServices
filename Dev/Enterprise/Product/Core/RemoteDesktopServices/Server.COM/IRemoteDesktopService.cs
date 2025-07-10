using System.Runtime.InteropServices;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[Guid("08082787-9FFA-4B38-9961-0F03E51E92F8")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IRemoteDesktopService
	{
		bool IsConnected();
		void OpenFile(string file, bool readOnly);
		void OpenWebUrl(string url);
		void RegisterRemoteDropHandler(IRemoteFileDropHandler dropHandler);
	}
}
