namespace Enterprise.Customs.CN.Business
{
	public class InvoiceLineApportionChargeValidation : Customs.Business.JobComInvHeaderChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}
	}
}
