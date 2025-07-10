using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class BLLFunctionValidation : AutoBLLFunctionValidation
	{
		public BLLFunctionValidation(AutoBLLFunction parent) : base(parent)
		{
		}

		protected override void CheckJPM_BillOfLadingNumber()
		{
			base.CheckJPM_BillOfLadingNumber();
			MandatoryValidation.CheckEntered(Parent.JPM_BillOfLadingNumberInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JPM_BillOfLadingNumberInfo);
		}

		protected override void CheckJPM_ChangeReasonCode()
		{
			base.CheckJPM_ChangeReasonCode();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JPM_ChangeReasonCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.JPM_ChangeReasonCodeInfo);
		}
	}
}
