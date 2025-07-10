using Enterprise.RemoteDeviceManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentLookups : DmgDeviceComponentLookups
	{
		public ClientDeviceComponentLookups(AutoDmgDeviceComponent parent)
			: base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList ComponentTypeList
		{
			get { return EDIDataRegistry.Instance.TelematicsDeviceComponents.Value; }
		}
	}
}

