using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class SubscriptionPlugInMenuTest : TestCaseWithFactory
	{
		public void TestSendSubscription_Click()
		{
			var staff = Factory.New<GlbStaff>();
			var staffWrapper = BRGlbStaffWrapper.Get(staff);
			staffWrapper.EventSubscriptions.AddNew();

			using (var plugIn = new StaffCredentialsPlugIn(staff))
			{
				var menu = (SubscriptionPlugInMenu)plugIn.TopLevelMenu;
				var menuItem = menu.MenuItems[0];
				AssertEquals("Send Subscription", menuItem.Text);

				staffWrapper.CCTPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(-10);
				menuItem.PerformClick();
				AssertContains("The certificate has expired. Please create a new certificate.", UnitTestUserNotification.Instance.LastMessage.Text);

				staffWrapper.CCTPassword.GP_ExpiryDate = ZDateTime.Now.AddDays(100);
				menuItem.PerformClick();
				AssertEquals(5, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Certificate not found or invalid.", UnitTestUserNotification.Instance.LastMessage.Text);

				var password = staffWrapper.CCTPassword;
				password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;

				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (SubscriptionMessageSendingForm)obj;
					var sendButton = form.FindSingle<ZButton>("SendButton");
					Assert("SendButton is disable", !sendButton.Enabled);

					form.MessageSendingObjectParent.SendingObjectsCollection.Cast<SubscriptionMessageSendingObject>().ForEach(x => x.ShouldSend = true);
					Assert("SendButton is enable", sendButton.Enabled);

					sendButton.PerformClick();
				});

				menuItem.PerformClick();

				AssertType<SubscriptionMessageSendingForm>("Subscription Sending form should popup", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("1 message(s) have been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
