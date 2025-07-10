using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Accounting.GUI.ARAP.Payment.PaymentDocumentsPrintPopup;
using static Enterprise.Accounting.GUI.ARAP.ReceiptPayment.PaymentPrintManager;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment.Testing
{
	public class PaymentPrintManagerTest : TestCaseWithFactory
	{
		public void TestPrintOptions()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestPaymentPrint.Reset();
			TestPrintManager.PaymentPrintOption_ForTestOnly = PaymentPrintOptions.None;
			TestPrintManager.Print();

			Assert(TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrintForm.Visible);

			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPaymentPrint.Reset();
			TestPrintManager.PaymentPrintOption_ForTestOnly = PaymentPrintOptions.None;
			TestPrintManager.Print();

			Assert(!TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrintForm.Visible);

			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPaymentPrint.Reset();
			TestPrintManager.PaymentPrintOption_ForTestOnly = PaymentPrintOptions.None;
			TestPrintManager.Print();

			Assert(!TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(!TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrintForm.Visible);
		}

		public void TestPrintRemittanceAdvice()
		{
			using (TestPaymentPrintParentForm testForm = new TestPaymentPrintParentForm())
			{
				try
				{
					testForm.Show();
					AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					TestPrintManager.Print();

					AssertNotNull("Modaliser should remember last form", ZFormModaliser.LastFormShownDialogForTest);
					Assert("Should have shown Payment Documents Print Popup", typeof(PaymentDocumentsPrintPopup).IsAssignableFrom(ZFormModaliser.LastFormShownDialogForTest.GetType()));
				}
				finally
				{
					TestPrintManager.LastPrintForm.Close();
				}
			}
		}

		public void TestPrintingMultipleTimes()
		{
			AssertNoExceptionThrown(delegate
			{
				using (ZForm parentForm = new ZForm())
				{
					TestPrintManager.Print();
					TestPrintManager.LastPrintForm.Close();
					TestPrintManager.Print();
					TestPrintManager.LastPrintForm.Close();
				}
				TestPrintManager.Print();
				TestPrintManager.LastPrintForm.Close();
				TestPrintManager.Print();
				TestPrintManager.LastPrintForm.Close();
			});
		}

		public void TestItemsPrintedWhenFormClosedWithOptionsSelected()
		{
			using (TestPaymentPrintParentForm testForm = new TestPaymentPrintParentForm())
			{
				testForm.Show();
				AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				using (ZFormModaliser.SuspendDispose())
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestPrintManager.Print();
					Assert("Should have printed remittance advice", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
					Assert("Should not have printed Cheque", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
					Assert("Should not have printed payment voucher", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
				}
			}
		}

		public void TestNoItemsPrintedWhenFormClosedWithCancel()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			using (ZFormModaliser.SuspendDispose())
			{
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				TestPrintManager.Print();
				Assert("Should not have printed remittance advice", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
				Assert("Should not have printed Cheque", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
				Assert("Should not have printed payment voucher", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
			}
		}

		public void TestItemsPrintedWithMultipleItemsSelected()
		{
			using (TestPaymentPrintParentForm testForm = new TestPaymentPrintParentForm())
			{
				testForm.Show();
				AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				using (ZFormModaliser.SuspendDispose())
				{
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestPrintManager.Print();
					Assert("Should have printed remittance advice", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
					Assert("Should have printed Cheque", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
					Assert("Should have printed payment voucher", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
				}
			}
		}

		public void TestAutoPrintCheque()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			ZGuid fakeGUIAsPrinter = ZGuid.NewZGuid();
			TestPrintManager = new TestPaymentPrintManager(payment.PK.ToGuid(), ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
			Assert("Flag is not set yet", !TestPrintManager.ChequeIsAutoPrinted);
			TestPrintManager.AutoPrintCheque(fakeGUIAsPrinter);
			Assert("Flag should be set", TestPrintManager.ChequeIsAutoPrinted);
			Assert("Flag should be set on PaymentPrinter", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequeAutoPrinted);
			AssertEquals("PaymentPrinter should have correct Payment passed to", payment.PK, ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).PaymentPKPassedForAutoPrinting);
			AssertEquals("PaymentPrinter should have correct Printer passed to", fakeGUIAsPrinter, ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).PrinterPKPassedForAutoPrinting);
		}

		public void TestChequeIsAutoPrinted()
		{
			Assert("Flag is not set yet", !TestPrintManager.ChequeIsAutoPrinted);
			TestPrintManager.ChequeIsAutoPrinted = ZBool.True;
			Assert("Flag should be set", TestPrintManager.ChequeIsAutoPrinted);
		}

		public void TestPaymentPrintingFormShownWhenInvoicePostedUsingSaveAndCloseButton()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.SetupPeriods();

			APInvoice apInv = Factory.NewWithValidTestData<APInvoice>();

			APInvoiceLine line1 = (APInvoiceLine)apInv.Lines.AddNew();
			line1.AL_OSExTaxAmount = 100M;
			line1.GenericCharge = Factory.LoadTop1<Business.GenericCharge.GenericCharge>(line1.ChargeList.CompleteFilter).PK;
			line1.AL_AT = testObjectCreator.GST1.PK;

			using (InvoiceForm form = new InvoiceForm(apInv))
			{
				form.Show();

				apInv.AH_OH = testObjectCreator.AALSHI.PK;
				apInv.SubmittedFromInvoicingForm = true;
				apInv.IsInvoiceReceiptPayment = true;
				apInv.AH_Desc = "Description";
				apInv.ReceiptPaymentAH_AB = testObjectCreator.AUDBankAccount.PK;
				apInv.ReceiptPaymentAH_ChequeDrawer = "Cheque Drawer";
				apInv.ReceiptPaymentAH_ChequeOrReference = "1";
				apInv.ReceiptPaymentAH_DrawerBank = "NAB";
				apInv.ReceiptPaymentAH_DrawerBranch = "Branch";
				apInv.ReceiptPaymentAH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
				apInv.ReceiptPaymentAK_AB = testObjectCreator.AUDChequeBook.PK;

				form.PostingButtonsUserControl.SaveAndCloseButton.PerformClick();

				using (var popup = ZFormModaliser.LastFormShownDialogForTest)
				{
					Assert("Payment Documents Print Popup should be instantiated", popup is PaymentDocumentsPrintPopup);
				}
			}
		}

		bool PrintChequeOriginal;
		bool PrintRemittanceAdviceOriginal;
		bool PrintPaymentVoucherOriginal;

		protected MockPaymentPrint TestPaymentPrint;
		protected MockPaymentDocumentsPrintPopup TestPaymentPrintForm;
		protected TestPaymentPrintManager TestPrintManager;

		protected override void SetUp()
		{
			base.SetUp();

			TestPaymentPrint = new MockPaymentPrint();
			TestPaymentPrintForm = new MockPaymentDocumentsPrintPopup();
			TestPrintManager = new TestPaymentPrintManager(Guid.Empty, ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
			TestPrintManager.PaymentPrinterField_ForTestOnly = TestPaymentPrint;

			PrintChequeOriginal = AccountingConfigurationRegistry.Instance.PrintCheque.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			PrintRemittanceAdviceOriginal = AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
			PrintPaymentVoucherOriginal = AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK);
		}

		protected override void TearDown()
		{
			base.TearDown();

			ResetOriginalRegistryValues();
			TestPaymentPrintForm.Dispose();
		}

		void ResetOriginalRegistryValues()
		{
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintChequeOriginal);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintRemittanceAdviceOriginal);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, PrintPaymentVoucherOriginal);
		}
	}
}
