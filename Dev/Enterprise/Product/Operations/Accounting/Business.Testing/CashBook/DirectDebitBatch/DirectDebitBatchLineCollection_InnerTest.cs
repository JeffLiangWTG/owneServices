using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	[TestedType(typeof(DirectDebitBatchLineCollection))]
	public class DirectDebitBatchLineCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData(typeof(APPayment));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DirectDebitBatchLineCollection(Factory);
		}

		public void TestClearTotalAmount()
		{
			var collection = GetCollectionToTest() as DirectDebitBatchLineCollection;
			collection.FSelectedTotal_ForTestOnly = 12m;
			collection.FLocalSelectedTotal_ForTestOnly = 12m;

			collection.ClearTotalAmount();
			AssertEquals(0m, collection.FSelectedTotal_ForTestOnly);
			AssertEquals(0m, collection.FLocalSelectedTotal_ForTestOnly);
		}

		/// What is the Selection Criteria?
		/// 
		/// 1. Bank - if Bank is set to DDR
		/// 2. TransactionTypes - AR/AP Pay, DPY
		/// 3. Transactions without AH_ReceiptBatchNo
		/// 4. Same Company
		/// 5. Not Cancelled
		/// 6. DDR type AH_ReceiptType

		public void TestCollectionForLoadingNewLines()
		{
			DDRBankAccount.AB_AllowAutoDDR = true;

			DirectPayment.DirectPayment directPayment;
			// Inclusion Case
			SetupPayments(out directPayment);

			// Exclusion Case
			SetupTransactionsToBeExcluded();
			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			DirectDebitBatchLineCollection testCollection = header.Lines;

			testCollection.LoadNewLines(DDRBankAccount);
			AssertEquals("Row Count", 3, testCollection.Count);

			testCollection.LoadNewLines(TestObjectCreator.USDBankAccount);
			AssertEquals("Row Count", 1, testCollection.Count);

			header.RelatedTransactionPK = directPayment.PK;
			testCollection.LoadNewLines(DDRBankAccount);
			AssertEquals("Row Count", 1, testCollection.Count);
			AssertEquals("DirectPayment", directPayment.PK, ((DirectPayment.DirectPayment)testCollection[0]).PK);

			header.RelatedTransactionPK = ZGuid.Empty;
			testCollection.LoadNewLines(DDRBankAccount);
			AssertEquals("Row Count", 3, testCollection.Count);
		}

		void SetupPayments(out DirectPayment.DirectPayment directPayment)
		{
			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			TestHelper.SetUpDDRTransaction(aPPayment, DDRBankAccount, 10m, 0m, "");
			TestHelper.SetUpDDRTransaction(aRPayment, DDRBankAccount, 20m, 0m, "");
			TestHelper.SetUpDDRTransaction(directPayment, DDRBankAccount, 40m, 0m, "");
		}

		void SetupTransactionsToBeExcluded()
		{
			APPayment aPPaymentWithDifferentBank = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment aRPaymentAlreadyBatched = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPaymentWithDifferentCompany = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			APPayment aPPaymentCancelled = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;

			TestHelper.SetUpDDRTransaction(aPInvoice, DDRBankAccount, 1000m, 0, "");
			TestHelper.SetUpDDRTransaction(aPPaymentWithDifferentBank, TestObjectCreator.USDBankAccount, 1000m, 0, "");
			TestHelper.SetUpDDRTransaction(aRPaymentAlreadyBatched, DDRBankAccount, 1000m, 0, "");
			TestHelper.SetUpDDRTransaction(directPaymentWithDifferentCompany, DDRBankAccount, 1000m, 0, "", null, true);
			TestHelper.SetUpDDRTransaction(aPPaymentCancelled, DDRBankAccount, 1000m, 0, "");

			aRPaymentAlreadyBatched.AH_ReceiptBatchNo = "Batched";
			directPaymentWithDifferentCompany.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			directPaymentWithDifferentCompany.Lines[0].AL_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			aPPaymentCancelled.AH_IsCancelled = ZBool.True;
			((IMatching)aPPaymentCancelled).CurrentMatchGroup.AddNew().AP_AH = aPPaymentCancelled.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(aPPaymentCancelled);
		}

		public void TestCollectionLoadExistingLines()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			testHeader.AH_TransactionNum = "00001001";
			testHeader.IsManuallySetTransactionNumber_ForTestOnly = true;

			TestHelper.SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, testHeader.AH_TransactionNum);
			TestHelper.SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 80m, testHeader.AH_TransactionNum);
			TestHelper.SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 310m, testHeader.AH_TransactionNum, null, true);

			SetupTransactionsToBeExcluded();

			APPayment aPPaymentWithDifferentCompany = Factory.New(typeof(APPayment)) as APPayment;
			TestHelper.SetUpDDRTransaction(aPPaymentWithDifferentCompany, DDRBankAccount, 110m, 70m, testHeader.AH_TransactionNum);
			aPPaymentWithDifferentCompany.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;

			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			DirectDebitBatchLineCollection testCollection = new DirectDebitBatchLineCollection(Factory, testHeader);
			testCollection.Load();

			AssertEquals(3, testCollection.Count);
		}

		public void TestUpdateReceiptTypeToDDL()
		{
			DDRBankAccount.AB_AllowAutoDDR = true;

			// Inclusion Case
			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			TestHelper.SetUpDDRTransaction(aPPayment, DDRBankAccount, 10m, 0m, "");
			TestHelper.SetUpDDRTransaction(aRPayment, DDRBankAccount, 20m, 0m, "");
			TestHelper.SetUpDDRTransaction(directPayment, DDRBankAccount, 40m, 0m, "");

			// Exclusion Case
			SetupTransactionsToBeExcluded();
			DirectDebitBatchLineCollection testCollection = new DirectDebitBatchLineCollection(Factory);

			//TestCollection.Load();

			testCollection.LoadNewLines(DDRBankAccount);
			AssertEquals("Row Count", 3, testCollection.Count);

			testCollection.SetReceiptType_ForTestOnly(ReceiptTypes.DirectDebitLine);

			foreach (TransactionHeader transaction in testCollection)
			{
				AssertEquals(ReceiptTypes.DirectDebitLine, transaction.AH_ReceiptType);
			}
		}

		public void TestUpdateReceiptTypeToDDLDoNotUpdatePostedLines()
		{
			DDRBankAccount.AB_AllowAutoDDR = true;
			GlbCompany.CurrentCompany.SetCountry("AU");
			DirectDebitBatchHeader testHeader = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			ARPayment aRPayment = Factory.New(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			TestHelper.SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, testHeader.AH_TransactionNum);
			TestHelper.SetUpDDRTransaction(aRPayment, DDRBankAccount, 220m, 80m, testHeader.AH_TransactionNum);
			TestHelper.SetUpDDRTransaction(directPayment, DDRBankAccount, 420m, 310m, testHeader.AH_TransactionNum, null, true);

			testHeader.AH_AB = DDRBankAccount.PK;

			Factory.Save();

			DirectDebitBatchLineCollection testCollection = new DirectDebitBatchLineCollection(Factory, testHeader);
			testCollection.Load();

			testCollection.SetReceiptType_ForTestOnly(ReceiptTypes.DirectDebitLine);

			foreach (TransactionHeader transaction in testCollection)
			{
				AssertEquals(ReceiptTypes.DirectDebit, transaction.AH_ReceiptType);
			}
		}

		public void TestSetIncludeBatchFlags()
		{
			DirectDebitBatchLineCollection testCollection = new DirectDebitBatchLineCollection(Factory);

			APPayment trans1 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment trans2 = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment trans3 = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			testCollection.Add(trans1);
			testCollection.Add(trans2);
			testCollection.Add(trans3);

			testCollection.SetIncludeBatchFlags(true);

			Assert(trans1.IncludeInTheBatch);
			Assert(trans2.IncludeInTheBatch);
			Assert(trans3.IncludeInTheBatch);

			testCollection.SetIncludeBatchFlags(false);

			Assert(!trans1.IncludeInTheBatch);
			Assert(!trans2.IncludeInTheBatch);
			Assert(!trans3.IncludeInTheBatch);
		}

		public void TestOnlySelectedTransactionUpdateTheTotal()
		{
			DirectDebitBatchLineCollection testCollection = new DirectDebitBatchLineCollection(Factory);
			APPayment trans1 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			ARPayment trans2 = Factory.NewWithValidTestData(typeof(ARPayment)) as ARPayment;
			DirectPayment.DirectPayment trans3 = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			trans1.AH_OSExTaxAmount = 10m;
			trans2.AH_OSExTaxAmount = 20m;
			trans3.AH_OSExTaxAmount = 30m;

			testCollection.Add(trans1);
			testCollection.Add(trans2);
			testCollection.Add(trans3);

			trans1.IncludeInTheBatch = ZBool.True;
			trans2.IncludeInTheBatch = ZBool.True;
			trans3.IncludeInTheBatch = ZBool.True;

			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(60m, testCollection.FSelectedTotal_ForTestOnly);

			trans2.IncludeInTheBatch = ZBool.False;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(40m, testCollection.FSelectedTotal_ForTestOnly);

			trans1.IncludeInTheBatch = ZBool.False;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(30m, testCollection.FSelectedTotal_ForTestOnly);

			trans3.IncludeInTheBatch = ZBool.False;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(0m, testCollection.FSelectedTotal_ForTestOnly);

			trans2.IncludeInTheBatch = ZBool.True;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(20m, testCollection.FSelectedTotal_ForTestOnly);

			trans1.IncludeInTheBatch = ZBool.True;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(30m, testCollection.FSelectedTotal_ForTestOnly);

			trans3.IncludeInTheBatch = ZBool.True;
			//AssertEquals(60m, TestCollection.CollectionTotal);
			AssertEquals(60m, testCollection.FSelectedTotal_ForTestOnly);
		}

		public void TestLoadNewLinesWhenCollectionSortedByIncludeInTheBatchDoesntThrowException()
		{
			APPayment aPPayment = Factory.New<APPayment>();
			TestHelper.SetUpDDRTransaction(aPPayment, DDRBankAccount, 110m, 70m, "");
			Factory.Save();

			DirectDebitBatchHeader testHeader = Factory.New<DirectDebitBatchHeader>();
			((IBindingListView)testHeader.Lines).ApplySort(aPPayment.IncludeInTheBatchInfo.PropertyDescriptor, ListSortDirection.Ascending);
			AssertEquals("Precondition: sorting is setup for column which is not in collection TypeOfElements (TransactionHeader now)",
				aPPayment.IncludeInTheBatchInfo.Name, testHeader.Lines.SortInformation.PropertyName);

			AssertNoExceptionThrown(() => testHeader.AH_AB = DDRBankAccount.PK);

			AssertEquals(1, testHeader.Lines.Count);
			Assert("IncludeInTheBatch", testHeader.Lines[0].IncludeInTheBatch);
		}

		#region Implementation

		DirectDebitBatchHeaderTest fTestHelper;
		DirectDebitBatchHeaderTest TestHelper
		{
			get
			{
				if (fTestHelper == null)
				{
					fTestHelper = new DirectDebitBatchHeaderTest();
				}
				return fTestHelper;
			}
		}

		AccBankAccount fDDRBankAccount;
		AccBankAccount DDRBankAccount
		{
			get
			{
				if (fDDRBankAccount == null)
				{
					fDDRBankAccount = TestObjectCreator.AUDBankAccount;
				}
				return fDDRBankAccount;
			}
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

		#endregion
	}
}
