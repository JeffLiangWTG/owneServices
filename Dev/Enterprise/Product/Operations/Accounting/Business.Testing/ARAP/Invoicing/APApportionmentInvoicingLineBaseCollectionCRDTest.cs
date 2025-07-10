using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APApportionmentInvoicingLineBaseCollectionCRDTest : APApportionmentInvoicingLineBaseCollectionTest
	{
		protected override InvoicingBase GetInvoicingBase(BusinessObjectFactory factory)
		{
			return factory.New<APCreditNote>();
		}

		protected override InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory)
		{
			return factory.New<APCreditNoteLine>();
		}
	}
}
