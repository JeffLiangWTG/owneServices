using CargoWise.EntityFramework;

using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.DocumentWrappers
{
	public class DocARInvoiceDataProvider : Integration.DocumentWrappers.IDocARInvoiceDataProvider
	{
		public ZString GetRecipientTaxIDNumber(BusinessObject parent, BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			if (parent is InvoicingBase invoicing)
			{
				var docARInvoice = DocARInvoice.New(invoicing, factory);
				result = docARInvoice.RecipientTaxIDNumber;
			}

			return result;
		}

		public ZString GetTaxId(BusinessObject parent, BusinessObjectFactory factory)
		{
			var result = ZString.Empty;
			if (parent is InvoicingBase invoicing)
			{
				var docARInvoice = DocARInvoice.New(invoicing, factory);
				result = docARInvoice.TaxId;
			}

			return result;
		}
	}
}
