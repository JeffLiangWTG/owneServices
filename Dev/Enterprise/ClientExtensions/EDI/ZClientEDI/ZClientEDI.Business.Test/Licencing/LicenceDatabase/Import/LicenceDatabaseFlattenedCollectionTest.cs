using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceDatabaseFlattenedCollection))]
	public class LicenceDatabaseFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LicenceDatabaseFlattenedCollection>
	{
		protected override LicenceDatabaseFlattenedCollection GetCollectionToTest()
		{
			return new LicenceDatabaseFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LicenceDatabaseFlattened();
		}
	}
}
