namespace Enterprise.Customs.BR.Business
{
	public class BillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public BillValidation(Bill houseBill)
			: base(houseBill)
		{
		}

		public new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		protected new Bill Parent
		{
			get { return (Bill)base.Parent; }
		}
	}
}
