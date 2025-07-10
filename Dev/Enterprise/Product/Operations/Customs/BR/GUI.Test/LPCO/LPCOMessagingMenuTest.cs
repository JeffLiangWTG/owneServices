using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class LPCOMessagingMenuTest : TestCaseWithFactory
	{
		public void TestSendLPCO_Click()
		{
			var header = Factory.NewWithValidTestData<CusLPCOHeader>();

			using (var menu = new LPCOMessagingMenu(header))
			{
				var menuItem = menu.MenuItems[0];
				AssertEquals("Send LPCO", menuItem.Text);

				menuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

				menuItem.PerformClick();
				AssertType<LPCOMessageSendingForm>("LPCO Sending form should popup", ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
