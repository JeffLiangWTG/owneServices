namespace Enterprise.Customs.BR.Business
{
	public class CustomsOfficeValidation : Customs.Business.CusCodeDataValidation
	{
		public CustomsOfficeValidation(CustomsOffice parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
