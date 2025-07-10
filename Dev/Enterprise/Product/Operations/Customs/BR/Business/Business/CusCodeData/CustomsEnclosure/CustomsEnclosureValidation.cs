namespace Enterprise.Customs.BR.Business
{
	public class CustomsEnclosureValidation : Customs.Business.CusCodeDataValidation
	{
		public CustomsEnclosureValidation(CustomsEnclosure parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
