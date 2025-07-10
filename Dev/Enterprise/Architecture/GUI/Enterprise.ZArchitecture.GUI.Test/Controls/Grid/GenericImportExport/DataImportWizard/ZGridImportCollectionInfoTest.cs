using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

#pragma warning disable CW1108 // Do Not Use DataSet

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	sealed class ZGridImportCollectionInfoTest : TestCase
	{
		public void TestProperties()
		{
			var data = new DataSet(); // for testing only

			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1", typeof(ZDecimal));

			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				grid.GridId = Guid.NewGuid().ToString();
				grid.Columns.Add(new ZCalcEditColumnStyleInfo("Column1", 80, 3));

				form.Controls.Add(grid);
				grid.SetDataBinding(data, "Table");

				var info = new ZGridImportCollectionInfo(grid);
				AssertEquals("valid column count", 1, info.Properties.Count());
				AssertEquals(typeof(ZDecimal), info.Properties.First().PropertyType);
				AssertEquals(typeof(DataRowView), info.Properties.First().ComponentType);
				grid.Columns[0].IsVisible = false;
				info = new ZGridImportCollectionInfo(grid);
				AssertEquals("hidden columns not imported", 0, info.Properties.Count());
			}
		}

		public void TestProperties_InvalidColumns()
		{
			using (var grid = new ZGrid())
			{
				grid.Columns.AddBoolColumn("ColA", 100);
				var info = new ZGridImportCollectionInfo(grid);
				AssertEquals("column without property descriptor not imported", 0, info.Properties.Count());
			}
		}

		public void TestProperty_WithIZType_ReturnsActualType()
		{
			var data = new DataSet(); // for testing only
			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1", typeof(IZType));
			table.Columns.Add("Column2", typeof(IZType));
			table.Columns.Add("Column3", typeof(IZType));

			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				grid.GridId = Guid.NewGuid().ToString();
				grid.Columns.Add(new ZCalcEditColumnStyleInfo("Column1", 80, 3));
				grid.Columns.Add(new ZTextBoxColumnStyleInfo("Column2", 80));
				grid.Columns.Add(new ZCheckBoxColumnStyleInfo("Column3", 80));

				form.Controls.Add(grid);
				grid.SetDataBinding(data, "Table");

				var info = new ZGridImportCollectionInfo(grid);
				AssertEquals("valid column count", 3, info.Properties.Count());
				AssertEquals(typeof(ZDecimal), info.Properties.First(x => x.MappingName == "Column1").PropertyType);
				AssertEquals(typeof(ZString), info.Properties.First(x => x.MappingName == "Column2").PropertyType);
				AssertEquals(typeof(IZType), info.Properties.First(x => x.MappingName == "Column3").PropertyType);
				AssertEquals(typeof(DataRowView), info.Properties.First().ComponentType);
			}
		}

		public void TestGridGetFocusedAfterImport()
		{
			var data = new DataSet(); // for testing only
			var table = data.Tables.Add("Table");
			table.Columns.Add("Column1", typeof(ZDecimal));

			using (var form = new ZForm())
			{
				var grid = new ZGrid();
				grid.GridId = Guid.NewGuid().ToString();
				grid.Columns.Add(new ZCalcEditColumnStyleInfo("Column1", 80, 3));

				form.Controls.Add(grid);
				grid.SetDataBinding(data, "Table");

				form.Show();
				Application.DoEvents();

				var info = new ZGridImportCollectionInfo(grid);
				AssertEquals("Grid do not contains focus", false, grid.Focused);
				(info as IImportCollectionInfo).OnImportCompleted(true);
				AssertEquals("Grid contains focus", true, grid.Focused);
			}
		}

		[ExpectNoExceptions]
		public void TestFieldTypeToZType()
		{
			CombineAssertions(() =>
			{
				foreach (var fieldType in Enum.GetValues(typeof(FieldType)).Cast<FieldType>())
				{
					try
					{
						ZGridImportPropertyInfo.FieldTypeToZType(fieldType);
					}
					catch (NotSupportedException)
					{
						// That's all right, this FieldType is not supposed to be on a grid
					}
					catch (ArgumentOutOfRangeException e)
					{
						Fail(e.Message);
					}
				}
			});
		}
	}
}
