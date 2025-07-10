using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DataGridLayoutDataSetSerialiserTest : TestCaseWithDummy
	{
		public void TestLayoutStream()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			var layout2 = Factory.New<StmModuleFilter>();
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				//should correctly serialise columns
				using (var layoutStream = serialiser.GetLayoutStream(form.TabGrid.Columns))
				{
					layout1.S9_ColumnLayoutData = layoutStream.ToArray();
					layout1.S9_FilterName = "DescriptionVisible";
					layout1.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layout1.S9_SaveColumnLayout = true;
				}

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				//should correctly serialise columns
				using (var layoutStream = serialiser.GetLayoutStream(form.TabGrid.Columns))
				{
					layout2.S9_ColumnLayoutData = layoutStream.ToArray();
					layout2.S9_FilterName = "NumberVisible";
					layout2.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layout2.S9_SaveColumnLayout = true;
				}

				Factory.Save();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.CurrentColumnLayout = layout1;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);

				form.TabGrid.CurrentColumnLayout = layout2;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);

				AssertEquals(true, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible);
				AssertEquals(false, form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestIsSavedAsUnscaled()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(96);
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(96);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(144);

				using (var tables = serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid))
				{
					var formattingTable = tables.Tables[0];

					var numberRow = FindRowWithMappingName(formattingTable, DummyBizoSchema.Constants.Z0_Number);

					AssertEquals("We should be saving the unscaled value so that we can rescale it on a different DPI", 96, numberRow[1]);

					var descriptionRow = FindRowWithMappingName(formattingTable, DummyBizoSchema.Constants.Z0_Description);
					AssertEquals("We should be saving the unscaled value so that we can rescale it on a different DPI", 144, descriptionRow[1]);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:Do Not Use DataSet", Justification = "Baseline")]
		public void TestIsLoadedAsUnscaled()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			using (var form = new ZTestGridForm(Dummy))
			{
				//create something to be loaded
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(96);

				var numberColumn = form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number];
				numberColumn.IsVisible = true;
				numberColumn.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(96);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].ColumnStyle.Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(144);

				using (var layoutStream = serialiser.GetLayoutStream(form.TabGrid.Columns))
				{
					layout1.S9_ColumnLayoutData = layoutStream.ToArray();
					layout1.S9_FilterName = "DescriptionVisible";
					layout1.S9_ModuleID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					layout1.S9_SaveColumnLayout = true;
				}

				Factory.Save();
			}

			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.CurrentColumnLayout = layout1;
				new DataGridLayoutManager().LoadUserLayoutSettings(form.TabGrid);
				var serialiser = new DataGridLayoutDataSetSerialiser();

				using (var tables = serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid))
				{
					var formattingTable = tables.Tables[0];

					var numberRow = FindRowWithMappingName(formattingTable, DummyBizoSchema.Constants.Z0_Number);
					AssertEquals("We should be loading a value that is unscaled", 96, numberRow[1]);

					var descriptionRow = FindRowWithMappingName(formattingTable, DummyBizoSchema.Constants.Z0_Description);
					AssertEquals("We should be loading a value that is unscaled", 144, descriptionRow[1]);
				}
			}
		}

		DataRow FindRowWithMappingName(DataTable table, string mappingName)
		{
			for (var i = 0; i < table.Rows.Count; i++)
			{
				var row = table.Rows[i];
				if (row[0].ToString() == mappingName)
				{
					return row;
				}
			}

			Fail("No row in table to match mapping name.");
			return null;
		}

		#region TestLayoutStreamWithICustomizableColumn

		public void TestLayoutStreamWithICustomizableColumn()
		{
			var columns =
				new List<ICustomizableColumn>
				{
					new CustomizableColumn { Caption = "test1", ColumnName = "name1", IsVisible = true },
					new CustomizableColumn { Caption = "test2", ColumnName = "name2", IsVisible = false },
					new CustomizableColumn { Caption = "test3", ColumnName = "name3", IsVisible = true },
					new CustomizableColumn { Caption = "test4", ColumnName = "name4", IsVisible = false },
					new CustomizableColumn { Caption = "test5", IsVisible = true }
				};

			var serialiser = new DataGridLayoutDataSetSerialiser();
			using (var layoutStream = serialiser.GetLayoutStream(columns))
			{
				var visibleColumns = new List<string>(serialiser.GetVisibleColumnNames(layoutStream));

				AssertEquals(3, visibleColumns.Count);
				AssertEquals("name1", visibleColumns[0]);
				AssertEquals("name3", visibleColumns[1]);
				AssertEquals("test5", visibleColumns[2]);
			}
		}

		#endregion

		public void TestGetVisibleColumnNames()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var serialiser = new DataGridLayoutDataSetSerialiser();

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					var visibleColumnNames = new List<string>(serialiser.GetVisibleColumnNames(layoutStream));
					AssertEquals("Two columns are visible", 2, visibleColumnNames.Count);
					Assert(visibleColumnNames.Contains(DummyBizoSchema.Constants.Z0_Number));
					Assert(visibleColumnNames.Contains(DummyBizoSchema.Constants.Z0_Description));
				}

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;

				using (var layoutStream = serialiser.GetLayoutStreamFromDataSet(serialiser.SerialiseAndGetCurrentLayoutAsDataset(form.TabGrid)))
				{
					var visibleColumnNames = new List<string>(serialiser.GetVisibleColumnNames(layoutStream));
					AssertEquals("One column is visible", 1, visibleColumnNames.Count);
					AssertEquals("One column is visible", DummyBizoSchema.Constants.Z0_Number, visibleColumnNames[0]);
				}
			}
		}

		public void TestGetVisibleColumnNamesWithInvalidData()
		{
			var serialiser = new DataGridLayoutDataSetSerialiser();
			var layoutStream = new MemoryStream(Encoding.UTF8.GetBytes("bad data"));
			var visibleColumnNames = new List<string>(serialiser.GetVisibleColumnNames(layoutStream));
			AssertEquals(0, visibleColumnNames.Count);
		}

		public void TestGetSortSettingTableWithCustomProperty()
		{
			using (var form = new ZTestGridForm(Dummy))
			{
				((IBindingList)Dummy.Collection).ApplySort(new ZCustomPropertyDescriptor("Notes", typeof(string)), ListSortDirection.Ascending);
				form.Show();

				var serialiser = new DataGridLayoutDataSetSerialiser();
				var layoutSortTable = serialiser.GetSortSettingTable(form.TabGrid);

				AssertEquals(0, layoutSortTable.Rows.Count);
			}
		}
	}
}
