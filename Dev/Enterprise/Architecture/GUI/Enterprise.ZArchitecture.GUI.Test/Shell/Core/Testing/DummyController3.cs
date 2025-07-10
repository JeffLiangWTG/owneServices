using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ModulePlugIn;
using Enterprise.ZArchitecture.ModulePlugIn.Testing;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyController3 : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.Dummy3;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPlugIn3(businessEntity);

		protected override ZModulePlugin GetModulePluginCore(ZFilterGridModule module) => new DummyModulePlugin3();
	}
}
