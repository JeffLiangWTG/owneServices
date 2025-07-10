using System.Collections.Generic;
using Enterprise.Customs.ES.Business.Documents.DocDataObjects;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin
{
	public class EURGoodsSummaryWrapper : EU.Business.Documents.CertificateOfOrigin.EURGoodsSummaryWrapper
	{
		public EURGoodsSummaryWrapper(IEnumerable<EU.Business.Declaration.JobComInvoiceLine> invoiceLines) : base(invoiceLines)
		{
		}

		protected override EU.Business.Documents.DocDataObjects.EUR1BoxItemsBuilder GetNewBuilder() => new EUR1BoxItemsBuilder(InvoiceLines);
	}
}
