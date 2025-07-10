namespace Enterprise.Barcode.Business.Testing
{
	using NUnit.Framework;

	public class ImageProcessingTest : TestCase
	{
		public void TestRemoveIsolatedPixels()
		{
			int imageHeight = 10, imageWidth = 10;
			byte[,] testImageData = new byte[imageHeight, imageWidth];

			for (int y = 0; y < imageWidth; y++)
			{
				for (int x = 0; x < imageHeight; x++)
				{
					testImageData[y, x] = 0;
				}
			}

			//insert an isolated value
			testImageData[5, 5] = 5;
			ImageProcessor.RemoveIsolatedPixels(testImageData, imageHeight, imageWidth);

			Assert("The cell in ImageData[5,5] should be 0", testImageData[5, 5] == 0);
		}
	}
}
