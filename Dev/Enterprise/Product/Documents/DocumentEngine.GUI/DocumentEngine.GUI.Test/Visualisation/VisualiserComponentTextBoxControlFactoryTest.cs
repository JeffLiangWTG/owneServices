using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserComponentTextBoxControlFactoryTest : TestCase
	{
		public void TestAllHorizontalAlignmentsCateredFor()
		{
			CombineAssertions(delegate
			{
				foreach (HorizontalTextAlignment alignment in Enum.GetValues(typeof(HorizontalTextAlignment)))
				{
					AssertNoExceptionThrown(alignment.ToString() + " must be catered for in GetHorizontalAlignment() method.", delegate
					{
						VisualiserComponentTextBoxControlFactory.GetHorizontalAlignmentForTesting(alignment);
					});
				}
			});
		}

		public void TestControlGetsAlignment()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");

			CellFormat cellFormat = new CellFormat();

			cellFormat.HTextAlign = HorizontalTextAlignment.General;
			VisualiserComponentTextBox textBox = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(textBox))
			{
				AssertEquals("renderedTextBox.TextAlign", HorizontalAlignment.Left, renderedTextBox.TextAlign);
			}

			cellFormat.HTextAlign = HorizontalTextAlignment.Left;
			textBox = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(textBox))
			{
				AssertEquals("renderedTextBox.TextAlign", HorizontalAlignment.Left, renderedTextBox.TextAlign);
			}

			cellFormat.HTextAlign = HorizontalTextAlignment.Centre;
			textBox = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(textBox))
			{
				AssertEquals("renderedTextBox.TextAlign", HorizontalAlignment.Center, renderedTextBox.TextAlign);
			}

			cellFormat.HTextAlign = HorizontalTextAlignment.Right;
			textBox = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(textBox))
			{
				AssertEquals("renderedTextBox.TextAlign", HorizontalAlignment.Right, renderedTextBox.TextAlign);
			}
		}

		[ExpectNoExceptions]
		public void TestTransparentBackgroundColor()
		{
			var dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");

			var cellFormat = new CellFormat();
			cellFormat.BackgroundColor = Color.Transparent;
			cellFormat.FillPattern = FillPatternStyle.Automatic;
			cellFormat.HTextAlign = HorizontalTextAlignment.General;
			var textBox = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);

			ErrorReporter.Clear();
			using (var renderedTextBox = ControlFactory.Create(textBox))
			{
				AssertEquals("should have no error", 0, ErrorReporter.TotalErrorCount);
			}

			cellFormat.BackgroundColor = Color.FromArgb(0, 0, 0, 0);
			var textBox2 = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cellFormat);

			ErrorReporter.Clear();
			using (var renderedTextBox2 = ControlFactory.Create(textBox2))
			{
				AssertEquals("should have no error", 0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestBasicProperties()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");

			VisualiserComponentTextBox box = new VisualiserComponentTextBox(ControlDpiScalingHelper.NewScaledPoint(10, 20), ControlDpiScalingHelper.NewScaledSize(100, 25), dS, "Test", new CellFormat());
			using (ZTextBox renderedTextBox = ControlFactory.Create(box))
			{
				AssertEquals("Location", ControlDpiScalingHelper.NewScaledPoint(12, 22), renderedTextBox.Location);
				AssertEquals("Size", ControlDpiScalingHelper.NewScaledSize(95, 22), renderedTextBox.Size);
				AssertEquals("Data source", dS.MainTable, renderedTextBox.DataBindings[0].DataSource);
				AssertEquals("property name", "Text", renderedTextBox.DataBindings[0].PropertyName);
				AssertEquals("Multiline", true, renderedTextBox.Multiline);
				AssertEquals(CharacterCasing.Normal, renderedTextBox.CharacterCasing);
			}
		}

		public void TestCellFont()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");

			CellFormat cF = new CellFormat();
			cF.FontName = "Times New Roman";
			cF.FontSize = 46;
			VisualiserComponentTextBox box = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cF);
			using (ZTextBox renderedTextBox = ControlFactory.Create(box))
			{
				AssertEquals("Times New Roman", renderedTextBox.Font.Name);
				AssertEquals((float)46, renderedTextBox.Font.Size);
			}
		}

		public void TestColorsAndFillings()
		{
			var dataSet = new VisualiserDataSet();
			dataSet.MainTable.Columns.Add("Test");

			var cellFormat = new CellFormat();
			cellFormat.TextColor = Color.Blue;
			cellFormat.BackgroundColor = Color.Yellow;
			cellFormat.FillPattern = FillPatternStyle.Solid;

			var boxWithFillPatern = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dataSet, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(boxWithFillPatern))
			{
				AssertEquals(Color.Blue, renderedTextBox.ForeColor);
				AssertEquals(Color.Yellow, renderedTextBox.BackColor);
			}

			cellFormat.FillPattern = FillPatternStyle.None;
			var boxWithoutFillPatern = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dataSet, "Test", cellFormat);
			using (ZTextBox renderedTextBox = ControlFactory.Create(boxWithFillPatern))
			{
				AssertEquals(Color.Blue, renderedTextBox.ForeColor);
				AssertEquals(Color.AliceBlue, renderedTextBox.BackColor);
			}
		}

		public void TestAcceptChangesOnLeave()
		{
			VisualiserDataSet dS = new VisualiserDataSet();
			dS.MainTable.Columns.Add("Test");
			dS.MainRow["Test"] = "blah";

			CellFormat cF = new CellFormat();
			VisualiserComponentTextBox box = new VisualiserComponentTextBox(new Point(10, 20), new Size(100, 25), dS, "Test", cF);
			using (Form frm = new Form())
			{
				using (ZTextBox anotherTextBox = new ZTextBox())
				{
					using (ZTextBox renderedTextBox = ControlFactory.Create(box))
					{
						frm.Controls.Add(renderedTextBox);
						frm.Controls.Add(anotherTextBox);
						frm.Show();
						renderedTextBox.Focus();
						AssertEquals("blah", renderedTextBox.Text);
						renderedTextBox.Text = "boo";
						anotherTextBox.Focus();
						AssertEquals("boo", dS.MainRow["Test"]);
					}
				}
			}
		}

		#region Implementation

		VisualiserComponentTextBoxControlFactory ControlFactory
		{
			get { return controlFactory ?? (controlFactory = new VisualiserComponentTextBoxControlFactory()); }
		}
		VisualiserComponentTextBoxControlFactory controlFactory;

		#endregion
	}
}
