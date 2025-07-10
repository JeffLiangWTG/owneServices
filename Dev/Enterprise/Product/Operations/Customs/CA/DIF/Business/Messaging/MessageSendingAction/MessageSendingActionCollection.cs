using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class MessageSendingActionCollection : MessageSendingActionCollectionBase<MessageSendingAction, DIFDocument>
	{
		public MessageSendingActionCollection(DIFHostWrapper hostWrapper)
			: base(hostWrapper)
		{
		}

		public MessageSendingActionCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override IMessageSendingActionBase CreateElement(IDISDocumentBase disDocument) => new MessageSendingAction((DIFDocument)disDocument);

		protected override DISMessageManagerBase GetMessageManager(IDISDocumentBase disDocument) => new MessageManager((DIFDocument)disDocument);
	}
}
