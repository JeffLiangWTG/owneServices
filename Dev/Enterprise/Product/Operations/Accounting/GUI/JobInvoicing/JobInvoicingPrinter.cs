using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public delegate void JobInvoincePrintedHandler(TransactionPrintingResults printingResult);

	public partial class JobInvoicingPrinter : InvoicePrinter
	{
		public JobInvoicingPrinter(IJobHeaderParent jobParent)
		{
			this.jobParent = jobParent;
		}

		readonly IJobHeaderParent jobParent;

		public event JobInvoincePrintedHandler JobInvoincePrinted;

		public void Print(Form parentForm, InvoicePrintContext context, params TransactionHeader[] transactions)
		{
			if (!transactions.Any())
			{
				return;
			}

			var transactionsEligibleToPrint = InvoicePrintHelper.GetEligibleForPrintingTransactions(transactions);

			if (transactionsEligibleToPrint.Any())
			{
				Print(transactionsEligibleToPrint);
			}

			void Print(IEnumerable<TransactionHeader> transactionsToPrint)
			{
				foreach (var transaction in transactionsToPrint.ToArray())
				{
					var printingResult = PrintTransaction(transaction, parentForm, context);
					JobInvoincePrinted?.Invoke(printingResult);
				}
			}
		}

		protected override InvoicePrintTask NewTask(InvoicingBase arTransaction, InvoicePrintContext context)
		{
			var onlyPrintCommonInvoiceforVN = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.VietNam && AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value == GovtTaxInvoicePrintTask.EnterpriseInvoice && AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value;

			return arTransaction.IsGovtTaxInvoice && !onlyPrintCommonInvoiceforVN ? new GovtTaxInvoicePrintTask(new InvoicePrintTask.Configuration(arTransaction.PK) { JobParent = jobParent, Context = context }) : new InvoicePrintTask(new InvoicePrintTask.Configuration(arTransaction.PK) { JobParent = jobParent });
		}
	}
}
