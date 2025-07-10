using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCargoMasterMenuWithScanTest : AirCargoMasterMenuTest
	{
		public void TestScanForOutturnClickErrors()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			MasterBill.CM_MAWB = "08111111111";
			using (var menu = new AirCargoMasterMenuForTest(MasterBill, new CusMAWBMessageManager(() => MasterBill), null))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				menu.OnPopupInternal(EventArgs.Empty);
				MenuItem scanForOutturnMenuItem = null;
				foreach (MenuItem menuItem in menu.MenuItems)
				{
					if (menuItem.Text == "Scan for Outturn")
					{
						scanForOutturnMenuItem = menuItem;
						break;
					}
				}

				AssertNotNull(scanForOutturnMenuItem);
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("License error", menu.ErrorCaption);
				AssertEquals("To start scanning for outturn you need to obtain HVLVClearance license.", menu.ErrorMessage);
				Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
				scanForOutturnMenuItem.PerformClick();
				AssertEquals("Validation error", menu.ErrorCaption);
				AssertEquals("You cannot start outturn scanning until you have created and sent Air Cargo messages, and have the associated Underbond record. To send Air Cargo messages, click on the AirCargo tab and create AirCargo first, then save Consol. You will then need to close and reopen the consol, before you can proceed with outturn scanning.", menu.ErrorMessage);
			}
		}

		public void TestScanForOutturnClick()
		{
			MasterBill.CM_ArrivalDate = ZDateTime.Now.AddDays(4);
			MasterBill.CM_MAWB = "08111111111";
			using (var form = new ZForm())
			{
				var host = new AirScanForOutturnHost(form, MasterBill);
				using (var menu = new AirCargoMasterMenuForTest(MasterBill, new CusMAWBMessageManager(() => MasterBill), host))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					menu.OnPopupInternal(EventArgs.Empty);
					MenuItem scanForOutturnMenuItem = null;
					foreach (MenuItem menuItem in menu.MenuItems)
					{
						if (menuItem.Text == "Scan for Outturn")
						{
							scanForOutturnMenuItem = menuItem;
							break;
						}
					}

					Registry.Business.HVLVDataRegistry.HasHVLVClearance = true;
					scanForOutturnMenuItem.PerformClick();
					AssertNull(menu.ErrorCaption);
					AssertNull(menu.ErrorMessage);
				}
			}
		}

		public void TestScanOverriddenNewMAWBDelegates()
		{
			CusMAWB masterbill = Factory.New<CusMAWB>();
			AirCargoMasterMenuWithScanForTest.RegisterThisTypeOverride();
			using (var airCargoMasterMenu = AirCargoMasterMenu.New(masterbill, new CusMAWBMessageManager(() => masterbill)))
			{
				AssertEquals(typeof(AirCargoMasterMenuWithScanForTest), airCargoMasterMenu.GetType());
			}
		}

		public void TestScanOverriddenNewConsolDelegates()
		{
			CusMAWB masterbill = Factory.New<CusMAWB>();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AirCargoMasterMenuWithScanForTest.RegisterThisTypeOverride();
			using (var airCargoMasterMenu = AirCargoMasterMenu.New(consol, new CusMAWBMessageManager(() => masterbill)))
			{
				AssertEquals(typeof(AirCargoMasterMenuWithScanForTest), airCargoMasterMenu.GetType());
			}
		}

		sealed class AirCargoMasterMenuWithScanForTest : AirCargoMasterMenuWithScan
		{
			AirCargoMasterMenuWithScanForTest(CusMAWB masterBill, CusMAWBMessageManager manager, AirScanForOutturnHost host) : base(masterBill, manager, host)
			{
			}

			AirCargoMasterMenuWithScanForTest(ForwardingConsol consol, CusMAWBMessageManager manager, AirScanForOutturnHost host) : base(consol, manager, host)
			{
			}

			public static void RegisterThisTypeOverride()
			{
				OverridableNewMAWBDelegate.Value = new NewMAWBDelegate(OverriddenNewMAWB);
				OverridableNewConsolDelegate.Value = new NewConsolDelegate(OverriddenNewConsol);
			}

			static AirCargoMasterMenu OverriddenNewMAWB(CusMAWB masterBill, CusMAWBMessageManager manager)
			{
				return new AirCargoMasterMenuWithScanForTest(masterBill, manager, null);
			}

			static AirCargoMasterMenu OverriddenNewConsol(ForwardingConsol consol, CusMAWBMessageManager manager)
			{
				return new AirCargoMasterMenuWithScanForTest(consol, manager, null);
			}
		}

		sealed class AirCargoMasterMenuForTest : AirCargoMasterMenuWithScan
		{
			public string ErrorCaption { get; set; }
			public string ErrorMessage { get; set; }

			public AirCargoMasterMenuForTest(CusMAWB masterBill, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
				: base(masterBill, manager, scanHost)
			{
			}

			public AirCargoMasterMenuForTest(ForwardingConsol consol, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
				: base(consol, manager, scanHost)
			{
			}

			protected override void ShowError(string caption, string message)
			{
				ErrorCaption = caption;
				ErrorMessage = message;
			}
		}
	}
}
