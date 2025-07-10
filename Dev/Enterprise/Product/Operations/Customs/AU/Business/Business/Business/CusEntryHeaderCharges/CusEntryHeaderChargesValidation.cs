namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		protected new CusEntryHeaderCharges Parent => (CusEntryHeaderCharges)base.Parent;

		protected override void CheckC1_ChargeType()
		{
		}
	}
}
