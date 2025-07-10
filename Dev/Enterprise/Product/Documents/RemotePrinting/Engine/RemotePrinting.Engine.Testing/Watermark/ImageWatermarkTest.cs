using CargoWise.IO;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	sealed class ImageWatermarkTest : PrintEngineTestCase
	{
		public void TestConstructorParamtersAreSet()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var bytes = resourceRetriever.GetBytes("Enterprise.RemotePrinting.Engine.Testing.TestFiles.TransparentPNG.png");
				var watermark = new ImageWatermark(bytes, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45);
				AssertEquals("Bytes should be set", bytes, watermark.AsImage);
			}
		}

		public void TestAsImage()
		{
			var imageData1 = new byte[] { 0, 1, 2 };
			var imageData2 = new byte[] { 3, 4, 5, 6 };
			AssertEquals(imageData1, new ImageWatermark(imageData1, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45).AsImage);
			AssertEquals(imageData2, new ImageWatermark(imageData2, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45).AsImage);
		}

		public void TestAsText()
		{
			var imageData = new byte[] { 0, 1, 2 };
			AssertEquals(string.Empty, new ImageWatermark(imageData, WatermarkHorizontalAlign.Centre, WatermarkVerticalAlign.Middle, 0, 0, 45).AsText);
		}
	}
}
