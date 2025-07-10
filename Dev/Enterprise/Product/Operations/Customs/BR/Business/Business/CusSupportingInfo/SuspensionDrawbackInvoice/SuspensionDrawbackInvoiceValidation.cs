namespace Enterprise.Customs.BR.Business
{
	public class SuspensionDrawbackInvoiceValidation : Customs.Business.CusSupportingInfoValidation
	{
		public SuspensionDrawbackInvoiceValidation(SuspensionDrawbackInvoice parent) : base(parent)
		{
		}

		public new SuspensionDrawbackInvoice Parent => (SuspensionDrawbackInvoice)base.Parent;
	}
}
