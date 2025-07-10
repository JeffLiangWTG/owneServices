namespace Enterprise.Customs.CN.Business
{
	public class BillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public BillLookups(Bill houseBill)
			: base(houseBill)
		{
		}

		public Bill HouseBill => Parent;

		protected new Bill Parent => (Bill)base.Parent;
	}
}
