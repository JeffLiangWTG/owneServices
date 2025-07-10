using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class ARTermsAndDueDateCalculationProvider : TermsAndDueDateCalculationProvider
	{
		public ARTermsAndDueDateCalculationProvider(IInvoiceTerms invoice)
				: base(invoice)
		{
		}

		protected override InvoiceTerm GetInvoiceTerm(bool useFallbackByInvoiceTerm)
		{
			InvoiceTerm result = new InvoiceTerm();
			if (Invoice.Header != null)
			{
				result = Invoice.Header.CompanyData.GetARTerm(Invoice.JobType, Invoice.Direction, Invoice.TransportMode, Invoice.AH_GB, Invoice.AH_GE, Invoice.AH_TransactionCategory, useFallbackByInvoiceTerm ? Invoice.AH_InvoiceTerm : null);
			}

			return result;
		}
	}
}
