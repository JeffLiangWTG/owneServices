using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageModule))]
	public class UCC6TemporaryStorageModuleTest : ZModuleBasherTest
	{
		public override void TestAutoAddedMilestoneDateFilter()
		{
			Assert("Not available on UCC6TemporaryStorage Module", true);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.UCC6TemporaryStorage;

		protected override bool HasController() => true;
	}
}
