using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ASYCUDA.GUI.MenuBuilderHelper;

namespace Enterprise.Customs.ASYCUDA.GUI.Testing
{
	public class MenuBuilderHelperTest : TestCaseWithFactory
	{
		public void TestAddSendManifestMenuItemWithoutMessageStatusProvider()
		{
			var headerAE = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedArabEmirates, "AES");
			AssertNoExceptionThrown(() =>
			{
				using (var form = new ZForm(headerAE))
				using (var menu = new AsycudaMenuForTest(headerAE))
				{
					form.Menu.MenuItems.Add(menu);
					form.Show();
					menu.OnPopup(EventArgs.Empty);
				}
			});
		}

		public void TestAddSendManifestMenuItemValidateManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var menuItems = new List<ZMenuItem>();
			var actionCalled = false;

			CreateManifestMessage menuAction = (h, s) =>
			{
				actionCalled = true;
			};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (var form = new ZForm(header))
			{
				MenuBuilderHelper.AddSendManifestMenuItem(form, menuItems, header, menuAction, "with validation", true);
				MenuBuilderHelper.AddSendManifestMenuItem(form, menuItems, header, menuAction, "no validation", false);

				var menuItem = menuItems.FirstOrDefault(x => x.Caption.GetUnresolvedString() == "Send &with validation");
				AssertNotNull("'Send with validation' menu item not found", menuItem);
				menuItem.PerformClick();
				AssertEquals("Save cancelled, action should not be called", false, actionCalled);

				menuItem = menuItems.FirstOrDefault(x => x.Caption.GetUnresolvedString() == "Send &no validation");
				AssertNotNull("'Send no validation' menu item not found", menuItem);
				menuItem.PerformClick();
				AssertEquals("Validation skipped, action should be called", true, actionCalled);
			}
		}

		public void TestAddBillLevelMenuItemValidateManifest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "B0001";
			bill1.ABL_BillIssuer = "Billy";

			var menuItems = new List<ZMenuItem>();
			var actionCalled = false;

			CreateMessageFromSelectedItems menuAction = (h, s, p, c) =>
			{
				actionCalled = true;
			};

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK; // SendDialog
			ZFormModaliser.SetDelegateToCallOnFormShown((dialog) =>
			{
				if (dialog is AsycudaItemSelectionDialog selectionDlg)
				{
					selectionDlg.SelectAll();
				}
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			using (var form = new ZForm(header))
			{
				MenuBuilderHelper.AddBillLevelMenuItem(form, menuItems, header, menuAction, "with validation", true);
				MenuBuilderHelper.AddBillLevelMenuItem(form, menuItems, header, menuAction, "no validation", false);

				var menuItem = menuItems.FirstOrDefault(x => x.Caption.GetUnresolvedString() == "Send &with validation");
				AssertNotNull("'Send with validation' menu item not found", menuItem);
				menuItem.PerformClick();
				AssertEquals("Save cancelled, action should not be called", false, actionCalled);

				menuItem = menuItems.FirstOrDefault(x => x.Caption.GetUnresolvedString() == "Send &no validation");
				AssertNotNull("'Send no validation' menu item not found", menuItem);
				menuItem.PerformClick();
				AssertEquals("Validation skipped, action should be called", true, actionCalled);
			}
		}
	}
}
