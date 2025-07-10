using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Vietnam
{
	class VietnamComplianceInvoiceBookRegime :
		IComplianceInvoiceBookRegime
	{
		public bool AllowSeriesPrefixEmpty(bool enableEInvoicingFunctionalityReceivables)
		{
			return !enableEInvoicingFunctionalityReceivables;
		}
	}
}
