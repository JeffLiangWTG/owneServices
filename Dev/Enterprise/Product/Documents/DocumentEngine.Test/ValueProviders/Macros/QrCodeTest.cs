using System;
using System.Drawing;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(QrCode))]
	sealed class QrCodeTest : BarcodeTest
	{
		public void TestGetLogo_WhenSystemImageAndUserImageWithTheSameCode_ShouldReturnUserImage()
		{
			using Bitmap systemImageBitmap = new Bitmap(1, 1);
			using Bitmap userImageBitmap = new Bitmap(2, 2);
			var imageCode = "IMG";
			var collection = new SystemDefinableRegistryImageCollection();

			var systemImage = collection.AddNew();
			systemImage.SystemDefined = true;
			systemImage.Code = imageCode;
			systemImage.EnglishDescription = "system image";
			systemImage.Image = systemImageBitmap;

			var userImage = collection.AddNew();
			userImage.SystemDefined = false;
			userImage.Code = imageCode;
			userImage.EnglishDescription = "user image";
			userImage.Image = userImageBitmap;

			using (DocumentsDataRegistry.Instance.DocumentImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var qrCode = new QrCode();
				var logo = qrCode.GetLogo(imageCode);
				Assert("Should select user image", ZArchitecture.Core.Utilities.IsImageEqual(userImageBitmap, logo));
			}
		}

		#region Replace

		public override void TestIsResponsibleForReplacing()
		{
			AssertMacroMatch("<>", false);
			AssertMacroMatch("<QrCode>", false);
			AssertMacroMatch("<QrCode(abc,1,2)>", false);
			AssertMacroMatch("< QrCode(\"abc\",1,2)       >", true);
			AssertMacroMatch("<QrCode(\"abc\",1,2,-15)>", false);
			AssertMacroMatch("<QrCode(\"abc\",3,4,15)>", true);
			AssertMacroMatch("<QrCode(\"abc\",3,4,15,1)>", true);
			AssertMacroMatch("<QrCode(\"abc\",1,2,15,M)>", false);
			AssertMacroMatch("<QrCode(\"abc\",1,2,15,\"UTF-8\")>", true);
			AssertMacroMatch("<QrCode(\"abc\",3,4,15,\"M\")>", true);
			AssertMacroMatch("<QrCode(\"abc\",1,2,15,\"R\",UTF-8)>", false);
			AssertMacroMatch("<QrCode(\"abc\",3,4,15,\"M\",\"UTF-8\")>", true);

			AssertMacroMatch("<QrCode(\"abc\",6,6,\"UTF-8\")>", true);
			AssertMacroMatch("<QrCode(\"abc\",6,6,\"M\")>", true);
			AssertMacroMatch("<QrCode(\"abc\",6,6,\"M\",\"UTF-8\")>", true);

			AssertMacroMatch("<QrCode(\"abc\",6,6,\" Image : ABC\")>", true);
			AssertMacroMatch("<QrCode(\"abc\",6,6,\"UTF-8\", \"Image:ABC\")>", true);

			AssertMacroMatch("<QrCode(\"abc\ndef\",3,4,15)>", true);
			AssertMacroMatch("<QrCode(\"abc\",3,4,True)>", true);

			void AssertMacroMatch(string macro, bool shouldMatch)
			{
				AssertEquals($"Should{(shouldMatch ? "" : " not")} match {macro}", shouldMatch, ValueProviderToTest.IsResponsibleForReplacing(macro, Passes.FirstPass));
			}
		}

		public override void TestReplace()
		{
			// Arrange
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();
			// Act
			var result1 = (ExcelImage)ValueProviderToTest.GetReplacement("< QrCode ( \"Hello World\" , 2  , 4 )  >", Report);
			var result2 = (ExcelImage)ValueProviderToTest.GetReplacement("< QrCode ( \"Stone's Magic World\" , 3  , 4 , 2 , \"M\" )  >", Report);
			var result3 = (ExcelImage)ValueProviderToTest.GetReplacement("< QrCode ( \"这是什么？\" , 4  , 3 , 3 , \"Q\" , \"UTF-8\" )  >", Report);
			var result4 = (ExcelImage)ValueProviderToTest.GetReplacement("< QrCode ( \"Hello World\nHello World 2\" , 2  , 4 )  >", Report);
			var result5 = (ExcelImage)ValueProviderToTest.GetReplacement("< QrCode ( \"Hello World\", 2, 4, True)  >", Report);
			var resultEmpty = ValueProviderToTest.GetReplacement("<QrCode ( \"\" , 2  , 4 )>", Report);
			// Assert
			AssertNotNull(result1);
			AssertNotNull(result2);
			AssertNotNull(result3);
			AssertNotNull(result4);
			AssertNotNull(result5);
			AssertNotNull(resultEmpty);

			Assert(result1.IsAspectRatioLocked);
			Assert(result1.IsDimensionInPixel);
			AssertEquals("The QR code image should be square.", result1.Width, result1.Height);
			AssertEquals("The QR code image and its containing excel image should be of the same size.", result1.Width, result1.Image.Width);
			AssertEquals("The QR code image size should use the smaller one of width and height.", result1.Width, result2.Width);
			Assert("The QR code image size should use the smaller one of width and height.", result1.Width > result3.Width);
			// Act
			var reversedResult1 = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)result1.Image);
			var reversedResult2 = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)result2.Image);
			var reversedResult3 = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)result3.Image);
			var reversedResult4 = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)result4.Image);
			var reversedResult5 = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode((Bitmap)result5.Image);
			// Assert
			AssertNotNull(reversedResult1);
			AssertNotNull(reversedResult2);
			AssertNotNull(reversedResult3);
			AssertNotNull(reversedResult5);
			AssertEquals("Hello World", reversedResult1.Text);
			AssertEquals("Stone's Magic World", reversedResult2.Text);
			AssertEquals("这是什么？", reversedResult3.Text);
			AssertEquals(@"Hello World
Hello World 2", reversedResult4.Text);
			AssertEquals("Hello World", reversedResult5.Text);
			AssertEquals("Not possible to create QR Code, missing required data", resultEmpty);
		}

		public override void TestReplaceWithErrors()
		{
			// Arrange
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();
			// Act
			var result = (ExcelImage)ValueProviderToTest.GetReplacement(MacroWithUnExpectedContent, Report);
			// Assert
			AssertNull(result);
			AssertContains("Error generating QR code by macro", Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false), true);
		}

		public void TestReplaceWithCenterImage()
		{
			PrepareRenderer();
			Report.Renderer.SaveOriginalColumnWidths();

			var logoBitmap = ((QrCode)ValueProviderToTest).BarcodeProcessor.CreateCode("ABC", new QrCodeCreationOptions
			{
				Width = 100,
				Height = 100
			});
			logoBitmap.SetResolution(192, 192);

			var collection = new SystemDefinableRegistryImageCollection();
			var image = collection.AddNew();
			image.Code = "ABC";
			image.Description = (NoResString)"ABC Description";
			image.Image = logoBitmap;
			image.SystemDefined = false;

			using (DocumentsDataRegistry.Instance.DocumentImages.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				var result = (ExcelImage)ValueProviderToTest.GetReplacement("<QrCode( \"123456\" , 20  , 20, \"Image:ABC\" )  >", Report);

				AssertNotNull(result);

				var qrCodeImage = (Bitmap)result.Image;
				var reversedResult = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode(qrCodeImage);
				var qrCodeImageLogo = new Bitmap((int)(logoBitmap.Width * 0.5), (int)(logoBitmap.Height * 0.5));
				using (var graphics = Graphics.FromImage(qrCodeImageLogo))
				{
					var destPoint = new Point(0, 0);
					var destRect = new Rectangle(destPoint, new Size(qrCodeImageLogo.Width, qrCodeImageLogo.Height));

					var origPoint = new Point((qrCodeImage.Width - qrCodeImageLogo.Width) / 2, (qrCodeImage.Height - qrCodeImageLogo.Height) / 2);
					var origRect = new Rectangle(origPoint, new Size(qrCodeImageLogo.Width, qrCodeImageLogo.Height));

					graphics.DrawImage(qrCodeImage, destRect, origRect, GraphicsUnit.Pixel);
				}
				var logoResult = ((QrCode)ValueProviderToTest).BarcodeProcessor.ParseCode(qrCodeImageLogo);

				AssertEquals("123456", reversedResult.Text);
				AssertEquals("ABC", logoResult.Text);
			}
		}

		#endregion

		#region Implementations

		protected override string MacroWithUnExpectedContent { get; } = "<QrCode(\"Your Australia Company\", 3, 3, 2, \"H\", true)>";

		protected override string CodeType { get; } = "QrCode";

		protected override ValueProvider GetNewValueProvider()
		{
			return new QrCode();
		}

		#endregion
	}
}
