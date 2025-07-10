using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoConsolMenuWithScanTest : TestCaseWithFactory
	{
		public void TestScanForOutturnClickErrors()
		{
			var consol = Factory.New<ForwardingConsol>();
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.CB_ParentId = consol.PK;
			oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			using (var plugin = new SeaCargoConsolPlugIn(consol))
			using (var menu = new SeaCargoConsolMenuWithScanForTest(plugin, new CusSCAOceanBillMessageManager(oceanBill), null))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.OnPopup();
				var scanForOutturnMenuItem = menu.MenuItems.FindByText("Scan for Outturn");
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("To start scanning for outturn you need to obtain HVLVClearance license.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("You cannot start outturn scanning until you have created and sent Sea Cargo messages, and have the associated Underbond record.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		sealed class SeaCargoConsolMenuWithScanForTest : SeaCargoConsolMenuWithScan
		{
			internal SeaCargoConsolMenuWithScanForTest(SeaCargoConsolPlugIn plugin, CusSCAOceanBillMessageManager manager, SeaScanForOutturnHost scanHost) : base(plugin, manager, scanHost)
			{
			}

			internal void OnPopup() => base.OnPopup(EventArgs.Empty);
		}
	}
}
