using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AdditionalInfoValidation : CusSupportingInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_CodeInfo, Parent.CSI_DescriptionInfo, Res.GetString("246bd495-493e-41b7-adf2-4377d2f8f709", "You have not entered a Code."));
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}
}
