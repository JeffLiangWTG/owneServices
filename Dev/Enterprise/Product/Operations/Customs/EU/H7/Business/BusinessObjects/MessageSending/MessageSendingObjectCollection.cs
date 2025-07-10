using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business
{
	public class MessageSendingObjectCollection<TMessageSendingObject> : NonPersistentBusinessObjectCollection<TMessageSendingObject>
		where TMessageSendingObject : MessageSendingObject
	{
		public MessageSendingObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
