using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.PaymentChequeNumberAllocator;

namespace Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting.Testing
{
	public class PaymentChequeNumberAllocatorRollBackTest : BaseRollBackTestWithoutFactory
	{
		#region HotCheque

		[ExpectNoExceptions("Should be no other exceptions during the roll back")]
		public void TestHotCheque_AnErrorDuringPrinting()
		{
			AccHotCheque cheque = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 2, 4, ZBool.False);
				cheque = Factory.NewWithValidTestData<AccHotCheque>();
				cheque.AQ_AK = TestBookWithAutoAllocation.PK;
				TestAllocator = new DummyPaymentChequeNumberAllocator(cheque, PrintingMode.HotCheque, Factory);
				TestAllocator.RaiseErrorOnPrinting = ZBool.True;
				Assert("Cheque number should be empty", cheque.AQ_ChequeNumber.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)cheque).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				cheque.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				Assert("ZCannotSaveException should occur", ex is ZCannotSaveException);
				AssertNotNull("Inner exception", ex.InnerException);
				AssertEquals("Test exception should occur", "Exception for test", ex.InnerException.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !cheque.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", cheque.AQ_ChequeNumber.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 2m, cheque.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 2m, TestBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings.", ex.Message);
			}
		}

		public void TestHotCheque_AnErrorDuringAllocation()
		{
			AccHotCheque cheque = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 2, 4, ZBool.False);
				cheque = Factory.NewWithValidTestData<AccHotCheque>();
				cheque.AQ_AK = TestBookWithAutoAllocation.PK;
				TestAllocator = new DummyPaymentChequeNumberAllocator(cheque, PrintingMode.HotCheque, Factory);
				TestAllocator.RaiseErrorOnAllocation = ZBool.True;
				Assert("Cheque number should be empty", cheque.AQ_ChequeNumber.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)cheque).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				cheque.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				AssertEquals("Test exception should occur", "Exception for test", ex.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)cheque).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)cheque).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !cheque.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", cheque.AQ_ChequeNumber.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 2m, cheque.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 2m, TestBookWithAutoAllocation.AK_CurrentNo);
			}
		}

		#endregion

		#region PaymentApproval

		public void TestPaymentApproval_AnErrorDuringPrinting()
		{
			APPaymentApprovalWithoutAuthorisation testPaymentApproval = null;

			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 3, 4, ZBool.True);
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
				testHelper.SetupPeriods();

				testPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				testPaymentApproval.AV_OH = TestOrgHeader.PK;
				testPaymentApproval.AV_AB = BankAccount.PK;
				testPaymentApproval.AV_AK = TestBookWithAutoAllocation.PK;
				testPaymentApproval.AV_Amount = 100m;
				TestHelper.TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, testPaymentApproval, 100m);
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPaymentApproval, PrintingMode.PaymentApproval, Factory);
				TestAllocator.RaiseErrorOnPrinting = ZBool.True;
				Assert("Cheque number should be empty", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPaymentApproval.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				Assert("ZCannotSaveException should occur", ex is ZCannotSaveException);
				AssertNotNull("Inner exception", ex.InnerException);
				AssertEquals("Test exception should occur", "Exception for test", ex.InnerException.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Payment approval should not be in database", !testPaymentApproval.IsInDatabase);
				Assert("New Payment should not be saved yet", !testPaymentApproval.NewPayment.IsInDatabase);
				Assert("Cheque number on PaymentApproval should be empty", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
				Assert("Cheque number should be empty on payment", testPaymentApproval.NewPayment.AH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 3m, testPaymentApproval.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 3m, TestBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings.", ex.Message);
			}
		}

		public void TestPaymentApproval_AnErrorDuringAllocation()
		{
			APPaymentApprovalWithoutAuthorisation testPaymentApproval = null;

			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 3, 4, ZBool.True);
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
				testHelper.SetupPeriods();

				testPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				testPaymentApproval.AV_OH = TestOrgHeader.PK;
				testPaymentApproval.AV_AB = BankAccount.PK;
				testPaymentApproval.AV_AK = TestBookWithAutoAllocation.PK;
				testPaymentApproval.AV_Amount = 100m;
				TestHelper.TestObjectCreator.FillPaymentMatchTransactions("1", typeof(APInvoice), TestOrgHeader, testPaymentApproval, 100m);
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPaymentApproval, PrintingMode.PaymentApproval, Factory);
				TestAllocator.RaiseErrorOnAllocation = ZBool.True;
				Assert("Cheque number should be empty", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPaymentApproval).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPaymentApproval.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				AssertEquals("Test exception should occur", "Exception for test", ex.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPaymentApproval).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPaymentApproval).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Payment approval should not be in database", !testPaymentApproval.IsInDatabase);
				Assert("New Payment should not be saved yet", !testPaymentApproval.NewPayment.IsInDatabase);
				Assert("Cheque number on PaymentApproval should be empty", testPaymentApproval.AV_ChequeOrReference.IsEmpty);
				Assert("Cheque number should be empty on payment", testPaymentApproval.NewPayment.AH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 3m, testPaymentApproval.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 3m, TestBookWithAutoAllocation.AK_CurrentNo);
			}
		}

		#endregion

		#region Payment

		public void TestPayment_AnErrorDuringPrinting()
		{
			ARPayment testPayment = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 35, 100, ZBool.False);
				testPayment = Factory.NewWithValidTestData<ARPayment>();
				testPayment.AH_AB = TestBookWithAutoAllocation.AK_AB;
				testPayment.ChequeBook = TestBookWithAutoAllocation.PK;

				APPaymentApprovalWithoutAuthorisation dummyRelatedPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				dummyRelatedPaymentApproval.AV_AK = TestBookWithAutoAllocation.PK;
				dummyRelatedPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
				testPayment.RelatedPaymentApproval = dummyRelatedPaymentApproval;
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.PaymentApproval, Factory);
				TestAllocator.RaiseErrorOnPrinting = ZBool.True;
				Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPayment.ChequeBookBizO.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				Assert("ZCannotSaveException should occur", ex is ZCannotSaveException);
				AssertNotNull("Inner exception", ex.InnerException);
				AssertEquals("Test exception should occur", "Exception for test", ex.InnerException.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testPayment.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number should be empty on related PaymentApproval", testPayment.RelatedPaymentApproval.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 35m, testPayment.ChequeBookBizO.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 35m, TestBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings.", ex.Message);
			}
		}

		public void TestPayment_AnErrorDuringAllocation()
		{
			ARPayment testPayment = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 35, 100, ZBool.False);
				testPayment = Factory.NewWithValidTestData<ARPayment>();
				testPayment.AH_AB = TestBookWithAutoAllocation.AK_AB;
				testPayment.ChequeBook = TestBookWithAutoAllocation.PK;

				APPaymentApprovalWithoutAuthorisation dummyRelatedPaymentApproval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
				dummyRelatedPaymentApproval.AV_AK = TestBookWithAutoAllocation.PK;
				dummyRelatedPaymentApproval.AV_PaymentType = ReceiptTypes.Cheque;
				testPayment.RelatedPaymentApproval = dummyRelatedPaymentApproval;
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.PaymentApproval, Factory);
				TestAllocator.RaiseErrorOnAllocation = ZBool.True;
				Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPayment.ChequeBookBizO.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				AssertEquals("Test exception should occur", "Exception for test", ex.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testPayment.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number should be empty on related PaymentApproval", testPayment.RelatedPaymentApproval.AV_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 35m, testPayment.ChequeBookBizO.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 35m, TestBookWithAutoAllocation.AK_CurrentNo);
			}
		}

		#endregion

		#region DirectPayment

		public void TestDirectPayment_AnErrorDuringPrinting()
		{
			DirectPayment testPayment = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 3, 3, ZBool.False);
				testPayment = Factory.NewWithValidTestData<DirectPayment>();
				testPayment.AH_AB = TestBookWithAutoAllocation.AK_AB;
				testPayment.ChequeBookPK = TestBookWithAutoAllocation.PK;
				testPayment.AH_InvoiceAmount = 100m;
				testPayment.AH_OSTotal = 100m;
				testPayment.Lines.AddNew(testPayment.DependentTransactionLineType);
				testPayment.Lines[0].AL_LineAmount = 100m;
				testPayment.Lines[0].AL_OSAmount = 100m;
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.DirectPayment, Factory);
				TestAllocator.RaiseErrorOnPrinting = ZBool.True;
				Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPayment.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				Assert("ZCannotSaveException should occur", ex is ZCannotSaveException);
				AssertNotNull("Inner exception", ex.InnerException);
				AssertEquals("Test exception should occur", "Exception for test", ex.InnerException.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testPayment.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 3m, testPayment.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 3m, TestBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings.", ex.Message);
			}
		}

		public void TestDirectPayment_AnErrorDuringAllocation()
		{
			DirectPayment testPayment = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 3, 3, ZBool.False);
				testPayment = Factory.NewWithValidTestData<DirectPayment>();
				testPayment.AH_AB = TestBookWithAutoAllocation.AK_AB;
				testPayment.ChequeBookPK = TestBookWithAutoAllocation.PK;
				testPayment.AH_InvoiceAmount = 100m;
				TestAllocator = new DummyPaymentChequeNumberAllocator(testPayment, PrintingMode.DirectPayment, Factory);
				TestAllocator.RaiseErrorOnAllocation = ZBool.True;
				Assert("Cheque number should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testPayment.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				AssertEquals("Test exception should occur", "Exception for test", ex.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testPayment).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testPayment.IsInDatabase);
				Assert("Cheque number on Cheque should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 3m, testPayment.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 3m, TestBookWithAutoAllocation.AK_CurrentNo);
			}
		}

		#endregion

		#region Invoice

		public void TestInvoice_AnErrorDuringPrinting()
		{
			APInvoice testInvoice = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 2, 4, ZBool.False);
				testInvoice = Factory.NewWithValidTestData<APInvoice>();
				testInvoice.SubmittedFromInvoicingForm = true;
				testInvoice.IsInvoiceReceiptPayment = true;
				testInvoice.ReceiptPaymentAH_AB = TestBookWithAutoAllocation.AK_AB;
				testInvoice.ReceiptPaymentAK_AB = TestBookWithAutoAllocation.PK;
				testInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

				TestAllocator = new DummyPaymentChequeNumberAllocator(testInvoice, PrintingMode.Invoice, Factory);
				TestAllocator.RaiseErrorOnPrinting = ZBool.True;
				Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testInvoice).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testInvoice.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				Assert("ZCannotSaveException should occur", ex is ZCannotSaveException);
				AssertNotNull("Inner exception", ex.InnerException);
				AssertEquals("Test exception should occur", "Exception for test", ex.InnerException.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testInvoice.IsInDatabase);
				AssertNull("Related Payment should be deleted as saving failed", testInvoice.ReceiptPayment);
				Assert("Cheque number on Cheque should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 2m, testInvoice.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 2m, TestBookWithAutoAllocation.AK_CurrentNo);
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings.", ex.Message);
			}
		}

		public void TestInvoice_AnErrorDuringAllocation()
		{
			APInvoice testInvoice = null;
			try
			{
				SetUpTestObjectsAndBeginTransaction(1, 2, 4, ZBool.False);
				testInvoice = Factory.NewWithValidTestData<APInvoice>();
				testInvoice.SubmittedFromInvoicingForm = true;
				testInvoice.IsInvoiceReceiptPayment = true;
				testInvoice.ReceiptPaymentAH_AB = TestBookWithAutoAllocation.AK_AB;
				testInvoice.ReceiptPaymentAK_AB = TestBookWithAutoAllocation.PK;
				testInvoice.ReceiptPaymentAH_ReceiptType = ReceiptTypes.Cheque;

				TestAllocator = new DummyPaymentChequeNumberAllocator(testInvoice, PrintingMode.Invoice, Factory);
				TestAllocator.RaiseErrorOnAllocation = ZBool.True;
				Assert("Cheque number should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				Assert("Cheque number auto allocation should enabled", ((IChequeNumberAutoAllocation)testInvoice).IsAutoAllocationEnabled);
				Assert("Auto Aloocation will be performed on saving", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("Auto Printing will be performed on saving", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				testInvoice.ChequeBook.Reloaded += new EventHandler(ChequeBook_Reloaded);
				BusinessObjectFactory.SaveTogether(TestAllocator.GetFactoriesForTest());
				Fail("An exception must be thrown.");
			}
			catch (Exception ex)
			{
				ExceptionOccured = ZBool.True;
				AssertEquals("Test exception should occur", "Exception for test", ex.Message);
				Assert("Allocation is not performed as the was an error on saving", !((IChequeNumberAutoAllocation)testInvoice).IsAllocationPerformed);
				Assert("AutoAllocation should be still enabled, as there was an error", TestAllocator.CanContinueWithAllocation_ForTestOnly);
				Assert("AutoPrinting should not be performed", !((IChequeNumberAutoAllocation)testInvoice).ChequeIsAutoPrinted);
				Assert("AutoPrinting should be still enabled, as there was an error", TestAllocator.CanContinueWithPrinting_ForTestOnly);
				Assert("Cheque should not be in database", !testInvoice.IsInDatabase);
				AssertNull("Related Payment should not be created before allocation", testInvoice.ReceiptPayment);
				Assert("Cheque number on Cheque should be empty", testInvoice.ReceiptPaymentAH_ChequeOrReference.IsEmpty);
				AssertEquals("Cheque book should be reloaded once", 1, Test_ChequeBookReloads_Counter);
				AssertEquals("CurrentNo should remain same", 2m, testInvoice.ChequeBook.AK_CurrentNo);
				TestBookWithAutoAllocation.Reload();
				AssertEquals("CurrentNo should remain old on the cheque book", 2m, TestBookWithAutoAllocation.AK_CurrentNo);
			}
		}

		#endregion

		#region Implementation

		DummyPaymentChequeNumberAllocator TestAllocator;

		#endregion
	}
}
