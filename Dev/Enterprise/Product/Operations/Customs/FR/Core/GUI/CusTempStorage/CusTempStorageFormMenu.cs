using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.MessagesWrappers.Send;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.FR.Messaging.MessageBuilders.CIN.CINImportMessageBuilder;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public class CusTempStorageFormMenu : ZMenuItem
	{
		public CusTempStorageFormMenu()
		{
			Caption = ResString.GetMultilingualString("522B9CC8-E65A-48FA-A8F5-C57654B4CDE9", "&Messages");
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

				if (Header.IsFRC)
				{
					AddFrcMessageMenus();
				}
				else
				{
					AddDdtMessageMenus();
				}
			}
		}

		void AddFrcMessageMenus()
		{
			var receiveIntoTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("73061BA1-79FD-49C8-A235-6454D233A402", "Receive into Transit Shed"));
			receiveIntoTransitShedMenu.Click += ReceiveIntoTransitShedMenu_Click;
			MenuItems.Add(receiveIntoTransitShedMenu);

			var transferToTransitShedMenu = new ZMenuItem(ResString.GetMultilingualString("1F350123-89CF-42BB-8413-AA739AC2F691", "Transfer to Onward Transit Shed"));
			transferToTransitShedMenu.Click += TransferToTransitShedMenu_Click;
			MenuItems.Add(transferToTransitShedMenu);

			var reportDeconsolidationMenu = new ZMenuItem(ResString.GetMultilingualString("3D6EC46D-BA4B-403C-80A2-501D10771D50", "Report De-consolidation"));
			reportDeconsolidationMenu.Click += ReportDeconsolidationMenu_Click;
			MenuItems.Add(reportDeconsolidationMenu);
		}

		void AddDdtMessageMenus()
		{
			var sendDdtMenu = new ZMenuItem(ResString.GetMultilingualString("181069CB-16B0-456B-87EC-96E0D28148EC", "Send DDT"));
			sendDdtMenu.Visible = SendDdtMenuVisible;
			sendDdtMenu.Click += SendDdtMenu_Click;
			MenuItems.Add(sendDdtMenu);
		}

		bool SendDdtMenuVisible => !Header.HasInStoreEvent;

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			RefreshMenuItems();
		}

		void ReportDeconsolidationMenu_Click(object sender, EventArgs e)
		{
			SendMessage(MessageBuilderType.Deconsolidation);
		}

		void TransferToTransitShedMenu_Click(object sender, EventArgs e)
		{
			SendMessage(MessageBuilderType.MovementOut);
		}

		void ReceiveIntoTransitShedMenu_Click(object sender, EventArgs e)
		{
			SendMessage(MessageBuilderType.MovementIn);
		}

		void SendDdtMenu_Click(object sender, EventArgs e)
		{
			var menu = (ZMenuItem)sender;
			var form = (CusTempStorageForm)menu.GetMainMenu().GetForm();
			SendDdt(form);
		}

		void SendDdt(CusTempStorageForm form)
		{
			if (SaveAndContinue() && WarnAboutLockingAndContinue())
			{
				var errorCollector = new ErrorCollector();
				var result = SendWrapperHelper.SendDDT(Header.Factory, Header, errorCollector);
				if (errorCollector.ErrorCount == 0)
				{
					form.ForceUpdateReadOnly();
					Globals.Message.Show(result);
				}
				else
				{
					Globals.Message.ShowError(errorCollector.GetErrorsAsString());
				}
			}
		}

		void SendMessage(MessageBuilderType messageType)
		{
			if (SaveAndContinue())
			{
				var messageSender = new FRCINImportMessageSender(Header, null, messageType);

				var (canSend, reason) = messageSender.CanSend();

				if (canSend)
				{
					var (success, resultMessage) = messageSender.Send();

					Globals.Message.Show(resultMessage);

					if (!success)
					{
						messageSender.CancelMessageOnFailure();
					}
				}
				else
				{
					Globals.Message.Show(reason);
				}
			}
		}

		protected ZForm Form => (ZForm)GetMainMenu()?.GetForm();

		bool SaveAndContinue()
		{
			var canContinue = true;

			if (Header.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("AABF5262-26B2-4B88-BCEF-D8B4243E0843", "The Job has not yet been saved. Do you want to save and proceed?"),
					Res.GetString("41534525-28EA-40AA-BB87-8F5055A632C6", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);

				canContinue = messageBoxResult == DialogResult.Yes && Form.FireSaveButton() == ContinueWithSave.Yes;
			}

			return canContinue && !Header.HasChanges;
		}

		bool WarnAboutLockingAndContinue()
		{
			var messageBoxResult = Globals.Message.Show(
				Res.GetString("5B147A8F-16B7-4312-B657-338F41FE6A89", "At sending DDT Register is created. No modification possible on this IST. Do you want to continue?"),
				Res.GetString("F3109DDF-2B5A-4B41-A0A5-C172C20CD074", "Send DDT"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning,
				DialogResult.Yes);

			return messageBoxResult == DialogResult.Yes;
		}
	}
}
