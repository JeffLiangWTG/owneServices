using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public sealed class DummyDependentWithCodeBusinessObjectCollection : DependentBusinessObjectCollection<DummyDependentWithCodeBusinessObject, DummyWithLookups>
	{
		public bool? OnAddedCalledByDataRefresh;

		public DummyDependentWithCodeBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyDependentWithCodeBusinessObjectCollection(DummyWithLookups parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public DummyDependentWithCodeBusinessObjectCollection(DummyWithLookups parent, ZQuery filter)
			: base(parent, filter)
		{
		}
	}
}
