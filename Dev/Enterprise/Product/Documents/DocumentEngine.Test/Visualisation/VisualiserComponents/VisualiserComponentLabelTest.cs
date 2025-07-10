using System.Drawing;
using Enterprise.DocumentEngineIntegration;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualiserComponentLabelTest : TestCase
	{
		public void TestValueGetter()
		{
			VisualiserComponentLabel label = new VisualiserComponentLabel(new Point(0, 0), new Size(80, 16), delegate
			{ return "Hello World"; }, new CellFormat());
			AssertNull("Pre-condition: label.Caption", label.Caption);
			label.ReadCaption();
			AssertEquals("label.Caption", "Hello World", label.Caption);
		}
	}
}
