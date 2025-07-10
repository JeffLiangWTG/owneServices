namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusDecHouseBillValidation : Customs.Business.CusDecHouseBillValidation
	{
		public CusDecHouseBillValidation(Bill houseBill)
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
