using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class ZEditableGridWoLayotsTest : TestCaseWithDummy
	{
		public void TestIsGridLayoutConfigurable()
		{
			using (var grid = new ZFilterGrid())
			{
				Assert(!grid.IsGridLayoutConfigurable);
			}
		}

		public void TestModuleIDName()
		{
			using (var grid = new ZFilterGrid())
			using (var module = new DummyFilterGridModule())
			{
				AssertEquals("", grid.ModuleIDName);

				grid.SetParentFilterGridModule(module);

				AssertEquals("ModuleIDName", DummyModuleIDs.Dummy.Name, grid.ModuleIDName);
			}
		}

		public void TestIDataGridLayoutIdentifierRoot()
		{
			using (var grid = new ZFilterGrid())
			using (var module = new DummyFilterGridModule())
			{
				AssertNull("PreCondition:IdentifierRootID being null should not be a problem", grid.IdentifierRootID);
				AssertNoExceptionThrown(delegate
				{ var result = new DataGridLayoutContextKeyProvider(grid).ContextKeyForStmModuleFilter; });

				grid.SetParentFilterGridModule(module);

				IDataGridLayoutIdentifierRoot idRoot = grid;
				AssertEquals("IDataGridLayoutIdentifierRoot.ID", "", idRoot.ID);
				AssertEquals("ColumnLayoutContext", DummyModuleIDs.Dummy.Name, grid.ColumnLayoutContext);
			}

			using (var grid = new ZFilterGrid())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.CreateNewWithCountry(ModuleIDs.Customs.JobDeclaration, "AU"))
			{
				grid.SetParentFilterGridModule(module);

				IDataGridLayoutIdentifierRoot idRoot = grid;
				AssertEquals("IDataGridLayoutIdentifierRoot.ID", "AU", idRoot.ID);
				AssertEquals("ColumnLayoutContext", ModuleIDs.Customs.JobDeclaration.Name, grid.ColumnLayoutContext);
			}
		}
	}
}
