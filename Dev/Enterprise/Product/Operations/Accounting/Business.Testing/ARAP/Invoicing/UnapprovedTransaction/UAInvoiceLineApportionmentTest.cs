using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class UAInvoiceLineApportionmentTest : APInvoiceLineApportionmentTest
	{
		protected override InvoicingBase GetInvoicingBase(BusinessObjectFactory factory)
		{
			var result = factory.New<UAInvoice>();
			result.AH_TransactionNum = "000001";
			return result;
		}

		protected override InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory)
		{
			return factory.New<UAInvoiceLine>();
		}
	}
}
