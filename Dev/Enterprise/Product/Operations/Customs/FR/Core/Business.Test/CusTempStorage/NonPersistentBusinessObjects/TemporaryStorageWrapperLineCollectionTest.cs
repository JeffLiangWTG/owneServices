using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageWrapperLineCollection))]
	public class TemporaryStorageWrapperLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemporaryStorageWrapperLineCollection>
	{
		protected override TemporaryStorageWrapperLineCollection GetCollectionToTest() => new TemporaryStorageWrapperLineCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemporaryStorageWrapperLine();
	}
}
