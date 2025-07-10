using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeaderImport;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionFlattenedCollection))]
	sealed class CusTempStorageRegLineTransactionFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusTempStorageRegLineTransactionFlattenedCollection>
	{
		protected override CusTempStorageRegLineTransactionFlattenedCollection GetCollectionToTest()
		{
			return new CusTempStorageRegLineTransactionFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CusTempStorageRegLineTransactionFlattened();
		}
	}
}
