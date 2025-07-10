using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyNonPersistentBusinessObjectCollectionFindBoxListProviderTest : NonPersistentBusinessObjectFindBoxListProvider
	{
		public DummyNonPersistentBusinessObjectCollectionFindBoxListProviderTest(DummyNonPersistentBusinessObjectCollection collection)
			: base(collection)
		{
		}

		public override string CodeFromPrimaryKey(ZGuid pK) => string.Empty;

		public override string DescriptionFromPrimaryKey(ZGuid pK) => string.Empty;
	}
}
