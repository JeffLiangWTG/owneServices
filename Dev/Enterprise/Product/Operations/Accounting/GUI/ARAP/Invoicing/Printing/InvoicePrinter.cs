using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI
{
	public enum TransactionPrintingResults
	{
		None, TransactionIsClassAInvoice, TransactionIsNotClassAInvoice, TransactionIsAPPayment
	}

	public partial class InvoicePrinter
	{
		public TransactionPrintingResults PrintTransaction(TransactionHeader transaction, Form parentForm, InvoicePrintContext context, bool showTransactionNotInDBWarning = false)
		{
			var transactionResult = TransactionPrintingResults.None;
			APPayment apPayment;
			APInvoice apInvoice = transaction as APInvoice;

			if (transaction.IsInDatabase)
			{
				if (transaction is ARInvoice || transaction is ARCreditNote || transaction is ARAdjustmentNote)
				{
					var invoicingBase = (InvoicingBase)transaction;
					var canPrint = invoicingBase.CheckCanPrintPostedInvoicingBase();
					if (!canPrint.Result)
					{
						Globals.Message.ShowError(Res.GetString("a0163e3d-5bb6-4976-b49f-644f7fc2ade5", @"Cannot print {0} {1} {2}.
Reason: {3}"
							, invoicingBase.AH_Ledger
							, invoicingBase.AH_TransactionType
							, invoicingBase.AH_TransactionNum
							, canPrint.ReasonForNotBeingAbleToPrint));
					}
					else
					{
						transactionResult = PrintARTransaction(parentForm, context, invoicingBase);
					}
				}
				else if (transaction.AH_Ledger != LedgerTypes.UnapprovedPayableTransactions && (apInvoice != null || transaction is APCreditNote || transaction is APAdjustmentNote))
				{
					if (transaction.IsSelfBillingInvoice)
					{
						PrintSelfBillingInvoice(transaction);
					}
					if (AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.Value)
					{
						PrintCostConfirmationDocument(transaction);
					}
					if (((InvoicingBase)transaction).SupportPrintAutofatturaDocument() && AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.Value)
					{
						PrintAutofatturaDocument(transaction);
					}
				}
				else if (transaction.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
				{
					if (AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.Value)
					{
						PrintCostConfirmationDocument(transaction);
					}
					if (((InvoicingBase)transaction).SupportPrintAutofatturaDocument() && AccountingConfigurationRegistry.Instance.PrintAutofatturaItalyDocument.Value)
					{
						PrintAutofatturaDocument(transaction);
					}
				}
				else if ((apPayment = transaction as APPayment) != null)
				{
					PrintRemittanceAdvice(apPayment, parentForm);
					transactionResult = TransactionPrintingResults.TransactionIsAPPayment;
				}
			}
			else
			{
				if (context == InvoicePrintContext.PostFromBilling && apInvoice != null)
				{
					InvoicePrintHelper.PrintCostConfirmationDocument(transaction.Factory, true, transaction);
				}
				else if (showTransactionNotInDBWarning)
				{
					Globals.Message.ShowWarning(Res.GetString("0C51377E-2011-4721-B654-8F639F4C9075", "Attempt to print document for unsaved transaction"));
				}
				else
				{
					string message = (NoResString)"Attempt to print document for unsaved transaction"; // Error message reported to Developer
					ExceptionReporter.Instance.ReportDeveloperException(message, message, new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
				}
			}

			return transactionResult;
		}

		TransactionPrintingResults PrintARTransaction(Form parentForm, InvoicePrintContext context, InvoicingBase arTransaction)
		{
			TransactionPrintingResults transactionResult = TransactionPrintingResults.None;

			string caption = GetCaption(arTransaction);
			string transactionType =
				arTransaction.AH_TransactionType == TransactionTypes.AdjustmentNote ?
					Res.GetString("1D160F14-DD95-43d3-9C0E-6C896F1FD03B", "adjustment note") :
				arTransaction.AH_TransactionType == TransactionTypes.CreditNote ?
					Res.GetString("cbf442d3-f27b-4615-bcbd-b29b2fc08d68", "credit note") :
					Res.GetString("3caa335a-d863-4456-a9ca-e1a7632b4e04", "invoice");
			string message = Res.GetString("822b9e17-8171-44f8-9a94-d05bf6ce9b94", "Do you want to print {0} {1}?", transactionType, arTransaction.InvoiceNumber);

			string invoicePrintingOption = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;
			if (IsGovtTaxInvoicePrintingUsed && arTransaction.IsGovtTaxInvoice && invoicePrintingOption != GovtTaxInvoicePrintTask.EnterpriseInvoice)
			{
				PrintClassAInvoiceBasedOnCountry(arTransaction, caption, message, parentForm, context, invoicePrintingOption);
				transactionResult = TransactionPrintingResults.TransactionIsClassAInvoice;
			}
			else // show normal dialog
			{
				caption = Res.GetString("Accounting|JobInvoicingPrinter|PrintARInvoice", "Print AR Invoice");
				PrintNormalInvoice(arTransaction, message, caption, context);
				transactionResult = TransactionPrintingResults.TransactionIsNotClassAInvoice;
			}

			return transactionResult;
		}

		protected virtual bool IsGovtTaxInvoicePrintingUsed
		{
			get { return true; }
		}

		protected string GetCaption(InvoicingBase aRTransaction)
		{
			return aRTransaction.IsGovtTaxInvoice ? Res.GetString("Accounting|JobInvoicingPrinter|PrintGovtTaxInvoice", "Print Govt Tax Invoice") : Res.GetString("Accounting|JobInvoicingPrinter|PrintARInvoice", "Print AR Invoice");
		}

		#region ClassA Invoice

		void PrintClassAInvoiceBasedOnCountry(InvoicingBase aRTransaction, string caption, string message, Form parentForm, InvoicePrintContext context, string invoicePrintingOption)
		{
			PrintClassAInvoice(aRTransaction, caption, message, parentForm, context, invoicePrintingOption);
		}

		void PrintClassAInvoice(InvoicingBase aRTransaction, string caption, string message, Form parentForm, InvoicePrintContext context, string invoicePrintingOption)
		{
			DialogResult result = GetUserChoice(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if (result == DialogResult.Yes)
			{
				PrintClassAInvoiceCore(aRTransaction, context, invoicePrintingOption);
			}
		}

		void PrintClassAInvoiceCore(InvoicingBase aRTransaction, InvoicePrintContext context, string invoicePrintingOption)
		{
			new GovtTaxInvoicePrinter(invoicePrintingOption).PrintGovtTaxInvoices(new TransactionHeader[] { aRTransaction });
		}

		#endregion

		void PrintNormalInvoice(InvoicingBase aRTransaction, string message, string caption, InvoicePrintContext context)
		{
			DialogResult result = GetUserChoice(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
			{
				try
				{
					using (InvoicePrintTask printTask = NewTask(aRTransaction, context))
					{
						if (printTask.TaskCount == 0)
						{
							ErrorReporter.ReportOnce("The user requested printing of an item, but no print tasks were generated. Check that the post manager posts before printing.");
						}
						else
						{
							RunPrint(printTask);
						}
					}
				}
				catch (Exception ex) when (ex is UnableToFindInvoiceDocumentCommandException)
				{
					Globals.Message.ShowError(ex.Message);
				}
				catch (Exception ex) when (ex is ReprintingInvoiceException)
				{
					Globals.Message.ShowError(AccountingConstants.ReprintingInvoiceMessage);
				}
				catch (Exception ex) when (ex is ComplianceSequenceHasNoDocumentMenuDefinedException sequenceHasNoDocumentMenuDefinedException)
				{
					Globals.Message.ShowError(sequenceHasNoDocumentMenuDefinedException.UserFriendlyMessage);
				}
			}
		}

		void PrintSelfBillingInvoice(TransactionHeader selfBillingInvoice)
		{
			ZString invoiceOrCreditNote = selfBillingInvoice is APAdjustmentNote ? Res.GetString("5154478D-1401-42ef-A477-FCDC01645112", "Adjustment Note") : (selfBillingInvoice is APCreditNote ? Res.GetString("D9FF156A-22DF-4231-A554-C496C6C84341", "Credit Note") : Res.GetString("d66f4691-3d51-4fd7-99cc-86ee5a4d7efd", "Invoice"));
			ZString question = Res.GetString("2ddf00b6-ecd8-4d48-9cf8-7d45b5724319", "Do you want to print Self Billing {0} {1}?", invoiceOrCreditNote, selfBillingInvoice.AH_TransactionNum);
			DialogResult result = Globals.Message.Show(question, Res.GetString("4a421c58-12d3-44c5-98b5-ea6166183579", "Print Self Billing Invoice"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			if (result == DialogResult.Yes)
			{
				InvoicePrintHelper.PrintSelfBillingInvoice(selfBillingInvoice);
			}
		}

		void PrintCostConfirmationDocument(TransactionHeader transaction)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("00d72253-612b-4fc9-a0c2-857e3d4be24b", "Do you want to print a Cost Confirmation Document for this transaction?"), Res.GetString("0281b346-3ec6-42fd-8f80-8e78b79ffbdb", "Print Cost Confirmation Document"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			if (result == DialogResult.Yes)
			{
				InvoicePrintHelper.PrintCostConfirmationDocument(transaction);
			}
		}

		void PrintAutofatturaDocument(TransactionHeader transaction)
		{
			DialogResult result = Globals.Message.Show(Res.GetString("2CB1FDCE-B138-4152-9374-3D6CCEFA43FA", "Do you want to print the Autofattura document for this transaction?"), Res.GetString("E69758E1-0CB9-4F92-9647-5DF7A249EA96", "Print Autofattura Document"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
			if (result == DialogResult.Yes)
			{
				InvoicePrintHelper.PrintITAutoFattura(transaction);
			}
		}

		protected void PrintRemittanceAdvice(APPayment transaction, Form parentForm)
		{
			PaymentPrintManager printManager = new PaymentPrintManager(transaction.PK.ToGuid(), TransactionTypes.Payment, new BusinessObjectFactory());
			if (((IChequeNumberAutoAllocation)transaction).ChequeIsAutoPrinted)
			{
				printManager.SetChequeIsAutoPrinted();
#if DEBUG
				ChequeIsAutoPrintedFlagWasSetOnPrintManager = ZBool.True;
#endif
			}
			printManager.Print();
		}

		protected virtual void RunPrint(InvoicePrintTask printTask)
		{
			printTask.Run();
		}

		protected virtual DialogResult GetUserChoice(string message, string caption, MessageBoxButtons messageBoxButton, MessageBoxIcon messageBoxIcon)
		{
			return Globals.Message.Show(message, caption, messageBoxButton, messageBoxIcon);
		}

		protected virtual InvoicePrintTask NewTask(InvoicingBase arTransaction, InvoicePrintContext context)
		{
			var onlyPrintCommonInvoiceforVN = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.VietNam && AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value == GovtTaxInvoicePrintTask.EnterpriseInvoice && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

			return arTransaction.IsGovtTaxInvoice && !onlyPrintCommonInvoiceforVN ? new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(arTransaction.PK) { Context = context }) : new InvoicePrintTask(new InvoicePrintTask.Configuration(arTransaction.PK));
		}

		#region Test Case
#if DEBUG

		public ZBool ChequeIsAutoPrintedFlagWasSetOnPrintManager;

#endif
		#endregion
	}
}
