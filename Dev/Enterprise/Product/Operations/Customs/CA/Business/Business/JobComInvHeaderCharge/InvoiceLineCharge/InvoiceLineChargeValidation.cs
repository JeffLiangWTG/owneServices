namespace Enterprise.Customs.CA.Business
{
	public class InvoiceLineChargeValidation : Customs.Business.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge InvoiceLineCharge
		{
			get { return Parent; }
		}

		protected new InvoiceLineCharge Parent
		{
			get { return (InvoiceLineCharge)base.Parent; }
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
