

namespace Enterprise.Customs.JP.Business
{
	public class InvoiceLineApportionChargeValidation : Customs.Business.JobComInvHeaderChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}
	}
}
