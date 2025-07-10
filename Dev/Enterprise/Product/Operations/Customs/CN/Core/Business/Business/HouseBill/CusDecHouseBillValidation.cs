namespace Enterprise.Customs.CN.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill => (Bill)base.Bill;

		protected new Bill Parent => (Bill)base.Parent;
	}
}
