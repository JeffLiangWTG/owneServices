using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyWithZAddressCollection : BusinessObjectCollection<DummyWithZAddress>
	{
		public DummyWithZAddressCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
