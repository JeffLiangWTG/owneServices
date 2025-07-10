using Enterprise.Security;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyControllerWithTemplateModule : DummyController
	{
		public override ControllerID ID => DummyControllerIDs.DummyControllerWithTemplateModule;

		public override ModuleIdentifier ModuleID => DummyModuleIDs.DummyWithTemplates;

		protected override SecurityCheckpoint CheckPointForDelete => new DummyCheckPointWithSecuritySet(false);
	}
}
