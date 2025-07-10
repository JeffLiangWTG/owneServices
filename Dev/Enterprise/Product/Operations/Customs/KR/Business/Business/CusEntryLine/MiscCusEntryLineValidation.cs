namespace Enterprise.Customs.KR.Business
{
	public class MiscCusEntryLineValidation : AutoKRCusEntryLineValidation
	{
		public MiscCusEntryLineValidation(CusEntryLine parent)
			: base(parent)
		{
		}

		protected override void CheckCL_CustomsValue()
		{
		}
	}
}
