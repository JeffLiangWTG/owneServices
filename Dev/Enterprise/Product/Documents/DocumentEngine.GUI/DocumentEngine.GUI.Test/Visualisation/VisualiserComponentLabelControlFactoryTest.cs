using System.Drawing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation
{
	sealed class VisualiserComponentLabelControlFactoryTest : TestCase
	{
		public void TestBasicProperties()
		{
			VisualiserComponentLabel componentLabel = new VisualiserComponentLabel(new Point(1, 2), new Size(80, 10), "Test Label", new CellFormat());

			AssertEquals("Location", new Point(1, 2), componentLabel.Location);
			AssertEquals("Height", 10, componentLabel.Size.Height);
			AssertEquals("Width", 80, componentLabel.Size.Width);
			AssertEquals("Caption", "Test Label", componentLabel.Caption);

			using (ZLabel renderedLabel = new VisualiserComponentLabelControlFactory().Create(componentLabel))
			{
				AssertEquals("Location", new Point(1, 2), renderedLabel.Location);
				AssertEquals("Height", 10, renderedLabel.Size.Height);
				AssertEquals("Width", 80, renderedLabel.Size.Width);
				AssertEquals("Caption", "Test Label", renderedLabel.Text);
			}
		}

		public void TestCellFont()
		{
			CellFormat cellFormat = new CellFormat();
			cellFormat.FontName = "Times New Roman";
			cellFormat.FontSize = 46;
			VisualiserComponentLabel componentLabel = new VisualiserComponentLabel(new Point(10, 20), new Size(100, 25), "TestLbl", cellFormat);
			using (ZLabel renderedLabel = new VisualiserComponentLabelControlFactory().Create(componentLabel))
			{
				AssertEquals("Times New Roman", renderedLabel.Font.Name);
				AssertEquals((float)46, renderedLabel.Font.Size);
			}
		}

		public void TestColorsAndFillings()
		{
			CellFormat cellFormat = new CellFormat();
			cellFormat.TextColor = Color.Blue;
			cellFormat.BackgroundColor = Color.Yellow;
			cellFormat.FillPattern = FillPatternStyle.Solid;

			VisualiserComponentLabel componentLabel = new VisualiserComponentLabel(new Point(10, 20), new Size(100, 25), "TestLbl", cellFormat);
			using (ZLabel renderedLabel = new VisualiserComponentLabelControlFactory().Create(componentLabel))
			{
				AssertEquals(Color.Blue, renderedLabel.ForeColor);
				AssertEquals(Color.Yellow, renderedLabel.BackColor);
			}
		}
	}
}
