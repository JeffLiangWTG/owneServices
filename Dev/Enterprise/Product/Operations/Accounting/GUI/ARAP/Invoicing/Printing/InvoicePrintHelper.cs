using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public static class InvoicePrintHelper
	{
		public static string CannotPrintAsASelfBilledTransactionMessage
		{
			get { return Res.GetString("13e4bcb8-3c4e-475f-8ee5-cde4ce96a348", "This transaction was not posted as a self billing transaction. Please print the Cost Confirmation Document instead."); }
		}

		public static void PrintMatchingReport(TransactionHeader transactionToPrint)
		{
			if (transactionToPrint != null)
			{
				if (transactionToPrint.Header != null && !transactionToPrint.Header.OH_IsActive)
				{
					Globals.Message.ShowInformation(Res.GetString("6875b362-f058-4e53-9f0f-5bbcbb7a38ea", "The match report for this transaction cannot be printed because the organization is inactive"), Res.GetString("5e144519-1ff3-4ef1-b8f0-553719435a27", "Print Match Report"));
				}
				else
				{
					ReceiptPrint printHandler = new ReceiptPrint();
					printHandler.PrintReceiptMatchingReport(transactionToPrint, AccountingUtils.AccountingDocumentTitles.ReceiptMatching);
				}
			}
		}

		public static void PrintCostConfirmationDocument(params TransactionHeader[] transactionToPrint)
		{
			PrintCostConfirmationDocument(null, false, transactionToPrint);
		}

		public static void PrintCostConfirmationDocument(BusinessObjectFactory factory, bool isPreviewOnly, params TransactionHeader[] transactionToPrint)
		{
			CostConfirmationDocTypeBizo docTypeBizo = new CostConfirmationDocTypeBizo();
			CostConfirmationDocTypePopupForm docTypePopupForm = new CostConfirmationDocTypePopupForm(docTypeBizo);
			if (ZFormModaliser.ShowDialogAndDispose(docTypePopupForm) == DialogResult.OK)
			{
				List<ZString> costConfirmationTypeForPrinting = new List<ZString>();
				if (docTypeBizo.CostConfirmationDocType == AccountingConstants.CostConfirmationDocumentSettingsCodes.Summary ||
					docTypeBizo.CostConfirmationDocType == AccountingConstants.CostConfirmationDocumentSettingsCodes.Both)
				{
					costConfirmationTypeForPrinting.Add(JobInvoicingEDocsProviderSupporter.CostConfirmationSummary);
				}
				if (docTypeBizo.CostConfirmationDocType == AccountingConstants.CostConfirmationDocumentSettingsCodes.Detail ||
					docTypeBizo.CostConfirmationDocType == AccountingConstants.CostConfirmationDocumentSettingsCodes.Both)
				{
					costConfirmationTypeForPrinting.Add(JobInvoicingEDocsProviderSupporter.CostConfirmationDocument);
				}

				PrintTransaction(isPreviewOnly: isPreviewOnly, transactionToPrint: transactionToPrint, menuNames: costConfirmationTypeForPrinting.ToArray(),
					applicabilityForPrintChecker: transactionHeaders => transactionHeaders.Any(transactionHeader => transactionHeader != null && new List<string> { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote, TransactionTypes.UAInvoice, TransactionTypes.UACreditNote }.Contains(transactionHeader.AH_TransactionType)), errorMessage: null,
					factory: factory,
					isLegacyDocument: !Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.UseNewDocBuilderCostConfirmationDocument.Value);
			}
		}

		public static void PrintSelfBillingInvoice(params TransactionHeader[] transactionToPrint)
		{
			PrintTransaction(isPreviewOnly: false, transactionToPrint: transactionToPrint, menuNames: new ZString[] { JobInvoicingEDocsProviderSupporter.SelfBillingInvoice },
				applicabilityForPrintChecker: transactionHeaders => transactionHeaders.Any(transactionHeader => transactionHeader != null && transactionHeader.AH_TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling), errorMessage: CannotPrintAsASelfBilledTransactionMessage, factory: null);
		}

		public static void PrintITAutoFattura(params TransactionHeader[] transactionToPrint)
		{
			PrintTransaction(isPreviewOnly: false, transactionToPrint: transactionToPrint, menuNames: new ZString[] { JobInvoicingEDocsProviderSupporter.ITAutofattura },
				applicabilityForPrintChecker: transactionHeaders => transactionHeaders.Any(transactionHeader => transactionHeader != null && new List<string> { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote, TransactionTypes.UAInvoice, TransactionTypes.UACreditNote }.Contains(transactionHeader.AH_TransactionType)), errorMessage: null,
				factory: null,
				isLegacyDocument: false);
		}

		public static void HandleNotImplemented()
		{
			HandleNotImplemented(null);
		}

		public static IEnumerable<TransactionHeader> GetEligibleForPrintingTransactions(IEnumerable<TransactionHeader> transactionHeaders)
		{
			var result = Enumerable.Empty<TransactionHeader>();

			var invoicesThatCanNotBePrintedAndMessage = transactionHeaders?.OfType<InvoicingBase>().GetInvoicingBasesThatCanNotBePrintedDueToCompliance()
				?? new AccountingExtension.InvoicesAndMessage(Enumerable.Empty<InvoicingBase>(), string.Empty);
			var invoicesThatCanBePrinted = transactionHeaders?.Except(invoicesThatCanNotBePrintedAndMessage.Invoices)
				?? Enumerable.Empty<TransactionHeader>();

			if (!invoicesThatCanBePrinted.Any())
			{
				Globals.Message.ShowError(!string.IsNullOrEmpty(invoicesThatCanNotBePrintedAndMessage.Message) ? invoicesThatCanNotBePrintedAndMessage.Message : InvoicePrintHelper.DefaultErrorMessageIfThereIsNothingForPrinting);
			}
			else if (invoicesThatCanNotBePrintedAndMessage.Invoices.Any())
			{
				if (InvoicePrintHelper.AskForConfirmationToContinuePrintingOfEligibeTransactionsOnly(invoicesThatCanNotBePrintedAndMessage.Message) == DialogResult.Yes)
				{
					result = invoicesThatCanBePrinted;
				}
			}
			else
			{
				result = transactionHeaders;
			}

			return result;
		}

		static string DefaultErrorMessageIfThereIsNothingForPrinting => Res.GetString("7cd37231-c2e8-44e4-b90a-4192fb074ded", "There is no eligible transaction for printing");

		static DialogResult AskForConfirmationToContinuePrintingOfEligibeTransactionsOnly(string cannotPrintNotEligibleTransctionsMessage)
		{
			var errMsgBuilder = new ZStringBuilder(cannotPrintNotEligibleTransctionsMessage);
			errMsgBuilder.AppendLine(Res.GetString("62304510-303b-4eb2-9367-c971efad6579", "Do you want to print rest of the transactions?"));
			return Globals.Message.Show(errMsgBuilder.ToStringWithNewLineBetweenAppends(), Res.GetString("731a9e67-53df-4f23-8eef-8d16bbc2fa25", "Continue Printing"), MessageBoxButtons.YesNo, DialogResult.Yes);
		}

		#region Implementation

		delegate bool TransactionHeaderApplicabilityForPrintChecker(TransactionHeader[] transactionHeader);

		static void PrintTransaction(bool isPreviewOnly, TransactionHeader[] transactionToPrint, ZString[] menuNames, TransactionHeaderApplicabilityForPrintChecker applicabilityForPrintChecker, string errorMessage, BusinessObjectFactory factory, bool isLegacyDocument = false)
		{
			if (transactionToPrint != null)
			{
				if (transactionToPrint.Any(transactionHeader => transactionHeader != null && transactionHeader.Header != null) &&
					transactionToPrint.Any(transactionHeader => transactionHeader != null && transactionHeader.Header.OH_IsActive))
				{
					if (applicabilityForPrintChecker(transactionToPrint))
					{
						using (var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(transactionToPrint) { MenuNames = menuNames, IsLegacyDocument = isLegacyDocument, Factory = factory }))
						{
							printTask.Run(!isPreviewOnly);
							OnPrintTaskRun(printTask);
						}
					}
					else
					{
						HandleNotImplemented(errorMessage);
					}
				}
			}
		}

		static void HandleNotImplemented(string message)
		{
			Globals.Message.ShowError(string.IsNullOrEmpty(message) ? Res.GetString("d8a92593-6f53-472f-b06a-edec313118a8", "Not Implemented") : message);
		}

		static void OnPrintTaskRun(InvoicePrintTask printTask)
		{
#if DEBUG
			if (printTaskRun_ForTestOnly != null)
			{
				printTaskRun_ForTestOnly(printTask);
			}
#endif
		}

#if DEBUG
		public delegate void PrintTaskRunHandler(InvoicePrintTask printTask);

		public static event PrintTaskRunHandler PrintTaskRun_ForTestOnly
		{
			add
			{
				printTaskRun_ForTestOnly += value;
			}
			remove
			{
				printTaskRun_ForTestOnly -= value;
			}
		}
		[ThreadStatic]
		static PrintTaskRunHandler printTaskRun_ForTestOnly;

		public static void ResetPrintTaskRun_ForTestOnly()
		{
			printTaskRun_ForTestOnly = null;
		}
#endif

		#endregion
	}
}
