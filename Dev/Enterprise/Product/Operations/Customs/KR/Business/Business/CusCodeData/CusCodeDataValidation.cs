using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class CusCodeDataValidation : Customs.Business.CusCodeDataValidation
	{
		public CusCodeDataValidation(CusCodeData parent)
			: base(parent)
		{
		}

		protected override void CheckCY_CodeList()
		{
		}

		protected override void CheckCY_CodeIsNotEmpty()
		{
		}
	}
}
