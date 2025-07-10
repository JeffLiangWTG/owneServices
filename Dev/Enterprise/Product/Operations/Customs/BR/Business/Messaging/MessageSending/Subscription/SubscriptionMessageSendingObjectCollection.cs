using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class SubscriptionMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<SubscriptionMessageSendingObject>
	{
		public SubscriptionMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;
	}
}
