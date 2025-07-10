using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserComponentGridControlFactoryTest : TestCaseWithFactory
	{
		[GuiTest]
		[RequiresSTA]
		public void TestDataGridViewCheckBoxDoesNotThrowException()
		{
			DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
			businessObject.Z0_Bool = true;

			VisualiserDataSet dataSet = new VisualiserDataSet();

			dataSet.MainTable.Columns.Add("Z0_Bool", typeof(bool));
			dataSet.VisualiserGridStyles.Add(new VisualiserDataSet.VisualiserGridStyle("MainBizObject", "Z0_Bool", false, new CellFormat(), Size.Empty));

			using (ZForm form = new ZForm(businessObject))
			{
				VisualiserComponentGrid component = new VisualiserComponentGrid(Point.Empty, Size.Empty, dataSet, "MainBizObject");
				DataGridView control = new VisualiserComponentGridControlFactory().Create(component);
				control.DataError += (sender, e) => throw e.Exception;

				form.Controls.Add(control);
				AssertNoExceptionThrown("Showing the form should not throw exceptions for checkbox columns.", form.Show);
			}
		}

		public void TestTVCGrid()
		{
			DataTable table;
			VisualiserDataSet dataSet = new VisualiserDataSet();
			table = new DataTable("TblName");
			table.Columns.Add("StrCol", typeof(string));
			Enterprise.DocumentEngine.Visualisation.VisualiserDataSet.VisualiserGridStyle visGridStyle1 =
				new VisualiserDataSet.VisualiserGridStyle("TblName", "StrCol", false, new CellFormat() { HTextAlign = HorizontalTextAlignment.Left }, ControlDpiScalingHelper.NewScaledSize(50, 10));
			dataSet.VisualiserGridStyles.Add(visGridStyle1);

			table.Columns.Add("DecCol", typeof(decimal));
			Enterprise.DocumentEngine.Visualisation.VisualiserDataSet.VisualiserGridStyle visGridStyle2 =
				new VisualiserDataSet.VisualiserGridStyle("TblName", "DecCol", true, new CellFormat() { HTextAlign = HorizontalTextAlignment.Right }, ControlDpiScalingHelper.NewScaledSize(20, 10));
			dataSet.VisualiserGridStyles.Add(visGridStyle2);

			dataSet.Tables.Add(table);

			table.Rows.Add("Line 1", 3242.2352m);
			table.Rows.Add("Line 2", 3523.22352m);
			table.Rows.Add("Line 3", 3242.2552m);
			table.Rows.Add("Line 4", 6142.2352m);
			table.Rows.Add("Line 5", 3242.24122m);

			VisualiserComponentGrid grid1 = new VisualiserComponentGrid(new Point(1, 2), new Size(60, 30), dataSet, "TblName");

			AssertEquals("Location", new Point(1, 2), grid1.Location);
			AssertEquals("Height", 30, grid1.Size.Height);
			AssertEquals("Width", 60, grid1.Size.Width);

			using (DataGridView renderedGrid = new VisualiserComponentGridControlFactory().Create(grid1))
			{
				AssertEquals("Location", new Point(1, 2), renderedGrid.Location);
				AssertEquals("Height", 30, renderedGrid.Size.Height);
				AssertEquals("Width should be VisualiserComponentGrid.Width + GridRowSelectorPlusScrollBarWidth<Scaled(30)>", 60 + ControlDpiScalingHelper.ScaleToCurrentDpiX(30), renderedGrid.Size.Width);
				AssertEquals("RowHeadersWidth", 10, renderedGrid.RowHeadersWidth);
				AssertEquals("RowHeadersWidthSizeMode", DataGridViewRowHeadersWidthSizeMode.DisableResizing, renderedGrid.RowHeadersWidthSizeMode);
				AssertEquals("Dataset", dataSet, renderedGrid.DataSource);
				AssertEquals("DataMember", "TblName", renderedGrid.DataMember);

				using (Form form = new Form())
				{
					form.Controls.Add(renderedGrid);
					form.Show();
					foreach (DataGridViewColumn col in renderedGrid.Columns)
					{
						AssertEquals(DataGridViewColumnSortMode.NotSortable, col.SortMode);
					}

					DataGridViewColumn strCol = renderedGrid.Columns["StrCol"];
					AssertEquals(DataGridViewContentAlignment.MiddleLeft, strCol.DefaultCellStyle.Alignment);
					AssertEquals(strCol.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(50));

					DataGridViewColumn decCol = renderedGrid.Columns["DecCol"];
					AssertEquals(DataGridViewContentAlignment.MiddleRight, decCol.DefaultCellStyle.Alignment);
					AssertEquals(decCol.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(30));
				}
			}
		}

		public void TestColorsAndFillings()
		{
			var dataSet = new VisualiserDataSet();
			var table = new DataTable("Test");
			table.Columns.Add("TestColumn", typeof(string));
			table.Rows.Add("TestRow");
			dataSet.Tables.Add(table);

			var cellFormat = new CellFormat();
			cellFormat.TextColor = Color.Blue;
			cellFormat.BackgroundColor = Color.Yellow;
			cellFormat.FillPattern = FillPatternStyle.Solid;

			var grid = new VisualiserComponentGrid(new Point(10, 20), new Size(100, 25), dataSet, "Test");
			using (DataGridView renderedGrid = ControlFactory.Create(grid))
			{
				AssertEquals("renderedGrid.BackgroundColor", Color.AliceBlue, renderedGrid.BackgroundColor);
				AssertEquals("renderedGrid.DefaultCellStyle.BackColor", Color.AliceBlue, renderedGrid.DefaultCellStyle.BackColor);
				AssertEquals("renderedGrid.DefaultCellStyle.SelectionBackColor", EnterpriseFormLookStrategy.SelectedControlColor, renderedGrid.DefaultCellStyle.SelectionBackColor);
				AssertEquals("renderedGrid.DefaultCellStyle.SelectionForeColor", Color.Black, renderedGrid.DefaultCellStyle.SelectionForeColor);

				using (Form form = new Form())
				{
					form.Controls.Add(renderedGrid);
					Assert("make sure renderedGrid is bind to the datasource", renderedGrid.BindingContext != null);
					AssertEquals("renderedGrid.Rows.Count", 2, renderedGrid.Rows.Count);
					foreach (DataGridViewRow row in renderedGrid.Rows)
					{
						AssertEquals("dataGridRow.DefaultCellStyle.WrapMode", DataGridViewTriState.NotSet, row.DefaultCellStyle.WrapMode);
						AssertEquals("dataGridRow.DefaultCellStyle.Alignment", DataGridViewContentAlignment.NotSet, row.DefaultCellStyle.Alignment);
					}
				}
			}

			grid = new VisualiserComponentGrid(new Point(10, 20), new Size(100, 25), dataSet, "Test", true);
			using (DataGridView renderedGrid = ControlFactory.Create(grid))
			{
				using (Form form = new Form())
				{
					form.Controls.Add(renderedGrid);
					Assert("make sure renderedGrid is bind to the datasource", renderedGrid.BindingContext != null);
					AssertEquals("renderedGrid.Rows.Count", 2, renderedGrid.Rows.Count);
					foreach (DataGridViewRow row in renderedGrid.Rows)
					{
						AssertEquals("dataGridRow.DefaultCellStyle.WrapMode", DataGridViewTriState.True, row.DefaultCellStyle.WrapMode);
						AssertEquals("dataGridRow.DefaultCellStyle.Alignment", DataGridViewContentAlignment.TopLeft, row.DefaultCellStyle.Alignment);
					}
				}
			}
		}

		public void TestModifiableFiledHeaderText()
		{
			var dataSet = new VisualiserDataSet();
			var table = new DataTable("TestDataTable");
			dataSet.Tables.Add(table);
			var grid = new VisualiserComponentGrid(new Point(10, 20), new Size(100, 25), dataSet, "TestDataTable");
			using (var renderedGrid = ControlFactory.Create(grid))
			{
				renderedGrid.Columns.Add("<ModifiableField(\"ABC\", \"BCD\")>", "<ModifiableField(\"ABC\", \"BCD\")>");
				AssertEquals("ABC", renderedGrid.Columns[0].HeaderText);
			}
		}

		#region Implementation

		VisualiserComponentGridControlFactory ControlFactory
		{
			get { return controlFactory ?? (controlFactory = new VisualiserComponentGridControlFactory()); }
		}
		VisualiserComponentGridControlFactory controlFactory;

		#endregion
	}
}
