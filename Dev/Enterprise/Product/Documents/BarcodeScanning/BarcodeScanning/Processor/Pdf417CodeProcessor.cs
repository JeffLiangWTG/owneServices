using System;
using System.Drawing;
using ZXing;
using ZXing.PDF417;
using ZXing.PDF417.Internal;

namespace Enterprise.Barcode.Business
{
	public class Pdf417CodeProcessor : ZXingBarcodeProcessor
	{
		public Pdf417CodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.PDF_417;
		}

		protected override Bitmap CreateCodeCore(string content, BarCodeCreationOptions options)
		{
			if (!(options is Pdf417CodeCreationOptions pdf417CodeOptions))
			{
				throw new ArgumentException("Parameter options should be Pdf417CodeCreationOptions");
			}

			BarcodeWriter.Options = new PDF417EncodingOptions
			{
				Width = pdf417CodeOptions.Width,
				Height = pdf417CodeOptions.Height,
				ErrorCorrection = GetPDF417ErrorCorrectionLevel(pdf417CodeOptions.ErrorCorrectionLevel),
				CharacterSet = pdf417CodeOptions.CharacterSet
			};
			return BarcodeWriter.Write(content);
		}

		PDF417ErrorCorrectionLevel GetPDF417ErrorCorrectionLevel(Pdf417CodeErrorCorrectionLevel? level)
		{
			switch (level)
			{
				case Pdf417CodeErrorCorrectionLevel.L0:
					return PDF417ErrorCorrectionLevel.L0;
				case Pdf417CodeErrorCorrectionLevel.L1:
					return PDF417ErrorCorrectionLevel.L1;
				case Pdf417CodeErrorCorrectionLevel.L2:
					return PDF417ErrorCorrectionLevel.L2;
				case Pdf417CodeErrorCorrectionLevel.L3:
					return PDF417ErrorCorrectionLevel.L3;
				case Pdf417CodeErrorCorrectionLevel.L4:
					return PDF417ErrorCorrectionLevel.L4;
				case Pdf417CodeErrorCorrectionLevel.L5:
					return PDF417ErrorCorrectionLevel.L5;
				case Pdf417CodeErrorCorrectionLevel.L6:
					return PDF417ErrorCorrectionLevel.L6;
				case Pdf417CodeErrorCorrectionLevel.L7:
					return PDF417ErrorCorrectionLevel.L7;
				case Pdf417CodeErrorCorrectionLevel.L8:
					return PDF417ErrorCorrectionLevel.L8;
				case Pdf417CodeErrorCorrectionLevel.AUTO:
					return PDF417ErrorCorrectionLevel.AUTO;
				default:
					return PDF417ErrorCorrectionLevel.AUTO;
			}
		}
	}
}
