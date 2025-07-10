using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineCollection))]
sealed public class CusTempStorageRegLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineCollection>
{
	protected override CusTempStorageRegLineCollection GetCollectionToTest() => new CusTempStorageRegLineCollection(Factory.New<CusTempStorageRegHeader>());
}
