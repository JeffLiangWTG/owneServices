using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageWrapperFurtherDetailCollection))]
	public class TemporaryStorageFutherDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageWrapperFurtherDetailCollection>
	{
		protected override TemporaryStorageWrapperFurtherDetailCollection GetCollectionToTest() => new TemporaryStorageWrapperFurtherDetailCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageWrapperFurtherDetail();
	}
}
