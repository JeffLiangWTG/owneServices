using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AdditionalFiscalReferenceValidation : EU.Business.Declaration.CusFiscalReferenceValidation
	{
		public AdditionalFiscalReferenceValidation(AdditionalFiscalReference parent)
			: base(parent)
		{
		}

		protected override void CheckCFR_ReferenceIsNotEmpty()
		{
			var parent = Parent;
			var targetInfo = parent.CFR_ReferenceInfo;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.CFR_CodeInfo, Res.GetString("8980B84C-476F-4467-B0B1-83454655EA55", "You have not entered an Identification Number."));
		}

		protected override void CheckCFR_Code()
		{
			var parent = Parent;
			var targetInfo = parent.CFR_CodeInfo;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(targetInfo, parent.CFR_ReferenceInfo, Res.GetString("BEEBFAFD-2A21-4E24-8A5E-5D8FEBD14676", "You have not entered a Type."));
			ListValidation.MessageErrorIfInvalidCode(targetInfo);
		}
	}
}
