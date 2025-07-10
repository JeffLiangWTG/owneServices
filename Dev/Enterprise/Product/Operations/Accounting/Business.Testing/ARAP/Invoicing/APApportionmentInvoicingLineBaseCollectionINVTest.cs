using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APApportionmentInvoicingLineBaseCollectionINVTest : APApportionmentInvoicingLineBaseCollectionTest
	{
		protected override InvoicingBase GetInvoicingBase(BusinessObjectFactory factory)
		{
			return factory.New<APInvoice>();
		}

		protected override InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory)
		{
			return factory.New<APInvoiceLine>();
		}
	}
}
