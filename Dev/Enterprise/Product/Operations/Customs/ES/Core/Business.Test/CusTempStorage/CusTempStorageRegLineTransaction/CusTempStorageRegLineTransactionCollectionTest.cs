using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionCollection))]
	class CusTempStorageRegLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineTransactionCollection>
	{
		public void TestDefaultValues()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			AssertEquals("SRT_TransactionType is Adjustment when create a new line", TransactionTypes.Codes.Adjustment, transaction.SRT_TransactionType);
		}

		protected override CusTempStorageRegLineTransactionCollection GetCollectionToTest() => new CusTempStorageRegLineTransactionCollection(Factory.New<CusTempStorageRegLine>());
	}
}
