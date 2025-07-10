using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ScreeningMethodValidation : CusSupportingInfoValidation
	{
		public ScreeningMethodValidation(AutoCusSupportingInfo parent) : base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
		}
	}
}
