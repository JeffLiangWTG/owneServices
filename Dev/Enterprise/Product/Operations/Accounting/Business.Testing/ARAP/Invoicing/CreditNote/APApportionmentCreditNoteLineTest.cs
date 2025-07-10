using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class APApportionmentCreditNoteLineTest : APApportionementLineTest
	{
		protected override InvoicingBase GetInvoicingBase(BusinessObjectFactory factory)
		{
			var result = factory.New<APCreditNote>();
			result.AH_TransactionNum = "000001";
			return result;
		}

		protected override InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory)
		{
			return factory.New<APCreditNoteLine>();
		}
	}
}
