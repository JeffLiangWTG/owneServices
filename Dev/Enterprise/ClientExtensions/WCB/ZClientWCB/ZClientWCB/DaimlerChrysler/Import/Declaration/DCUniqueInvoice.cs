using System.Collections.Generic;

namespace Enterprise.Client.WCB.DaimlerChrysler
{
	internal class DCUniqueInvoice
	{
		public DCUniqueInvoice(DecInvoiceHeaderDataRow invoiceHeader)
		{
			InvoiceHeader = invoiceHeader;
			InvoiceLineCollection = new List<DecInvoiceLineDataRow>();
		}

		internal DecInvoiceHeaderDataRow InvoiceHeader { get; set; }
		internal List<DecInvoiceLineDataRow> InvoiceLineCollection { get; set; }
	}
}
