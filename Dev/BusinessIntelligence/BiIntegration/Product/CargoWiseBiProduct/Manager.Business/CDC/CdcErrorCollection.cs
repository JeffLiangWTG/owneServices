using CargoWise.EntityFramework;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class CdcErrorCollection : NonPersistentBusinessObjectCollection<CdcError>
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CdcError();
		}
	}
}
