using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using ErrorMessages = Enterprise.Accounting.Business.AccountingConstants.ChequeNumberAllocationErrorMessages;

namespace Enterprise.Accounting.GUI.CashBook.Testing
{
	[TestedType(typeof(DirectPaymentForm))]
	public class DirectPaymentFormTest : DirectCashBookBaseFormTest
	{
		protected override Form GetFormToBashCore()
		{
			DirectPayment testPayment = Factory.New<DirectPayment>();
			return new DirectPaymentForm(testPayment);
		}

		public void TestHideChequeControl()
		{
			DirectPayment testDirectPayment = Factory.NewWithValidTestData<DirectPayment>();
			Factory.Save();
			using (DirectPaymentForm newForm = new DirectPaymentForm(testDirectPayment))
			{
				newForm.DisplayMode = ODisplayMode.ReadOnly;
				newForm.Show();

				AssertEquals("Should not change the context string now. Grid layouts persisted for this context will be lost", "Payment", newForm.CashBookLineBoundGrid_ForTestOnly.ColumnLayoutContext);

				Assert("ChequeBookGuidFindBox should be invisible", !newForm.ChequeBookFindBox_ForTestOnly.Visible);
			}
			using (DirectPaymentForm newForm = new DirectPaymentForm(testDirectPayment))
			{
				newForm.DisplayMode = ODisplayMode.Browse;
				newForm.Show();
				Assert("ChequeBookGuidFindBox should be visible", newForm.ChequeBookFindBox_ForTestOnly.Visible);
			}
		}

		[ExpectNoExceptions]
		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			var creator = new TestObjectCreator(Factory);
			var taxrate = creator.CreateTaxRateWithoutZZ("ABC", "Test Tax Rate", 0);
			(new AccountingPeriodTestHelper()).SetupPeriods();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			var testBank = Factory.NewWithValidTestData<AccBankAccount>();
			var chequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 3);
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			Factory.Save();

			var testPayment = creator.CreateDirectPayment(ZDateTime.Now, 100m, 0m, 200m, 0m);
			testPayment.Lines[0].AL_AT = taxrate.PK;
			testPayment.Lines[1].AL_AT = taxrate.PK;
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = chequeBook.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			testPayment.AH_ExchangeRate = 0.2m;

