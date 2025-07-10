using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using Enterprise.Environment;

namespace Enterprise.Barcode.Business
{
	public class EnterpriseScanner : IBarcodeScanner
	{
		internal bool DebuggingOutput;
		internal string DebuggingOutputFilename = Path.Combine(Env.TempPath, "output_barcode.bmp");

		public IEnumerable<string> Read(Bitmap inputImage)
		{
			// Barcode might be rotated initially, so read it, rotate it and read it again just to be sure.
			return RotateImage ?
				Read(inputImage, false).Concat(Read(inputImage, true)).ToList() :
				Read(inputImage, false).ToList();
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Using GDI+ to manipulate images, no WinForms here")]
		IEnumerable<string> Read(Bitmap inputImage, bool rotate)
		{
			var workingImage = rotate ?
				RotateImageBy90(inputImage) :
				inputImage.Clone(new Rectangle(0, 0, inputImage.Width, inputImage.Height), inputImage.PixelFormat);

			using (workingImage)
			{
				var imageData = ImageProcessor.GetImageDataFromBitmap(workingImage, out var imageHeight, out var imageWidth);
				return Read(workingImage, imageData, imageHeight, imageWidth);
			}
		}

		protected Bitmap RotateImageBy90(Bitmap input)
		{
			var result = new Bitmap(input.Height, input.Width);
			result.SetResolution(input.HorizontalResolution, input.VerticalResolution);

			using (var graphic = Graphics.FromImage(result))
			{
				graphic.TranslateTransform(result.Width, 0);
				graphic.RotateTransform(90);
				graphic.DrawImageUnscaled(input, Point.Empty);
			}

			return result;
		}

		internal bool RotateImage = true;

		IEnumerable<string> Read(Bitmap inputImage, byte[,] imageData, int imageHeight, int imageWidth)
		{
			var barcodeRegions = new BarcodeLocator().GetBarcodeRegions(imageData, imageHeight, imageWidth);

			if (DebuggingOutput)
			{
				using (var finalImage = new Bitmap(inputImage))
				{
					HighlightRegions(finalImage, barcodeRegions, Color.Red);
					finalImage.Save(DebuggingOutputFilename);
				}
			}

			var barcodeValues = ReadBarcodesFromRegions(imageData, barcodeRegions);
			return barcodeValues.Select(barcode => barcode.Text).Distinct();
		}

		[SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", Justification = "I don't see how this wastes space.")]
		ICollection<BarcodeValue> ReadBarcodesFromRegions(byte[,] imageData, ArrayList barcodeRegions)
		{
			var barcodeValues = new List<BarcodeValue>();

			foreach (BarcodeRegion barcodeRegion in barcodeRegions)
			{
				byte[,] dataCopy = CopyArray(imageData, barcodeRegion.X, barcodeRegion.Y, barcodeRegion.Height, barcodeRegion.Width);
				BarcodeValue barcode = new BarcodeReader().ReadBarcode(dataCopy, barcodeRegion.Height, barcodeRegion.Width);
				if (barcode.Symbologie != BarcodeSymbologie.InvalidBarcode)
				{
					barcodeValues.Add(barcode);
				}
			}

			return barcodeValues;
		}

		/// <summary>
		/// Return a copy of a subsection of a two dimensional array
		/// </summary>
		byte[,] CopyArray(byte[,] sourceArray, int xOffset, int yOffset, int height, int width)
		{
			byte[,] dataCopy = new byte[height, width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					dataCopy[y, x] = sourceArray[y + yOffset, x + xOffset];
				}
			}
			return dataCopy;
		}

		#region Debugging Functions

		void HighlightRegions(Bitmap outputBitmap, ArrayList regions, Color colour)
		{
			foreach (BarcodeRegion barcodeRegion in regions)
			{
				HighlightBarcodeRegion(outputBitmap, barcodeRegion.X, barcodeRegion.Y, barcodeRegion.Width, barcodeRegion.Height, colour, "");
			}
		}

		void HighlightBarcodeRegion(Bitmap output, int x, int y, int width, int height, Color colour, string text)
		{
			if (height < 0 || width < 0)
			{
				throw new ArgumentException("Both height and width must be positive : (" + height + ", " + width + ")");
			}

			using (var graphic = Graphics.FromImage(output))
			using (var font = new Font("Tahoma", 20, FontStyle.Bold))
			using (var brush = new SolidBrush(Color.FromArgb(64, colour.R, colour.B, colour.G)))
			using (var pen = new Pen(colour, 1))
			{
				graphic.DrawRectangle(pen, x, y, width, height);
				graphic.FillRectangle(brush, x, y, width, height);
				graphic.DrawString(text, font, Brushes.Orange, x, y);
			}
		}

		#endregion
	}
}
