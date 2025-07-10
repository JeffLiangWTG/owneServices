using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideTransactionDescriptionValidation : OverrideInvoiceDetailValidation
	{
		public OverrideTransactionDescriptionValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_Desc()
		{
			base.CheckAH_Desc();
			MandatoryValidation.CheckEntered(Parent.AH_DescInfo);
		}
	}
}
