using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class ARTransactionsModuleImportedTransactionsTest : TestCaseWithFactory
	{
		public void TestDisplayImportedTransactions_ARInvoice()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.Invoice,
				typeof(ARInvoice), typeof(GUI.InvoiceForm));
		}

		public void TestDisplayImportedTransactions_ARCreditNote()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.CreditNote,
				typeof(ARCreditNote), typeof(GUI.CreditNoteForm));
		}

		public void TestDisplayImportedTransactions_ARAdjustmentNote()
		{
			AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(ZArchitecture.Core.LedgerTypes.AccountsReceivable, ZArchitecture.Core.TransactionTypes.AdjustmentNote,
				typeof(ARAdjustmentNote), typeof(GUI.AdjustmentNoteForm));
		}

		public void TestDisplayImportedTransactions_ARIncorrectType()
		{
			using (ARTransactionModuleStrip module = new ARTransactionModuleStrip())
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(typeof(ARAdjustmentNote));
				invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
				invoice.AH_TransactionType = ZArchitecture.Core.TransactionTypes.Contra;

				module.DisplayImportedTransactionForm_ForTestOnly(invoice);

				ZString expectedErrorMessage = "Only Accounts Receivable Invoices, Credit Notes and Adjustment Notes can be imported from this menu";
				AssertEquals("Error Message", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		void AssertDisplayImportedTransactionsFormIsDisplayedCorrectly(ZString ledger, ZString transactionType,
			Type typeOfNewBusinessObject, Type expectedFormType)
		{
			using (ARTransactionModuleForTest module = new ARTransactionModuleForTest())
			{
				InvoicingBase invoice = (InvoicingBase)Factory.New(typeOfNewBusinessObject);
				invoice.AH_Ledger = ledger;
				invoice.AH_TransactionType = transactionType;
				module.DisplayImportedTransactionForm_ForTestOnly(invoice);
				AssertEquals("Last Shown Form Type", expectedFormType, module.LastDisplayedFormType);
			}
		}

		class ARTransactionModuleForTest : ARTransactionModuleStrip
		{
			protected override void ShowImportedDataForm(InvoicingBaseController controller, InvoicingBase importedTransaction)
			{
				using (IZForm form = controller.ShowImportedDataForm(importedTransaction))
				{
					fLastDisplayedFormType = form.GetType();
				}
			}

			public Type LastDisplayedFormType
			{
				get { return fLastDisplayedFormType; }
			}

			Type fLastDisplayedFormType;
		}

		public void TestSupportsWorkflow()
		{
			using (ARTransactionModuleStrip module = new ARTransactionModuleStrip())
			{
				AssertEquals("SupportsWorkflow", true, module.SupportsWorkflow);
			}
		}
	}
}
