using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualComponentsToVisualControlsConverterTest : TestCase
	{
		public void TestSumOfColumnsFillWeightValuesExceedMaximumValue()
		{
			var visualComponents = new List<VisualiserComponent>();
			var dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");
			dS.Tables.Add("Tbl");

			for (int i = 0; i < 680; i++)
			{
				dS.Tables["Tbl"].Columns.Add("Col" + i);
			}

			visualComponents.Add(new VisualiserComponentGrid(new Point(0, 120), new Size(1, 120), dS, "Tbl"));

			using (var form = new Form())
			{
				AssertEquals(0, form.Controls.Count);

				var vis = new VisualiserComponentsToVisualControlsConverter(form);
				vis.Draw(visualComponents);
				AssertNoExceptionThrown(form.Show);
			}
		}

		public void TestCreateControls()
		{
			List<VisualiserComponent> visualComponents = new List<VisualiserComponent>();
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");
			dS.Tables.Add("Tbl");
			dS.Tables["Tbl"].Columns.Add("Col1");
			dS.Tables["Tbl"].Columns.Add("Col2");

			Size labelSize = new Size(70, 30);
			visualComponents.Add(new VisualiserComponentLabel(new Point(10, 10), labelSize, "Label 1", new CellFormat()));
			visualComponents.Add(new VisualiserComponentLabel(new Point(10, 40), labelSize, "Label 2", new CellFormat()));
			visualComponents.Add(new VisualiserComponentLabel(new Point(10, 70), labelSize, "Label 3", new CellFormat()));

			Size textBoxSize = new Size(120, 30);
			visualComponents.Add(new VisualiserComponentTextBox(new Point(100, 10), textBoxSize, dS, "Test", new CellFormat()));
			visualComponents.Add(new VisualiserComponentTextBox(new Point(100, 40), textBoxSize, dS, "Test", new CellFormat()));
			visualComponents.Add(new VisualiserComponentTextBox(new Point(100, 70), textBoxSize, dS, "Test", new CellFormat()));

			visualComponents.Add(new VisualiserComponentGrid(new Point(0, 120), new Size(1, 120), dS, "Tbl"));

			visualComponents.Add(new VisualiserComponentBorder(new Point(0, 0), new Size(100, 120), new CellFormat()));

			using (var form = new Form())
			{
				AssertEquals(0, form.Controls.Count);

				VisualiserComponentsToVisualControlsConverter vis = new VisualiserComponentsToVisualControlsConverter(form);
				vis.Draw(visualComponents);
				form.Show();
				AssertEquals(7, form.Controls.Count);

				AssertEquals(typeof(ZLabel), form.Controls[0].GetType());
				AssertEquals(typeof(ZLabel), form.Controls[1].GetType());
				AssertEquals(typeof(ZLabel), form.Controls[2].GetType());

				AssertEquals(typeof(ZTextBox), form.Controls[3].GetType());
				AssertEquals(typeof(ZTextBox), form.Controls[4].GetType());
				AssertEquals(typeof(ZTextBox), form.Controls[5].GetType());

				AssertEquals(typeof(DataGridView), form.Controls[6].GetType());
				AssertEquals(217, form.Controls[6].Width);
				AssertEquals(2, ((DataGridView)form.Controls[6]).Columns.Count);
				AssertEquals("1", ((DataGridView)form.Controls[6]).Columns[0].HeaderText);
				AssertEquals("2", ((DataGridView)form.Controls[6]).Columns[1].HeaderText);
			}
		}

		public void TestShowMessageWhenControlValueIsChangedToBeInValid()
		{
			List<VisualiserComponent> visualComponents = new List<VisualiserComponent>();

			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");
			dS.Tables.Add("Tbl");
			dS.Tables["Tbl"].Columns.Add("Col1", typeof(DateTime));

			var expectedDate = DateTime.Now;
			dS.Tables["Tbl"].Rows.Add(expectedDate);
			visualComponents.Add(new VisualiserComponentGrid(new Point(0, 0), new Size(1, 120), dS, "Tbl"));

			using (var form = new Form())
			{
				AssertEquals("Precondition: LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, form.Controls.Count);

				VisualiserComponentsToVisualControlsConverter vis = new VisualiserComponentsToVisualControlsConverter(form);
				vis.Draw(visualComponents);
				form.Show();
				AssertEquals(1, form.Controls.Count);

				var grid = form.Controls[0] as DataGridView;
				AssertNotNull(grid);

				var gridCell = grid[0, 0];
				AssertEquals("Grid has a cell with the DateTime value", expectedDate, gridCell.Value);

				gridCell.Value = "1";
				Assert("A global message is shown.", !string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				AssertEquals("Cannot change the cell to an invalid value", expectedDate, gridCell.Value);
			}
		}
	}
}
