using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class DummyDependentWithCodeBusinessObjectCollection : DependentBusinessObjectCollection<DummyDependentWithCodeBusinessObject, DummyWithLookups>
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
	}
}
