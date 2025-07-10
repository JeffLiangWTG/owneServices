using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.IO;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	public enum ColorType
	{
		Foreground,
		Background,
	}

	public struct CoordColorType
	{
		public int X { get; }
		public int Y { get; }
		public Color Color { get; }
		public ColorType ColorType { get; }

		public CoordColorType(int x, int y, Color color, ColorType colorType)
		{
			X = x;
			Y = y;
			Color = color;
			ColorType = colorType;
		}

		public static implicit operator CoordColorType((int, int, Color, ColorType) t)
			=> new CoordColorType(t.Item1, t.Item2, t.Item3, t.Item4);
	}

	public class ImageHelper : IDisposable
	{
		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		readonly Lazy<ImageConverter> imageConverter = new Lazy<ImageConverter>(() => new ImageConverter());

		public static void DumpImage(Bitmap image, CoordColorType[] info = null)
		{
			var fileName = Guid.NewGuid().ToString();

			var imageFileName = Path.Combine(Temp.TempPath, $"{fileName}.png");
			image.Save(imageFileName, ImageFormat.Png);

			if (info == null)
			{
				return;
			}

			var csvFileName = Path.Combine(Temp.TempPath, $"{fileName}.csv");
			var csFileName = Path.Combine(Temp.TempPath, $"{fileName}.cs");
			using StreamWriter csvFile = new StreamWriter(csvFileName),
				csFile = new StreamWriter(csFileName);
			csvFile.WriteLine("X, Y, COLOR_ARGB, COLOR_TYPE, COLOR_STR");
			csFile.WriteLine("CoordColorType[] pixels = new CoordColorType[]\n{");
			foreach (var item in info)
			{
				var color = image.GetPixel(item.X, item.Y);
				csvFile.WriteLine($"{item.X}, {item.Y}, {color.ToArgb():X08}, {item.ColorType}, \"{color}\"");
				csFile.WriteLine($"   ({item.X}, {item.Y}, 0x{color.ToArgb():X08}.ToColor(), ColorType.{item.ColorType}),");
			}

			csFile.WriteLine("};");
		}

		public void Dispose()
		{
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		public Bitmap GetImageFromByteArray(byte[] byteArray)
		{
			return (Bitmap)(imageConverter.Value.ConvertFrom(byteArray));
		}

		public Image GetImageFromResource(string imageResourceName)
		{
			using (var stream = resourceRetriever.Value.GetStream(imageResourceName))
			{
				return Image.FromStream(stream);
			}
		}
	}

	public static class ExtensionMethods
	{
		public static Color ToColor(this uint argb)
		{
			return Color.FromArgb(unchecked((int)argb));
		}

		public static Color ToColor(this int argb)
		{
			return Color.FromArgb(argb);
		}

		public static Color Normalize(this Color color)
		{
			return color.ToArgb().ToColor();
		}
	}
}
