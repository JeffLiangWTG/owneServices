using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.H7.Business
{
	public class RF415MessageSendingObjectCollection : NonPersistentBusinessObjectCollection<RF415MessageSendingObject>
	{
		public RF415MessageSendingObjectCollection(BusinessObjectFactory factory)
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
