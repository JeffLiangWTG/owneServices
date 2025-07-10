using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithNullPlugIn : DummyController
	{
		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => null;
	}
}
