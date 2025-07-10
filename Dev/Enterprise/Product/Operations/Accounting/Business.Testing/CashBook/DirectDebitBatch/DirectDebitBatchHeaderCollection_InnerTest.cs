using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	[TestedType(typeof(DirectDebitBatchHeaderCollection))]
	public class DirectDebitBatchHeaderCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		#region TestCollectionReturnsCorrectTransactions

		public void TestCollectionReturnsCorrectTransactions()
		{
			ARInvoice aRInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			APInvoice aPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			ARReceipt aRReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;

			APPayment aPPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			DirectPayment.DirectPayment directPayment = Factory.NewWithValidTestData(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;

			DirectDebitBatchHeader directDebitBatchHeader = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			directDebitBatchHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			directDebitBatchHeader.AH_TransactionNum = "TEST001";

			DirectDebitBatchHeader directDebitBatchHeaderWithDiffCompany = Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			directDebitBatchHeaderWithDiffCompany.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			directDebitBatchHeaderWithDiffCompany.AH_TransactionNum = "Test002";

			DirectDebitBatchHeaderCollection testCollection = new DirectDebitBatchHeaderCollection(Factory);
			testCollection.Load();

			AssertEquals(1, testCollection.Count);
			AssertEquals(LedgerTypes.CashBook, testCollection[0].AH_Ledger);
			AssertEquals(TransactionTypes.DDRBatch, testCollection[0].AH_TransactionType);
			AssertEquals("TEST001", testCollection[0].AH_TransactionNum);
		}

		#endregion

		#region TestFindBoxProviderAlwaysAppliesRelationshipFilter

		/// <summary>
		/// Needed because DocScanning will try to load the first matching Header
		/// when allocating eDocs (the Code field on AccTransactionHeader is not unique).
		/// </summary>
		public void TestFindBoxProviderAlwaysAppliesRelationshipFilter()
		{
			PropertyInfo info = Collection.GetType().GetProperty("FindBoxListProvider", BindingFlags.NonPublic | BindingFlags.Instance);
			IFindBoxListProvider listProvider = (IFindBoxListProvider)info.GetValue(Collection, null);

			DirectDebitBatchHeader invalidHeader = Factory.New<DirectDebitBatchHeader>();
			invalidHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Payment;
			invalidHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;
			invalidHeader.AH_TransactionNum = "1000";
			AssertNull(listProvider.GetBusinessObjectFromCode("1000"));

			DirectDebitBatchHeader validHeader = Factory.New<DirectDebitBatchHeader>();
			validHeader.AH_TransactionType = ZArchitecture.Core.TransactionTypes.DDRBatch;
			validHeader.AH_Ledger = ZArchitecture.Core.LedgerTypes.CashBook;
			validHeader.AH_TransactionNum = "1000";
			AssertEquals(validHeader, listProvider.GetBusinessObjectFromCode("1000"));
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData(typeof(DirectDebitBatchHeader));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DirectDebitBatchHeaderCollection(Factory);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;

		#endregion
	}
}
