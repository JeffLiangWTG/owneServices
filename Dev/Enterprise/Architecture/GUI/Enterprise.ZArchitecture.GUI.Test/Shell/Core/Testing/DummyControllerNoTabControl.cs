using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class DummyControllerNoTabControl : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerNoTabControl;

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity) => new DummyPlugInNoTabControl(businessEntity);
	}
}
