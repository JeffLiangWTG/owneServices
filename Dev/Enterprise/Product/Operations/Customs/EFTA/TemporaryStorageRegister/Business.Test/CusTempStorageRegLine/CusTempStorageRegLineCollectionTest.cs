using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineCollection<CusTempStorageRegLine>))]
sealed class CusTempStorageRegLineCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineCollection<CusTempStorageRegLine>>
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageRegLine>();

	protected override CusTempStorageRegLineCollection<CusTempStorageRegLine> GetCollectionToTest() => new (Factory.New<CusTempStorageRegHeader>());
}
