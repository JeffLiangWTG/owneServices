using System.Runtime.InteropServices;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
	[Guid("4C3704B3-D71E-4FA0-85A4-DDCDD84A21CB")]
	[InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IRemoteFileDropHandler
	{
		void HandleRemoteFileDrop(string[] droppedFiles);
	}
}
