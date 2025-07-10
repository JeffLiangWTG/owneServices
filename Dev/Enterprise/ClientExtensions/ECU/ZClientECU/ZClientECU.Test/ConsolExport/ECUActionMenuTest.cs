using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	public class ECUActionMenuTest : TestCaseWithFactory
	{
		public void TestMenuOnConsolFormWithSeaFreight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			using (ConsolFormForActionMenuTesting form = new ConsolFormForActionMenuTesting(consol))
			{
				form.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem dataMenu = GetMenuItem(form.ActionMenuItemForTesting.MenuItems, "&Data");
				AssertNotNull("Should have the data menu item", dataMenu);
				AssertEquals("Initialised Menu Type", typeof(ECUActionMenu), dataMenu.GetType());
				AssertEquals("Data menu's item count should be same as the number added in AddCustomMenuItems() method", 1, dataMenu.MenuItems.Count);
			}
		}

		public void TestMenuOnConsolFormWithAirFreight()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			using (ConsolFormForActionMenuTesting form = new ConsolFormForActionMenuTesting(consol))
			{
				form.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem dataMenu = GetMenuItem(form.ActionMenuItemForTesting.MenuItems, "&Data");
				AssertNull("Should NOT have the data menu item - should only appear if its sea freight", dataMenu);
			}
		}

		[GuiTest]
		public void TestNoErrorMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			using (ConsolFormForActionMenuTesting testForm = new ConsolFormForActionMenuTesting(consol))
			{
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				testForm.Show();
				testForm.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem actionMenu = GetMenuItem(testForm.ActionMenuItemForTesting.MenuItems, "&Data");
				MenuItem eCUDataExportMenu = GetMenuItem(actionMenu.MenuItems, "Export to ECU Format");
				eCUDataExportMenu.PerformClick();
				AssertEquals("No errors shown", 0, UnitTestUserNotification.Instance.ShownErrorKeys.Length);
			}
		}

		[GuiTest]
		public void TestShowDialogBox()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Logs.AddNew(Events.DataExport, "");
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_RL_NKLoadPort = "AUSYD";
			using (ConsolFormForActionMenuTesting testForm = new ConsolFormForActionMenuTesting(consol))
			{
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				userNotify.AddAnswer(DialogResult.No);
				testForm.Show();
				testForm.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem actionMenu = GetMenuItem(testForm.ActionMenuItemForTesting.MenuItems, "&Data");
				MenuItem eCUDataExportMenu = GetMenuItem(actionMenu.MenuItems, "Export to ECU Format");
				eCUDataExportMenu.PerformClick();
				ZString expectedMesg = "Consol is Already Exported. Do you want to Re-export the Consol?";
				AssertEquals(expectedMesg, userNotify.LastMessage.Text);
			}
		}

		[GuiTest]
		public void TestShowErrorMessage()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "NZAKL";
			using (ConsolFormForActionMenuTesting testForm = new ConsolFormForActionMenuTesting(consol))
			{
				UnitTestUserNotification userNotify = UnitTestUserNotification.Instance;
				userNotify.ClearMessagesAndAnswers();
				testForm.Show();
				testForm.ActionMenuItemForTesting.OnPopup(EventArgs.Empty);
				MenuItem actionMenu = GetMenuItem(testForm.ActionMenuItemForTesting.MenuItems, "&Data");
				MenuItem eCUDataExportMenu = GetMenuItem(actionMenu.MenuItems, "Export to ECU Format");
				eCUDataExportMenu.PerformClick();
				ZString expectedMesg = "This Operation is not Supported for Import Consol.";
				AssertEquals(expectedMesg, userNotify.LastMessage.Text);
			}
		}

		MenuItem GetMenuItem(Menu.MenuItemCollection menuItems, string menuItemText)
		{
			MenuItem returnValue = null;
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == menuItemText)
				{
					returnValue = item;
					break;
				}
			}

			return returnValue;
		}

		class ConsolFormForActionMenuTesting : ConsolForm
		{
			public ConsolFormForActionMenuTesting(ForwardingConsol consol) : base(consol)
			{
			}

			public MenuItem ActionMenuItemForTesting
			{
				get
				{
					return ActionsMenuItem;
				}
			}
		}
	}
}
