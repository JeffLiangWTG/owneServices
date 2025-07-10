using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class AdditionalIdentificationValidation : CusCodeDataValidation
	{
		public AdditionalIdentificationValidation(AdditionalIdentification parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
