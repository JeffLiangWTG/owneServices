using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyController2 : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.Dummy2;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPlugIn2(businessEntity);

		protected override ZModulePlugin GetModulePluginCore(ZFilterGridModule module) => null;

		protected override SecurityCheckpoint CheckPointForEdit => DummyCheckPoint.Instance;
	}
}
