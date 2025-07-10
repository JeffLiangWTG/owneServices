using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI.Testing
{
	public class ClientDeviceHeaderFormPostingButtonsTest : TestCaseWithFactory
	{
		public void TestAllowsNewForTemplates()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = true;
			using (var form = new ClientDeviceHeaderForm(device))
			{
				var provider = form as IPostingButtonsProvider;
				AssertNotNull(provider);
				AssertEquals(true, provider.AllowNew);
			}
		}

		public void TestDoesNotNewForDevices()
		{
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_IsTemplate = false;
			using (var form = new ClientDeviceHeaderForm(device))
			{
				var provider = form as IPostingButtonsProvider;
				AssertNotNull(provider);
				AssertEquals(false, provider.AllowNew);
			}
		}
	}
}
