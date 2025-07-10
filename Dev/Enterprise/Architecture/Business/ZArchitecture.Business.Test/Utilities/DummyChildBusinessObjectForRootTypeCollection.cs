using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyChildBusinessObjectForRootTypeCollection : DummyChildBusinessObjectCollection
	{
		public DummyChildBusinessObjectForRootTypeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DummyChildBusinessObjectForRootType this[int i] => (DummyChildBusinessObjectForRootType)base[i];

		public new DummyChildBusinessObjectForRootType AddNew() => (DummyChildBusinessObjectForRootType)base.AddNew();
	}
}
