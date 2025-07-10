using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>))]
sealed class CusTempStorageRegLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>>
{
	protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<CusTempStorageRegLineTransaction>();

	protected override CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction> GetCollectionToTest() => new CusTempStorageRegLineTransactionCollection<CusTempStorageRegLineTransaction>(Factory.New<CusTempStorageRegLine>());
}
