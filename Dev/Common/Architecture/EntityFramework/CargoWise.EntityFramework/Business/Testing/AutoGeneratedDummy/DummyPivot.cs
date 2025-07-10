#if DEBUG
using System.Data;

namespace CargoWise.EntityFramework.Testing
{
	[AllowAllObjectsToBeLoaded]
	public class DummyPivot : AutoDummyPivot
	{
		public DummyPivot(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ExposedIsNewAndAllPropertiesDuplicatesOfExistingObject
		{
			get { return IsNewAndAllPropertiesDuplicatesOfExistingObject; }
		}
	}
}
#endif
