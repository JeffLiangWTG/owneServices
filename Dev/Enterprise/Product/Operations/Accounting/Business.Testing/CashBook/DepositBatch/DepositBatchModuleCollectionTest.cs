using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchModuleCollection))]
	public class DepositBatchModuleCollectionTest : TransactionHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DepositBatchModuleCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<DepositBatch>();
		}

		public void TestCollectionFiltering()
		{
			ARReceipt testReceipt = TestObjectCreator.CreateARReceipt(ZArchitecture.Core.ReceiptTypes.Cash, ZArchitecture.Core.TransactionTypes.Receipt, ZArchitecture.Core.LedgerTypes.AccountsReceivable, 100M, TestObjectCreator.AUDBankAccount.PK);
			Factory.Save();

			DepositBatchParent testDepositParent = new DepositBatchParent(Factory);
			AssertEquals(1, testDepositParent.DepositBatchLines.Count);
			Factory.Save();

			DepositBatchModuleCollection testCollection = new DepositBatchModuleCollection(Factory);
			testCollection.Load();

			AssertEquals("There should be 1 elements in the collection", 1, testCollection.Count);
		}

		#region Implementations

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
