using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestSendMessageToCustoms()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendMessageMenuItem = menu.MenuItems.FindByText("Send Message to Customs");

				sendMessageMenuItem.PerformClick();
				UnitTestUserNotification.Instance.ClearMessages();

				form.FireSaveButton();

				sendMessageMenuItem.PerformClick();

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
