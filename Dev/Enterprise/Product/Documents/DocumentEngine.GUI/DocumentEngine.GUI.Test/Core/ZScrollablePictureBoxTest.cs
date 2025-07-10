using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	using CargoWise.EntityFramework.Testing;

	class ZScrollablePictureBoxTest : TestCaseWithFactory
	{
		[DpiState(DpiState.ScaleX)]
		readonly int scaledPictureBoxWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
		[DpiState(DpiState.ScaleY)]
		readonly int scaledPictureBoxHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(100);

		public void TestImage()
		{
			using (var pictureBox = new ZScrollablePictureBox())
			{
				pictureBox.Width = 100;
				pictureBox.Height = 100;
				CombineAssertions(delegate()
				{
					AssertEquals("Pre-condition: Width", 100, pictureBox.Width);
					AssertEquals("Pre-condition: Height", 100, pictureBox.Height);
					AssertEquals("Pre-condition: Zoom", 100, pictureBox.Zoom);
					AssertEquals("Pre-condition: Image", null, pictureBox.Image);
					AssertEquals("Pre-condition: HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
					AssertEquals("Pre-condition: VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
				});

				using (var image = new Bitmap(100, 100))
				{
					image.SetResolution(100, 100);

					pictureBox.Image = image;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", 100, pictureBox.Image.Width);
						AssertEquals("Image.Height", 100, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Image = null;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", null, pictureBox.Image);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});
				}
			}
		}

		public void TestImageThatOverflows()
		{
			using (var pictureBox = new ZScrollablePictureBox())
			{
				pictureBox.Width = scaledPictureBoxWidth;
				pictureBox.Height = scaledPictureBoxHeight;
				pictureBox.Zoom = 100;
				CombineAssertions(delegate()
				{
					AssertEquals("Pre-condition: Width", scaledPictureBoxWidth, pictureBox.Width);
					AssertEquals("Pre-condition: Height", scaledPictureBoxHeight, pictureBox.Height);
					AssertEquals("Pre-condition: Zoom", 100, pictureBox.Zoom);
					AssertEquals("Pre-condition: Image", null, pictureBox.Image);
					AssertEquals("Pre-condition: HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
					AssertEquals("Pre-condition: VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
				});

				var scaledImageWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
				var scaledImageHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
				using (var image = new Bitmap(scaledImageWidth, scaledImageHeight))
				{
					image.SetResolution(100, 100);

					pictureBox.Image = image;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", scaledImageWidth, pictureBox.Image.Width);
						AssertEquals("Image.Height", scaledImageHeight, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", true, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", true, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Image = null;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", null, pictureBox.Image);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});
				}
			}
		}

		public void TestImageThatOverflowsHorizontally()
		{
			using (var pictureBox = new ZScrollablePictureBox())
			{
				pictureBox.Width = scaledPictureBoxWidth;
				pictureBox.Height = scaledPictureBoxHeight;
				pictureBox.Zoom = 100;
				CombineAssertions(delegate()
				{
					AssertEquals("Pre-condition: Width", scaledPictureBoxWidth, pictureBox.Width);
					AssertEquals("Pre-condition: Height", scaledPictureBoxHeight, pictureBox.Height);
					AssertEquals("Pre-condition: Zoom", 100, pictureBox.Zoom);
					AssertEquals("Pre-condition: Image", null, pictureBox.Image);
					AssertEquals("Pre-condition: HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
					AssertEquals("Pre-condition: VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
				});

				var scaledImageWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
				var scaledImageHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
				using (var image = new Bitmap(scaledImageWidth, scaledImageHeight))
				{
					image.SetResolution(100, 100);

					pictureBox.Image = image;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", scaledImageWidth, pictureBox.Image.Width);
						AssertEquals("Image.Height", scaledImageHeight, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", true, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Image = null;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", null, pictureBox.Image);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});
				}
			}
		}

		public void TestImageThatOverflowsVertically()
		{
			using (var pictureBox = new ZScrollablePictureBox())
			{
				pictureBox.Width = scaledPictureBoxWidth;
				pictureBox.Height = scaledPictureBoxHeight;
				pictureBox.Zoom = 100;
				CombineAssertions(delegate()
				{
					AssertEquals("Pre-condition: Width", scaledPictureBoxWidth, pictureBox.Width);
					AssertEquals("Pre-condition: Height", scaledPictureBoxHeight, pictureBox.Height);
					AssertEquals("Pre-condition: Zoom", 100, pictureBox.Zoom);
					AssertEquals("Pre-condition: Image", null, pictureBox.Image);
					AssertEquals("Pre-condition: HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
					AssertEquals("Pre-condition: VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
				});

				var scaledImageWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
				var scaledImageHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
				using (var image = new Bitmap(scaledImageWidth, scaledImageHeight))
				{
					image.SetResolution(100, 100);

					pictureBox.Image = image;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", scaledImageWidth, pictureBox.Image.Width);
						AssertEquals("Image.Height", scaledImageHeight, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", true, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Image = null;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", scaledPictureBoxWidth, pictureBox.Width);
						AssertEquals("Height", scaledPictureBoxHeight, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("Image", null, pictureBox.Image);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});
				}
			}
		}

		public void TestZoom()
		{
			using (var pictureBox = new ZScrollablePictureBox())
			{
				pictureBox.Width = 100;
				pictureBox.Height = 100;
				pictureBox.Zoom = 100;
				CombineAssertions(delegate()
				{
					AssertEquals("Pre-condition: Width", 100, pictureBox.Width);
					AssertEquals("Pre-condition: Height", 100, pictureBox.Height);
					AssertEquals("Pre-condition: Zoom", 100, pictureBox.Zoom);
					AssertEquals("Pre-condition: Image", null, pictureBox.Image);
					AssertEquals("Pre-condition: HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
					AssertEquals("Pre-condition: VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
				});

				using (var image = new Bitmap(100, 100))
				{
					image.SetResolution(100, 100);

					pictureBox.Image = image;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 100, pictureBox.Zoom);
						AssertEquals("ZoomedImageWidth", 100, pictureBox.ZoomedImageWidth);
						AssertEquals("ZoomedImageHeight", 100, pictureBox.ZoomedImageHeight);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", 100, pictureBox.Image.Width);
						AssertEquals("Image.Height", 100, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Zoom = 200;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 200, pictureBox.Zoom);
						AssertEquals("ZoomedImageWidth", 200, pictureBox.ZoomedImageWidth);

						AssertEquals("ZoomedImageHeight", 200, pictureBox.ZoomedImageHeight);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", 100, pictureBox.Image.Width);
						AssertEquals("Image.Height", 100, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", true, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", true, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Zoom = 150;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 150, pictureBox.Zoom);
						AssertEquals("ZoomedImageWidth", 150, pictureBox.ZoomedImageWidth);
						AssertEquals("ZoomedImageHeight", 150, pictureBox.ZoomedImageHeight);
						AssertEquals("Image", image, pictureBox.Image);
						AssertEquals("Image.Width", 100, pictureBox.Image.Width);
						AssertEquals("Image.Height", 100, pictureBox.Image.Height);
						AssertEquals("HorizontalScroll.Visible", true, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", true, pictureBox.VerticalScroll.Visible);
					});

					pictureBox.Image = null;
					CombineAssertions(delegate()
					{
						AssertEquals("Width", 100, pictureBox.Width);
						AssertEquals("Height", 100, pictureBox.Height);
						AssertEquals("Zoom", 150, pictureBox.Zoom);
						AssertEquals("Image", null, pictureBox.Image);
						AssertEquals("HorizontalScroll.Visible", false, pictureBox.HorizontalScroll.Visible);
						AssertEquals("VerticalScroll.Visible", false, pictureBox.VerticalScroll.Visible);
					});
				}
			}
		}
	}
}
