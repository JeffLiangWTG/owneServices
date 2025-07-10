using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	public abstract class TemporaryStorageModuleAbstractTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.TemporaryStorage;

		protected override bool HasController() => true;
	}
}
