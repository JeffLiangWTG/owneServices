using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineSelCollection))]
sealed class CusTempStorageRegLineSelCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineSelCollection>
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageRegLine>();

	protected override CusTempStorageRegLineSelCollection GetCollectionToTest() => new(Factory, new ZDBOnlyQuery(typeof(CusTempStorageRegLine)));
}
