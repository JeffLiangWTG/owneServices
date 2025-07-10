using System;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(WebCustomImagesRegistryDataType))]
	sealed class WebCustomImagesRegistryDataTypeTest : RegistryDataTypeTestCase<WebCustomImagesRegistryDataType>
	{
		public void TestSerialization()
		{
			var dataType = new WebCustomImagesRegistryDataType();

			var image1 = new WebTrackerCustomImage("Image1.png", "web1.com/", new byte[] { 0, 1, 2, 3 });
			var image2 = new WebTrackerCustomImage("Image2.png", "web2.com/", new byte[] { 4, 5, 6, 7 });
			var image3 = new WebTrackerCustomImage("Image3.png", "web3.com/", new byte[] { 0, 2, 4, 6 });

			var imagesBefore = new WebTrackerCustomImage[] { image1, image2, image3 };
			var imagesDuring = dataType.Serialise(imagesBefore);
			var imagesAfter = dataType.Deserialise(imagesDuring);

			AssertValuesEqual("Deserialized data should match the data before serialization.", imagesBefore, imagesAfter);
		}

		public void TestSupportedFileTypes()
		{
			Assert(WebTrackerCustomImage.IsSupportedFileType(".bmp"));
			Assert(WebTrackerCustomImage.IsSupportedFileType(".png"));
			Assert(WebTrackerCustomImage.IsSupportedFileType(".gif"));
			Assert(WebTrackerCustomImage.IsSupportedFileType(".jpg"));
			Assert(WebTrackerCustomImage.IsSupportedFileType(".jpeg"));
			Assert(!WebTrackerCustomImage.IsSupportedFileType(".txt"));
			Assert(!WebTrackerCustomImage.IsSupportedFileType(".docx"));
			Assert(!WebTrackerCustomImage.IsSupportedFileType(".exe"));
			Assert(!WebTrackerCustomImage.IsSupportedFileType(""));
		}

		#region Implementation

		protected override WebCustomImagesRegistryDataType GetNewDataType()
		{
			return new WebCustomImagesRegistryDataType();
		}

		protected override void AssertValuesEqual(string message, object lhs, object rhs)
		{
			var lhsImages = (WebTrackerCustomImage[])lhs;
			var rhsImages = (WebTrackerCustomImage[])rhs;

			AssertEquals(message, lhsImages.Length, rhsImages.Length);

			for (var i = 0; i < lhsImages.Length; i++)
			{
				AssertEquals(message, lhsImages[i].Name, rhsImages[i].Name);
				AssertEquals(message, lhsImages[i].Url, rhsImages[i].Url);
				AssertImagesEqual(message, lhsImages[i].Data, rhsImages[i].Data);
			}
		}

		void AssertImagesEqual(string message, byte[] lhs, byte[] rhs)
		{
			AssertEquals(message, lhs.Length, rhs.Length);

			for (var i = 0; i < lhs.Length; i++)
			{
				AssertEquals(message, lhs[i], rhs[i]);
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var image1 = new WebTrackerCustomImage("Image1.png", "web1.com/", new byte[] { 0, 1, 2, 3 });
			var image2 = new WebTrackerCustomImage("Image2.png", "web2.com/", new byte[] { 4, 5, 6, 7 });
			var image3 = new WebTrackerCustomImage("Image3.png", "web3.com/", new byte[] { 0, 2, 4, 6 });

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(Array.Empty<WebTrackerCustomImage>(), Encoding.Unicode.GetBytes(JsonSerializer.Serialize(Array.Empty<WebTrackerCustomCss>()))),
				new ValidSampleAndBinaryValueInDB(new[] { image1 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { image1 }))),
				new ValidSampleAndBinaryValueInDB(new[] { image1, image2, image3 }, Encoding.Unicode.GetBytes(JsonSerializer.Serialize(new[] { image1, image2, image3 })))
			};
		}

		#endregion
	}
}
