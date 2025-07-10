using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Customs.AU.Declaration.GUI.Res;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoConsolMenuWithScan : SeaCargoConsolMenu
	{
		public SeaCargoConsolMenuWithScan(SeaCargoConsolPlugIn plugin, CusSCAOceanBillMessageManager manager, SeaScanForOutturnHost scanHost)
			: base(plugin, manager)
		{
			this.scanHost = scanHost;
		}
		readonly SeaScanForOutturnHost scanHost;

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			if (scanHost == null)
			{
				MenuItems.Add(Res.GetString("CE62ECE5-B4D6-4BDB-A407-3710E001BFA7", "Scan for Outturn"), ScanForOutturnClick);
			}
			else
			{
				MenuItems.Add(Res.GetString("CE62ECE5-B4D6-4BDB-A407-3710E001BFA7", "Scan for Outturn"), scanHost.ScanForOutturnClick);
			}
		}

		public void ScanForOutturnClick(object sender, EventArgs args)
		{
			var accessManager = new ScanAccessManager();
			if (!accessManager.HasAccess)
			{
				ShowError(
					Res.GetString("C58F35F4-E627-4D88-85CE-41DAC0A2CDCB", "License error"),
					Res.GetString("F7333AC3-F9F6-4CDB-9E94-C35D7D9E840F", "To start scanning for outturn you need to obtain {0} license.", accessManager.LicenceName)
					);

				return;
			}

			ShowError(
				Res.GetString("CE1D5CF1-C48C-47CF-A15E-1DB9FA2F8683", "Validation error"),
				Res.GetString("FE145892-4664-4BC4-8494-F9E89A30505E", "You cannot start outturn scanning until you have created and sent Sea Cargo messages, and have the associated Underbond record.")
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
