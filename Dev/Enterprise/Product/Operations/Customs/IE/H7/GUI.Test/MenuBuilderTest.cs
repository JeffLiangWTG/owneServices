using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.IE.H7.Business;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.H7.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuItemVisibility_SendRefundApplication()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendRF415MenuItem = menu.MenuItems.FindByText("Send Refund Application");
				AssertNotNull(sendRF415MenuItem);
			}
		}

		public void TestMenuItemAction_CheckROSCredential()
		{
			var (company, branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland);
			InterchangeProcessorTestHelper.CreateValidCredential(company);

			var header = Factory.New<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var menu = new AsycudaMenuForTest(header))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendMessageMenuItem = menu.MenuItems.FindByText("Send Message to Customs");
				var refundApplicationMenuItem = menu.MenuItems.FindByText("Send Refund Application");

				AssertROSCredentialPopupForMenuItemClick(sendMessageMenuItem);
				AssertROSCredentialPopupForMenuItemClick(refundApplicationMenuItem);

				header.AMA_GB = branch.PK;
				form.FireSaveButton();

				sendMessageMenuItem.PerformClick();
				refundApplicationMenuItem.PerformClick();

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertROSCredentialPopupForMenuItemClick(MenuItem menuItemToAssert)
		{
			menuItemToAssert.PerformClick();

			AssertEquals("ROS Credential should be validated", "Cannot send message as Company (EDI) doesn't have a Revenue ROS certificate uploaded or credential added. Please see eLearning 'Set up CargoWise for Customs Messaging in Ireland' on My Account for assistance.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
		}

		public void TestMenuItemAction_SendRF415MessageToCustoms_ShouldPopupWarningMessageBeforeSavingChanges()
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ApplicationCode = "LV1";
			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				manifestHeader.AMA_AgentType = "CHY";
				Assert(manifestHeader.HasChanges);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.MenuItems.FindByText("Send Refund Application").PerformClick();

				AssertEquals("Should popup warning message if the form is not saved.", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage?.Text);
			}
		}
	}
}
