using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public class LPCOMessagingMenu : ZMenuItem
	{
		public LPCOMessagingMenu(CusLPCOHeader lpcoHeader)
		{
			this.lpcoHeader = Argument.NotNull(lpcoHeader, nameof(lpcoHeader));
			Caption = ResString.GetMultilingualString("5E3B00BA-28DE-4F6A-B0A4-A44264F9E435", "Send to Customs");
			InitialiseMenu();
		}

		readonly CusLPCOHeader lpcoHeader;

		MenuItem sendLPCO;

		void InitialiseMenu()
		{
			sendLPCO = new ZMenuItem(ResString.GetMultilingualString("D2EA0462-3BD8-4196-A8EF-88649BFE1E1A", "Send LPCO"), SendToCustoms_Click);
			MenuItems.Add(sendLPCO);
		}

		ZForm Form => (ZForm)(GetMainMenu()?.GetForm());

		void SendToCustoms_Click(object sender, EventArgs e)
		{
			if (CustomsPlugIn.FormPreSaved(lpcoHeader, Form))
			{
				var messageSendingObjectParent = new LPCOMessageSendingObjectParent(lpcoHeader);
				using (var form = new LPCOMessageSendingForm(messageSendingObjectParent))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var countOfMessages = messageSendingObjectParent.SendMessagesAndSave();
						Globals.Message.Show(Res.GetString("56acc64c-7aff-42eb-9f8d-c4ba032847e7", "{0} message(s) have been sent.", countOfMessages));
					}
				}
			}
		}
	}
}
