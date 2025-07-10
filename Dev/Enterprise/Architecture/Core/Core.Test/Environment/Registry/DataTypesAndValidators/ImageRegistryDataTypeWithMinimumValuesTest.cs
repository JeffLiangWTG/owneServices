using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImageRegistryDataType))]
	class ImageRegistryDataTypeWithMinimumValuesTest : ImageRegistryDataTypeNullBitmapTest
	{
		protected override ImageRegistryDataType GetNewDataType()
		{
			return new ImageRegistryDataType(100, 200, 10, 50);
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			Bitmap testBitmap1 = new Bitmap(10, 200);
			MemoryStream testBitmapStream1 = new MemoryStream();
			testBitmap1.Save(testBitmapStream1, ImageFormat.Bmp);

			Bitmap testBitmap2 = new Bitmap(100, 50);
			MemoryStream testBitmapStream2 = new MemoryStream();
			testBitmap2.Save(testBitmapStream2, ImageFormat.Bmp);

			ItemsToDispose.Add(testBitmap1);
			ItemsToDispose.Add(testBitmapStream1);
			ItemsToDispose.Add(testBitmap2);
			ItemsToDispose.Add(testBitmapStream2);

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(null, ImageRegistryDataType.MagicNullImage),
				new ValidSampleAndBinaryValueInDB(testBitmap1, testBitmapStream1.ToArray()),
				new ValidSampleAndBinaryValueInDB(testBitmap2, testBitmapStream2.ToArray())
			};
		}

		protected override object[] GetInvalidSamples()
		{
			Image image1 = new Bitmap(101, 199);
			Image image2 = new Bitmap(99, 201);
			Image image3 = new Bitmap(11, 49);
			Image image4 = new Bitmap(9, 51);

			ItemsToDispose.Add(image1);
			ItemsToDispose.Add(image2);
			ItemsToDispose.Add(image3);
			ItemsToDispose.Add(image4);

			return new object[] { image1, image2, image3, image4 };
		}
	}
}
