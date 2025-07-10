using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Accounting.GUI.ARAP.Payment.PaymentDocumentsPrintPopup;
using static Enterprise.Accounting.GUI.ARAP.ReceiptPayment.PaymentBatchPrintManager;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment.Testing
{
	public class PaymentBatchPrintManagerTest : TestCaseWithFactory
	{
		public void TestPrintOptions()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			TestPaymentPrint.Reset();
			TestPrintManager.Print();

			Assert(TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrintForm.Visible);

			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPaymentPrint.Reset();
			TestPrintManager.Print();

			Assert(!TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrintForm.Visible);

			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			TestPaymentPrint.Reset();
			TestPrintManager.Print();

			Assert(!TestPaymentPrint.IsPaymentVoucherPrinted);
			Assert(!TestPaymentPrint.IsRemittanceAdvicePrinted);
			Assert(!TestPaymentPrint.IsChequePrinted);
			Assert(!TestPaymentPrintForm.Visible);
		}

		public void TestCheckIfAllChequesAreAutoPrintedAlready()
		{
			Assert("Should return False as the collection is empty", !TestPrintManager.CheckIfAllChequesAreAutoPrintedAlready_ForTestOnly(TestPrintManager.PaymentCollection_ForTestOnly));

			var paymentCollection = new List<TransactionHeader>();
			APPayment payment1 = Factory.NewWithValidTestData<APPayment>();
			((IChequeNumberAutoAllocation)payment1).ChequeIsAutoPrinted = ZBool.False;
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			((IChequeNumberAutoAllocation)payment2).ChequeIsAutoPrinted = ZBool.True;
			paymentCollection.Add(payment1);
			paymentCollection.Add(payment2);
			TestPrintManager = new TestPaymentPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
			Assert("Should return False as one of the payments is not printed", !TestPrintManager.CheckIfAllChequesAreAutoPrintedAlready_ForTestOnly(TestPrintManager.PaymentCollection_ForTestOnly));

			paymentCollection = new List<TransactionHeader>();
			payment1 = Factory.NewWithValidTestData<APPayment>();
			((IChequeNumberAutoAllocation)payment1).ChequeIsAutoPrinted = ZBool.True;
			payment2 = Factory.NewWithValidTestData<APPayment>();
			((IChequeNumberAutoAllocation)payment2).ChequeIsAutoPrinted = ZBool.True;
			paymentCollection.Add(payment1);
			paymentCollection.Add(payment2);
			TestPrintManager = new TestPaymentPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
			Assert("Should return True as both of payments are printed", TestPrintManager.CheckIfAllChequesAreAutoPrintedAlready_ForTestOnly(TestPrintManager.PaymentCollection_ForTestOnly));
		}

		public void TestPrintPaymentDocuments()
		{
			using (TestPaymentPrintParentForm testForm = new TestPaymentPrintParentForm())
			{
				try
				{
					testForm.Show();
					AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					TestPrintManager.Print();

					AssertNotNull("Modaliser should close last form", ZFormModaliser.LastFormShownDialogForTest);
					Assert("Should have shown Payment Documents Print Form", typeof(PaymentDocumentsPrintPopup).IsAssignableFrom(ZFormModaliser.LastFormShownDialogForTest.GetType()));
				}
				finally
				{
					TestPrintManager.PaymentPrintForm_ForTestOnly.Close();
				}
			}
		}

		public void TestPrint_RemittanceAdvicePrintForm_NotDisposed()
		{
			var paymentCollection = new List<TransactionHeader>();
			var testPrintManager = new TestPaymentPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, Factory);
			ZFormModaliser.ShowDialogsInTest = true;

			AssertNoExceptionThrown(() =>
				{
					using (var blankForm = new ZForm())
					{
						blankForm.Show();
						testPrintManager.PaymentPrintForm_Exposed.Dispose();
						testPrintManager.Print();
					}
				});
		}

		public void TestItemsPrintedWhenFormClosedWithOptionsSelected()
		{
			using (TestPaymentPrintParentForm testForm = new TestPaymentPrintParentForm())
			{
				testForm.Show();
				AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				TestPrintManager.Print();
				Assert("Should have printed remittance advice", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
				Assert("Should not have printed Cheque", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
				Assert("Should not have printed payment voucher", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
			}
		}

		public void TestNoItemsPrintedWhenFormClosedWithCancel()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			TestPrintManager.Print();
			((MockPaymentDocumentsPrintPopup)ZFormModaliser.LastFormShownDialogForTest).ExecuteCancelButton();

			Assert("Should have printed remittance advice", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
			Assert("Should not have printed Cheque", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
			Assert("Should not have printed payment voucher", !((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
		}

		public void TestItemsPrintedWithMultipleItemsSelected()
		{
			AccountingConfigurationRegistry.Instance.ShowPaymentRemittancePrintDialogue.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintRemittanceAdvice.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintCheque.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.PrintPaymentVoucher.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			TestPrintManager.Print();
			Assert("Should have printed remittance advice", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsRemittanceAdvicePrinted);
			Assert("Should have printed Cheque", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsChequePrinted);
			Assert("Should have printed payment voucher", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsPaymentVoucherPrinted);
		}

		public void TestGetIPaymentBatchPrint()
		{
			var paymentCollection = new List<TransactionHeader>();
			PaymentBatchPrintManager testPrintManager = new PaymentBatchPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, Factory);
			AssertEquals("GetIPaymentBatchPrint should return object of the valid type", typeof(PaymentDocumentsPrinter), testPrintManager.GetIPaymentBatchPrint_ForTestOnly().GetType());
		}

		public void TestAutoPrintCheques()
		{
			var paymentCollection = new List<TransactionHeader>();
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			paymentCollection.Add(payment);
			paymentCollection.Add(payment2);
			ZGuid fakeGUIAsPrinter = ZGuid.NewZGuid();
			TestPrintManager = new TestPaymentPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
			Assert("Flag is not set yet", !TestPrintManager.ChequeIsAutoPrinted);
			TestPrintManager.PrintChequesDirectly(fakeGUIAsPrinter);
			Assert("Flag should be set", TestPrintManager.ChequeIsAutoPrinted);
			Assert("Flag should be set on PaymentPrinter", ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).IsAutoChequePrinted);
			AssertEquals("PaymentPrinter should have correct Payment passed to", paymentCollection, ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).PaymentCollectionPassedForAutoPrinting);
			AssertEquals("PaymentPrinter should have correct Printer passed to", fakeGUIAsPrinter, ((MockPaymentPrint)TestPrintManager.PaymentPrinter_ForTestOnly).PrinterPKPassedForAutoPrinting);
		}

		bool PrintChequeOriginal;
		bool PrintRemittanceAdviceOriginal;
		bool PrintPaymentVoucherOriginal;

		protected MockPaymentPrint TestPaymentPrint;
		protected MockPaymentDocumentsPrintPopup TestPaymentPrintForm;
		protected PaymentBatchPrintManager TestPrintManager;

		protected override void SetUp()
		{
			base.SetUp();

			TestPaymentPrint = new MockPaymentPrint();
			TestPaymentPrintForm = new MockPaymentDocumentsPrintPopup();
			var paymentCollection = new List<TransactionHeader>();
			TestPrintManager = new TestPaymentPrintManager(paymentCollection, null, ZArchitecture.Core.TransactionTypes.Payment, new BusinessObjectFactory());
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
