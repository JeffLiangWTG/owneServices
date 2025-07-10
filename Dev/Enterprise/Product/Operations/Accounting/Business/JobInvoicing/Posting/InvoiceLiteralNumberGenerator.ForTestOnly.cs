#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	partial class InvoiceLiteralNumberGenerator
	{
		public static IEnumerable<int> ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(IEnumerable<ZString> invoiceNumbers, string baseInvoiceNumber, bool reportErrors)
			=> ConvertInvoiceNumbersSuffixesToInts(invoiceNumbers, baseInvoiceNumber, reportErrors);

		public static string GetNextConsolInvoiceNumber_ForTestOnly(BusinessObjectFactory factory, IEnumerable<ZString> existingInvoiceNumbersForJob, string invoiceNumber, bool reportError = true)
			=> GetNextConsolInvoiceNumber(factory, existingInvoiceNumbersForJob, invoiceNumber, reportError);

		public static ZQuery GetInvoiceFilter_ForTestOnly(InvoicingBase invoicingBase, IPostingJob currentJob)
			=> GetInvoiceFilter(invoicingBase, currentJob);

		public static string GetLetterRepresentation_ForTestOnly(int number)
			=> GetLetterRepresentation(number);
	}
}

#endif
