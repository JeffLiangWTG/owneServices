using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using static CargoWise.PdfiumWrapper.Testing.ImageTestingHelpers;

namespace Enterprise.DocumentEngine.PreviewableDocument.Testing
{
	public static class PreviewableImageTestHelpers
	{
		public static Bitmap GetPage(this IPreviewableDocument image, int page)
		{
			var size = image.GetPageSize(page);
			var bitmap = new Bitmap(size.Width, size.Height);
			try
			{
				using (var graphics = Graphics.FromImage(bitmap))
				{
					image.Render(graphics, page, size);
				}
			}
			catch
			{
				bitmap.Dispose();
				throw;
			}

			return bitmap;
		}

		public static Color[] GetPageColors(IPreviewableDocument file, bool convertToNamed = true)
		{
			var colors = SelectForEachPage(file, GetCenterPixelColor);
			return convertToNamed ?
				colors.Select(AsNamedColor).ToArray() :
				colors;
		}

		public static T[] SelectForEachPage<T>(IPreviewableDocument file, Func<Image, T> getter)
		{
			var numberOfPages = file.NumberOfPages;
			var stuff = new T[numberOfPages];
			for (var i = 0; i < numberOfPages; i++)
			{
				using (var img = file.GetPage(i))
				{
					stuff[i] = getter(img);
				}
			}

			return stuff;
		}

		public static void AssertHasPages(IPreviewableDocument file, params Color[] colors)
		{
			Assertion.AssertArrayEqualsByElements(colors, GetPageColors(file));
		}
	}
}
