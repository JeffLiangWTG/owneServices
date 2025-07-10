using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.FR.H7.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.GUI.Testing
{
	[TestedType(typeof(MenuBuilder))]
	class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestSendMessageToCustoms_DisplaysMessageSendingForm()
		{
			var header = Factory.New<H7ManifestHeader>();

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
}
