using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class OverrideInvoiceRemittanceTypeValidation : OverrideInvoiceDetailValidation
	{
		public OverrideInvoiceRemittanceTypeValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_InvoicePaymentReferenceCode()
		{
			base.CheckAH_InvoicePaymentReferenceCode();
			MandatoryValidation.CheckEntered(Parent.AH_InvoicePaymentReferenceCodeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.AH_InvoicePaymentReferenceCodeInfo);
		}
	}
}
