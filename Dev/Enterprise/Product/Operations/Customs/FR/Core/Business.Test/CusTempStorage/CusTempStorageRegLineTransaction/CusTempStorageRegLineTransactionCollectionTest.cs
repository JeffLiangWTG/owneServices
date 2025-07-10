using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using TransactionTypes = Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineTransactionTypeList;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransactionCollection))]
	class CusTempStorageRegLineTransactionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusTempStorageRegLineTransactionCollection>
	{
		public void TestDefaultValues()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Opening Balance", TransactionTypes.Codes.OpeningBalance, transaction1.SRT_TransactionType);

				var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
				AssertEquals("Adjustment", TransactionTypes.Codes.Adjustment, transaction2.SRT_TransactionType);
			});
		}

		protected override CusTempStorageRegLineTransactionCollection GetCollectionToTest() => new CusTempStorageRegLineTransactionCollection(Factory.New<CusTempStorageRegLine>());
	}
}
