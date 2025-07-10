using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.PaymentChequeNumberAllocator;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.Testing
{
	[TestedType(typeof(PaymentChequeNumberAllocator))]
	public class PaymentChequeNumberAllocatorBaseTest : BaseAllocatorTest
	{
		#region BaseTests

		public override void TestGetFactoriesWithAllocationCodeToBeCalledOnSaving_Core()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			APPayment newPayment = newFactory.NewWithValidTestData<APPayment>();
			var allocator = new PaymentChequeNumberAllocator(newPayment, PrintingMode.Invoice, newPayment.Factory);
			var factories = allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(newFactory);
			AssertEquals("Factories array should contain 2 elements", 2, factories.Length);
			AssertEquals("First element is main Factory", newFactory, factories[0]);
			AssertEquals("Second element is SaveInTransactionWithRollBackAction ", typeof(SaveInTransactionWithRollBackAction), factories[1].GetType());
		}

		public override void TestCanContinueWithAllocation()
		{
			AccChequeBook testAutoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 1, 3);
			APPayment newPayment = TestHelper.GetAutoAllocateAPPayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			TestAllocator = new DummyPaymentChequeNumberAllocator(newPayment, PrintingMode.PaymentApproval, Factory);
			Assert("Allocation should be enabled by default", ((IChequeNumberAutoAllocation)newPayment).IsAutoAllocationEnabled);
			Assert("Allocation should be enabled by default", TestAllocator.CanContinueWithAllocation_ForTestOnly);
			Assert("Allocation should not be performed on payment yet", !((IChequeNumberAutoAllocation)newPayment).IsAllocationPerformed);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Allocation should be performed", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Allocation should be performed on payment", ((IChequeNumberAutoAllocation)newPayment).IsAllocationPerformed);
			Assert("Allocation should be disabled now", !TestAllocator.CanContinueWithAllocation_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Another Factory.Save call won't start another auto allocation", 1, TestAllocator.AllocationCalled_Counter);
		}

		public override void TestCanContinueWithPrinting()
		{
			AccChequeBook testAutoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 1, 3);
			APPayment newPayment = TestHelper.GetAutoAllocateAPPayment(testAutoPrintChequeBook.PK, testAutoPrintChequeBook.AK_AB);
			TestAllocator = new DummyPaymentChequeNumberAllocator(newPayment, PrintingMode.PaymentApproval, Factory);
			Assert("Printing should be enabled by default", TestAllocator.CanContinueWithPrinting_ForTestOnly);
			Assert("Printing should not be performed on payment yet", !((IChequeNumberAutoAllocation)newPayment).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Printing should be performed", 1, TestAllocator.PrintingCalled_Counter);
			Assert("Printing should be performed on payment", ((IChequeNumberAutoAllocation)newPayment).ChequeIsAutoPrinted);
			Assert("Printing should be disabled now", !TestAllocator.CanContinueWithPrinting_ForTestOnly);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Another Factory.Save call won't start another auto printing", 1, TestAllocator.PrintingCalled_Counter);
		}

		public override void TestMultipleFactorySavesWillNotCauseANewAllocation()
		{
			AccChequeBook testBookWithAutoAllocation = TestHelper.GetAutoPrintChequeBook(1, 2, 4);
			AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_AK = testBookWithAutoAllocation.PK;
			TestAllocator = new DummyPaymentChequeNumberAllocator(cheque, PrintingMode.HotCheque, Factory);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Cheque number should be autoallocated", "2", cheque.AQ_ChequeNumber);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", testBookWithAutoAllocation.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			Assert("Cheque is in database", cheque.IsInDatabase);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);

			testBookWithAutoAllocation.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 3m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque book is sitll active", testBookWithAutoAllocation.AK_IsActive);

			DummyPaymentChequeNumberAllocator anotherAllocator = new DummyPaymentChequeNumberAllocator(cheque, PrintingMode.HotCheque, Factory);
			BusinessObjectFactory.SaveTogether(anotherAllocator.GetFactoriesForTest());

			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);
			AssertEquals("Printing should not be called on another allocator", 0, anotherAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation should not be called on another allocator", 0, anotherAllocator.AllocationCalled_Counter);
			AssertEquals("Cheque number should be autoallocated", "2", cheque.AQ_ChequeNumber);

			testBookWithAutoAllocation.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 3m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque book is sitll active", testBookWithAutoAllocation.AK_IsActive);
		}

		#endregion

		#region Other Tests

		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			var chequeBook = TestHelper.GetAutoPrintChequeBook(1, 1, 3);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			Factory.Save();

			var payment = TestHelper.GetAutoAllocateAPPayment(chequeBook.PK, chequeBook.AK_AB);
			var allocator = new PaymentChequeNumberAllocator(payment, PrintingMode.PaymentApproval, Factory);

			var ex = AssertExceptionThrown<ZCannotSaveException>("throws", () => BusinessObjectFactory.SaveTogether(allocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving()));
			AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", ex.Message);
		}

		public void TestHotCheque_AutoAllocateAndPrint()
		{
			AccChequeBook testBookWithAutoAllocation = TestHelper.GetAutoPrintChequeBook(1, 2, 4);
			AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();
			cheque.AQ_AK = testBookWithAutoAllocation.PK;
			TestAllocator = new DummyPaymentChequeNumberAllocator(cheque, PrintingMode.HotCheque, Factory);
			Assert("Cheque number should be empty so far", cheque.AQ_ChequeNumber.IsEmpty);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)cheque).IsAutoAllocationEnabled);
			Assert("AutoAllocation should not be performed", !((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
			Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
			AssertEquals("Cheque number should be autoallocated", "2", cheque.AQ_ChequeNumber);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", testBookWithAutoAllocation.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			Assert("Cheque is in database", cheque.IsInDatabase);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);

			testBookWithAutoAllocation.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 3m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque book is sitll active", testBookWithAutoAllocation.AK_IsActive);
		}

		public void TestPaymentApproval_AutoAllocateAndPrint()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			OrgHeader testOrgHeader = TestHelper.TestObjectCreator.TestOrganisation;
			AccChequeBook testBookWithAutoAllocation = TestHelper.GetAutoPrintChequeBook(1, 3, 3);
			AccBankAccount bankAccount = testBookWithAutoAllocation.BankAccount;

			APPaymentApprovalWithoutAuthorisation testPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			testPaymentApproval.AV_OH = testOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = testBookWithAutoAllocation.PK;
			testPaymentApproval.AV_Amount = 100m;
			TestHelper.TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), testOrgHeader, testPaymentApproval, 100m);

			Assert("Cheque number should be empty", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
			TestAllocator = new DummyPaymentChequeNumberAllocator(testPaymentApproval, PrintingMode.PaymentApproval, Factory);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
			Assert("AutoAllocation should not be performed", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
			Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());

			Assert("New Payment should be created", !testPaymentApproval.AV_AH.IsEmpty);
			AssertNotNull("New Payment", testPaymentApproval.TransactionHeader);
			TransactionHeader newPayment = Factory.Load<TransactionHeader>(testPaymentApproval.AV_AH);
			AssertEquals("ChequeOrReference should be set to current number", "3", testPaymentApproval.AV_ChequeOrReference);
			AssertEquals("Payment ChequeOrReference", testPaymentApproval.AV_ChequeOrReference, newPayment.AH_ChequeOrReference);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", testBookWithAutoAllocation.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			Assert("Test Payment is in database", testPaymentApproval.IsInDatabase);

			testBookWithAutoAllocation.Reload();
			AssertEquals("CurrentNo should change on the cheque book", 4m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque book should become inactive", !testBookWithAutoAllocation.AK_IsActive);
		}

		public void TestPayment_AutoAllocateAndPrint()
		{
			AccChequeBook autoPrintChequeBook = TestHelper.GetAutoPrintChequeBook(1, 35, 100);
			ARPayment testPayment = Factory.NewWithValidTestData<ARPayment>();
			testPayment.AH_AB = autoPrintChequeBook.AK_AB;
			testPayment.ChequeBook = autoPrintChequeBook.PK;

			APPaymentApprovalWithoutAuthorisation dummyRelatedPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			dummyRelatedPaymentApproval.AV_AK = autoPrintChequeBook.PK;
			dummyRelatedPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
			testPayment.RelatedPaymentApproval = dummyRelatedPaymentApproval;

			Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
			TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.PaymentApproval, Factory);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
			Assert("AutoAllocation should not be performed", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());

			AssertEquals("ChequeOrReference should be set to current number", "35", dummyRelatedPaymentApproval.AV_ChequeOrReference);
			AssertEquals("Payment ChequeOrReference", dummyRelatedPaymentApproval.AV_ChequeOrReference, testPayment.AH_ChequeOrReference);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", autoPrintChequeBook.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Payment should be saved", testPayment.IsInDatabase);
			Assert("Test Payment is in database", dummyRelatedPaymentApproval.IsInDatabase);

			autoPrintChequeBook.Reload();
			AssertEquals("ChequeBook should have correct AK_CurrentNo", 36m, autoPrintChequeBook.AK_CurrentNo);
			Assert("Cheque book is sitll active", autoPrintChequeBook.AK_IsActive);
		}

		public void TestDirectPayment_AutoAllocateAndPrint()
		{
			AccChequeBook chequeBook = TestHelper.GetAutoPrintChequeBook(1, 3, 3);

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_AB = chequeBook.AK_AB;
			testPayment.ChequeBookPK = chequeBook.PK;
			testPayment.AH_InvoiceAmount = 100m;
			testPayment.AH_OSTotal = 100m;
			testPayment.Lines.AddNew(testPayment.DependentTransactionLineType);
			testPayment.Lines[0].AL_LineAmount = 100m;
			testPayment.Lines[0].AL_OSAmount = 100m;

			Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
			TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.DirectPayment, Factory);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
			Assert("AutoAllocation should not be performed", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());

			AssertEquals("ChequeOrReference should be set to current number", "3", testPayment.AH_ChequeOrReference);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", chequeBook.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Payment should be saved", testPayment.IsInDatabase);

			chequeBook.Reload();
			AssertEquals("ChequeBook should have correct AK_CurrentNo", 4m, chequeBook.AK_CurrentNo);
			Assert("Cheque book should become inactive", !chequeBook.AK_IsActive);
		}

		public void TestInvoice_AutoAllocateAndPrint()
		{
			AccChequeBook chequeBook = TestHelper.GetAutoPrintChequeBook(1, 3, 3);
			APInvoice testInvoice = Factory.NewWithValidTestData<APInvoice>();
			testInvoice.SubmittedFromInvoicingForm = true;
			testInvoice.IsInvoiceReceiptPayment = true;
			testInvoice.ReceiptPaymentAH_AB = chequeBook.AK_AB;
			testInvoice.ReceiptPaymentAK_AB = chequeBook.PK;
			testInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

			Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
			TestAllocator = new DummyPaymentChequeNumberAllocator(testInvoice, PrintingMode.Invoice, Factory);
			Assert("AutoAllocation should be enabled", ((IChequeNumberAutoAllocation)testInvoice).IsAutoAllocationEnabled);
			Assert("AutoAllocation should not be performed", !((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
			Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
			BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());

			AssertNotNull("Receipt should be created", testInvoice.ReceiptPayment);
			AssertEquals("Cheque number should be passed to the payment", "3", testInvoice.ReceiptPayment.AH_ChequeOrReference);
			AssertEquals("ChequeOrReference should be set to current number", "3", testInvoice.ReceiptPaymentAH_ChequeOrReference);
			Assert("AutoAllocation should be performed", ((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
			Assert("AutoPrinting should be performed", ((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
			Assert("Auto printing should be performed", TestAllocator.ChequeWasAutoPrinted);
			AssertEquals("Printer passed for printing", chequeBook.AK_SQ, TestAllocator.PrinterPassedForAutoPrinting);
			AssertEquals("Printing called only once", 1, TestAllocator.PrintingCalled_Counter);
			AssertEquals("Allocation called only once", 1, TestAllocator.AllocationCalled_Counter);
			Assert("Payment should be saved", testInvoice.ReceiptPayment.IsInDatabase);
			Assert("Invoice shoulod be saved", testInvoice.IsInDatabase);

			chequeBook.Reload();
			AssertEquals("ChequeBook should have correct AK_CurrentNo", 4m, chequeBook.AK_CurrentNo);
			Assert("Cheque book should become inactive", !chequeBook.AK_IsActive);
		}

		#endregion

		#region Implementation

		DummyPaymentChequeNumberAllocator TestAllocator;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PaymentChequeNumberAllocator(null, PrintingMode.DirectPayment, Factory);
		}

		#endregion
	}
}
