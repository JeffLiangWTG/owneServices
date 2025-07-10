using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public class ExitControlSendToCustomsMenuCreator
	{
		public ExitControlSendToCustomsMenuCreator(CusExitHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
		}
		protected readonly CusExitHeader header;

		public const string SendToCustomsMenuItemName = "SendToCustomsMenuItem";

		public ZMenuItem Create() => createdMenuItem = new ZMenuItem(Res.GetData("4AD748EB-2BB3-4315-9845-4EC638A5BEFD", "Send to Customs"), SendToCustomsClick) { Name = SendToCustomsMenuItemName };
		protected ZMenuItem createdMenuItem;

		bool CheckBeforeSending()
		{
			var form = (ZForm)(createdMenuItem.GetMainMenu()?.GetForm());
			var topLevelBizObj = (form?.BusinessEntity as BusinessObject) ?? header.Parent ?? header;
			return CustomsPlugIn.FormPreSaved(topLevelBizObj, form) && HasValidSystemSettings();
		}

		protected virtual bool HasValidSystemSettings() => true;

		void SendToCustomsClick(object sender, EventArgs e)
		{
			if (CheckBeforeSending())
			{
				SendToCustoms();
			}
		}

		void SendToCustoms()
		{
			var messageSendingParent = GetMessageSendingParent();
			using (var form = new ExitControlMessageSendingForm(messageSendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
				{
					OnMessageSendingFormOk(messageSendingParent);
				}
			}
		}

		protected virtual void OnMessageSendingFormOk(ExitControlMessageSendingObjectParent sendingParent)
		{
		}

		protected virtual ExitControlMessageSendingObjectParent GetMessageSendingParent() => new ExitControlMessageSendingObjectParent(header);
	}
}
