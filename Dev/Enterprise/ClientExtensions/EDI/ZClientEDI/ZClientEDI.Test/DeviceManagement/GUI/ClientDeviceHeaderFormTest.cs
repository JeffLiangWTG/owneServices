using System.Windows.Forms;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.GUI.Testing
{
	[TestedType(typeof(ClientDeviceHeaderForm))]
	public class ClientDeviceHeaderFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var device = Factory.New<ClientDeviceHeader>();
			return new ClientDeviceHeaderForm(device);
		}
	}
}
