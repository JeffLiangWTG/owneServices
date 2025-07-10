using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class AdditionalInfoValidation : EU.ExitControl.Business.AdditionalInfoValidation
	{
		public AdditionalInfoValidation(AdditionalInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			var info = Parent.CSI_ReferenceNumberInfo;
			if (Parent.CSI_ReferenceNumber.IsEmpty)
			{
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
		}
	}
}
