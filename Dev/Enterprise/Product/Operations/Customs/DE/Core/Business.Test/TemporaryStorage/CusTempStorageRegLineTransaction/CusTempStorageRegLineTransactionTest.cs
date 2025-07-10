using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(CusTempStorageRegLineTransaction))]
	public class CusTempStorageRegLineTransactionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUpdateLinePackageRemainingWhenSRT_PackageQtyChanged()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction1.SRT_PackageQty = 5;
			AssertEquals(line.SRL_PackagesRemaining, 5);

			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			transaction2.SRT_PackageQty = -2;
			AssertEquals(line.SRL_PackagesRemaining, 3);

			transaction2.SRT_PackageQty = 4;
			AssertEquals(line.SRL_PackagesRemaining, 9);
		}

		public void TestReadOnly()
		{
			var transaction = (CusTempStorageRegLineTransaction)GetNewBusinessObject(Factory);
			Assert(!transaction.ReadOnly);
			Assert(transaction.SRT_TransactionTypeInfo.ReadOnly);
			Assert(transaction.SRT_SystemCreateTimeUtcInfo.ReadOnly);
			Assert(transaction.SRT_SystemCreateUserInfo.ReadOnly);

			Factory.Save();

			Assert(transaction.ReadOnly);
			Assert(transaction.SRT_TransactionTypeInfo.ReadOnly);
			Assert(transaction.SRT_SystemCreateTimeUtcInfo.ReadOnly);
			Assert(transaction.SRT_SystemCreateUserInfo.ReadOnly);
		}

		public void TestCanDelete()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction1 = line.CusTempStorageRegLineTransactions.AddNew();
			AssertEquals(TransactionTypes.Codes.OpeningBalance, transaction1.SRT_TransactionType);
			Assert(!transaction1.CanDelete);

			var transaction2 = line.CusTempStorageRegLineTransactions.AddNew();
			AssertEquals(TransactionTypes.Codes.Adjustment, transaction2.SRT_TransactionType);
			Assert(transaction2.CanDelete);
		}

		public void TestTransactionTypeDescription()
		{
			var line = Factory.New<CusTempStorageRegLine>();
			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_TransactionType = string.Empty;
			AssertEquals(string.Empty, transaction.TransactionTypeDescription);

			transaction.SRT_TransactionType = TransactionTypes.Codes.Adjustment;
			AssertEquals(TransactionTypes.Descriptions.Adjustment, transaction.TransactionTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return GetNewBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject(factory);
		}

		protected BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusTempStorageRegHeader>();
			header.SRH_ArrivalDate = ZDate.Today;
			header.SRH_PresentationDate = ZDate.Today;
			header.SRH_AppCode = "123";
			header.SRH_Status = "OK";
			header.SRH_Reference = "TEST";

			var line = header.CusTempStorageRegLines.AddNew();
			line.SRL_SRH = header.PK;
			line.SRL_LimitDate = ZDate.Today;
			line.SRL_LineNumber = 1;
			line.SRL_LimitDate = ZDateTime.Now.Date;
			line.SRL_PackagesRemaining = 0;
			line.SRL_LocationOfGoods = "DE";

			var transaction = line.CusTempStorageRegLineTransactions.AddNew();
			transaction.SRT_SRL = line.PK;
			transaction.SRT_TransactionType = "TRN";
			transaction.SRT_GrossWeight = 1;

			return transaction;
		}
	}
}

