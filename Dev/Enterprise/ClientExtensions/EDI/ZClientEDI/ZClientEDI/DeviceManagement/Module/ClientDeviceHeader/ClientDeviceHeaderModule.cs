using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.DeviceManagement.GUI;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public class ClientDeviceHeaderModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public ClientDeviceHeaderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID => ClientModuleRegistration.ClientDevice;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ClientControllerRegistration.ClientDevice);

		protected override IFilterControl GetNewFilterControl() => new ClientDeviceHeaderFilterControl(GridCollection, FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new ClientDeviceHeaderCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ClientDeviceHeaderFilterBusinessObject();

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.Devices;

		public OperationalActionSupporter OperationalActionSupporter => new ClientDeviceHeaderOperationalActionSupporter();

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			return ClientDeviceMenuItemGenerator.GetNewMenuWithModelTemplates<ZMenuItem>(
				base.GetNewStandardMenuItems().Cast<IClickableNestedMenuItem>().Where(x => x != NewMenuItem),
				Factory).Cast<MenuItem>().ToArray();
		}
	}
}
