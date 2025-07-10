using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	sealed class TableColumnCalculatorTest : TestCaseWithFactory
	{
		public void TestGetTableColumns()
		{
			var bizO = Factory.New<DummyDependantBusinessObject>();
			using (var grid = new ZGrid())
			using (var columns = new ZGridColumns(grid))
			{
				CreateDummyColumns(columns, bizO);
				var tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(bizO, columns);
				AssertEquals(2, tableColumns.Length);
				AssertEquals("ZD1_Z0", tableColumns[0].ColumnName);
				AssertEquals("DummyBizo", tableColumns[0].TableName);
				AssertEquals("CantFetchHintOnMe+ZD1_Z0", tableColumns[1].ColumnName);
				AssertEquals("", tableColumns[1].TableName);
			}
		}

		public void TestGetTableColumns_AllColumns()
		{
			var bizO = Factory.New<DummyDependantBusinessObject>();
			using (var grid = new ZGrid())
			using (var columns = new ZGridColumns(grid))
			{
				CreateDummyColumns(columns, bizO);
				var tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject(bizO, columns, true);
				AssertEquals(3, tableColumns.Length);
				AssertEquals("ZD1_Z0", tableColumns[0].ColumnName);
				AssertEquals("ZD1_Code", tableColumns[1].ColumnName);
				AssertEquals("CantFetchHintOnMe+ZD1_Z0", tableColumns[2].ColumnName);
			}
		}

		void CreateDummyColumns(ZGridColumns columns, DummyDependantBusinessObject bizO)
		{
			var columnInfo1 = new ZGuidFindBoxColumnStyleInfo();
			columnInfo1.ColumnName = DummyDependantBusinessObject.Schema.ZD1_Z0;
			columnInfo1.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			columnInfo1.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
			columnInfo1.Width = 80;
			columns.Add(columnInfo1);
			var columnStyle = columns[0].ColumnStyle as ZGuidFindBoxColumnStyle;
			((ZGridFindBox)columnStyle.EditControl).List = bizO.PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett;

			var columnInfo2 = new ZCodeFindBoxColumnStyleInfo();
			columnInfo2.IsVisible = false;
			columnInfo2.ColumnName = DummyDependantBusinessObject.Schema.ZD1_Code;
			columnInfo2.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			columnInfo2.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
			columnInfo2.Width = 80;
			columns.Add(columnInfo2);

			var columnInfo3 = new ZGuidFindBoxColumnStyleInfo();
			columnInfo3.ColumnName = "CantFetchHintOnMe+" + DummyDependantBusinessObject.Schema.ZD1_Z0;
			columnInfo3.ModuleID = Modules.Testing.DummyModuleIDs.Dummy;
			columnInfo3.BindToList = "PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett";
			columnInfo3.Width = 80;
			columns.Add(columnInfo3);
			columnStyle = columns[0].ColumnStyle as ZGuidFindBoxColumnStyle;
			((ZGridFindBox)columnStyle.EditControl).List = bizO.PotentialDummiesNotYouClintYoureAnActualDummy_ScrewuBrett;
		}
	}
}
