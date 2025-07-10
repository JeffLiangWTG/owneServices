using System.Drawing;
using CargoWise.IO;

namespace Enterprise.Barcode.Business.Processor.Testing
{
	sealed class QrCodeProcessorTest : ZXingBarcodeProcessorTestCase<QrCodeProcessor>
	{
		#region Create Tests

		public void TestCreateQrCodeWithShortContent()
		{
			// Arrange
			const string content = "Hello, World!";
			var options = new QrCodeCreationOptions
			{
				Width = 200,
				Height = 200,
				ErrorCorrectionLevel = QrCodeErrorCorrectionLevel.L
			};
			// Act & Assert
			CreateQrCodeSuccessfully(content, options);
		}

		public void TestCreateQrCodeWithLongContent()
		{
			// Arrange
			const string content = "Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World! Hello, World!";
			var options = new QrCodeCreationOptions
			{
				Width = 200,
				Height = 200,
				ErrorCorrectionLevel = QrCodeErrorCorrectionLevel.M
			};
			// Act & Assert
			CreateQrCodeSuccessfully(content, options);
		}

		public void TestCreateQrCodeOfVersion1()
		{
			// Arrange
			const string content = "Hello, World!";
			var options = new QrCodeCreationOptions
			{
				Width = 200,
				Height = 200,
				Version = 1,
				ErrorCorrectionLevel = QrCodeErrorCorrectionLevel.L
			};
			// Act & Assert
			CreateQrCodeSuccessfully(content, options);
		}

		public void TestCreateQrCodeOfVersion2()
		{
			// Arrange
			const string content = "Hello, World!";
			var options = new QrCodeCreationOptions
			{
				Width = 300,
				Height = 300,
				Version = 2,
				ErrorCorrectionLevel = QrCodeErrorCorrectionLevel.L
			};
			// Act & Assert
			CreateQrCodeSuccessfully(content, options);
		}

		public void TestCreateQrCodeOfVersion3()
		{
			// Arrange
			const string content = "Hello, World!";
			var options = new QrCodeCreationOptions
			{
				Width = 400,
				Height = 400,
				Version = 3,
				ErrorCorrectionLevel = QrCodeErrorCorrectionLevel.L
			};
			// Act & Assert
			CreateQrCodeSuccessfully(content, options);
		}

		void CreateQrCodeSuccessfully(string content, QrCodeCreationOptions options)
		{
			// Act
			var result = ProcessorForTest.CreateCode(content, options);
			// Assert
			AssertNotNull("The QR code should be created.", result);
			if (options.Width > 20)
			{
				AssertEquals("The desired width of QR code should be set.", options.Width, result.Width);
			}
			if (options.Height > 20)
			{
				AssertEquals("The desired height of QR code should be set.", options.Height, result.Height);
			}
			// Act
			var reversedResult = ProcessorForTest.ParseCode(result);
			// Assert
			AssertNotNull("The created QR code should be parsed.", reversedResult);
			AssertEquals("The created QR code that is parsed again should be the same as original.", content, reversedResult.Text);
		}

		#endregion

		#region Implementations

		protected override QrCodeProcessor CreateProcessorForTest()
		{
			return new QrCodeProcessor();
		}

		protected override Bitmap SingleBarCodeBitmap
		{
			get
			{
				using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
				{
					return new Bitmap(resourceRetriever.GetStream("qr_single.png"));
				}
			}
		}

		protected override string SingleBarCodeBitmapContent { get; } = "MECARD:N:Sean Owen;TEL:+12125658770;EMAIL:srowen@google.com;;";

		#endregion
	}
}
