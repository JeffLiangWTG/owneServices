using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public sealed class DocumentRequestForm : EU.H7.GUI.DocumentRequestForm
	{
		public DocumentRequestForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override int SendMessageToCustoms()
		{
			var messagesSent = 0;

			try
			{
				var messageSendingObjects = BusinessEntity.SelectedSendingObjects.Cast<DocumentRequestSendingAction>();
				foreach (var messageSendingObject in messageSendingObjects)
				{
					var bill = messageSendingObject.Bill;
					var documentRequest = new H7DocumentRequest(bill, bill.Header.AMA_CustomsProfile);

					messagesSent += documentRequest.RequestMissingDocuments();
					bill.Messages.Reload(false);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(CouldNotCreateMessageError);
			}

			return messagesSent;
		}

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new DocumentRequestGridColumnLayout();

		static ZString CouldNotCreateMessageError => Res.GetString("94f174c9-f3fc-4200-b73e-2dca609d0a1d", "Could not create document request message for bill(s).");
	}
}
