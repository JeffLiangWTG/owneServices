using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.DeviceManagement.Module
{
	public class ClientDeviceHeaderTemplateModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ClientModuleRegistration.ClientDeviceTemplate; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.ClientDeviceTemplate);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ClientDeviceHeaderTemplateFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ClientDeviceHeaderTemplateCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ClientDeviceHeaderTemplateFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.DeviceTemplates; }
		}
	}
}
