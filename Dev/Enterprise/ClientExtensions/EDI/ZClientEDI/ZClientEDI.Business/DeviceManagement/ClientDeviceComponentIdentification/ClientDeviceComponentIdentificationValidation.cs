using CargoWise.EntityFramework;
using Enterprise.RemoteDeviceManagement;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentIdentificationValidation : DmgDeviceComponentIdentificationValidation
	{
		public ClientDeviceComponentIdentificationValidation(AutoDmgDeviceComponentIdentification parent) : base(parent)
		{
		}

		protected override void CheckCDD_IdentificationType()
		{
			base.CheckCDD_IdentificationType();
			MandatoryValidation.CheckEntered(Parent.CDD_IdentificationTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CDD_IdentificationTypeInfo);
		}

		protected override void CheckCDD_Identifier()
		{
			base.CheckCDD_Identifier();
			MandatoryValidation.CheckEntered(Parent.CDD_IdentifierInfo);
		}
	}
}

