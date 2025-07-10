using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.BE.GUI.Testing;

class ExtensionsTest : TestCaseWithFactory
{
	public void TestRemoveUnneededColumnsColumns()
	{
		using (var grid = new ZGrid())
		{
			ZGridColumnInfo[] columnsToAdd =
			{
				new ZTextBoxColumnStyleInfo("column1", 100),
				new ZTextBoxColumnStyleInfo("column2", 100),
				new ZTextBoxColumnStyleInfo("column3", 100),
			};
			var columnsToKeep = new string[] { "column1", "column3" };

			grid.ColumnStyles.AddRange(columnsToAdd);

			grid.RemoveUnneededColumns(columnsToKeep);

			CombineAssertions(() =>
			{
				AssertEquals(2, grid.ColumnStyles.Count);
				AssertArrayEqualsByElements(columnsToKeep, grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName).ToArray());
			});
		}
	}

	public void TestRemoveColumn()
	{
		using (var grid = new ZGrid())
		{
			ZGridColumnInfo[] columnsToAdd =
			{
				new ZTextBoxColumnStyleInfo("column1", 100),
				new ZTextBoxColumnStyleInfo("column2", 100),
				new ZTextBoxColumnStyleInfo("column3", 100),
			};

			grid.ColumnStyles.AddRange(columnsToAdd);

			grid.RemoveColumn("column2");

			CombineAssertions(() =>
			{
				AssertEquals(2, grid.ColumnStyles.Count);
				AssertArrayEqualsByElements(new string[] { "column1", "column3" }, grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName).ToArray());
			});
		}
	}

	public void TestSetResourceStringForColumn()
	{
		using (var grid = new ZGrid())
		{
			var columnName = "column1";
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(columnName, 100));

			grid.SetResourceStringForColumn(columnName, NoResourceStringData.GetData("columnname"));

			AssertEquals("columnname", grid.ColumnStyles.OfType<ZGridColumnInfo>().SingleOrDefault(x => x.ColumnName == columnName)?.CaptionResourceString.Caption);
		}
	}
}
