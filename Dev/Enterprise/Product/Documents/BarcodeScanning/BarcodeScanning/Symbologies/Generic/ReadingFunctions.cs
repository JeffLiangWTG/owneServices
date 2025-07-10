
namespace Enterprise.Barcode.Business.Symbologies.Generic
{
	internal abstract class ReadingFunctions
	{
		public static void PreprocessImageData(byte[,] imageData, int imageHeight, int imageWidth)
		{
			ImageProcessor.RemoveIsolatedPixels(imageData, imageHeight, imageWidth);
		}

		/// <summary>
		/// Calculate the width of indivual bars in the given "swipe" of ImageData.
		/// </summary>
		/// <remarks>
		/// Will always return an odd lengthed array: First and last width will always
		/// refer to black bars.
		/// </remarks>
		/// <returns>
		/// An integer array of bar widths in pixels.
		/// </returns>
		public unsafe static int[] GetBarWidths(byte[,] imageData, int imageWidth, int yOffset)
		{
			bool blackState = true;
			int pixelCount = 0;

			int* workingArray = stackalloc int[imageWidth];
			int stripeCount = 0;

			//x : the index of the first black pixel
			for (int x = FirstBlackPixel(imageData, imageWidth, yOffset); x < imageWidth; x++)
			{
				if ((imageData[yOffset, x] == BlackPixel) != blackState)
				{
					blackState = (imageData[yOffset, x] == BlackPixel);
					workingArray[stripeCount++] = pixelCount;
					pixelCount = 0;
				}
				pixelCount++;
			}
			workingArray[stripeCount++] = pixelCount;

			// TODO : Remove shuffling of data in memory
			if (stripeCount == 0)
			{
				return System.Array.Empty<int>();
			}

			if (stripeCount % 2 == 0)
			{
				stripeCount--;
			}

			int[] result = new int[stripeCount];
			for (int i = 0; i < stripeCount; i++)
			{
				result[i] = workingArray[i];
			}

			return result;
		}

		/// <summary>
		/// Calculate the width of indivual bars in the given "swipe" of ImageData for
		/// every row in the given image.
		/// </summary>
		/// <returns>
		/// An array of integer arrays, each top level array referring to a row of the
		/// given image, the second level arrays referring to the widths of the bars
		/// in the given image in pixels.
		/// </returns>
		public static int[][] GetAllBarWidths(byte[,] imageData, int imageHeight, int imageWidth)
		{
			int[][] result = new int[imageHeight][];

			for (int y = 0; y < imageHeight; y++)
			{
				result[y] = GetBarWidths(imageData, imageWidth, y);
			}

			return result;
		}

		#region Implementation

		protected const byte BlackPixel = 0;

		/// <summary>
		/// Find the index of the first black pixel in the given row of data.
		/// </summary>
		protected static int FirstBlackPixel(byte[,] imageData, int imageWidth, int yOffset)
		{
			int firstPixel;

			for (firstPixel = 0; firstPixel < imageWidth; firstPixel++)
			{
				if (imageData[yOffset, firstPixel] == BlackPixel)
				{
					break;
				}
			}

			return firstPixel;
		}

		#endregion

	}
}
