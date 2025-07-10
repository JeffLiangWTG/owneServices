using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.H7.Business
{
	public class AdditionalInfoValidation : EU.H7.Business.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_Code()
		{
			var info = Parent.CSI_CodeInfo;
			MandatoryValidation.MessageErrorIfNotEntered(info);
			ListValidation.MessageErrorIfInvalidCode(info);
		}
	}
}
