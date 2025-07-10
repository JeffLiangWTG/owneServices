using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridColumnLayoutTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			var layout = new GridColumnLayout();
			layout.AddColumn(new ZTextBoxColumnStyleInfo("Property1", 100));
			layout.AddColumn(new ZCheckBoxColumnStyleInfo("Property2", 100));

			var gridColumnLayout = (IGridColumnLayout)layout;

			AssertEquals("Count", 2, gridColumnLayout.Columns.Count);
			Assert("Column for Property1", gridColumnLayout.Columns.Any(c => c.ColumnName == "Property1"));
			Assert("Column for Property2", gridColumnLayout.Columns.Any(c => c.ColumnName == "Property2"));
		}

		public void TestAddColumn()
		{
			var layout = new GridColumnLayout();
			AssertNoExceptionThrown(() => layout.AddColumn(new ZTextBoxColumnStyleInfo("Property1", 200)));
			AssertNoExceptionThrown(() => layout.AddColumn(new ZTextBoxColumnStyleInfo("Property2", 200)));
			AssertExceptionThrown<InvalidOperationException>("When Duplicate Column is added",
				expectedExceptionMessage: "Column with the same name already exists: Property1",
				codeToRun: () => layout.AddColumn(new ZCalcEditColumnStyleInfo("Property1", 100, 2)));
		}

		public void TestHasColumn()
		{
			var layout = new GridColumnLayout();
			layout.AddColumn(new ZTextBoxColumnStyleInfo("Property1", 100));
			layout.AddColumn(new ZCheckBoxColumnStyleInfo("Property2", 100));

			var gridColumnLayout = (IGridColumnLayout)layout;
			AssertEquals("Column with name Property1", true, gridColumnLayout.HasColumn("Property1"));
			AssertEquals("Column with name PropertyX", false, gridColumnLayout.HasColumn("PropertyX"));
		}
	}
}
