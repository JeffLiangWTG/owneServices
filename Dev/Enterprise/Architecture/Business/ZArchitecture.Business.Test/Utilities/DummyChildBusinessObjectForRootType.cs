using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class DummyChildBusinessObjectForRootType : DummyChildBusinessObject, IAllowAdditionalRootType
	{
		public DummyChildBusinessObjectForRootType(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}
	}
}
