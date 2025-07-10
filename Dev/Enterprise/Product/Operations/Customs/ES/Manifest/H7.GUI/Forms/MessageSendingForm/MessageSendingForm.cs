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
	public partial class MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		public MessageSendingForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override int SendMessageToCustoms(Action<int, int> updateProgressCallback)
		{
			var messagesSent = 0;
			var messageSender = new H7MessageSender(BusinessEntity.SelectedSendingObjects.Cast<H7MessageSendingObject>(), updateProgressCallback);
			var messageBuildersToSend = messageSender.GetMessageBuildersData();

			try
			{
				var messagesInfo = messageSender.Send(messageBuildersToSend);
				messagesSent = messagesInfo.MessagesSent;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(CouldNotCreateMessageError);
			}

			return messagesSent;
		}

		static ZString CouldNotCreateMessageError => Res.GetString("0f5c2377-6724-4223-bd2b-ab4e984769f1", "Could not create declaration message for bill(s).");

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new MessageSendingGridColumnLayout();
	}
}
