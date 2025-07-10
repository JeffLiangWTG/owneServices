using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ReferenceableCollection : BusinessObjectCollection<DummyWithReferenceableCollection>
	{
		public ReferenceableCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
