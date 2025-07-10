namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class OverrideGovernmentAllocatedIDValidation : OverrideInvoiceDetailValidation
	{
		public OverrideGovernmentAllocatedIDValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		protected override void CheckAH_GovernmentAllocatedID()
		{
			base.CheckAH_GovernmentAllocatedID();

			CheckAH_GovernmentAllocatedID_BasedOnRegistry((InvoicingBase)Parent);
		}
	}
}
