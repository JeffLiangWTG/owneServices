using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class LPCOMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<LPCOMessageSendingObject>
	{
		public LPCOMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
