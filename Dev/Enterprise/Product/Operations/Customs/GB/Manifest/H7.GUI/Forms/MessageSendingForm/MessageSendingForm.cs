using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture.GUI;
using MessageSender = Enterprise.Customs.GB.H7.Messaging.MessageSender;

namespace Enterprise.Customs.GB.H7.GUI
{
	public partial class MessageSendingForm : EU.H7.GUI.MessageSendingForm
	{
		public MessageSendingForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new MessageSendingGridColumnLayout();

		protected override int SendMessageToCustoms(Action<int, int> updateProgressCallback)
		{
			var messageSender = new MessageSender(BusinessEntity.SelectedSendingObjects.Cast<MessageSendingObject>(), updateProgressCallback);
			return messageSender.Send();
		}
	}
}
