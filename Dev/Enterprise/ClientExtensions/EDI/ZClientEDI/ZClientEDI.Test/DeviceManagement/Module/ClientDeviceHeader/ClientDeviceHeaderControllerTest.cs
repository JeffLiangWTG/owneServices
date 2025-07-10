using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderController))]
	public class ClientDeviceHeaderControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ClientControllerRegistration.ClientDevice;
	}
}
