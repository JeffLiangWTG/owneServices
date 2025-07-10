using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APTermsAndDueDateCalculationProvider : TermsAndDueDateCalculationProvider
	{
		public APTermsAndDueDateCalculationProvider(IInvoiceTerms invoice)
			: base(invoice)
		{
		}

		protected override InvoiceTerm GetInvoiceTerm(bool useFallbackByInvoiceTerm)
		{
			InvoiceTerm result = new InvoiceTerm();
			if (Invoice.Header != null)
			{
				result = Invoice.Header.CompanyData.GetAPTerm();
			}

			return result;
		}

		protected override ZDateTime GetInvoiceDate()
		{
			return DueDateCalculation.GetCalculateDate(GetInvoiceTerm(true), Invoice.AH_InvoiceDate, Invoice.AH_DocumentReceivedDate);
		}
	}
}
