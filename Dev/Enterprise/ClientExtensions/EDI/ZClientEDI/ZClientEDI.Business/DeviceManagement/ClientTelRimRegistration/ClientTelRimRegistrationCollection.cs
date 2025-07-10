using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	public class ClientTelRimRegistrationCollection : ActiveBusinessObjectCollection<ClientTelRimRegistration>
	{
		public ClientTelRimRegistrationCollection(ClientDeviceHeader device)
			: base(
				device.Factory,
				device,
				null,
				ClientTelRimRegistrationSchema.TRR_CDH_ClientDeviceHeader)
		{
		}
	}
}
