using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APInvoiceLineApportionmentTest : APApportionementLineTest
	{
		protected override InvoicingBase GetInvoicingBase(BusinessObjectFactory factory)
		{
			var result = factory.New<APInvoice>();
			result.AH_TransactionNum = "000001";
			return result;
		}

		protected override InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory)
		{
			return factory.New<APInvoiceLine>();
		}
	}
}
