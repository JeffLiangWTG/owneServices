using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class SuperDummyCollection : BusinessObjectCollection<SuperDummy>
	{
		public SuperDummyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SuperDummyCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}
	}
}
