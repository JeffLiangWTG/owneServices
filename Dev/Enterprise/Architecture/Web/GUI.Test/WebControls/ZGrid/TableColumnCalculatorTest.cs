using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class TableColumnCalculatorTest : TestCaseWithFactory
	{
		public void TestGetTableColumns_ZDataGrid()
		{
			DummyDependantBusinessObject dummy = Factory.New<DummyDependantBusinessObject>();
			var columns = new[]
						  {
							new ZFindBoxColumn("ZD1_Z0", DummyDependantBusinessObject.Schema.ZD1_Z0, "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett")
							{ ModuleID = WebModuleIDs.Dummy },
							new ZFindBoxColumn("ZD1_Code", DummyDependantBusinessObject.Schema.ZD1_Code, "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett")
							{ ModuleID = WebModuleIDs.Dummy, Visible = false },
							new ZFindBoxColumn("CantFetchHintOnMe+ZD1_Z0", "CantFetchHintOnMe+" + DummyDependantBusinessObject.Schema.ZD1_Z0, "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett")
							{ ModuleID = WebModuleIDs.Dummy },
							new ZFindBoxColumn("ZD1_Code", DummyDependantBusinessObject.Schema.ZD1_Code, string.Empty)
							{ ModuleID = WebModuleIDs.Dummy }
						  };

			TableColumn[] tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(dummy, columns);
			AssertEquals(3, tableColumns.Length);
			AssertEquals("ZD1_Z0", tableColumns[0].ColumnName);
			AssertEquals("DummyBizo", tableColumns[0].TableName);
			AssertEquals("CantFetchHintOnMe+ZD1_Z0", tableColumns[1].ColumnName);
			AssertEquals("", tableColumns[1].TableName);
		}
	}
}
