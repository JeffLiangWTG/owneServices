using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPluginWithGetNewTopMenuInPlugInIsExistingMenu(businessEntity);
	}
}
