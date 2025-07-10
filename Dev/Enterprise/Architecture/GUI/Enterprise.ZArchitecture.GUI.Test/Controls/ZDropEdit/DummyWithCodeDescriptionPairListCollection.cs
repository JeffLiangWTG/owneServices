using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class DummyWithCodeDescriptionPairListCollection : BusinessObjectCollection<DummyWithCodeDescriptionPairList>
	{
		public DummyWithCodeDescriptionPairListCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
