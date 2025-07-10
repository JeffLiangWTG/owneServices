using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business.Testing
{
	public class DummyRelatedBusinessObjectCollection : NonPersistentBusinessObjectCollection<DummyRelatedBusinessObject>, IObsoleteValidation
	{
		public DummyRelatedBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyRelatedBusinessObject();
		}
	}
}
