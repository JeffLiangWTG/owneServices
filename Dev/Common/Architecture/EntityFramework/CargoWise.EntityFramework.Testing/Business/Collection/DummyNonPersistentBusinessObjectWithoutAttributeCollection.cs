namespace CargoWise.EntityFramework.Testing
{
	sealed class DummyNonPersistentBusinessObjectWithoutAttributeCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObjectWithoutAttribute>
	{
		public DummyNonPersistentBusinessObjectWithoutAttributeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DummyNonPersistentBusinessObjectWithoutAttribute();
		}
	}
}
