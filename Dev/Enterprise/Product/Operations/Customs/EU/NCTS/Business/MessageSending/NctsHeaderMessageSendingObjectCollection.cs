using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject>
	{
		public NctsHeaderMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => null;

		protected override bool AllowNewCore => false;
	}
}
