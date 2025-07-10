using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class TestWrapperCollection : DocBaseWrapperCollection<TestWrapper>
	{
		public TestWrapperCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
