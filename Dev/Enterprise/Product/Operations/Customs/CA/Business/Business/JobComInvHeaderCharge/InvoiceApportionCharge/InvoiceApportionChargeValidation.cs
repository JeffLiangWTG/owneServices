namespace Enterprise.Customs.CA.Business
{
	public class InvoiceApportionChargeValidation : Customs.Business.BaseApportionedChargeValidation
	{
		public InvoiceApportionChargeValidation(InvoiceApportionCharge invoiceApportionCharge)
			: base(invoiceApportionCharge)
		{
		}

		public InvoiceApportionCharge InvoiceApportionCharge
		{
			get { return Parent; }
		}

		protected new InvoiceApportionCharge Parent
		{
			get { return (InvoiceApportionCharge)base.Parent; }
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
