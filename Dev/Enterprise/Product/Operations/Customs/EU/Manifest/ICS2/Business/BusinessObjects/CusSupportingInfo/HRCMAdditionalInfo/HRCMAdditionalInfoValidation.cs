using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	internal class HRCMAdditionalInfoValidation : CusSupportingInfoValidation
	{
		public HRCMAdditionalInfoValidation(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
		}
	}
}
