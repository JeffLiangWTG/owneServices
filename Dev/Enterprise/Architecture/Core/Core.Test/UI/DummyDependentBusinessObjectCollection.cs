using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class DummyDependentBusinessObjectCollection : BusinessObjectCollection<DummyDependentBusinessObject>
	{
		public DummyDependentBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
