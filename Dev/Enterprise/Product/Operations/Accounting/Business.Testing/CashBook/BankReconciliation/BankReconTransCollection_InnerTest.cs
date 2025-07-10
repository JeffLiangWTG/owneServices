using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconTransCollection))]
	public class BankReconTransCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionAddNew()
		{
			AssertNotNull(TestCollection.AddNew());
			AssertEquals(1, TestCollection.Count);
		}

		public void TestIndexer()
		{
			BusinessObject obj1 = TestCollection.AddNew();
			AssertEquals("Index 0", obj1, TestCollection[0]);

			BusinessObject obj2 = TestCollection.AddNew();
			AssertEquals("Index 1", obj2, TestCollection[1]);
		}

		[SuspendCriticalValidation]
		public void TestLoadingTransaction()
		{
			BankReconciliationTest bankReconciliationTest = new BankReconciliationTest();
			bankReconciliationTest.SetupTransactions(TestObjectCreator.AUDBankAccount, Factory);

			APPayment dDLPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDLPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			dDLPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRPayment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDRPayment.AH_ReceiptBatchNo = "TEST484";

			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDRBatch.BankAccount.AB_ShowDetailsOnDirectDebits = false;

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.ResetDatabaseLoadCount();
			AssertEquals("DatabaseLoadCount", 0, newFactory.DatabaseLoadCount);

			BankReconTransCollection collectionToTest = new BankReconTransCollection(newFactory, TestObjectCreator.AUDBankAccount.PK);
			collectionToTest.Load();
			AssertEquals("DatabaseLoadCount", 3, newFactory.DatabaseLoadCount);

			AssertNotNull(collectionToTest);
			AssertEquals(9, collectionToTest.Count);
			AssertNull(collectionToTest.FindByPK(dDLPayment.PK));
			AssertNotNull(collectionToTest.FindByPK(dDRPayment.PK));
			AssertNotNull(collectionToTest.FindByPK(dDRBatch.PK));
		}

		public void TestCancelledDDRBatchNotIncluded()
		{
			DirectDebitBatchHeader dDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			dDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			dDRBatch.BankAccount.AB_ShowDetailsOnDirectDebits = false;

			DirectDebitBatchHeader cancelledDDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			cancelledDDRBatch.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			cancelledDDRBatch.AH_IsCancelled = true;

			APPayment payment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			payment.AH_ReceiptType = ReceiptTypes.Cheque;
			payment.AH_AB = TestObjectCreator.AUDBankAccount.PK;
			new ReversingFactory().NewReversing(payment).Reverse();

			Factory.Save();

			Assert("Precondition: Payment must be cancelled", payment.IsCancelled);

			BankReconTransCollection collectionToTest = new BankReconTransCollection(Factory, TestObjectCreator.AUDBankAccount.PK);
			collectionToTest.Load();

			AssertEquals(3, collectionToTest.Count);
			AssertNull(collectionToTest.FindByPK(cancelledDDRBatch.PK));
			AssertNotNull(collectionToTest.FindByPK(dDRBatch.PK));
			AssertNotNull(collectionToTest.FindByPK(payment.PK));
		}

		public void TestNonRolledUPDirectBatchHeaderDoesNotShow()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			testObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = false;
			DirectDebitBatchHeader rolledUpDDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			rolledUpDDRBatch.AH_AB = testObjectCreator.AUDBankAccount.PK;
			rolledUpDDRBatch.AH_TransactionNum = "TEST1001";

			APPayment dDLPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDLPayment.AH_ReceiptType = ReceiptTypes.DirectDebitLine;
			dDLPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			dDLPayment.AH_ReceiptBatchNo = rolledUpDDRBatch.AH_TransactionNum;
			Factory.Save();

			testObjectCreator.AUDBankAccount.AB_ShowDetailsOnDirectDebits = true;
			DirectDebitBatchHeader nonRolledUpDDRBatch = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			nonRolledUpDDRBatch.AH_AB = testObjectCreator.AUDBankAccount.PK;
			nonRolledUpDDRBatch.AH_TransactionNum = "TEST1002";

			APPayment dDRPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			dDRPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			dDRPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
			dDRPayment.AH_ReceiptBatchNo = nonRolledUpDDRBatch.AH_TransactionNum;
			Factory.Save();

			BankReconTransCollection collectionToTest = new BankReconTransCollection(Factory, testObjectCreator.AUDBankAccount.PK);
			collectionToTest.Load();

			AssertNotNull(collectionToTest);
			AssertEquals(2, collectionToTest.Count);
			AssertNull(collectionToTest.FindByPK(nonRolledUpDDRBatch.PK));
			AssertNull(collectionToTest.FindByPK(dDLPayment.PK));
			AssertNotNull(collectionToTest.FindByPK(dDRPayment.PK));
			AssertNotNull(collectionToTest.FindByPK(rolledUpDDRBatch.PK));
		}

		protected BankReconTransCollection TestCollection;

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new BankReconTransCollection(Factory, ZGuid.NewZGuid());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BankReconTransCollection(Factory, ZGuid.Empty);
		}

		TestObjectCreator fTestObjectCreator;

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
	}
}
