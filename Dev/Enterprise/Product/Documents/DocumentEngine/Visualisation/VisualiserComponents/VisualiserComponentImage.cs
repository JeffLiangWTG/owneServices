using System;
using System.Drawing;

namespace Enterprise.DocumentEngine.Visualisation
{
	public class VisualiserComponentImage : VisualiserComponent
	{
		public VisualiserComponentImage(Point location, Size size, Image image)
			: base(location, size)
		{
			if (image == null)
			{
				throw new ArgumentNullException("Image Stream");
			}

			Image = image;
		}
		public readonly Image Image;

#if DEBUG
		public override string GetControlDescriptionForTesting()
		{
			return base.GetControlDescriptionForTesting()
					+ "Image Size: " + Image.Size.ToString() + "\r\n"
					+ "Image V.Scale: " + Convert.ToInt32(Math.Round(Image.VerticalResolution, 0)).ToString() + " DPI\r\n"
					+ "Image H.Scale: " + Convert.ToInt32(Math.Round(Image.HorizontalResolution, 0)).ToString() + " DPI\r\n";
		}
#endif
	}
}
