using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class CdcHistoryCollection : NonPersistentBusinessObjectCollection<CdcHistory>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CdcHistory("", "");
		}
	}
}
