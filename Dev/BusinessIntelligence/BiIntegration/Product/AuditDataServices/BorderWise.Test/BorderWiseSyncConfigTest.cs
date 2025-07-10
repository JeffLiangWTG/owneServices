using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.AuditDataServices.BorderWise.Subscribers;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class BorderWiseSyncConfigTest : TestCaseWithFactory
	{
		public void TestEnabled()
		{
			var registryItem = new BoolDbRegistryItem("BorderWise_SyncEnabled", defaultValue: false);

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				// Enable registry flag and set expected registration key
				registryItem.SaveValue(true, Db.Connection);
				Assert(BorderWiseSyncConfig.Enabled);

				// Disable registry flag
				registryItem.SaveValue(false, Db.Connection);
				Assert(!BorderWiseSyncConfig.Enabled);
			}

			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.None))
			{
				// Enable registry flag and set expected registration key
				registryItem.SaveValue(true, Db.Connection);
				Assert(!BorderWiseSyncConfig.Enabled);

				// Disable registry flag
				registryItem.SaveValue(false, Db.Connection);
				Assert(!BorderWiseSyncConfig.Enabled);
			}
		}
	}
}
