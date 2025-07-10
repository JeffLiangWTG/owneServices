using Enterprise.Security;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWhichAllowsDelete : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWhichAllowsDelete;

		protected override SecurityCheckpoint CheckPointForDelete => new DummyCheckPointWithSecuritySet(true);
	}
}
