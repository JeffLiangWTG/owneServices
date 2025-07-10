namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class GroupInvoiceChargeValidation : Customs.Business.BaseGroupInvoiceChargeValidation
	{
		public GroupInvoiceChargeValidation(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public GroupInvoiceCharge GroupInvoiceCharge
		{
			get { return Parent; }
		}

		protected new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}
	}
}
