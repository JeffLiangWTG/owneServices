using System.Collections.Generic;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class EURGoodsSummaryWrapper : EU.Business.Documents.CertificateOfOrigin.EURGoodsSummaryWrapper
	{
		public EURGoodsSummaryWrapper(IEnumerable<JobComInvoiceLine> invoiceLines) : base(invoiceLines)
		{
		}

		protected override EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder GetNewBuilder() => new EUR1BoxItemsBuilder(InvoiceLines);
	}
}
