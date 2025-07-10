using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class SupportingDocumentValidation : CusSupportingInfoValidation
	{
		public SupportingDocumentValidation(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_CodeInfo, Parent.CSI_ReferenceNumberInfo, Res.GetString("f9031c9e-e8d7-4035-89a9-2bc53a9bbcf6", "You have not entered a Type."));
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_ReferenceNumberInfo, Parent.CSI_CodeInfo, Res.GetString("360af8c0-073b-4617-ad9b-1bd57af85756", "You have not entered a Reference Number."));
		}
	}
}
