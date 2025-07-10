using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Testing
{
	class DummyWithActiveCollection : DummyBusinessObject
	{
		public DummyWithActiveCollection(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public DummyActiveCollection ActiveCollection
		{
			get
			{
				if (activeCollection == null)
				{
					activeCollection = new DummyActiveCollection(Factory, new ZQuery(DummyBizoSchema.Z0_Guid, PK));
					RegisterEditableChildObject(activeCollection);
				}
				return activeCollection;
			}
		}
		DummyActiveCollection activeCollection;

		public DummyWithActiveCollection AddChildDummy()
		{
			var child = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			child.Z0_Guid = PK;
			return child;
		}
	}
}
