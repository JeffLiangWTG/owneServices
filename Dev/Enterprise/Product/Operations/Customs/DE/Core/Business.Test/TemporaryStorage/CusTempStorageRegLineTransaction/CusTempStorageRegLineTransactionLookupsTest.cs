using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	public class CusTempStorageRegLineTransactionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPreviousReferenceTypeList()
		{
			Assert(ReferenceEquals(Factory.GetCachedValue<TransactionTypes>(), lineTransaction.Lookups.TransactionTypeList));
		}

		public void TestReferenceTypeList()
		{
			Assert(ReferenceEquals(Factory.GetCachedValue<TransactionReferenceTypes>(), lineTransaction.Lookups.ReferenceTypeList));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<CusTempStorageRegHeader>();
			var line = header.CusTempStorageRegLines.AddNew();
			lineTransaction = line.CusTempStorageRegLineTransactions.AddNew();
		}
		CusTempStorageRegLineTransaction lineTransaction;
	}
}
