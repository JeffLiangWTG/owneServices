using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public static class TestFileHelper
	{
		public static Image GenerateRandomJpg(string filePath, int? sizeInMB = null, RotateFlipType? exifOrientation = null)
		{
			var width = 1000;
			var height = 1000;

			var bitmap = new Bitmap(width, height);
			var random = new Random();
			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					var randomColor = Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));
					bitmap.SetPixel(x, y, randomColor);
				}
			}

			if (exifOrientation != null)
			{
				var propertyItem = (PropertyItem)typeof(PropertyItem).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, Array.Empty<Type>(), null).Invoke(null);
				propertyItem.Id = 274; // EXIF orientation tag
				propertyItem.Type = 3; // Short
				propertyItem.Len = 1;
				propertyItem.Value = BitConverter.GetBytes((ushort)exifOrientation);
				bitmap.SetPropertyItem(propertyItem);
			}

			if (sizeInMB != null)
			{
				var targetSize = (long)(sizeInMB * 1024 * 1024);
				var incrementSize = 100 * 1024; // 100 KB

				using var ms = new MemoryStream();
				bitmap.Save(ms, ImageFormat.Jpeg);
				while (ms.Length < targetSize)
				{
					var buffer = new byte[incrementSize];
					random.NextBytes(buffer);
					ms.Write(buffer, 0, buffer.Length);
				}

				File.WriteAllBytes(filePath, ms.ToArray());
			}
			else
			{
				bitmap.Save(filePath, ImageFormat.Jpeg);
			}

			return bitmap;
		}

		public static void GeneratePng(string filePath, int width)
		{
			var height = 1;

			using var image = new SixLabors.ImageSharp.Image<Rgba32>(width, height);
			var random = new Random();
			for (var x = 0; x < width; x++)
			{
				var randomColor = new Rgba32((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
				image[x, 0] = randomColor;
			}

			using var fileStream = new FileStream(filePath, FileMode.Create);
			image.Save(fileStream, new PngEncoder());
		}

		public static Image CloneJpgAndApplyExifOrientation(string inputFilePath)
		{
			using var originalImage = Image.FromFile(inputFilePath);
			RotateImageByExifOrientationIfRequired(originalImage);
			return (Image)originalImage.Clone();
		}

		static void RotateImageByExifOrientationIfRequired(Image image)
		{
			if (image.PropertyIdList.Contains(274))
			{
				PropertyItem propertyItem = image.GetPropertyItem(274);
				RotateFlipType rotateFlipTypeByExifOrientation = GetRotateFlipTypeByExifOrientation(propertyItem.Value[0]);
				if (rotateFlipTypeByExifOrientation != 0)
				{
					image.RotateFlip(rotateFlipTypeByExifOrientation);
				}

				image.RemovePropertyItem(274);
			}
		}

		static RotateFlipType GetRotateFlipTypeByExifOrientation(int orientation)
		{
			return orientation switch
			{
				2 => RotateFlipType.RotateNoneFlipX,
				3 => RotateFlipType.Rotate180FlipNone,
				4 => RotateFlipType.Rotate180FlipX,
				5 => RotateFlipType.Rotate90FlipX,
				6 => RotateFlipType.Rotate90FlipNone,
				7 => RotateFlipType.Rotate270FlipX,
				8 => RotateFlipType.Rotate270FlipNone,
				_ => RotateFlipType.RotateNoneFlipNone,
			};
		}
	}
}
