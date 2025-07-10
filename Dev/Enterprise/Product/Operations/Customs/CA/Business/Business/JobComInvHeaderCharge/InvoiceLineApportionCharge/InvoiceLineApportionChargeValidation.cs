namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineApportionChargeValidation : Customs.Business.JobComInvHeaderChargeValidation
	{
		public InvoiceLineApportionChargeValidation(InvoiceLineApportionCharge invoiceLineApportionCharge)
			: base(invoiceLineApportionCharge)
		{
		}

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		protected override bool IsCIFComponentUsed
		{
			get { return true; }
		}
	}
}
