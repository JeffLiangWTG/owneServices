using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class SubscriberCollection : NonPersistentBusinessObjectCollection<SubscriberInfo>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SubscriberInfo("");
		}
	}
}
