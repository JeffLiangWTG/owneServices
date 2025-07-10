using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoStandAloneMenuWithScanTest : TestCaseWithFactory
	{
		public void TestSeaCargoStandAloneMenuWithScan()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenuWithScan(manager, null))
			{
				menu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals("MenuItems[0].Text", "Send &Underbond Requests", menu.MenuItems[0].Text);
					AssertEquals("MenuItems[1].Text", "&Send Message(s)", menu.MenuItems[1].Text);
					AssertEquals("MenuItems[2].Text", "&Amend Message(s)", menu.MenuItems[2].Text);
					AssertEquals("MenuItems[3].Text", "&Withdraw Message(s)", menu.MenuItems[3].Text);
					AssertEquals("MenuItems[4].Text", "&Reset to Original", menu.MenuItems[4].Text);
					AssertEquals("MenuItems[5].Text", "Messaging Problems? Click for HELP.", menu.MenuItems[5].Text);
					AssertEquals("MenuItems[6].Text", "Schedule Out-of-Hours Original Message Sending", menu.MenuItems[6].Text);
					AssertEquals("MenuItems[7].Text", "-", menu.MenuItems[7].Text);
					AssertEquals("MenuItems[8].Text", "Create Contingency Data", menu.MenuItems[8].Text);
					AssertEquals("MenuItems[9].Text", "Export Sea Cargo Containers Data", menu.MenuItems[9].Text);
					AssertEquals("MenuItems[10].Text", "Scan for Outturn", menu.MenuItems[10].Text);
				});
				AssertEquals("MenuItems.Count", 11, menu.MenuItems.Count);
			}
		}

		public void TestScanForOutturnClickErrors()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			var manager = new CusSCAOceanBillMessageManager(oceanBill);
			using (var menu = new SeaCargoStandAloneMenuWithScan(manager, null))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.ShowPopupMenu();
				var scanForOutturnMenuItem = menu.MenuItems.FindByText("Scan for Outturn");
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("To start scanning for outturn you need to obtain HVLVClearance license.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("You cannot start outturn scanning until you have created and sent Sea Cargo messages, and have the associated Underbond record.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
