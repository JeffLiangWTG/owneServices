using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UAInvoiceApportionmentTest : APApportionmentTest
	{
		protected override InvoicingBase GetInvoiceBase(BusinessObjectFactory factory)
		{
			return factory.New<UAInvoice>();
		}

		protected override InvoicingLineBase GetInvoiceLineBase(BusinessObjectFactory factory)
		{
			return factory.New<UAInvoiceLine>();
		}
	}
}
