using System.Drawing;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	sealed class VisualiserTabPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestDisposingImages()
		{
			using (var pictureBox = new PictureBox())
			using (var bitmap = new Bitmap(1, 1))
			{
				pictureBox.Image = bitmap;

				using (var tabPage = new VisualiserTabPage())
				{
					tabPage.Controls.Add(pictureBox);
					AssertEquals("Image should not be disposed.", false, pictureBox.Image.IsDisposed());
				}

				AssertEquals("Image should be disposed.", true, pictureBox.Image.IsDisposed());
			}
		}
	}
}
