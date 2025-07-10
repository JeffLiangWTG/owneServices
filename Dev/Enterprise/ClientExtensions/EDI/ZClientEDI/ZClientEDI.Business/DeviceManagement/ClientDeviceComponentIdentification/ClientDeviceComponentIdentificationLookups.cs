using Enterprise.RemoteDeviceManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentIdentificationLookups : DmgDeviceComponentIdentificationLookups
	{
		public ClientDeviceComponentIdentificationLookups(AutoDmgDeviceComponentIdentification parent) : base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList IdentificationTypeList => EDIDataRegistry.Instance.TelematicsDevicesComponentIdentifications.Value;
	}
}

