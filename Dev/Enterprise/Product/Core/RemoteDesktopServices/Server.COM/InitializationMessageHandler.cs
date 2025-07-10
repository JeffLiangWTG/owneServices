using System.Runtime.InteropServices;
using Enterprise.RemoteDesktopServices.MessageElements;

namespace Enterprise.RemoteDesktopServices.Server.COM
{
	[ClassInterface(ClassInterfaceType.None)]
	class InitializationMessageHandler : XmlMessageHandler<InitializationMessage>
	{
		protected override void Handle(IEnterpriseChannel channel, InitializationMessage message)
		{
		}
	}
}
