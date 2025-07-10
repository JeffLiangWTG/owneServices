using System;
using System.Drawing;
using ZXing;

namespace Enterprise.Barcode.Business
{
	public class QrCodeProcessor : ZXingBarcodeProcessor
	{
		public QrCodeProcessor()
		{
			BarcodeWriter.Format = BarcodeFormat.QR_CODE;
		}

		protected override Bitmap CreateCodeCore(string content, BarCodeCreationOptions options)
		{
			if (!(options is QrCodeCreationOptions qrCodeOptions))
			{
				throw new ArgumentException("Parameter options should be QrCodeEncodingOptions");
			}

			BarcodeWriter.Options = new ZXing.QrCode.QrCodeEncodingOptions
			{
				Width = qrCodeOptions.Width,
				Height = qrCodeOptions.Height,
				Margin = qrCodeOptions.Margin,
				QrVersion = qrCodeOptions.Version,
				ErrorCorrection = GetQrCodeErrorCorrectionLevel(qrCodeOptions.ErrorCorrectionLevel),
				CharacterSet = qrCodeOptions.CharacterEncoding
			};
			return BarcodeWriter.Write(content);
		}

		ZXing.QrCode.Internal.ErrorCorrectionLevel GetQrCodeErrorCorrectionLevel(QrCodeErrorCorrectionLevel? errorCorrectionLevel)
		{
			switch (errorCorrectionLevel)
			{
				case QrCodeErrorCorrectionLevel.M:
					return ZXing.QrCode.Internal.ErrorCorrectionLevel.M;
				case QrCodeErrorCorrectionLevel.Q:
					return ZXing.QrCode.Internal.ErrorCorrectionLevel.Q;
				case QrCodeErrorCorrectionLevel.H:
					return ZXing.QrCode.Internal.ErrorCorrectionLevel.H;
				default:
					return ZXing.QrCode.Internal.ErrorCorrectionLevel.L;
			}
		}
	}
}
