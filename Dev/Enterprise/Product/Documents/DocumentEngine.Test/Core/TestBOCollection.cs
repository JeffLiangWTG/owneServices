using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Testing
{
	class TestBOCollection : NonPersistentBusinessObjectCollection<TestBO>
	{
		public TestBOCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TestBO();
		}
	}
}
