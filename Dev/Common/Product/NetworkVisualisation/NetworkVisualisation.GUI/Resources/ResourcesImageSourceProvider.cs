using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CargoWise.NetworkVisualisation.GUI
{
	public interface IImageSourceProvider
	{
		ImageSource GetImageSource(string imageId);
	}

	public class ResourcesImageSourceProvider : IImageSourceProvider
	{
		[ThreadStatic]
		static Dictionary<string, ImageSource> cache;

		public ResourcesImageSourceProvider()
		{
			if (cache == null)
			{
				cache = new Dictionary<string, ImageSource>();
			}
		}

		public ImageSource GetImageSource(string imageId)
		{
			if (imageId == null)
			{
				return null;
			}

			if (!cache.TryGetValue(imageId, out var imageSource))
			{
				imageSource = LoadImage(imageId);
				cache.Add(imageId, imageSource);
			}

			return imageSource;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1120:DoNotGetIconsImagesFromRexOrResourcesFile", Justification = "Baseline")]
		static ImageSource LoadImage(string imageName)
		{
			var bitmap = (Bitmap)Integration.Properties.Resources.ResourceManager.GetObject(imageName, Integration.Properties.Resources.Culture);
			return ToImageSource(bitmap);
		}

		#region Images

		static Stream ToStream(Image image, ImageFormat format)
		{
			var stream = new MemoryStream();
			try
			{
				image.Save(stream, format);
				stream.Position = 0;

				return stream;
			}
			catch (Exception)
			{
				stream.Dispose();
				throw;
			}
		}

		static ImageSource ToImageSource(Bitmap bitmap)
		{
			return ToImageSource(bitmap, ImageFormat.Png);
		}

		static ImageSource ToImageSource(Bitmap bitmap, ImageFormat imageFormat)
		{
			if (bitmap == null)
			{
				return null;
			}

			var bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.StreamSource = ToStream(bitmap, imageFormat);
			bitmapImage.EndInit();
			return bitmapImage;
		}

		#endregion
	}
}
