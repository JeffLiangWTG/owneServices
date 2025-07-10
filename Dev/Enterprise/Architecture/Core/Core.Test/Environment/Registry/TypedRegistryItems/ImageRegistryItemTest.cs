using System;
using System.Drawing;
using System.IO;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(ImageRegistryItem))]
	sealed class ImageRegistryItemTest : StronglyTypedRegistryItemTestCase<Image>
	{
		protected override StronglyTypedRegistryItem<Image, Image> GetNewRegistryItem()
		{
			return new ImageRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}

		protected override Image ValidValue
		{
			get { return new Bitmap(1, 1); }
		}

		public void TestRestoresImageIfItWasDisposedByExternalCode()
		{
			ImageRegistryItem imageRegistryItem = new ImageRegistryItem("TestRegistryItemWithDisposabelImage", null, null, null, RegistryStorageFlags.All);
			imageRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(10, 10));

			AssertEquals(10, imageRegistryItem.Value.Width);

			imageRegistryItem.Value.Dispose();
			AssertEquals(10, imageRegistryItem.Value.Width);

			imageRegistryItem.Value.Dispose();
			AssertEquals(10, imageRegistryItem.Value.Width);
		}

		public void TestGetValueProducesNullIfValueIsNullRepresentation()
		{
			var registryItem = new ImageRegistryItem("", null, null, null, RegistryStorageFlags.All);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ImageRegistryDataType.MagicNullBitmap);
			AssertNull("Null representation image should be returned as null", registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
		}

		Image CreateATestImage()
		{
			var bitmap = new Bitmap(1, 1);
			byte[] bytes;

			using (TempFile tempFile = TempFile.New())
			{
				bitmap.Save(tempFile.Filename);
				bytes = File.ReadAllBytes(tempFile.Filename);
			}

			var stream = new MemoryStream(bytes);
			return Image.FromStream(stream);
		}

		public void TestGetValueProducesRealValueWhenNotNullRepresentation()
		{
			var image = CreateATestImage();
			var registryItem = new ImageRegistryItem("", null, null, null, RegistryStorageFlags.All);
			registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, image);
			var retrievedValue = registryItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			AssertNotNull("Valid value should be retrieved", retrievedValue);

			var expectedBytes = registryItem.DataType.Serialise(image);
			var tempImage = registryItem.DataType.Deserialise(expectedBytes);
			expectedBytes = registryItem.DataType.Serialise(tempImage);
			var actualBytes = registryItem.DataType.Serialise(retrievedValue);
			AssertArrayEqualsByElements("Should get valid image value when not 'magic null image'", expectedBytes, actualBytes);
		}
	}
}
