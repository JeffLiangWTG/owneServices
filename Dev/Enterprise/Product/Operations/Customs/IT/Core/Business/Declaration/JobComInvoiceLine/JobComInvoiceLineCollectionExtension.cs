using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.IT.Business.Declaration;

static class JobComInvoiceLineCollectionExtension
{
	public static IEnumerable<JobComInvoiceHeader> GetDistinctInvoiceHeaders(this IEnumerable<JobComInvoiceLine> invoiceLines)
	{
		return invoiceLines.Where(x => x.InvoiceHeader != null).Select(x => x.InvoiceHeader).Distinct().ToArray();
	}
}
