using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class ProductPendingUpdateDataObjectCollection : NonPersistentBusinessObjectCollection<ProductPendingUpdateDataObject>
	{
		public ProductPendingUpdateDataObjectCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new ProductPendingUpdateDataObject(Factory);
	}
}
