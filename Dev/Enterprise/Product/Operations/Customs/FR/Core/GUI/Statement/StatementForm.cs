using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI
{
	public partial class StatementForm : ZTemplateForm
	{
		public StatementForm(CusStatementHeader statement) : base(statement)
		{
			InitializeComponent();
			AddMenuItems();
		}

		protected CusStatementHeader Statement => (CusStatementHeader)BusinessEntity;

		public override string FormCaption
		{
			get
			{
				if (!this.IsDesignMode())
				{
					return Statement.HumanReadableName;
				}
				else
				{
					return Res.GetString("249C48DA-F4C0-4D80-90B3-F4A1EB3432E5", "Liquidation");
				}
			}
		}

		void AddMenuItems()
		{
			var sendDcgMenuItem = new ZMenuItem(ResString.GetMultilingualString("0D3A431B-0594-410B-8ECD-427A1673B20F", "Send DCG"), SendDCGMenuItem_OnClick);
			ActionsMenuItem.MenuItems.Add(sendDcgMenuItem);
		}

		void SendDCGMenuItem_OnClick(object sender, EventArgs args)
		{
			if (SaveAndContinue() && CheckMessageErrorsAndContinue())
			{
				var errorCollector = new ErrorCollector();
				var objectToSend = new StatementMessageSendingObject(Statement);
				var messageSender = new StatementMessageSender(objectToSend, errorCollector);
				var result = messageSender.Send();
				if (errorCollector.ErrorCount == 0)
				{
					Globals.Message.Show(result);
				}
				else
				{
					Globals.Message.ShowError(Res.GetString("CDAF16A8-3C61-4896-BEBA-645E0D0F6E65", "Failed to create closure message due to the following errors:") + System.Environment.NewLine + errorCollector.GetErrorsAsString());
				}
			}
		}

		bool SaveAndContinue()
		{
			var canContinue = true;

			if (Statement.HasChanges)
			{
				var messageBoxResult = Globals.Message.Show(
					Res.GetString("C4E174D3-3904-4808-8CAF-B6FA3803E272", "The liquidation has not yet been saved, Do you want to save and proceed?"),
					Res.GetString("BF9D735A-0EEA-4F7C-B299-0072A908D3FD", "Save Job"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning,
					DialogResult.Yes);
				canContinue = messageBoxResult == DialogResult.Yes && FireSaveButton() == ContinueWithSave.Yes;
			}

			return canContinue && !Statement.HasChanges;
		}

		bool CheckMessageErrorsAndContinue()
		{
			var canContinue = true;
			Statement.RunPreSaveValidation();
			if (Statement.HasMessageErrors)
			{
				var messageErrorString = GetMessageErrorString();
				var warning = ResString.GetMultilingualString("E9CF6318-89A7-457A-8EEF-F6B4F44DF31B", "It is likely that your message(s) will be rejected by Customs, as they have the following message errors:\r\n\r\n{0}\r\nDo you want to send the message(s) despite these errors?", messageErrorString);
				canContinue = Globals.Message.ShowConfirmation(warning, Res.GetString("553DF01E-C736-4880-A18B-73DD3C833495", "Send DCG"),
					ResString.GetMultilingualString("510AA0F0-AE6E-4857-A282-F75A5478A6FC", "yes"), MessageBoxIcon.Question) == DialogResult.OK;
			}
			return canContinue;
		}

		ZString GetMessageErrorString()
		{
			var notificationCollection = new MessageSendingNotificationCollection();
			foreach (var propertyInfo in Statement.PropertiesWithNotifications)
			{
				foreach (var notification in propertyInfo.Notifications)
				{
					if (notification.Type == CargoWise.EntityFramework.NotificationType.MessageError)
					{
						var caption = propertyInfo.Description;
						var message = notification.Message;
						notificationCollection.AddError(caption + ": " + message);
					}
				}
			}
			return notificationCollection.NotificationsAsString();
		}
	}
}
