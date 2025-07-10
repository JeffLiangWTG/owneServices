using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	public static class InvoiceFileNameProvider
	{
		public static string GetInvoiceFileName(InvoicingBase invoice)
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"AR {0} {1}", invoice.AH_TransactionType, invoice.AH_TransactionNum);
		}

		public static string GetContainerListFileName(InvoicingBase invoice)
		{
			var invoiceName = GetInvoiceFileName(invoice);
			return (NoResString)$"{invoiceName} Container List";
		}

		public static string GetPeriodicInvoiceFileName(InvoicingBase invoice)
		{
			var invoiceName = GetInvoiceFileName(invoice);
			return (NoResString)$"{invoiceName} Periodic Details";
		}
	}
}
