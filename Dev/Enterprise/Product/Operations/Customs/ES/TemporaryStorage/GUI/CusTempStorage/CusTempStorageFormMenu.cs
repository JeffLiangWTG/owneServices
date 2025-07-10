using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	public class CusTempStorageFormMenu : ZMenuItem
	{
		public CusTempStorageFormMenu()
		{
			Caption = ResString.GetMultilingualString("A96FBA83-9693-445B-813C-6E46D7E73081", "&Messages");
		}

		public CusTempStorageJobHeader Header
		{
			get => header;
			set
			{
				header = value;
				RefreshMenuItems();
			}
		}
		CusTempStorageJobHeader header;

		void RefreshMenuItems()
		{
			if (Header != null)
			{
				MenuItems.Clear();

				AddMessageMenus(); //This is only temporary, will be changed in future WIs
			}
		}

		void AddMessageMenus()
		{
			var receiveIntoTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("8C1B7F6D-E957-41A3-A942-F634CDCE82E4", "Receive into Transit Shed"));
			receiveIntoTransitShedMenu.Click += ReceiveIntoTransitShedMenu_Click;
			MenuItems.Add(receiveIntoTransitShedMenu);

			var transferToTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("71167565-2126-437E-9322-9781520B3D5C", "Transfer to Onward Transit Shed"));
			transferToTransitShedMenu.Click += TransferToTransitShedMenu_Click;
			MenuItems.Add(transferToTransitShedMenu);

			var reportDeconsolidationMenu = new ZMenuItem(ResString.GetMultilingualString("3F3E52C2-D2DD-4A10-A4E4-95BFE964CB70", "Report De-consolidation"));
			reportDeconsolidationMenu.Click += ReportDeconsolidationMenu_Click;
			MenuItems.Add(reportDeconsolidationMenu);

			var sendDdtMenu = new ZMenuItem(ResString.GetMultilingualString("37694EC7-466A-411E-8CCF-8ECB9BEFFB45", "Send DDT"));
			sendDdtMenu.Click += SendDdtMenu_Click;
			MenuItems.Add(sendDdtMenu);
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			RefreshMenuItems();
		}

		void ReportDeconsolidationMenu_Click(object sender, EventArgs e)
		{
			SendMessage();
		}

		void TransferToTransitShedMenu_Click(object sender, EventArgs e)
		{
			SendMessage();
		}

		void ReceiveIntoTransitShedMenu_Click(object sender, EventArgs e)
		{
			SendMessage();
		}

		void SendDdtMenu_Click(object sender, EventArgs e)
		{
			SendMessage();
		}

		void SendMessage()
		{
			if (SaveAndContinue())
			{
				Globals.Message.Show(Res.GetString("11b4c7f3-21ff-4573-9ca9-1fccac708df7", "CW1 ES doesn't yet support building messages in Temporary Storage"));
			}
		}

		protected ZForm Form => (ZForm)GetMainMenu()?.GetForm();

		bool SaveAndContinue()
		{
			var canContinue = true;

			if (Header.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("62B58F00-60EE-45E6-91F6-239A6829DAAB", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("FDBAF64F-75B1-41D0-A0E2-73F6E83C43FD", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);

				canContinue = messageBoxResult == DialogResult.Yes && Form.FireSaveButton() == ContinueWithSave.Yes;
			}

			return canContinue && !Header.HasChanges;
		}
	}
}
