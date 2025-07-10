using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DeviceManagement.Business
{
	[ModuleID("ClientDeviceTemplate")]
	public class ClientDeviceHeaderTemplateCollection : ActiveBusinessObjectCollection<ClientDeviceHeader>
	{
		public ClientDeviceHeaderTemplateCollection(BusinessObjectFactory factory)
			: base(factory, new ZQuery(DmgDeviceHeaderSchema.CDH_IsTemplate, ZBool.True))
		{
		}

		protected override void SetDefaultsForNewElementCore(ClientDeviceHeader newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.CDH_IsTemplate = true;
		}
	}
}

