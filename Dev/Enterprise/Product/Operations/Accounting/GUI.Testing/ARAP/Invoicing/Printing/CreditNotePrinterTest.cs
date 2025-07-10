using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.Testing
{
	public class CreditNotePrinterTest : InvoicePrinterTest
	{
		protected override Type GetAPInvoiceType()
		{
			return typeof(APCreditNote);
		}

		protected override Type GetARInvoiceType()
		{
			return typeof(ARCreditNote);
		}
	}
}
