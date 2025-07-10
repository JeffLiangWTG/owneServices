using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[AdditionalRootType(nameof(DummyChildBusinessObjectForAdditionalRootType))]
	public sealed class DummyChildBusinessObjectForAdditionalRootType : DummyChildBusinessObject, IAllowAdditionalRootType
	{
		public DummyChildBusinessObjectForAdditionalRootType(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
		}
	}
}
