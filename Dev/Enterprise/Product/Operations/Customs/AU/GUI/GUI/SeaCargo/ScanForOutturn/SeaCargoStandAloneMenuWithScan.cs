using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public class SeaCargoStandAloneMenuWithScan : SeaCargoStandAloneMenu
	{
		public SeaCargoStandAloneMenuWithScan(CusSCAOceanBillMessageManager messageManager, SeaScanForOutturnHost scanHost)
			: base(messageManager)
		{
			this.scanHost = scanHost;
		}
		readonly SeaScanForOutturnHost scanHost;

		protected override void InitializeMenu()
		{
			base.InitializeMenu();

			if (scanHost == null)
			{
				MenuItems.Add(Declaration.GUI.Res.GetString("4EBAABC8-F2AC-4879-9D34-784BBD3C83FF", "Scan for Outturn"), ScanForOutturnClick);
			}
			else
			{
				MenuItems.Add(Declaration.GUI.Res.GetString("DDB1A1E6-91DA-412E-8C15-1B80D878F99D", "Scan for Outturn"), scanHost.ScanForOutturnClick);
			}
		}

		public void ScanForOutturnClick(object sender, EventArgs args)
		{
			var accessManager = new ScanAccessManager();
			if (!accessManager.HasAccess)
			{
				ShowError(
					Declaration.GUI.Res.GetString("F5647953-DE21-45D2-A08C-0B188297629F", "License error"),
					Declaration.GUI.Res.GetString("41A08DA9-A3B6-4ABF-8AF4-7341B5420668", "To start scanning for outturn you need to obtain {0} license.", accessManager.LicenceName)
					);

				return;
			}

			ShowError(
				Declaration.GUI.Res.GetString("C2571F03-D663-40D8-9BC5-A4A080B3ABC6", "Validation error"),
				Declaration.GUI.Res.GetString("69A34BAE-F72F-48D9-9402-F7B9E1E79599", "You cannot start outturn scanning until you have created and sent Sea Cargo messages, and have the associated Underbond record.")
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
