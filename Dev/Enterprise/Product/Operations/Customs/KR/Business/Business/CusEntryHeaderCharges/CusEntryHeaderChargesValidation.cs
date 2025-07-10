namespace Enterprise.Customs.KR.Business
{
	public class CusEntryHeaderChargesValidation : Customs.Business.CusEntryHeaderChargesValidation
	{
		public CusEntryHeaderChargesValidation(CusEntryHeaderCharges parent)
			: base(parent)
		{
		}

		protected override void CheckC1_ChargeType()
		{
		}
	}
}
