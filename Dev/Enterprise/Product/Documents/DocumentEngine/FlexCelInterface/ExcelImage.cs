using System.Drawing;
using System.Globalization;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public class ExcelImage
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="image">The image to be embedded.</param>
		/// <param name="height">The height of the image. If <see cref="IsDimensionInPixel" /> is true, its unit is pixel. Otherwise, it takes the number of row spans.</param>
		/// <param name="width">The width of the image. If <see cref="IsDimensionInPixel" /> is true, its unit is pixel. Otherwise, it takes the Excel size.</param>
		/// <param name="imageName">The name to be adopted by Excel.</param>
		/// <param name="isAspectRatioLocked">Whether the aspect ratio is to be respected when calculating non-pixel dimensions.</param>
		/// <param name="isDimensionInPixel">Whether the width and height are in the unit of pixels.</param>
		public ExcelImage(Image image, int height, int width, string imageName, bool isAspectRatioLocked = false, bool isDimensionInPixel = false)
		{
			Image = image;
			Height = height;
			Width = width;
			ImageName = imageName;
			IsAspectRatioLocked = isAspectRatioLocked;
			IsDimensionInPixel = isDimensionInPixel;
		}

		public readonly Image Image;
		public readonly string ImageName;
		public readonly int Height;
		public readonly int Width;
		public readonly bool IsAspectRatioLocked;
		public readonly bool IsDimensionInPixel;

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} ({1}x{2})", ImageName, Width, Height);
		}
	}
}
