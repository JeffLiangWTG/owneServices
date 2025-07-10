using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.Business
{
	public class VanningAddressValidation : JobDocAddressValidation
	{
		public VanningAddressValidation(AutoJobDocAddress parent) : base(parent)
		{
		}

		protected override void CheckE2_AddressType()
		{
			if (Parent.E2_AddressType != DocAddressTypes.Codes.VanningLocationAddress)
			{
				Parent.E2_AddressTypeInfo.AddMessageError(Res.GetString("B7678D11-01F1-398F-072F-FE70720E176C", "Invalid Vanning address type"));
			}
		}
		protected override void CheckE2_GovRegNumType()
		{
			base.CheckE2_GovRegNumType();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.E2_GovRegNumTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.E2_GovRegNumTypeInfo);
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();

			var parent = Parent;
			var codeType = parent.E2_GovRegNumType;
			var targetInfo = parent.E2_GovRegNumInfo;

			MandatoryValidation.MessageErrorIfNotEntered(targetInfo);

			if (codeType == OrgCusCode.JapanCodeTypes.LPC || codeType == OrgCusCode.JapanCodeTypes.CIE || codeType == OrgCusCode.CodeTypes.ControlledPremisesID)
			{
				CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, codeType, parent.E2_GovRegNum, targetInfo);
			}
		}
	}
}
