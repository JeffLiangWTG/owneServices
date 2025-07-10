using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class StmModuleFilterSettingsStorageTest : TestCaseWithFactory
	{
		public void TestConcurrencyInTwoClientsWhenSavingOrRemovingSettings()
		{
			var contextKey = ZGuid.NewZGuid().ToString();

			var storage1 = new StmModuleFilterSettingsStorage("TST:", contextKey);
			storage1.SaveSettings("123", "Test");

			var storage2 = new StmModuleFilterSettingsStorage("TST:", contextKey);
			storage2.factory.RefreshEnabled = false;
			AssertEquals("Test", storage2.LoadSettings("123"));

			var storage3 = new StmModuleFilterSettingsStorage("TST:", contextKey);
			storage3.factory.RefreshEnabled = false;
			AssertEquals("Test", storage3.LoadSettings("123"));

			storage1.RemoveSettings("123");

			AssertNoExceptionThrown(() => { storage2.RemoveSettings("123"); });
			AssertNull(storage2.LoadSettings("123"));

			AssertNoExceptionThrown(() => { storage3.SaveSettings("123", "TestTest"); });
			AssertNull(storage3.LoadSettings("123"));
		}

		public void TestStmModuleFilterSettingsStorage()
		{
			var contextKey = ZGuid.NewZGuid();
			var storage = new StmModuleFilterSettingsStorage("TST:", contextKey.ToString());

			AssertEquals(0, new List<string>(storage.GetSavedSettings()).Count);

			storage.SaveSettings("1", "Test1");
			AssertEquals(1, new List<string>(storage.GetSavedSettings()).Count);
			AssertEquals("Test1", storage.LoadSettings("1"));

			storage.SaveSettings("1", "Test1a");
			AssertEquals(1, new List<string>(storage.GetSavedSettings()).Count);
			AssertEquals("Test1a", storage.LoadSettings("1"));

			storage.SaveSettings("1", "测试UTF-8");
			AssertEquals(1, new List<string>(storage.GetSavedSettings()).Count);
			AssertEquals("测试UTF-8", storage.LoadSettings("1"));

			storage.SaveSettings("2", "Test2");
			AssertEquals(2, new List<string>(storage.GetSavedSettings()).Count);

			storage.RemoveSettings("1");
			AssertEquals(1, new List<string>(storage.GetSavedSettings()).Count);
			AssertEquals("Test2", storage.LoadSettings("2"));
		}

		public void TestStmModuleFilterReturnsSystemFilters()
		{
			var storage = new StmModuleFilterSettingsStorage("DIW:", "5F71BC3D-F466-4EE9-B33A-002C2177F45A");
			AssertNotNull(storage.LoadSettings("System Default Layout"));

			storage = new StmModuleFilterSettingsStorage("DEW:", "5F71BC3D-F466-4EE9-B33A-002C2177F45A");
			AssertNotNull(storage.LoadSettings("System Default Layout"));

			storage = new StmModuleFilterSettingsStorage("DEW:", "5F71BC3D-F466-4EE9-B33A-002C2177F45A");
			AssertEquals(1, new List<string>(storage.GetSavedSettings()).Count);
		}
	}
}
