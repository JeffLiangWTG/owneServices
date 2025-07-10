using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.IT.H7.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.H7.GUI.Testing;

sealed class MenuBuilderTest : TestCaseWithFactory
{
	public void TestSendMessageToCustoms_DisplaysMessageSendingForm()
	{
		var header = Factory.New<AsycudaManifestHeader>();

		using (var form = new ZForm(header))
		using (var menu = new AsycudaMenuForTest(header))
		{
			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			menu.MenuItems.FindByText("Send Message to Customs").PerformClick();
			AssertType<MessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
		}
	}
}
