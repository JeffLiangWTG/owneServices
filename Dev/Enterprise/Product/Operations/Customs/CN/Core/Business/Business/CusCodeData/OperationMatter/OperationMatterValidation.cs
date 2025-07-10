using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class OperationMatterValidation : CusCodeDataValidation
	{
		public OperationMatterValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
		}
	}
}
