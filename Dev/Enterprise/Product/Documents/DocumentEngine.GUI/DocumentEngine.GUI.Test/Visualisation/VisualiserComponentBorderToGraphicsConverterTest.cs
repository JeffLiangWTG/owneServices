using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserComponentBorderToGraphicsConverterTest : TestCase
	{
		public void TestCreateGraphics()
		{
			var visualComponents = new List<VisualiserComponent>();

			var bordersCellFormat = new CellFormat();
			bordersCellFormat.Borders.Bottom.BorderStyle = CellBorderStyle.Thin;

			visualComponents.Add(new VisualiserComponentLabel(new Point(10, 10), new Size(70, 30), "Label 1", bordersCellFormat));
			visualComponents.Add(new VisualiserComponentTextBox(new Point(100, 10), new Size(120, 30), null, "Test", bordersCellFormat));
			visualComponents.Add(new VisualiserComponentGrid(new Point(0, 120), new Size(1, 120), null, "Tbl"));
			visualComponents.Add(new VisualiserComponentBorder(new Point(10, 10), new Size(100, 100), bordersCellFormat));

			using (Form form = new Form())
			{
				AssertEquals(0, form.Controls.Count);

				var visualiser = new VisualiserComponentBorderToGraphicsConverter(form);
				visualiser.Draw(visualComponents);
				form.Show();
				AssertEquals("form.Controls.Count", 0, form.Controls.Count);
			}
		}
	}
}
