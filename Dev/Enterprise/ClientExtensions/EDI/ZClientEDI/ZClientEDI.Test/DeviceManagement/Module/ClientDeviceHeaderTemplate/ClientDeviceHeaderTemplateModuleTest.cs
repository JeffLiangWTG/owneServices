using CargoWise.EntityFramework;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Module.Testing
{
	[TestedType(typeof(ClientDeviceHeaderTemplateModule))]
	public class ClientDeviceHeaderTemplateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.ClientDeviceTemplate;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var item = collection.Factory.New<ClientDeviceHeader>();
			item.CDH_IsTemplate = true;
			collection.Factory.Save();
			base.AddTestObjects(collection);
		}
	}
}
