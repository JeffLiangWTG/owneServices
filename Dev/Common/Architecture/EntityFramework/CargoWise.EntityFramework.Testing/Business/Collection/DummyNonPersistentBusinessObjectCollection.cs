namespace CargoWise.EntityFramework.Testing
{
	public class DummyNonPersistentBusinessObjectCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject>
	{
		public DummyNonPersistentBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyNonPersistentBusinessObject();
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get { return new DummyNonPersistentBusinessObjectCollectionFindBoxListProviderTest(this); }
		}
	}
}
