namespace Enterprise.Customs.CN.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public GroupInvoiceCharge GroupInvoiceCharge => Parent;

		protected new GroupInvoiceCharge Parent => (GroupInvoiceCharge)base.Parent;
	}
}
