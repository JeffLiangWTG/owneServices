using System.Windows.Forms;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.GUI.Testing
{
	[TestedType(typeof(ClientTelRimRegistrationOpenRegistrationForm))]
	class ClientTelRimRegistrationOpenRegistrationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var device = Factory.New<ClientDeviceHeader>();
			return new ClientTelRimRegistrationOpenRegistrationForm(device);
		}
	}
}
