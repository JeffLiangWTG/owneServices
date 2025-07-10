namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineChargeValidation
		: EU.Business.Declaration.InvoiceLineChargeValidation
	{
		public InvoiceLineChargeValidation(InvoiceLineCharge invoiceLineCharge)
			: base(invoiceLineCharge)
		{
		}

		public new InvoiceLineCharge Parent => (InvoiceLineCharge)base.Parent;

		protected override void CheckJ7_ChargeType()
		{
			base.CheckJ7_ChargeType();
			if (Parent?.J7_ChargeType.Equals(EU.Business.UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge) ?? false)
			{
				Parent.J7_ChargeTypeInfo.AddMessageError(Res.GetString("753539AD-2393-4302-81A5-C117D8C9360A", @"CDS no longer allows the declaration of a charge type to reduce the item price by the duty that the price includes, e.g. payment term DDP.
Since CDS no longer allows use of charge type BC in element 4/9, you may not capture charge type IDO. 
Instead, manually reduce the item price (4/14) by this amount, and add a 9WKS document code to show your working."));
			}
		}
	}
}
