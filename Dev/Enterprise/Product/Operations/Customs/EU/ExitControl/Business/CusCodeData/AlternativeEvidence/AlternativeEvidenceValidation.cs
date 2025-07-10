using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class AlternativeEvidenceValidation : CusCodeDataValidation
	{
		public AlternativeEvidenceValidation(AutoCusCodeData parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}
	}
}
