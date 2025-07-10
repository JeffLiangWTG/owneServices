namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CusEntryLineFeeValidation : Customs.Business.CusEntryLineFeeValidation
	{
		public CusEntryLineFeeValidation(AutoCusEntryLineFee parent)
			: base(parent)
		{
		}

		protected new CusEntryLineFee Parent => (CusEntryLineFee)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNationalFeeTypeCode();
		}
		public void ValidateNationalFeeTypeCode()
		{
			ValidateCalculatedProperty(Parent.NationalFeeTypeCodeInfo);
		}

		protected virtual void CheckNationalFeeTypeCode()
		{
		}
	}
}
