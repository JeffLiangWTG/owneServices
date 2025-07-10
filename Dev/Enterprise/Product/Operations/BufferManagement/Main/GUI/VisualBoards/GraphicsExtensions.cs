using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.GUI
{
	public static class GraphicsExtensions
	{
		public static void DrawImageWithExtendedErrorMessage(this Graphics graphics, Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			try
			{
				graphics.DrawImage(image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr);
			}
			catch (ArgumentException ex) when (ex.Source.Contains("System.Drawing")) // I want this exception and just this exception
			{
				GenerateExceptionWithExtendedMessage(ex, image, destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr);
			}
		}

#if DEBUG
		public
#endif
			static void GenerateExceptionWithExtendedMessage(ArgumentException ex, Image image, Rectangle destRect, int srcX, int srcY, int srcWidth, int srcHeight, GraphicsUnit srcUnit, ImageAttributes imageAttr)
		{
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)@"Cannot draw image on the graphics object.
image.IsDisposed: {0},
image.Width: {1},
image.Height: {2},
destRect: {3},
srcX: {4},
srcY: {5},
srcWidth: {6},
srcHeight: {7},
srcUnit: {8},
imageAttr: {9}", // This is a diagnostic message
				GetImagePropertyAsStringSafely(image, () => image.IsDisposed()),
				GetImagePropertyAsStringSafely(image, () => image.Width),
				GetImagePropertyAsStringSafely(image, () => image.Height),
				destRect, srcX, srcY, srcWidth, srcHeight, srcUnit, imageAttr);
			throw new ArgumentException(message, ex)
			{
				Source = ex.Source
			};
		}

		[SuppressMessage("Microsoft.Design", "CA1031: Do not catch general exception types", Justification = "valid for the diagnostics")]

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Diagnostic message")]
		static string GetImagePropertyAsStringSafely<T>(Image image, Func<T> valueGetter)
		{
			const string calcErrorImageIsNullMessage = "error with obtaining the value: image is null";
			const string calcErrorExceptionThrownMessage = "error with obtaining the value: the following exception has been thrown: {0}";

			try
			{
				return image != null ? valueGetter().ToString() : calcErrorImageIsNullMessage;
			}
			catch (Exception ex)
			{
				return string.Format(CultureInfo.InvariantCulture, calcErrorExceptionThrownMessage, ex.Message);
			}
		}
	}
}
