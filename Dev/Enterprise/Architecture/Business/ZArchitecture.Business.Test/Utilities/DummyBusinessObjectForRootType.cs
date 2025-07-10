using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[AdditionalRootType(nameof(DummyBusinessObjectForRootType))]
	public sealed class DummyBusinessObjectForRootType : DummyBusinessObject
	{
		public DummyBusinessObjectForRootType(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}

		protected override DummyChildBusinessObjectCollection NewCollection() => new DummyChildBusinessObjectForRootTypeCollection(Factory);
	}
}
