namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusDecHouseBillLookups : Customs.Business.CusDecHouseBillLookups
	{
		public CusDecHouseBillLookups(Bill houseBill)
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
