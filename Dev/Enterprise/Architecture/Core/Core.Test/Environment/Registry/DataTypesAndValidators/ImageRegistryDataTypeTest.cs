using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImageRegistryDataType))]
	sealed class ImageRegistryDataTypeTest : ImageRegistryDataTypeNullBitmapTest
	{
		public void TestMagicNullImage()
		{
			byte[] bytes = DataType.Serialise(null);
			AssertEquals(bytes.Length, ImageRegistryDataType.MagicNullImage.Length);
			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] != ImageRegistryDataType.MagicNullImage[i])
				{
					Fail("result is different");
				}
			}
		}

		[ExpectNoExceptions]
		public void TestMagicNullImageIsValidImage()
		{
			using (MemoryStream ms = new MemoryStream(ImageRegistryDataType.MagicNullImage))
			{
				AssertNotNull(Image.FromStream(ms));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImageIsSerialisedInOriginalFormat()
		{
			string testPath = Path.Combine(TestCase.BaseSourcePath, "Enterprise", "Product", "Documents", "DocumentScanning", "DocumentScanning.Business.Test", "TestDocs");
			string gifFile = Path.Combine(testPath, "small.gif");
			string tifFile = Path.Combine(testPath, "small.tif");

			using (Image gif = Image.FromFile(gifFile))
			using (Image tif = Image.FromFile(tifFile))
			{
				using (MemoryStream stream = new MemoryStream())
				{
					Image deserialisedGif = DataType.Deserialise(DataType.Serialise(gif));
					Image deserialisedTif = DataType.Deserialise(DataType.Serialise(tif));

					AssertEquals(ImageFormat.Gif, deserialisedGif.RawFormat);
					AssertEquals(ImageFormat.Tiff, deserialisedTif.RawFormat);
				}
			}
		}

		public void TestIsDeserializedDataAlive()
		{
			Bitmap bmp = new Bitmap(10, 10);
			AssertEquals("Image should be alive", true, DataType.IsDeserializedDataAlive(bmp));

			bmp.Dispose();
			AssertEquals("Image should be dead", false, DataType.IsDeserializedDataAlive(bmp));
		}

		public void TestMagicNullImageDeserialisesToAValidBmp()
		{
			using (var image = DataType.Deserialise(ImageRegistryDataType.MagicNullImage))
			{
				AssertNotNull("'MagicNullImage' should not deserialise as null since this means it will never be a valid registry value", image);
			}
		}

		public void TestMagicNullImageIsDefinitelyMagicAndNull()
		{
			var nullImage = new Bitmap(new MemoryStream(ImageRegistryDataType.MagicNullImage));
			Assert(ImageRegistryDataType.IsNullDataRepresentation(nullImage));
		}

		protected override object GetNullRepresentation()
		{
			return ImageRegistryDataType.MagicNullImage.ToArray();
		}
	}
}
