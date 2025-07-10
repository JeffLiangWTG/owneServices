using CargoWise.EntityFramework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class TestWrapper : DocBaseWrapper
	{
		public TestWrapper(BusinessObjectFactory factory) : base(null, factory) { }
	}
}
