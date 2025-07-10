using System.Drawing;
using System.Linq;

namespace Enterprise.ZArchitecture.GUI
{
	public static class ImageHelper
	{
		public const int ExifOrientationId = 0x0112;

		public static Image GetRotatedImageByExifOrientationIfRequired(string filePath)
		{
			var image = Image.FromFile(filePath);
			RotateImageByExifOrientationIfRequired(image);
			return image;
		}

		public static void RotateImageByExifOrientationIfRequired(Image image)
		{
			if (image.PropertyIdList.Contains(ExifOrientationId))
			{
				var propertyItem = image.GetPropertyItem(ExifOrientationId);
				var flipType = GetRotateFlipTypeByExifOrientation(propertyItem.Value[0]);
				if (flipType != RotateFlipType.RotateNoneFlipNone)
				{
					image.RotateFlip(flipType);
				}
				image.RemovePropertyItem(ExifOrientationId);
			}
		}

		static RotateFlipType GetRotateFlipTypeByExifOrientation(int orientation)
		{
			switch (orientation)
			{
				case 2: // Image is mirrored horizontally
					return RotateFlipType.RotateNoneFlipX;
				case 3: // Image is rotated 180
					return RotateFlipType.Rotate180FlipNone;
				case 4: // Image is mirrored vertically
					return RotateFlipType.Rotate180FlipX;
				case 5: // Image is mirrored horizontally and rotated 270 CW
					return RotateFlipType.Rotate90FlipX;
				case 6: // Image is rotated 90 CW
					return RotateFlipType.Rotate90FlipNone;
				case 7: // Image is mirrored horizontaly and rotated 90 CW
					return RotateFlipType.Rotate270FlipX;
				case 8: // Image is rotated 270 CW
					return RotateFlipType.Rotate270FlipNone;
				default: // Either orientation = 1 (Image is normal), or it has an unexpected rotate/flip so we shouldn't alter it
					return RotateFlipType.RotateNoneFlipNone;
			}
		}
	}
}
