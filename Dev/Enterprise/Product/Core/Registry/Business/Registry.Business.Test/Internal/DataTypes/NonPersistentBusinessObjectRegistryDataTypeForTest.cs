namespace Enterprise.Registry.Business.Testing
{
	sealed class NonPersistentBusinessObjectRegistryDataTypeForTest : NonPersistentBusinessObjectRegistryDataType<DummyNonPersistentBusinessObject>
	{
		public NonPersistentBusinessObjectRegistryDataTypeForTest()
		{
		}

		public new DummyNonPersistentBusinessObject CloneValue(DummyNonPersistentBusinessObject value)
		{
			return base.CloneValue(value);
		}
	}
}