			using (var newForm = new TestDirectPaymentForm(testPayment))
			{
				newForm.Show();
				Assert("Error shown to user", PaymentDocumentsPrinter.PerformTestAutoAllocationAndPrintChequesFailure(() => newForm.FireSaveButton()));
			}
		}

		[ExpectNoExceptions]
		public void TestAutoPrintCheque()
		{
			var creator = new TestObjectCreator(Factory);
			var taxrate = creator.CreateTaxRateWithoutZZ("ABC", "Test Tax Rate", 0);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 3);
			DirectPayment testPayment = creator.CreateDirectPayment(ZDateTime.Now, 100m, 0m, 200m, 0m);
			testPayment.Lines[0].AL_AT = taxrate.PK;
			testPayment.Lines[1].AL_AT = taxrate.PK;
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = chequeBook.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			testPayment.AH_ExchangeRate = 0.2m;
			testPayment.RunPreSaveValidation();
			Assert("TestPayment should not have any errors before saving", !testPayment.HasErrors);

			Assert("ChequeNumber should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque Number allocation should be enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);

			using (TestDirectPaymentForm newForm = new TestDirectPaymentForm(testPayment))
			{
				newForm.Show();
				newForm.FireSaveButton();

				Assert("Auto printing should be performed", newForm.Test_Allocator.ChequeWasAutoPrinted);
				AssertEquals("Printing called only once", 1, newForm.Test_Allocator.PrintingCalled_Counter);
				AssertEquals("Allocation called only once", 1, newForm.Test_Allocator.AllocationCalled_Counter);
				AssertEquals("PaymentPrinter should have correct Printer passed to", chequeBook.AK_SQ, newForm.Test_Allocator.PrinterPassedForAutoPrinting);
				AssertEquals("Cheque number should be autoallocated on Direct Payment", "3", testPayment.AH_ChequeOrReference);
				Assert("Cheque should be auto printed for the payment", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);

				chequeBook.Reload();
				AssertEquals("CurrentNo should change on the cheque book", 4m, chequeBook.AK_CurrentNo);
				Assert("Out of cheque numbers - Cheque book should become inactive", !chequeBook.AK_IsActive);

				AssertEquals("Should have opened Payment Documents Print Popup", typeof(PaymentDocumentsPrintPopup), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((PaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).Close();
				Assert("The flag on the print manager should be set", newForm.PrintManager_ForTestOnly.ChequeIsAutoPrinted);
			}
		}

		public void TestAutoPrintChequeTwice()
		{
			var creator = new TestObjectCreator(Factory);
			var taxrate = creator.CreateTaxRateWithoutZZ("ABC", "Test Tax Rate", 0);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 3);
			DirectPayment testPayment = creator.CreateDirectPayment(ZDateTime.Now, 100m, 0m, 200m, 0m);
			testPayment.Lines[0].AL_AT = taxrate.PK;
			testPayment.Lines[1].AL_AT = taxrate.PK;
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = chequeBook.PK;
			testPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			testPayment.AH_ExchangeRate = 0.2m;
			testPayment.RunPreSaveValidation();
			Assert("TestPayment should not have any errors before saving", !testPayment.HasErrors);

			Assert("ChequeNumber should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque Number allocation should be enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);

			using (TestDirectPaymentForm newForm = new TestDirectPaymentForm(testPayment))
			{
				newForm.Show();
				newForm.FireSaveButton();

				Assert("Auto printing should be performed", newForm.Test_Allocator.ChequeWasAutoPrinted);
				AssertEquals("Printing called only once", 1, newForm.Test_Allocator.PrintingCalled_Counter);
				AssertEquals("Allocation called only once", 1, newForm.Test_Allocator.AllocationCalled_Counter);
				AssertEquals("PaymentPrinter should have correct Printer passed to", chequeBook.AK_SQ, newForm.Test_Allocator.PrinterPassedForAutoPrinting);
				AssertEquals("Cheque number should be autoallocated on Direct Payment", "3", testPayment.AH_ChequeOrReference);
				Assert("Cheque should be auto printed for the payment", ((IChequeNumberAutoAllocation)testPayment).ChequeIsAutoPrinted);
			}

			var payment = new BusinessObjectFactory().Load<DirectPayment>(testPayment.PK);
			using (TestDirectPaymentForm newForm = new TestDirectPaymentForm(payment))
			{
				testPayment.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "CHQ");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				newForm.FireSaveButton();

				AssertNull("allocator should be null as printing should not be performed", newForm.Test_Allocator);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinterShown()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 1000;
			chequeBook.AK_CurrentNo = 1;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_StartNo = 1;
			book2.AK_LastNo = 1000;
			book2.AK_CurrentNo = 1;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			Factory.Save();

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			using (TestDirectPaymentForm form = new TestDirectPaymentForm(testPayment))
			{
				testPayment.AH_AB = book2.AK_AB;
				testPayment.ChequeBookPK = book2.PK;
				testPayment.AH_ChequeDrawer = "BLah";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				bool containscorrectmessage = false;
				foreach (UnitTestUserNotification.PreviousMessage message in UnitTestUserNotification.Instance.PreviousMessages)
				{
					if (message.Text == AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo))
					{
						containscorrectmessage = true;
					}
				}
				Assert("Messages should contain warning that chequebook uses same printer", containscorrectmessage);
			}
		}

		public void TestMessageIfChequeBookUsesSamePrinterShownTwice()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			StmPrintQueue printer = Factory.New<StmPrintQueue>();
			printer.SQ_QueueName = "Queue Name";
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook.AK_AB = testBank.PK;
			chequeBook.AK_StartNo = 1;
			chequeBook.AK_LastNo = 1000;
			chequeBook.AK_CurrentNo = 1;
			chequeBook.AK_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			chequeBook.AK_Code = "Book1";
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			book2.AK_AB = testBank.PK;
			book2.AK_GB = chequeBook.AK_GB;
			book2.AK_StartNo = 1;
			book2.AK_LastNo = 1000;
			book2.AK_CurrentNo = 1;
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;

			Factory.Save();

			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			using (TestDirectPaymentForm form = new TestDirectPaymentForm(testPayment))
			{
				testPayment.AH_AB = book2.AK_AB;
				testPayment.ChequeBookPK = book2.PK;
				testPayment.AH_ChequeDrawer = "BLah";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.FireSaveButton();
				string warningMessage = AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(printer.SQ_QueueName, chequeBook.AK_CurrentNo);
				Assert("Messages should contain warning that chequebook uses same printer", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(warningMessage));

				testPayment.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "CHQ");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FireSaveButton();
				AssertEquals("Messages should not contain warning that chequebook uses same printer", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestChequeBookIsFullExceptionIsHandled()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			AccChequeBook chequeBook = GetAutoPrintChequeBook(testBank, 1, 3, 3);
			DirectPayment testPayment = Factory.NewWithValidTestData<DirectPayment>();
			testPayment.AH_AB = testBank.PK;
			testPayment.ChequeBookPK = chequeBook.PK;
			testPayment.AH_InvoiceAmount = 100m;
			testPayment.AH_ChequeDrawer = "BLah";
			testPayment.AH_PostDate = ZDateTime.Now;
			testPayment.AH_ExchangeRate = 0.2m;
			testPayment.AH_OSTotalAmount = 500m;
			testPayment.RunPreSaveValidation();
			Assert("TestPayment should not have any errors before saving", !testPayment.HasErrors);

			Assert("ChequeNumber should be empty", testPayment.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque Number allocation should be enabled", ((IChequeNumberAutoAllocation)testPayment).IsAutoAllocationEnabled);

			using (TestDirectPaymentForm newForm = new TestDirectPaymentForm(testPayment))
			{
				newForm.Show();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				newForm.Test_DeactivateChequeBookOnAllocation = ZBool.True;
				newForm.FireSaveButton();
				AssertEquals("There should be a message shown, saying that the Cheque Book is full.", ErrorMessages.ChequeBookIsFullExceptionMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				Assert("The posting form should not be closed as the posting is failed", newForm.Visible);
				Assert("Auto printing should not be performed", !newForm.Test_Allocator.ChequeWasAutoPrinted);
				AssertEquals("Printing is not called", 0, newForm.Test_Allocator.PrintingCalled_Counter);
				AssertEquals("Allocation is not called", 0, newForm.Test_Allocator.AllocationCalled_Counter);
				Assert("Cheque number should remain empty on PaymentApproval", testPayment.AH_ChequeOrReference.IsEmpty);

				AssertNull("Should not have opened Remittance Advice Printing Form", ZFormModaliser.LastFormShownDialogForTest);
				Assert("The flag on the print manager should not be set", !newForm.PrintManager_ForTestOnly.ChequeIsAutoPrinted);
				AssertEquals("CurrentNo should stay the same on the cheque book", 3m, testPayment.ChequeBook.AK_CurrentNo);
				Assert("Cheque book was set to inactive during the allocation and should be reloaded", !testPayment.ChequeBook.AK_IsActive);

				chequeBook.Reload();
				AssertEquals("CurrentNo should stay the same on the cheque book", 3m, chequeBook.AK_CurrentNo);
				Assert("Cheque book was set to inactive during the allocation and should be reloaded", !chequeBook.AK_IsActive);
			}
		}

		#region Implementation

		protected AccChequeBook GetAutoPrintChequeBook(AccBankAccount bankAccount, ZDecimal startNO, ZDecimal lastNO, ZDecimal currentNO)
		{
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_SQ = printQueue.PK;
			chequeBook.AK_StartNo = startNO;
			chequeBook.AK_LastNo = lastNO;
			chequeBook.AK_CurrentNo = currentNO;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		public class TestDirectPaymentForm : DirectPaymentForm
		{
			public TestDirectPaymentForm(DirectPayment directPaymentBizO)
				: base(directPaymentBizO)
			{
			}
		}

		#endregion
	}
}
