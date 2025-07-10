using System.IO;

namespace Enterprise.RemoteDesktopServices.Server
{
	public class DragCallBackHandler : XmlMessageHandlerWithReturn<string, byte[]>
	{
		protected override byte[] DoHandle(IEnterpriseChannel channel, string message)
		{
			return File.ReadAllBytes(message);
		}
	}
}
