using System.Drawing;
using System.IO;
using CargoWise.Common;
using FlexCel.Pdf;

namespace Enterprise.RemotePrinting.Engine
{
	public class ImageWatermark : Watermark
	{
		public ImageWatermark(byte[] imageData, WatermarkHorizontalAlign horizontalAlign, WatermarkVerticalAlign verticalAlign,
			float horizontalOffset, float verticalOffset, int rotation)
			: base(horizontalAlign, verticalAlign, horizontalOffset, verticalOffset, rotation)
		{
			ImageData = Argument.NotNull(imageData, nameof(imageData));
		}

		#region Properties

		readonly byte[] ImageData;

		public override byte[] AsImage => ImageData;

		#endregion

		public override void Draw(Graphics gr, SizeF pageSize)
		{
			Argument.NotNull(gr, nameof(gr));

			using (var imageStream = new MemoryStream(ImageData))
			using (var img = Image.FromStream(imageStream))
			{
				SizeF imageSize = img.Size;
				var imagePos = GetAlign(imageSize, pageSize);
				RotateCanvas(gr, imageSize, imagePos);
				gr.DrawImage(img, imagePos);
			}
		}

		public override void Draw(PdfWriter pdf, SizeF pageSize)
		{
			Argument.NotNull(pdf, nameof(pdf));

			using (var imageStream = new MemoryStream(ImageData))
			using (var img = Image.FromStream(imageStream))
			{
				imageStream.Position = 0;

				SizeF imageSize = img.Size;
				var imagePos = GetAlign(imageSize, pageSize);
				RotatePdfCanvas(pdf, imageSize, imagePos);
				pdf.DrawImage(img, new RectangleF(imagePos, imageSize), imageStream);
			}
		}
	}
}
