using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithGetNewTopLevelMenuException : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithGetNewTopLevelMenuException;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPlugInWithGetNewTopLevelMenuException(businessEntity);
	}
}
