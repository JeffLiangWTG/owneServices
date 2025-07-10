using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class GridColumnLayoutBuilderTest : TestCaseWithFactory
	{
		public void TestAddColumn_WithGridColumnReferenceAsInput()
		{
			var column1 = new GridColumnReference<ZTextBoxColumnStyleInfo>("Z0_Description", 100);
			var column2 = new GridColumnReference<ZTextBoxColumnStyleInfo>("Z0_Code", 120);
			var column3 = new GridColumnReference<ZTextBoxColumnStyleInfo>("Z0_Description", 150);

			var gridColumnLayoutBuilder = GridColumnLayoutBuilder.Create();

			AssertExceptionThrown<ArgumentNullException>(() => gridColumnLayoutBuilder.AddColumn(null));
			AssertNoExceptionThrown(() => gridColumnLayoutBuilder.AddColumn(column1));
			AssertNoExceptionThrown(() => gridColumnLayoutBuilder.AddColumn(column2));
			AssertExceptionThrown<InvalidOperationException>("Column with a duplicate name is added", () => gridColumnLayoutBuilder.AddColumn(column3));
		}

		public void TestBuild()
		{
			var columnReference = new GridColumnReference<ZTextBoxColumnStyleInfo>("SomeProperty", 100);

			var gridColumnLayoutBuilder = GridColumnLayoutBuilder.Create();
			gridColumnLayoutBuilder.AddColumn(columnReference);
			var layout = gridColumnLayoutBuilder.Build();

			AssertNotNull("GridColumnLayout", layout);
			AssertEquals("Columns Count", 1, layout.Columns.Count);
		}

		public void TestAddColumn_WithTypeAndAdditionalProperties()
		{
			var builder = GridColumnLayoutBuilder.Create();
			builder.AddColumn<ZCalcEditColumnStyleInfo>("Property1", 333, c =>
			{
				c.Decimals = 2;
				c.IsMandatory = true;
			});

			var layout = builder.Build();
			AssertEquals("Has Property1 column", true, layout.HasColumn("Property1"));
			var column = layout.Columns.Single(c => c.ColumnName == "Property1") as ZCalcEditColumnStyleInfo;

			AssertNotNull(column);
			AssertType<ZCalcEditColumnStyleInfo>(column);
			AssertEquals("Decimals", 2, column.Decimals);
			AssertEquals("IsMandatory", true, column.IsMandatory);
		}

		public void TestAddColumn_WithEmptyColumnName()
		{
			var builder = GridColumnLayoutBuilder.Create();
			AssertExceptionThrown<ArgumentException>(() => builder.AddColumn<ZTextBoxColumnStyleInfo>(null, 100));
			AssertExceptionThrown<ArgumentException>(() => builder.AddColumn<ZTextBoxColumnStyleInfo>("", 100));
		}
	}
}
