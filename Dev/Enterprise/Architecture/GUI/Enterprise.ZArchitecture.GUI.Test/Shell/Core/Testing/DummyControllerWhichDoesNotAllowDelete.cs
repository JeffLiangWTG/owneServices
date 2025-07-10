using Enterprise.Security;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWhichDoesNotAllowDelete : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWhichDoesNotAllowDelete;

		protected override SecurityCheckpoint CheckPointForDelete => new DummyCheckPointWithSecuritySet(false);
	}
}
