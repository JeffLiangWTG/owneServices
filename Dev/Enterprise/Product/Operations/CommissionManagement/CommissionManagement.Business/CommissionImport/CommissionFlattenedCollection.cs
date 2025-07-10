using CargoWise.EntityFramework;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionFlattenedCollection : NonPersistentBusinessObjectCollection<CommissionFlattened>
	{
		public CommissionFlattenedCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionFlattened();
		}
	}
}
