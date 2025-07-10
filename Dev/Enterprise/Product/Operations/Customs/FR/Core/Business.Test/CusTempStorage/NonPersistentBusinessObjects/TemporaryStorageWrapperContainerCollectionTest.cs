using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageWrapperContainerCollection))]
	public class TemporaryStorageWrapperContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageWrapperContainerCollection>
	{
		protected override TemporaryStorageWrapperContainerCollection GetCollectionToTest() => new TemporaryStorageWrapperContainerCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageWrapperContainer();
	}
}
