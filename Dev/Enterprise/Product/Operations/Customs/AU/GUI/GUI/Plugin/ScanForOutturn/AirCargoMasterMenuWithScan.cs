using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.AirCargo.GUI
{
	public class AirCargoMasterMenuWithScan : AirCargoMasterMenu
	{
		readonly AirScanForOutturnHost scanHost;

		protected internal AirCargoMasterMenuWithScan(CusMAWB masterBill, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
			: base(masterBill, manager)
		{
			this.scanHost = scanHost;
		}

		protected internal AirCargoMasterMenuWithScan(ForwardingConsol consol, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
			: base(consol, manager)
		{
			this.scanHost = scanHost;
		}

		public static AirCargoMasterMenu New(CusMAWB masterBill, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
		{
			AirCargoMasterMenu menu;
			var overridden = OverridableNewMAWBDelegate.Value;
			if (overridden == null)
			{
				menu = new AirCargoMasterMenuWithScan(masterBill, manager, scanHost);
			}
			else
			{
				menu = overridden(masterBill, manager);
			}
			return menu;
		}

		public static AirCargoMasterMenu New(ForwardingConsol consol, CusMAWBMessageManager manager, AirScanForOutturnHost scanHost)
		{
			AirCargoMasterMenu menu;
			var overridden = OverridableNewConsolDelegate.Value;
			if (overridden == null)
			{
				menu = new AirCargoMasterMenuWithScan(consol, manager, scanHost);
			}
			else
			{
				menu = overridden(consol, manager);
			}
			return menu;
		}

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			if (scanHost == null)
			{
				MenuItems.Add(Declaration.GUI.Res.GetString("c1c6b098-4fb1-4e54-a6cd-3ae66d438be0", "Scan for Outturn"), ScanForOutturnClick);
			}
			else
			{
				MenuItems.Add(Declaration.GUI.Res.GetString("c1c6b098-4fb1-4e54-a6cd-3ae66d438be0", "Scan for Outturn"), scanHost.ScanForOutturnClick);
			}
		}

		public void ScanForOutturnClick(object sender, EventArgs args)
		{
			var accessManager = new ScanAccessManager();
			if (!accessManager.HasAccess)
			{
				ShowError(
					Declaration.GUI.Res.GetString("8f923e09-18bb-4308-b351-ca7229ac48d9", "License error"),
					Declaration.GUI.Res.GetString("8f923e09-18bb-4308-b351-ca7229ac48d8", "To start scanning for outturn you need to obtain {0} license.", accessManager.LicenceName)
					);

				return;
			}

			ShowError(
				Declaration.GUI.Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c81ee48", "Validation error"),
				Declaration.GUI.Res.GetString("22ec7290-87bd-48c9-89fe-ef7d3c81ee49", "You cannot start outturn scanning until you have created and sent Air Cargo messages, and have the associated Underbond record. To send Air Cargo messages, click on the AirCargo tab and create AirCargo first, then save Consol. You will then need to close and reopen the consol, before you can proceed with outturn scanning.")
			);
		}

		protected virtual void ShowError(string caption, string message)
		{
			Globals.Message.Show(
				   message,
				   caption,
				   MessageBoxButtons.OK,
				   MessageBoxIcon.Error);
		}
	}
}
