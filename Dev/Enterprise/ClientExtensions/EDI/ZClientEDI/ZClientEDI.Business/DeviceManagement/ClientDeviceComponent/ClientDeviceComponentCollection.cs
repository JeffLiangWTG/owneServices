using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientDeviceComponentCollection : ActiveBusinessObjectCollection<ClientDeviceComponent>
	{
		public ClientDeviceComponentCollection(ClientDeviceHeader device)
			: base(device.Factory, device, null, DmgDeviceComponentSchema.CDC_CDH_Device)
		{
		}
	}
}

