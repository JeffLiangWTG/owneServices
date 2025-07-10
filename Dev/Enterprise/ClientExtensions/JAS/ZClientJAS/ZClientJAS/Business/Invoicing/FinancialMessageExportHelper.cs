using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.JXC.Export;
using Enterprise.Client.JAS.Business.JXC.Export.Validations;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	public class FinancialMessageExportHelper
	{
		public FinancialMessageExportHelper(IJASInvoicingBase invoice)
		{
			this.Invoice = invoice;
			if (invoice == null || invoice.InvoicingBase == null)
			{
				throw new ArgumentNullException(nameof(invoice));
			}

			reloadRequired = invoice is ARInvoice && invoice.InvoicingBase.AH_TransactionType == Enterprise.ZArchitecture.Core.TransactionTypes.CreditNote;
		}

		readonly bool reloadRequired;

		virtual
 public void OnSaving()
		{
			IsSavedForTheFirstTime = !Invoice.InvoicingBase.IsInDatabase;
		}

		virtual
 public void OnSaved(bool saveSucceeded)
		{
			if (saveSucceeded && JASDataRegistry.Instance.EnableAutoJXCMessaging && IsSavedForTheFirstTime)
			{
				var invoiceToBeExported = Invoice;
				if (reloadRequired)
				{
					var reloadedCreditNote = Invoice.Factory.Load<JASARCreditNote>(Invoice.InvoicingBase.PK);
					invoiceToBeExported = reloadedCreditNote ?? invoiceToBeExported;
				}

				ValidateForJXC(invoiceToBeExported.InvoicingBase);
				if (!new ValidationHelper().HasJXCWarnings(invoiceToBeExported.InvoicingBase))
				{
					InvoiceWrapper invoiceWrapper = new InvoiceWrapper(invoiceToBeExported);
					INVCDTMessageExporter exporter = new INVCDTMessageExporter(invoiceWrapper);
					exporter.WriteToFile();
				}
			}
		}

		void ValidateForJXC(InvoicingBase invoicingBase)
		{
			JXCDomainValidationManager validationManager = new JXCDomainValidationManager(invoicingBase.Factory);
			validationManager.ManageJXCValidations(JXCExportValidationType.Invoicing);
			invoicingBase.RunPreSaveValidation();
		}

		bool IsSavedForTheFirstTime;
		public readonly IJASInvoicingBase Invoice;
	}
}
