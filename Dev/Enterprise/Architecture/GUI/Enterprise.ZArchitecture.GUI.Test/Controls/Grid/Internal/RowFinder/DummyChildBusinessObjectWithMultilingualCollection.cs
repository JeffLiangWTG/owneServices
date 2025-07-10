using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	sealed class DummyChildBusinessObjectWithMultilingualCollection : BusinessObjectCollection<DummyChildBusinessObjectWithMultilingual>
	{
		public DummyChildBusinessObjectWithMultilingualCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyChildBusinessObjectWithMultilingualCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
