using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using DocumentsSendingAction = Enterprise.Customs.IE.ExitControl.Business.DocumentsSendingAction;
using DocumentsSendingActionParent = Enterprise.Customs.IE.ExitControl.Business.DocumentsSendingActionParent;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ExitControlUploadSupportingDocumentsMenuCreator
	{
		public const string UploadSupportingDocumentsMenuItemName = "UploadSingleSupportingDocumentsMenuItem";

		public ExitControlUploadSupportingDocumentsMenuCreator(IReportsGridUserControlProvider provider)
		{
			this.provider = provider;
		}
		readonly IReportsGridUserControlProvider provider;

		public ZMenuItem Create() => new ZMenuItem(ResString.GetMultilingualString("0D486A91-CB56-4F0E-BA09-D3D2F5CAFA8D", "Upload Supporting Documents"), UploadSupportingDocumentsClick) { Name = UploadSupportingDocumentsMenuItemName };

		void UploadSupportingDocumentsClick(object sender, EventArgs e)
		{
			var reportsGrid = provider?.UserControl is IReportsGridUserControl userControl ? userControl.ReportsGrid : null;
			var selectedRowCount = reportsGrid?.SelectedRowCount ?? 0;
			if (selectedRowCount == 0)
			{
				Globals.Message.ShowError(Res.GetString("689B5B74-1BE5-4BAE-873B-3AEC31052813", "Please select an Exit Report first."));
			}
			else if (selectedRowCount == 1)
			{
				if (reportsGrid.GetCurrent() is CusExitReport report)
				{
					var consignment = report.Consignment;
					if (consignment == null)
					{
						Globals.Message.ShowError(Res.GetString("DEF64B24-5504-4856-B3FE-D4E605FE5495", "'Entry/Consignment' must have a valid value before uploading supporting documents."));
					}
					else
					{
						UploadSupportingDocuments(report, AESOutgoingMessageTypeList.Codes.DocumentUpload);
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("1EB28B57-5A06-4723-8443-E777FEFD280E", "Multi selection is not allowed."));
			}
		}

		void UploadSupportingDocuments(CusExitReport cusExitReport, string messageType)
		{
			var sendingParent = new DocumentsSendingActionParent(cusExitReport, messageType);
			using (var messageSendingForm = new DocumentsSendingForm(sendingParent))
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(messageSendingForm) == DialogResult.OK)
				{
					var messageSent = 0;
					sendingParent.SendingObjectsCollection.Cast<DocumentsSendingAction>().Where(action => action.ShouldSend).ForEach(
						action =>
						{
							var sender = action.CreateSender();
							if (sender.Send() != null)
							{
								messageSent++;
							}
						});
					if (messageSent > 0)
					{
						TrySaveAndShowMessage(cusExitReport.Factory, messageSent);
					}
				}
			}
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				Globals.Message.ShowInformation(GetMessageSentText(messagesCreated));
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		string GetMessageSentText(int count)
		{
			return Res.GetString("5F2D7BEA-ADB0-462A-B6A5-C68CD4D79B94", "{0} message(s) queued for sending.", count);
		}
	}
}
