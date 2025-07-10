namespace Enterprise.Customs.CA.Business
{
	public class GroupInvoiceChargeLookups : CommonInvoiceHeaderChargeLookups
	{
		public GroupInvoiceChargeLookups(GroupInvoiceCharge groupInvoiceCharge)
			: base(groupInvoiceCharge)
		{
		}

		public new GroupInvoiceCharge Parent
		{
			get { return (GroupInvoiceCharge)base.Parent; }
		}
	}
}
