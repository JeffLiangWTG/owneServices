using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class LineDeliveryTermsProvider : IDeliveryTerms
	{
		readonly JobComInvoiceHeader invoiceHeader;

		public static LineDeliveryTermsProvider NewOrNull(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new LineDeliveryTermsProvider(invoiceLine);

		LineDeliveryTermsProvider(JobComInvoiceLine invoiceLine)
		{
			Argument.NotNull(invoiceLine, nameof(invoiceLine));
			invoiceHeader = invoiceLine.InvoiceHeader;
		}

		public string IncotermCode => invoiceHeader.JZ_IncoTerm;

		public string Location => invoiceHeader.JZ_IncoTermPlace;

		public string UNLocode => string.Empty;

		public string Country => string.Empty;

		public string Text => string.Empty;
	}
}
