namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithModuleGuiNotSupportedException : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithModuleGuiNotSupportedException;

		public override ModuleIdentifier ModuleID => throw new ModuleGuiNotSupportedException("Not supported");
	}
}
