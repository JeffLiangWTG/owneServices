using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	public class OptionalFilterCriteriaListTest : TestCaseWithFactory
	{
		public void TestAdd()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(new OptionalFilterCriteria());
			AssertEquals(1, testCollection.Count);
			testCollection.Add(new LedgerFilterCriteria("TEST", "TST"));
			AssertEquals(2, testCollection.Count);
		}

		public void TestIndexer()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(new TransactionFilterCriteria("Invoice", "INV"));
			AssertEquals(1, testCollection.Count);
			testCollection.Add(new TransactionFilterCriteria("Receipt", "INV"));
			AssertEquals(2, testCollection.Count);
			AssertEquals("Invoice", testCollection[0].Description);
			AssertEquals("Receipt", testCollection[1].Description);
		}

		public void TestGetFilterForTransactionTypes()
		{
			SetupTransaction();
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(new TransactionFilterCriteria("Invoice", "INV"));
			AccTransactionHeader[] result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("No filter returned if no item is checked", 5, result.Length);
			testCollection[0].Enabled = true;
			result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("Transaction Count should be 2", 2, result.Length);
		}

		public void TestGetFilterForMultipleTransactionTypes()
		{
			SetupTransaction();
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(new TransactionFilterCriteria("Invoice", "INV"));
			testCollection.Add(new TransactionFilterCriteria("Receipt", "REC"));
			AccTransactionHeader[] result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("No filter returned if no item is checked", 5, result.Length);
			testCollection[0].Enabled = true;
			result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("Transaction Count should be 2", 2, result.Length);
			testCollection[1].Enabled = true;
			result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("Transaction Count should be 3", 3, result.Length);
		}

		public void TestGetFilterLedgerTypes()
		{
			SetupTransaction();
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(new LedgerFilterCriteria("Account Receivable", "AR"));
			AccTransactionHeader[] result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("No filer returned if no item is checked", 5, result.Length);
			testCollection[0].Enabled = true;
			result = Factory.Load(typeof(AccTransactionHeader), testCollection.GetFilterFromSelectedItems().AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)) as AccTransactionHeader[];
			AssertEquals("Transaction Count should be 2", 2, result.Length);
		}

		public void TestFindFindItemByDescription()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(PaymentFilter);
			OptionalFilterCriteria result = testCollection.FindItemByDescription(AccrualFilter.Description);
			AssertNotNull(result);
			AssertEquals(TransactionDescription.Accrual, result.Description);
			Assert(!result.Enabled);
		}

		public void TestFindFindItemByDescriptionAndItsState()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(PaymentFilter);
			testCollection[0].Enabled = true;
			OptionalFilterCriteria result = testCollection.FindItemByDescription(AccrualFilter.Description);
			AssertNotNull(result);
			AssertEquals(TransactionDescription.Accrual, result.Description);
			Assert(result.Enabled);
		}

		public void TestFindFindItemByDescriptionNoResult()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(PaymentFilter);
			testCollection[0].Enabled = true;
			OptionalFilterCriteria result = testCollection.FindItemByDescription(TransactionDescription.Receipt);
			AssertNull(result);
		}

		public void TestSetItemByDescription()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(PaymentFilter);
			Assert(!testCollection[0].Enabled);
			Assert(!testCollection[1].Enabled);
			Assert(!testCollection[2].Enabled);
			testCollection.SetItemByDescription(InvoiceFilter.Description, true);
			Assert(!testCollection[0].Enabled);
			Assert(testCollection[1].Enabled);
			Assert(!testCollection[2].Enabled);
			testCollection.SetItemByDescription(InvoiceFilter.Description, false);
			Assert(!testCollection[0].Enabled);
			Assert(!testCollection[1].Enabled);
			Assert(!testCollection[2].Enabled);
		}

		public void TestAddOnlyUniqueItemByDescription()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(InvoiceFilter);
			AssertEquals("Duplicate Item should not be added to the collection.", 2, testCollection.Count);
		}

		public void TestFindSelectedValueByDescription()
		{
			OptionalFilterCriteriaList testCollection = new OptionalFilterCriteriaList();
			testCollection.Add(AccrualFilter);
			testCollection.Add(InvoiceFilter);
			testCollection.Add(PaymentFilter);
			testCollection[0].Enabled = true;
			testCollection[1].Enabled = false;
			testCollection[2].Enabled = true;
			Assert(testCollection.FindSelectedValueByDescription(AccrualFilter.Description));
			Assert(!testCollection.FindSelectedValueByDescription(InvoiceFilter.Description));
			Assert(testCollection.FindSelectedValueByDescription(PaymentFilter.Description));
			Assert(!testCollection.FindSelectedValueByDescription("987987"));
		}

		public void TestSelectedItemCount()
		{
			LedgerTransactionAssociator testAssociator = new LedgerTransactionAssociator();
			OptionalFilterCriteriaList ledgerList = testAssociator.GetLedgerList();
			AssertEquals(0, ledgerList.SelectedItemCount);
			ledgerList[0].Enabled = true;
			AssertEquals(1, ledgerList.SelectedItemCount);
			ledgerList[1].Enabled = true;
			AssertEquals(2, ledgerList.SelectedItemCount);
			ledgerList[1].Enabled = false;
			AssertEquals(1, ledgerList.SelectedItemCount);
		}

		void SetupTransaction()
		{
			ZInt testPeriod = 200401;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(testPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			ARInvoice testInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2004, 1, 15);
			testInvoice.AH_TransactionNum = "TEST012";
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie.AH_PostDate = new ZDateTime(2004, 1, 15);
			testAPInvocie.AH_TransactionNum = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2004, 1, 5);
			testPayment.AH_TransactionNum = "TES45i7";
			ARReceipt testARReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testARReceipt.AH_PostDate = new ZDateTime(2004, 1, 5);
			testARReceipt.AH_TransactionNum = "TES3452i7";
			DirectReceipt testDirectReceipt = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			testDirectReceipt.AH_PostDate = new ZDateTime(2004, 1, 5);
			testDirectReceipt.AH_TransactionNum = "TES345ew2i7";
			Factory.Save();
		}

		TransactionFilterCriteria AccrualFilter;
		TransactionFilterCriteria InvoiceFilter;
		TransactionFilterCriteria PaymentFilter;
		protected override void SetUp()
		{
			base.SetUp();
			AccrualFilter = new TransactionFilterCriteria(TransactionDescription.Accrual, TransactionLineTypes.Accrual);
			InvoiceFilter = new TransactionFilterCriteria(TransactionDescription.Invoice, TransactionTypes.Invoice);
			PaymentFilter = new TransactionFilterCriteria(TransactionDescription.Payment, TransactionTypes.Payment);
		}
	}
}