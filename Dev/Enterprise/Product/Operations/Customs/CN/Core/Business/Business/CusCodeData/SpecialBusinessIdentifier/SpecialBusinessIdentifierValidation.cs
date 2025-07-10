using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class SpecialBusinessIdentifierValidation : CusCodeDataValidation
	{
		public SpecialBusinessIdentifierValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
