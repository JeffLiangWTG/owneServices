using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Messaging
{
	public class InvoiceCrossReference : IInvoiceCrossReference
	{
		public InvoiceCrossReference(ZInt invoicePageNumber, ZInt invoiceLineNumber, ZDecimal invoiceValue)
		{
			InvoicePageNumber = invoicePageNumber;
			InvoiceLineNumber = invoiceLineNumber;
			InvoiceValue = invoiceValue;
		}

		public static IInvoiceCrossReference New(ZInt pageNumber, IEnumerable<IInvoiceCrossReference> references)
		{
			return new InvoiceCrossReference(pageNumber, 0, references.Sum(l => l.InvoiceValue));
		}

		#region IInvoiceCrossReference Members

		public ZInt InvoiceLineNumber { get; private set; }

		public ZInt InvoicePageNumber { get; private set; }
		public ZDecimal InvoiceValue { get; private set; }

		#endregion
	}
}
