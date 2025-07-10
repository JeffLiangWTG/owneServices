using System.Drawing;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.Visualisation;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserComponentImageControlFactoryTest : TestCase
	{
		public void TestTVCImage()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly))
			using (var soapBubbles = Image.FromFile(resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.Soap Bubbles.bmp")))
			{
				var image = new VisualiserComponentImage(new Point(10, 20), new Size(100, 25), soapBubbles);
				using (var renderedPictureBox = new VisualiserComponentImageControlFactory().Create(image))
				{
					AssertEquals("renderedPictureBox.Location", new Point(10, 20), renderedPictureBox.Location);
					AssertEquals("renderedPictureBox.Size", new Size(100, 25), renderedPictureBox.Size);
					AssertEquals("renderedPictureBox.SizeMode", PictureBoxSizeMode.StretchImage, renderedPictureBox.SizeMode);

					AssertEquals(soapBubbles.Width, renderedPictureBox.Image.Width);
					AssertEquals(soapBubbles.Height, renderedPictureBox.Image.Height);
				}
			}
		}
	}
}
