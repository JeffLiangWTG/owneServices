using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ZXing;
using ZXing.Datamatrix;

namespace Enterprise.Barcode.Business
{
	/// <summary>
	/// Represents a Barcode processor using ZXing library.
	/// </summary>
	public abstract class ZXingBarcodeProcessor : IBarcodeProcessor
	{
		#region Bar Code

		/// <summary>
		/// Parses a piece of content out of a bar code in a specific bitmap.
		/// </summary>
		/// <param name="bitmap">The bitmap containing the bar code.</param>
		/// <returns>The content contained in the bar code.</returns>
		public BarcodeResultContent ParseCode(Bitmap bitmap)
		{
			var zResult = BarcodeReader.Decode(bitmap);
			return zResult != null ? new BarcodeResultContent
			{
				Text = zResult.Text,
				RawData = zResult.RawBytes,
				Region = zResult.ResultPoints.Select(rp => new PointF(rp.X, rp.Y)).ToArray()
			} : null;
		}

		/// <summary>
		/// Creates a new bar code that encapsulates a specific content.
		/// </summary>
		/// <param name="content">The content contained.</param>
		/// <param name="options">The bar code creation options.</param>
		/// <returns>The created bar code bitmap.</returns>
		public Bitmap CreateCode(string content, BarCodeCreationOptions options)
		{
			return CreateCodeCore(content, options);
		}

		protected virtual Bitmap CreateCodeCore(string content, BarCodeCreationOptions options)
		{
			BarcodeWriter.Options = new DatamatrixEncodingOptions
			{
				Width = options.Width,
				Height = options.Height
			};

			return BarcodeWriter.Write(content);
		}

		#endregion

		#region Common

		protected ZXing.BarcodeReader BarcodeReader { get; } = new ZXing.BarcodeReader
		{
			AutoRotate = true,
			TryInverted = true,
			Options = new ZXing.Common.DecodingOptions
			{
				TryHarder = true,
				PossibleFormats = new List<BarcodeFormat>
				{
					BarcodeFormat.QR_CODE,
					BarcodeFormat.DATA_MATRIX,
					BarcodeFormat.PDF_417,
					BarcodeFormat.UPC_A,
					BarcodeFormat.EAN_13,
					BarcodeFormat.EAN_8,
					BarcodeFormat.CODE_39
				}
			}
		};
		protected BarcodeWriter BarcodeWriter { get; } = new BarcodeWriter
		{
			Renderer = Activator.CreateInstance<ZXing.Rendering.BitmapRenderer>()
		};

		#endregion

	}
}
