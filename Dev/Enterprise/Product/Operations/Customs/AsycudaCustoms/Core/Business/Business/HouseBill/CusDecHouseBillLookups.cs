namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class BillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public BillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		public Bill HouseBill
		{
			get { return Parent; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}
	}
}
