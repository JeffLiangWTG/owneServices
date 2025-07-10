using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class APTransactionsModuleImportedTransactionsTest : TestCaseWithFactory
	{
		public void TestDisplayImportedTransactions_APInvoice()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(LedgerTypes.AccountsPayable, TransactionTypes.Invoice, typeof(APInvoice), typeof(InvoiceForm));
		}

		public void TestDisplayImportedTransactions_APCreditNote()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote, typeof(APCreditNote), typeof(CreditNoteForm));
		}

		public void TestDisplayImportedTransactions_APAdjustmentNote()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote, typeof(APAdjustmentNote), typeof(AdjustmentNoteForm));
		}

		public void TestDisplayImportedTransactions_APIncorrectType()
		{
			using (APTransactionModuleStrip module = new APTransactionModuleStrip())
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(APAdjustmentNote));
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Contra;

				module.DisplayImportedTransactionForm_ForTestOnly(invoice);

				ZString expectedErrorMessage = "Only Accounts Payable Invoices, Credit Notes and Adjustment Notes can be imported from this menu";
				AssertEquals("Error Message", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDisplayImportedTransactions_APCashInvoice()
		{
			using (APTransactionModuleForTest module = new APTransactionModuleForTest())
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(APInvoice));
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_TransactionType = TransactionTypes.Invoice;
				invoice.IsImportedFromFile = true;
				invoice.SubmittedFromInvoicingForm = true;
				((Invoice)invoice).IsInvoiceReceiptPayment = true;

				module.DisplayImportedTransactionForm_ForTestOnly(invoice);

				Application.DoEvents();

				InvoiceForm invoiceForm = null;

				if (module.LastDisplayedForm is InvoiceForm)
				{
					invoiceForm = module.LastDisplayedForm as InvoiceForm;
				}

				AssertEquals("CashInvoiceOnCheckbox should be checked if IsInvoiceReceiptPayment is true and not UAInvoice", true, invoiceForm.CashInvoiceOnCheckbox.Checked);
				AssertEquals("ReceiptPaymentPanel should be visible if IsInvoiceReceiptPayment is true and not UAInvoice", true, invoiceForm.ReceiptPaymentOuterPanel.Visible);
			}
		}

		void AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(ZString ledger, ZString transactionType, Type typeOfNewBusinessObject, Type expectedFormType)
		{
			using (APTransactionModuleForTest module = new APTransactionModuleForTest())
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(typeOfNewBusinessObject);
				invoice.AH_Ledger = ledger;
				invoice.AH_TransactionType = transactionType;
				module.DisplayImportedTransactionForm_ForTestOnly(invoice);
				AssertEquals("Last Shown Form Type", expectedFormType, module.LastDisplayedFormType);
			}
		}

		class APTransactionModuleForTest : APTransactionModuleStrip
		{
			protected override void ShowImportedDataForm(InvoicingBaseController controller, InvoicingBase importedTransaction)
			{
				using (IZForm form = controller.ShowImportedDataForm(importedTransaction))
				{
					fLastDisplayedFormType = form.GetType();
					fLastDisplayedForm = form;
				}
			}

			public IZForm LastDisplayedForm => fLastDisplayedForm;

			public Type LastDisplayedFormType => fLastDisplayedFormType;

			IZForm fLastDisplayedForm;
			Type fLastDisplayedFormType;
		}
	}
}
