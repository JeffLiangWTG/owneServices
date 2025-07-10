using CargoWise.EntityFramework;
using Enterprise.RemoteDeviceManagement;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentValidation : DmgDeviceComponentValidation
	{
		public ClientDeviceComponentValidation(AutoDmgDeviceComponent parent) : base(parent)
		{
		}

		protected override void CheckCDC_ComponentType()
		{
			base.CheckCDC_ComponentType();
			MandatoryValidation.CheckEntered(Parent.CDC_ComponentTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CDC_ComponentTypeInfo);
		}

		protected override void CheckCDC_ModelIdentifier()
		{
			base.CheckCDC_ModelIdentifier();
			MandatoryValidation.CheckEntered(Parent.CDC_ModelIdentifierInfo);
		}
	}
}

