using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APInvoiceApportionmentTest : APApportionmentTest
	{
		protected override InvoicingBase GetInvoiceBase(BusinessObjectFactory factory)
		{
			return factory.New<APInvoice>();
		}

		protected override InvoicingLineBase GetInvoiceLineBase(BusinessObjectFactory factory)
		{
			return factory.New<APInvoiceLine>();
		}
	}
}
